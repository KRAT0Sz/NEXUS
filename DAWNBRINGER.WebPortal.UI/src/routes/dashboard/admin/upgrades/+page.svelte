<script lang="ts">
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalAdminStore } from '#lib/api/admin.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		Sparkles,
		Plus,
		Trash2,
		Search,
		CheckCircle,
		Tag
	} from 'lucide-svelte';

	// Grant section
	let grantAccountName = $state<string>('');
	let grantUpgradeCode = $state<string>('');
	let grantProcessing = $state<boolean>(false);
	let grantSuccessMsg = $state<string>('');

	// Revoke section
	let lookupAccountName = $state<string>('');
	let lookupProcessing = $state<boolean>(false);
	let ownedUpgrades = $state<string[]>([]);
	let hasLookedUp = $state<boolean>(false);
	let revokeProcessing = $state<string | null>(null);

	// Popular upgrade presets for quick selection
	const popularPresets = [
		{ label: 'Gold Name Color', code: 'cc.gold' },
		{ label: 'Diamond Name Color', code: 'cc.diamond' },
		{ label: 'Rainbow Name Color', code: 'cc.rainbow' },
		{ label: 'Flamboyant Announcer', code: 'an.flamboyant' },
		{ label: 'Samuel Jackson Announcer', code: 'an.samuel' },
		{ label: 'Baby Taunt', code: 't.baby' },
		{ label: 'Heavy Metal Taunt', code: 't.heavymetal' },
		{ label: 'VIP Chat Symbol', code: 'cs.vip' }
	];

	async function handleGrant(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		const acc = grantAccountName.trim();
		const code = grantUpgradeCode.trim();

		if (!acc || !code) {
			toast.warning('Please enter both the account name and upgrade code.');
			return;
		}

		grantProcessing = true;
		grantSuccessMsg = '';

		try {
			const res = await portalAdminStore.grantUpgrade(acc, code);
			if (res.ok) {
				grantSuccessMsg = t('dashboard.admin.upgrades.successfullyAddedUpgrades', {
					accountName: acc,
					upgrades: 1
				});
				toast.success(`Upgrade "${code}" granted to ${acc}.`);
				grantUpgradeCode = '';
				if (hasLookedUp && lookupAccountName.trim().toLowerCase() === acc.toLowerCase()) {
					await handleLookup();
				}
			} else {
				toast.error(res.error || 'Failed to grant upgrade.');
			}
		} finally {
			grantProcessing = false;
		}
	}

	async function handleLookup(): Promise<void> {
		const acc = lookupAccountName.trim();
		if (!acc) return;

		lookupProcessing = true;
		hasLookedUp = true;

		try {
			ownedUpgrades = await portalAdminStore.getAccountUpgrades(acc);
		} catch {
			toast.error('Failed to load account upgrades.');
		} finally {
			lookupProcessing = false;
		}
	}

	async function handleRevoke(code: string): Promise<void> {
		const acc = lookupAccountName.trim();
		if (!acc) return;

		if (!confirm(`Are you sure you want to remove upgrade "${code}" from ${acc}?`)) {
			return;
		}

		revokeProcessing = code;

		try {
			const res = await portalAdminStore.revokeUpgrade(acc, code);
			if (res.ok) {
				toast.success(`Removed "${code}" from ${acc}.`);
				ownedUpgrades = ownedUpgrades.filter((u) => u !== code);
			} else {
				toast.error(res.error || 'Failed to remove upgrade.');
			}
		} finally {
			revokeProcessing = null;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.admin.upgrades.title')}
	description="Grant exclusive store items, announcers, taunts, name colours, and avatars directly to player inventories."
>
	{#if grantSuccessMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{grantSuccessMsg}</div>
		</div>
	{/if}

	<div class="cards-grid-2">
		<!-- Add Upgrades Card -->
		<div class="glass-card-inner upgrade-card">
			<div class="card-header-block">
				<Sparkles size={24} class="accent-icon" />
				<h3 class="card-title">{t('dashboard.admin.upgrades.addUpgrades')}</h3>
			</div>

			<form class="upgrade-form" onsubmit={handleGrant}>
				<div class="form-group">
					<label for="grant-acc" class="field-label">{t('dashboard.admin.upgrades.accountName')}</label>
					<input
						id="grant-acc"
						type="text"
						bind:value={grantAccountName}
						placeholder="Target account name"
						class="kongor-field"
						required
					/>
				</div>

				<div class="form-group">
					<label for="grant-code" class="field-label">Upgrade Code</label>
					<input
						id="grant-code"
						type="text"
						bind:value={grantUpgradeCode}
						placeholder="e.g. cc.gold or an.flamboyant"
						class="kongor-field"
						required
					/>
				</div>

				<div class="presets-block">
					<span class="preset-label">Quick Presets:</span>
					<div class="presets-pills">
						{#each popularPresets as preset}
							<button
								type="button"
								class="preset-pill"
								onclick={() => (grantUpgradeCode = preset.code)}
							>
								{preset.label}
							</button>
						{/each}
					</div>
				</div>

				<button
					type="submit"
					class="btn-filled w-full mt-4"
					disabled={grantProcessing || !grantAccountName.trim() || !grantUpgradeCode.trim()}
				>
					<Plus size={18} />
					{grantProcessing ? 'Granting...' : t('dashboard.admin.upgrades.addUpgrades')}
				</button>
			</form>
		</div>

		<!-- View / Remove Upgrades Card -->
		<div class="glass-card-inner upgrade-card">
			<div class="card-header-block">
				<Trash2 size={24} class="accent-icon" />
				<h3 class="card-title">{t('dashboard.admin.upgrades.removeUpgrades')}</h3>
			</div>

			<form class="lookup-form" onsubmit={(e) => { e.preventDefault(); handleLookup(); }}>
				<div class="form-group">
					<label for="lookup-acc" class="field-label">{t('dashboard.admin.upgrades.accountName')}</label>
					<div class="search-field-row">
						<input
							id="lookup-acc"
							type="text"
							bind:value={lookupAccountName}
							placeholder="Lookup player to manage upgrades"
							class="kongor-field"
							required
						/>
						<button type="submit" class="btn-filled" disabled={lookupProcessing || !lookupAccountName.trim()}>
							<Search size={16} />
							Lookup
						</button>
					</div>
				</div>
			</form>

			{#if hasLookedUp}
				<div class="owned-upgrades-section mt-4">
					<div class="owned-header">
						<span class="owned-title">Owned Upgrades for {lookupAccountName}:</span>
						<span class="badge-chip">{ownedUpgrades.length} Total</span>
					</div>

					{#if ownedUpgrades.length === 0}
						<p class="empty-text">No custom upgrades found on this account.</p>
					{:else}
						<div class="upgrades-list">
							{#each ownedUpgrades as code}
								<div class="upgrade-item">
									<div class="upgrade-info">
										<Tag size={15} class="accent-icon" />
										<span class="upgrade-code">{code}</span>
									</div>
									<button
										type="button"
										class="btn-revoke"
										onclick={() => handleRevoke(code)}
										disabled={revokeProcessing === code}
										title="Remove upgrade"
									>
										<Trash2 size={14} />
										Remove
									</button>
								</div>
							{/each}
						</div>
					{/if}
				</div>
			{/if}
		</div>
	</div>
</DashboardContainer>

<style>
	.cards-grid-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 24px;
	}

	@media (max-width: 900px) {
		.cards-grid-2 {
			grid-template-columns: 1fr;
		}
	}

	.upgrade-card {
		padding: 24px;
		border-radius: 10px;
	}

	.card-header-block {
		display: flex;
		align-items: center;
		gap: 12px;
		margin-bottom: 20px;
	}

	.card-title {
		margin: 0;
		font-size: 1.15rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
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

	.search-field-row {
		display: flex;
		gap: 10px;
	}

	.search-field-row input {
		flex: 1;
	}

	.presets-block {
		margin: 14px 0;
	}

	.preset-label {
		display: block;
		font-size: 0.76rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.5px;
		color: var(--kongor-text-muted);
		margin-bottom: 8px;
	}

	.presets-pills {
		display: flex;
		flex-wrap: wrap;
		gap: 6px;
	}

	.preset-pill {
		padding: 4px 10px;
		border-radius: 4px;
		font-size: 0.78rem;
		background: rgba(var(--kongor-primary-rgb), 0.1);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.2);
		color: var(--kongor-text-heading);
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.preset-pill:hover {
		background: rgba(var(--kongor-primary-rgb), 0.25);
		color: var(--kongor-primary);
	}

	.owned-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 12px;
	}

	.owned-title {
		font-size: 0.88rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.upgrades-list {
		display: flex;
		flex-direction: column;
		gap: 8px;
		max-height: 320px;
		overflow-y: auto;
		padding-right: 4px;
	}

	.upgrade-item {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 8px 12px;
		border-radius: 6px;
		background: rgba(var(--kongor-surface-rgb), 0.35);
		border: 1px solid rgba(255, 255, 255, 0.06);
	}

	.upgrade-info {
		display: flex;
		align-items: center;
		gap: 8px;
	}

	.upgrade-code {
		font-size: 0.86rem;
		font-family: monospace;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.btn-revoke {
		background: none;
		border: 1px solid rgba(239, 68, 68, 0.3);
		color: #f87171;
		padding: 3px 8px;
		border-radius: 4px;
		font-size: 0.76rem;
		display: inline-flex;
		align-items: center;
		gap: 4px;
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.btn-revoke:hover {
		background: rgba(239, 68, 68, 0.15);
	}

	.w-full { width: 100%; }
	.mt-4 { margin-top: 20px; }
	.mb-4 { margin-bottom: 20px; }
	.empty-text { font-size: 0.88rem; color: var(--kongor-text-muted); }

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

