<script lang="ts">
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { Server, CheckCircle, ShieldAlert } from 'lucide-svelte';

	let accountName = $state<string>('');
	let processing = $state<boolean>(false);
	let successMsg = $state<string>('');

	async function handlePromote(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		if (!accountName.trim()) {
			toast.warning('Please enter an account name.');
			return;
		}

		processing = true;
		successMsg = '';

		try {
			const res = await portalStaffStore.promoteToHost(accountName.trim());
			if (res.ok) {
				successMsg = t('dashboard.staff.promoteToTMMHost.successfullyPromotedAccount', {
					accountName: accountName.trim()
				});
				toast.success(`Account ${accountName.trim()} promoted to TMM Host.`);
				accountName = '';
			} else {
				toast.error(res.error || 'Failed to promote account to host.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.staff.promoteToTMMHost.title')}
	description="Elevate trusted accounts to Server Host status, granting matchmaking host privileges."
>
	{#if successMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{successMsg}</div>
		</div>
	{/if}

	<div class="glass-card-inner promote-card">
		<div class="card-icon-header">
			<Server size={28} class="accent-icon" />
			<div>
				<h3 class="card-title">Promote Account to Match Server Host</h3>
				<p class="card-desc">
					Host accounts receive permissions to spawn dedicated match servers and automatically join the server hosts channel.
				</p>
			</div>
		</div>

		<form class="promote-form" onsubmit={handlePromote}>
			<div class="form-group">
				<label for="acc-name" class="field-label">
					{t('dashboard.staff.promoteToTMMHost.accountNameToPromote')}
				</label>
				<input
					id="acc-name"
					type="text"
					bind:value={accountName}
					placeholder="Enter account name to promote"
					class="kongor-field"
					required
				/>
			</div>

			<button
				type="submit"
				class="btn-filled"
				disabled={processing || !accountName.trim()}
			>
				{processing ? 'Promoting...' : t('dashboard.staff.promoteToTMMHost.promote')}
			</button>
		</form>
	</div>
</DashboardContainer>

<style>
	.promote-card {
		padding: 26px;
		border-radius: 10px;
		max-width: 600px;
	}

	.card-icon-header {
		display: flex;
		align-items: flex-start;
		gap: 16px;
		margin-bottom: 22px;
	}

	.card-title {
		margin: 0 0 6px 0;
		font-size: 1.15rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.card-desc {
		margin: 0;
		font-size: 0.86rem;
		color: var(--kongor-text-muted);
		line-height: 1.5;
	}

	.form-group {
		margin-bottom: 20px;
	}

	.field-label {
		display: block;
		margin-bottom: 6px;
		font-size: 0.84rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.mb-4 { margin-bottom: 20px; }

	.alert-box {
		display: flex;
		align-items: flex-start;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
		font-size: 0.88rem;
	}

	.alert-success {
		background: rgba(34, 197, 94, 0.12);
		border: 1px solid rgba(34, 197, 94, 0.35);
		color: #86efac;
	}

	.alert-icon { flex-shrink: 0; }
</style>

