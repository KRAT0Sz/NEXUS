<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalSuspensionStore } from '#lib/api/suspension.svelte';
	import type { SuspensionDTO } from '#lib/types';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		Search,
		ShieldAlert,
		CheckCircle,
		AlertTriangle,
		Gavel,
		Clock,
		Info,
		Calendar
	} from 'lucide-svelte';

	let accountName = $state<string>('');
	let showForm = $state<boolean>(false);
	let loading = $state<boolean>(false);
	let submitting = $state<boolean>(false);
	let pastSuspensions = $state<SuspensionDTO[]>([]);

	// Form fields
	let selectedOffenseKey = $state<string>('');
	let durationNumber = $state<number>(1);
	let durationSuffix = $state<string>('d');
	let reason = $state<string>('');
	let internalDetails = $state<string>('');
	let suspensionSuccessMsg = $state<string>('');

	interface CodeOfConduct {
		key: string;
		subject: string;
		description: string;
	}

	const codeOfConductItems = $derived<CodeOfConduct[]>([
		{
			key: 'chatVerbalAbuse',
			subject: t('dashboard.gm.suspension.codeOfConduct.chatVerbalAbuse.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.chatVerbalAbuse.description')
		},
		{
			key: 'abilityAbuse',
			subject: t('dashboard.gm.suspension.codeOfConduct.abilityAbuse.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.abilityAbuse.description')
		},
		{
			key: 'itemAbuse',
			subject: t('dashboard.gm.suspension.codeOfConduct.itemAbuse.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.itemAbuse.description')
		},
		{
			key: 'staffImpersonation',
			subject: t('dashboard.gm.suspension.codeOfConduct.staffImpersonation.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.staffImpersonation.description')
		},
		{
			key: 'threats',
			subject: t('dashboard.gm.suspension.codeOfConduct.threats.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.threats.description')
		},
		{
			key: 'hateSpeech',
			subject: t('dashboard.gm.suspension.codeOfConduct.hateSpeech.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.hateSpeech.description')
		},
		{
			key: 'intentToLose',
			subject: t('dashboard.gm.suspension.codeOfConduct.intentToLose.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.intentToLose.description')
		},
		{
			key: 'cheating',
			subject: t('dashboard.gm.suspension.codeOfConduct.cheating.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.cheating.description')
		},
		{
			key: 'inappropriate',
			subject: t('dashboard.gm.suspension.codeOfConduct.inappropriate.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.inappropriate.description')
		},
		{
			key: 'statManipulation',
			subject: t('dashboard.gm.suspension.codeOfConduct.statManipulation.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.statManipulation.description')
		},
		{
			key: 'serialLeaving',
			subject: t('dashboard.gm.suspension.codeOfConduct.serialLeaving.subject'),
			description: t('dashboard.gm.suspension.codeOfConduct.serialLeaving.description')
		}
	]);

	const selectedOffense = $derived(
		codeOfConductItems.find((item) => item.key === selectedOffenseKey) || null
	);

	const durationSuffixes = $derived([
		{ label: t('dashboard.gm.suspension.day'), value: 'd' },
		{ label: t('dashboard.gm.suspension.week'), value: 'w' },
		{ label: t('dashboard.gm.suspension.month'), value: 'm' },
		{ label: t('dashboard.gm.suspension.year'), value: 'y' }
	]);

	function sentenceServed(sus: SuspensionDTO): boolean {
		if (sus.override) return true;
		const end = new Date(sus.timestampEnd).getTime();
		return end <= Date.now();
	}

	function formatDate(ts: string): string {
		try {
			return new Date(ts).toLocaleString();
		} catch {
			return ts;
		}
	}

	async function handleLookup(): Promise<void> {
		if (!accountName.trim()) return;
		loading = true;
		suspensionSuccessMsg = '';

		try {
			const res = await portalSuspensionStore.getSuspensionsFor(accountName.trim());
			pastSuspensions = res;
			showForm = true;
		} catch {
			toast.error('Failed to load suspensions for this account.');
		} finally {
			loading = false;
		}
	}

	function onOffenseChange(): void {
		if (selectedOffense) {
			reason = selectedOffense.subject;
		}
	}

	async function handleSubmitSuspension(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		if (!accountName.trim() || !reason.trim() || !internalDetails.trim()) {
			toast.warning('Please fill in all required fields.');
			return;
		}

		if (durationNumber <= 0) {
			toast.warning('Please enter a valid positive duration.');
			return;
		}

		submitting = true;
		const fullDuration = `${durationNumber}${durationSuffix.toUpperCase()}`;

		try {
			const res = await portalSuspensionStore.suspend({
				offenderAccountName: accountName.trim(),
				duration: fullDuration,
				reason: reason.trim(),
				internalDetails: internalDetails.trim()
			});

			if (res.ok) {
				suspensionSuccessMsg = t('dashboard.gm.suspension.suspensionSuccessful', {
					accountName: accountName.trim(),
					duration: fullDuration,
					reason: reason.trim(),
					internalDetails: internalDetails.trim()
				});
				toast.success(`Account ${accountName.trim()} suspended successfully.`);
				// Refresh history
				await handleLookup();
			} else {
				toast.error(res.error || 'Failed to issue suspension.');
			}
		} finally {
			submitting = false;
		}
	}

	onMount(() => {
		const queryAcc = page.url.searchParams.get('accountName');
		if (queryAcc) {
			accountName = queryAcc;
			handleLookup();
		}
	});
</script>

<DashboardContainer
	title={t('dashboard.gm.suspension.title')}
	description="Search an account, review their disciplinary record, and issue suspensions based on the Code of Conduct."
	{loading}
>
	<!-- Lookup Box -->
	<div class="search-card glass-card-inner">
		<h3 class="card-subtitle">{t('dashboard.gm.suspension.lookup')}</h3>
		<form class="search-form" onsubmit={(e) => { e.preventDefault(); handleLookup(); }}>
			<div class="input-with-icon">
				<Search size={18} class="input-icon" />
				<input
					type="text"
					bind:value={accountName}
					placeholder={t('dashboard.gm.suspension.accountName')}
					class="kongor-field"
					required
				/>
			</div>
			<button type="submit" class="btn-filled" disabled={loading || !accountName.trim()}>
				{t('dashboard.gm.suspension.lookup')}
			</button>
		</form>
	</div>

	{#if suspensionSuccessMsg}
		<div class="alert-box alert-success mt-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{suspensionSuccessMsg}</div>
		</div>
	{/if}

	{#if showForm}
		<!-- Disciplinary History -->
		<div class="history-card glass-card-inner mt-4">
			<div class="card-header-row">
				<h3 class="card-subtitle">
					{t('dashboard.gm.suspension.previousSuspensions', { count: pastSuspensions.length })}
				</h3>
				<span class="badge-chip">{pastSuspensions.length} Records</span>
			</div>

			{#if pastSuspensions.length === 0}
				<div class="alert-box alert-info">
					<Info size={18} class="alert-icon" />
					<div class="alert-text">
						{t('dashboard.gm.suspension.noBanMessage', { accountName })}
					</div>
				</div>
			{:else}
				<div class="suspensions-list">
					{#each pastSuspensions as sus}
						<div class="suspension-row" class:served={sentenceServed(sus)} class:active-ban={!sentenceServed(sus)}>
							<div class="sus-status-icon">
								{#if sentenceServed(sus)}
									<CheckCircle size={22} class="text-success" />
								{:else}
									<Gavel size={22} class="text-danger" />
								{/if}
							</div>
							<div class="sus-info">
								<div class="sus-header">
									<span class="sus-id">#{sus.id}</span>
									<span class="sus-badge" class:badge-active={!sentenceServed(sus)}>
										{sus.override ? 'OVERRIDDEN' : sentenceServed(sus) ? 'SERVED' : 'ACTIVE'}
									</span>
									<span class="sus-dates">
										<Calendar size={13} /> {formatDate(sus.timestampStart)} &rarr; {formatDate(sus.timestampEnd)}
									</span>
								</div>
								<div class="sus-reason">
									<strong>Reason:</strong> {sus.reason}
								</div>
								{#if sus.multiplier > 1.0}
									<div class="sus-meta text-warning">
										Multiplier: {sus.multiplier.toFixed(2)}x (Repeat Offender Penalty)
									</div>
								{/if}
								{#if !sentenceServed(sus)}
									<div class="sus-actions-link">
										<a href="/dashboard/staff/suspensions?susId={sus.id}" class="edit-link">
											Manage / Override this suspension &rarr;
										</a>
									</div>
								{/if}
							</div>
						</div>
					{/each}
				</div>
			{/if}
		</div>

		<!-- New Suspension Form -->
		<form class="suspension-form glass-card-inner mt-4" onsubmit={handleSubmitSuspension}>
			<h3 class="card-subtitle">Issue Disciplinary Action</h3>

			<!-- Code of Conduct Dropdown -->
			<div class="form-group">
				<label for="offense-select" class="field-label">
					{t('dashboard.gm.suspension.codeOfConduct.title')}
				</label>
				<select
					id="offense-select"
					bind:value={selectedOffenseKey}
					onchange={onOffenseChange}
					class="kongor-field kongor-select"
				>
					<option value="">-- Choose Code of Conduct Offense --</option>
					{#each codeOfConductItems as item}
						<option value={item.key}>{item.subject}</option>
					{/each}
				</select>
			</div>

			{#if selectedOffense}
				<div class="alert-box alert-info">
					<Info size={18} class="alert-icon" />
					<div class="alert-text">
						<strong>{selectedOffense.subject}:</strong> {selectedOffense.description}
					</div>
				</div>
			{/if}

			<!-- Duration Group -->
			<div class="form-row-2">
				<div class="form-group">
					<label for="dur-num" class="field-label">{t('dashboard.gm.suspension.duration')} (Amount)</label>
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
					<label for="dur-suffix" class="field-label">Duration Unit</label>
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

			<!-- Reason Input -->
			<div class="form-group">
				<label for="sus-reason" class="field-label">{t('dashboard.gm.suspension.reason')}</label>
				<input
					id="sus-reason"
					type="text"
					bind:value={reason}
					placeholder="Brief justification for the ban"
					class="kongor-field"
					required
				/>
			</div>

			<!-- Internal Details -->
			<div class="form-group">
				<label for="sus-details" class="field-label">{t('dashboard.gm.suspension.internalDetails')}</label>
				<textarea
					id="sus-details"
					bind:value={internalDetails}
					placeholder="Evidence links, match IDs, staff notes (only visible to GMs & Admins)"
					class="kongor-field kongor-textarea"
					rows="3"
					required
				></textarea>
			</div>

			<button
				type="submit"
				class="btn-filled btn-danger w-full"
				disabled={submitting || !reason.trim() || !internalDetails.trim()}
			>
				<Gavel size={18} />
				{t('dashboard.gm.suspension.submit')}
			</button>
		</form>
	{/if}
</DashboardContainer>

<style>
	.search-card,
	.history-card,
	.suspension-form {
		padding: 22px;
		border-radius: 10px;
	}

	.card-subtitle {
		margin: 0 0 16px 0;
		font-size: 1.05rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.card-header-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 16px;
	}

	.search-form {
		display: flex;
		gap: 12px;
	}

	.input-with-icon {
		position: relative;
		flex: 1;
	}

	.input-with-icon input {
		padding-left: 42px;
	}

	.mt-4 {
		margin-top: 20px;
	}

	.w-full {
		width: 100%;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		padding: 12px;
		font-size: 0.95rem;
	}

	/* Alert Box */
	.alert-box {
		display: flex;
		align-items: flex-start;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
		font-size: 0.88rem;
		line-height: 1.45;
		margin-bottom: 16px;
	}

	.alert-success {
		background: rgba(34, 197, 94, 0.12);
		border: 1px solid rgba(34, 197, 94, 0.35);
		color: #86efac;
	}

	.alert-info {
		background: rgba(var(--kongor-primary-rgb), 0.1);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		color: var(--kongor-text-heading);
	}

	.alert-icon {
		flex-shrink: 0;
		margin-top: 1px;
	}

	/* Suspensions List */
	.suspensions-list {
		display: flex;
		flex-direction: column;
		gap: 12px;
	}

	.suspension-row {
		display: flex;
		gap: 16px;
		padding: 16px;
		border-radius: 8px;
		background: rgba(var(--kongor-surface-rgb), 0.3);
		border: 1px solid rgba(255, 255, 255, 0.08);
	}

	.suspension-row.active-ban {
		border-color: rgba(239, 68, 68, 0.4);
		background: rgba(239, 68, 68, 0.06);
	}

	.sus-status-icon {
		padding-top: 2px;
	}

	.sus-info {
		flex: 1;
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.sus-header {
		display: flex;
		align-items: center;
		flex-wrap: wrap;
		gap: 10px;
		font-size: 0.84rem;
	}

	.sus-id {
		font-weight: 700;
		color: var(--kongor-text-heading);
	}

	.sus-badge {
		font-size: 0.7rem;
		font-weight: 700;
		padding: 2px 6px;
		border-radius: 4px;
		background: rgba(255, 255, 255, 0.1);
		color: var(--kongor-text-muted);
	}

	.sus-badge.badge-active {
		background: rgba(239, 68, 68, 0.2);
		color: #fca5a5;
		border: 1px solid rgba(239, 68, 68, 0.4);
	}

	.sus-dates {
		display: inline-flex;
		align-items: center;
		gap: 5px;
		color: var(--kongor-text-muted);
	}

	.sus-reason {
		font-size: 0.88rem;
		color: var(--kongor-text-body);
		margin-top: 4px;
	}

	.sus-meta {
		font-size: 0.78rem;
		font-weight: 600;
	}

	.edit-link {
		font-size: 0.82rem;
		color: var(--kongor-primary);
		text-decoration: underline;
		margin-top: 6px;
		display: inline-block;
	}

	.text-success { color: #22c55e; }
	.text-danger { color: #ef4444; }
	.text-warning { color: #f59e0b; }

	/* Form */
	.form-group {
		margin-bottom: 18px;
	}

	.form-row-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
	}

	@media (max-width: 600px) {
		.form-row-2 {
			grid-template-columns: 1fr;
		}
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

	.kongor-textarea {
		resize: vertical;
		font-family: inherit;
	}

	.btn-danger {
		background: linear-gradient(135deg, #b91c1c, #991b1b) !important;
		color: #fff !important;
	}

	.btn-danger:hover {
		background: linear-gradient(135deg, #dc2626, #b91c1c) !important;
	}
</style>

