<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalSuspensionStore } from '#lib/api/suspension.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { CheckCircle, AlertTriangle, ShieldCheck, Clock, Key } from 'lucide-svelte';

	let overrideSuspensionId = $state<string>('');
	let changeDurationSuspensionId = $state<string>('');
	let durationNumber = $state<number>(1);
	let durationSuffix = $state<string>('d');

	let overrideProcessing = $state<boolean>(false);
	let changeDurationProcessing = $state<boolean>(false);

	let overrideSuccessMsg = $state<string>('');
	let changeDurationSuccessMsg = $state<string>('');

	const durationSuffixes = $derived([
		{ label: t('dashboard.staff.suspensions.day'), value: 'd' },
		{ label: t('dashboard.staff.suspensions.week'), value: 'w' },
		{ label: t('dashboard.staff.suspensions.month'), value: 'm' },
		{ label: t('dashboard.staff.suspensions.year'), value: 'y' }
	]);

	onMount(() => {
		const susId = page.url.searchParams.get('susId');
		if (susId) {
			overrideSuspensionId = susId;
			changeDurationSuspensionId = susId;
		}
	});

	async function handleOverride(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		const idNum = parseInt(overrideSuspensionId.trim(), 10);
		if (isNaN(idNum) || idNum <= 0) {
			toast.warning('Please enter a valid Suspension ID.');
			return;
		}

		overrideProcessing = true;
		overrideSuccessMsg = '';

		try {
			const res = await portalSuspensionStore.overrideSuspension(idNum);
			if (res.ok) {
				overrideSuccessMsg = t('dashboard.staff.suspensions.overrideSuccessful', {
					suspensionId: idNum
				});
				toast.success(`Suspension #${idNum} overridden successfully.`);
			} else {
				toast.error(res.error || 'Failed to override suspension.');
			}
		} finally {
			overrideProcessing = false;
		}
	}

	async function handleChangeDuration(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		const idNum = parseInt(changeDurationSuspensionId.trim(), 10);
		if (isNaN(idNum) || idNum <= 0) {
			toast.warning('Please enter a valid Suspension ID.');
			return;
		}

		if (durationNumber <= 0) {
			toast.warning('Please enter a valid positive duration.');
			return;
		}

		changeDurationProcessing = true;
		changeDurationSuccessMsg = '';
		const fullDuration = `${durationNumber}${durationSuffix.toUpperCase()}`;

		try {
			const res = await portalSuspensionStore.changeDuration(idNum, fullDuration);
			if (res.ok) {
				changeDurationSuccessMsg = t('dashboard.staff.suspensions.newDurationSuccessful', {
					suspensionId: idNum,
					newDuration: fullDuration
				});
				toast.success(`Suspension #${idNum} duration updated to ${fullDuration}.`);
			} else {
				toast.error(res.error || 'Failed to change suspension duration.');
			}
		} finally {
			changeDurationProcessing = false;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.staff.suspensions.title')}
	description="Administrative override and duration adjustment for active account suspensions."
>
	{#if overrideSuccessMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{overrideSuccessMsg}</div>
		</div>
	{/if}

	{#if changeDurationSuccessMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{changeDurationSuccessMsg}</div>
		</div>
	{/if}

	<div class="cards-grid-2">
		<!-- Override Suspension -->
		<form class="glass-card-inner control-card" onsubmit={handleOverride}>
			<div class="card-header">
				<ShieldCheck size={22} class="accent-icon" />
				<h3 class="card-title">{t('dashboard.staff.suspensions.override')}</h3>
			</div>
			<p class="card-desc">
				Immediately lift an active suspension, restore account access, and record the override in the staff audit logs.
			</p>

			<div class="form-group">
				<label for="override-id" class="field-label">
					{t('dashboard.staff.suspensions.suspensionID')}
				</label>
				<input
					id="override-id"
					type="number"
					bind:value={overrideSuspensionId}
					placeholder="e.g. 1042"
					class="kongor-field"
					required
				/>
			</div>

			<button
				type="submit"
				class="btn-filled w-full"
				disabled={overrideProcessing || !overrideSuspensionId.trim()}
			>
				{overrideProcessing ? 'Processing...' : t('dashboard.staff.suspensions.override')}
			</button>
		</form>

		<!-- Change Duration -->
		<form class="glass-card-inner control-card" onsubmit={handleChangeDuration}>
			<div class="card-header">
				<Clock size={22} class="accent-icon" />
				<h3 class="card-title">{t('dashboard.staff.suspensions.changeDuration')}</h3>
			</div>
			<p class="card-desc">
				Adjust the remaining duration of an ongoing suspension without removing the infraction record.
			</p>

			<div class="form-group">
				<label for="duration-id" class="field-label">
					{t('dashboard.staff.suspensions.suspensionID')}
				</label>
				<input
					id="duration-id"
					type="number"
					bind:value={changeDurationSuspensionId}
					placeholder="e.g. 1042"
					class="kongor-field"
					required
				/>
			</div>

			<div class="form-row-2">
				<div class="form-group">
					<label for="dur-num" class="field-label">
						{t('dashboard.staff.suspensions.duration')} (Amount)
					</label>
					<input
						id="dur-num"
						type="number"
						min="1"
						bind:value={durationNumber}
						class="kongor-field"
						required
					/>
				</div>
				<div class="form-group">
					<label for="dur-suffix" class="field-label">Unit</label>
					<select
						id="dur-suffix"
						bind:value={durationSuffix}
						class="kongor-field kongor-select"
					>
						{#each durationSuffixes as s}
							<option value={s.value}>{s.label}</option>
						{/each}
					</select>
				</div>
			</div>

			<button
				type="submit"
				class="btn-filled w-full"
				disabled={changeDurationProcessing || !changeDurationSuspensionId.trim()}
			>
				{changeDurationProcessing ? 'Processing...' : t('dashboard.staff.suspensions.changeDuration')}
			</button>
		</form>
	</div>
</DashboardContainer>

<style>
	.cards-grid-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 24px;
	}

	@media (max-width: 800px) {
		.cards-grid-2 {
			grid-template-columns: 1fr;
		}
	}

	.control-card {
		padding: 24px;
		border-radius: 10px;
		display: flex;
		flex-direction: column;
	}

	.card-header {
		display: flex;
		align-items: center;
		gap: 12px;
		margin-bottom: 10px;
	}

	.card-title {
		margin: 0;
		font-size: 1.15rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.card-desc {
		margin: 0 0 20px 0;
		font-size: 0.86rem;
		color: var(--kongor-text-muted);
		line-height: 1.5;
	}

	.form-group {
		margin-bottom: 18px;
	}

	.form-row-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 14px;
	}

	.field-label {
		display: block;
		margin-bottom: 6px;
		font-size: 0.84rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.kongor-select {
		cursor: pointer;
	}

	.w-full {
		width: 100%;
		margin-top: auto;
		padding: 12px;
	}

	.mb-4 {
		margin-bottom: 20px;
	}

	.alert-box {
		display: flex;
		align-items: flex-start;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
		font-size: 0.88rem;
		line-height: 1.45;
	}

	.alert-success {
		background: rgba(34, 197, 94, 0.12);
		border: 1px solid rgba(34, 197, 94, 0.35);
		color: #86efac;
	}

	.alert-icon {
		flex-shrink: 0;
		margin-top: 1px;
	}
</style>

