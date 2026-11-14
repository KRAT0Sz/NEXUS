<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { Search, Lock, Unlock, CheckCircle, AlertTriangle, UserCheck } from 'lucide-svelte';

	let accountName = $state<string>('');
	let loading = $state<boolean>(false);
	let processing = $state<boolean>(false);

	let currentAccountType = $state<string | null>(null);
	let targetAccountName = $state<string>('');
	let successMessage = $state<string>('');

	async function handleLookup(): Promise<void> {
		if (!accountName.trim()) return;
		loading = true;
		successMessage = '';

		try {
			const res = await portalStaffStore.getAccountType(accountName.trim());
			if (res) {
				targetAccountName = res.accountName;
				currentAccountType = res.accountType;
			} else {
				toast.error(`Account "${accountName.trim()}" not found.`);
				currentAccountType = null;
			}
		} catch {
			toast.error('Failed to lookup account type.');
		} finally {
			loading = false;
		}
	}

	async function handleEnable(): Promise<void> {
		if (!targetAccountName) return;
		processing = true;

		try {
			const res = await portalStaffStore.enableAccount(targetAccountName);
			if (res.ok) {
				successMessage = t('dashboard.staff.enableDisable.successfullyEnabledAccount', {
					accountName: targetAccountName
				});
				toast.success(`Account ${targetAccountName} enabled.`);
				currentAccountType = 'Legacy';
			} else {
				toast.error(res.error || 'Failed to enable account.');
			}
		} finally {
			processing = false;
		}
	}

	async function handleDisable(): Promise<void> {
		if (!targetAccountName) return;
		if (!confirm(`Are you sure you want to disable ${targetAccountName}? The player will not be able to log into HoN.`)) {
			return;
		}
		processing = true;

		try {
			const res = await portalStaffStore.disableAccount(targetAccountName);
			if (res.ok) {
				successMessage = t('dashboard.staff.enableDisable.successfullyDisabledAccount', {
					accountName: targetAccountName
				});
				toast.success(`Account ${targetAccountName} disabled.`);
				currentAccountType = 'Disabled';
			} else {
				toast.error(res.error || 'Failed to disable account.');
			}
		} finally {
			processing = false;
		}
	}

	onMount(() => {
		const acc = page.url.searchParams.get('accountName');
		if (acc) {
			accountName = acc;
			handleLookup();
		}
	});
</script>

<DashboardContainer
	title={t('dashboard.staff.enableDisable.title')}
	description="Check account access status and administratively enable or disable game login permissions."
	{loading}
>
	<!-- Lookup Box -->
	<div class="glass-card-inner search-card mb-4">
		<h3 class="card-subtitle">{t('dashboard.staff.enableDisable.lookup')}</h3>
		<form class="search-form" onsubmit={(e) => { e.preventDefault(); handleLookup(); }}>
			<div class="input-with-icon">
				<Search size={18} class="input-icon" />
				<input
					type="text"
					bind:value={accountName}
					placeholder={t('dashboard.staff.enableDisable.accountName')}
					class="kongor-field"
					required
				/>
			</div>
			<button type="submit" class="btn-filled" disabled={loading || !accountName.trim()}>
				{t('dashboard.staff.enableDisable.lookup')}
			</button>
		</form>
	</div>

	{#if successMessage}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{successMessage}</div>
		</div>
	{/if}

	{#if currentAccountType !== null}
		<div class="glass-card-inner status-card">
			<div class="status-top">
				<div>
					<h3 class="account-heading">{targetAccountName}</h3>
					<div class="type-indicator">
						{t('dashboard.staff.enableDisable.currentAccountType', {
							accountType: currentAccountType
						})}
						<span
							class="status-chip"
							class:chip-disabled={currentAccountType === 'Disabled'}
							class:chip-active={currentAccountType !== 'Disabled'}
						>
							{currentAccountType === 'Disabled' ? 'DISABLED' : 'ACTIVE'}
						</span>
					</div>
				</div>
			</div>

			<div class="actions-row mt-4">
				<button
					type="button"
					class="btn-filled btn-danger-action"
					onclick={handleDisable}
					disabled={processing || currentAccountType === 'Disabled'}
				>
					<Lock size={16} />
					{t('dashboard.staff.enableDisable.disableAccount')}
				</button>
				<button
					type="button"
					class="btn-filled btn-success-action"
					onclick={handleEnable}
					disabled={processing || currentAccountType !== 'Disabled'}
				>
					<Unlock size={16} />
					{t('dashboard.staff.enableDisable.enableAccount')}
				</button>
			</div>
		</div>
	{/if}
</DashboardContainer>

<style>
	.search-card,
	.status-card {
		padding: 24px;
		border-radius: 10px;
	}

	.card-subtitle {
		margin: 0 0 16px 0;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
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

	.account-heading {
		margin: 0 0 8px 0;
		font-size: 1.25rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.type-indicator {
		font-size: 0.92rem;
		color: var(--kongor-text-muted);
		display: flex;
		align-items: center;
		gap: 10px;
	}

	.status-chip {
		font-size: 0.72rem;
		font-weight: 700;
		padding: 2px 8px;
		border-radius: 4px;
	}

	.chip-active {
		background: rgba(34, 197, 94, 0.15);
		color: #86efac;
		border: 1px solid rgba(34, 197, 94, 0.3);
	}

	.chip-disabled {
		background: rgba(239, 68, 68, 0.15);
		color: #fca5a5;
		border: 1px solid rgba(239, 68, 68, 0.3);
	}

	.actions-row {
		display: flex;
		gap: 16px;
	}

	.btn-danger-action {
		background: linear-gradient(135deg, #b91c1c, #991b1b) !important;
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 22px;
	}

	.btn-success-action {
		background: linear-gradient(135deg, #15803d, #166534) !important;
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 22px;
	}

	.mb-4 { margin-bottom: 20px; }
	.mt-4 { margin-top: 20px; }

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

