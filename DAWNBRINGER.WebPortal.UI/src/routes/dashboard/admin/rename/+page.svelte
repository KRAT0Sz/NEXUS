<script lang="ts">
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalAdminStore } from '#lib/api/admin.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { Edit3, CheckCircle, AlertTriangle } from 'lucide-svelte';

	let accountName = $state<string>('');
	let newAccountName = $state<string>('');
	let processing = $state<boolean>(false);
	let successMsg = $state<string>('');

	async function handleRename(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		const oldName = accountName.trim();
		const newName = newAccountName.trim();

		if (!oldName || !newName) {
			toast.warning('Please enter both the current and new account names.');
			return;
		}

		if (newName.length > 15) {
			toast.warning('New account name must not exceed 15 characters.');
			return;
		}

		if (!confirm(`Are you sure you want to rename "${oldName}" to "${newName}"?`)) {
			return;
		}

		processing = true;
		successMsg = '';

		try {
			const res = await portalAdminStore.renameAccount(oldName, newName);
			if (res.ok) {
				successMsg = t('dashboard.admin.renameAccount.successfullyRenamedAccount', {
					accountName: oldName,
					newAccountName: newName
				});
				toast.success(`Account renamed successfully to ${newName}.`);
				accountName = '';
				newAccountName = '';
			} else {
				toast.error(res.error || 'Failed to rename account.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.admin.renameAccount.title')}
	description="Forcefully rename an account to resolve naming disputes or inappropriate names without consuming tokens."
>
	{#if successMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{successMsg}</div>
		</div>
	{/if}

	<div class="glass-card-inner rename-card">
		<div class="card-header-block">
			<Edit3 size={26} class="accent-icon" />
			<div>
				<h3 class="card-title">{t('dashboard.admin.renameAccount.title')}</h3>
				<p class="card-desc">
					The target player’s match statistics, MMR, and inventory will automatically link to the new nickname.
				</p>
			</div>
		</div>

		<form class="rename-form" onsubmit={handleRename}>
			<div class="form-group">
				<label for="old-name" class="field-label">
					{t('dashboard.admin.renameAccount.accountName')}
				</label>
				<input
					id="old-name"
					type="text"
					bind:value={accountName}
					placeholder="Current account name"
					class="kongor-field"
					required
				/>
			</div>

			<div class="form-group">
				<label for="new-name" class="field-label">
					{t('dashboard.admin.renameAccount.newAccountName')}
				</label>
				<input
					id="new-name"
					type="text"
					maxlength="15"
					bind:value={newAccountName}
					placeholder="New account name (max 15 chars)"
					class="kongor-field"
					required
				/>
			</div>

			<button
				type="submit"
				class="btn-filled"
				disabled={processing || !accountName.trim() || !newAccountName.trim()}
			>
				{processing ? 'Renaming Account...' : t('dashboard.admin.renameAccount.rename')}
			</button>
		</form>
	</div>
</DashboardContainer>

<style>
	.rename-card {
		padding: 26px;
		border-radius: 10px;
		max-width: 600px;
	}

	.card-header-block {
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
		margin-bottom: 18px;
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

