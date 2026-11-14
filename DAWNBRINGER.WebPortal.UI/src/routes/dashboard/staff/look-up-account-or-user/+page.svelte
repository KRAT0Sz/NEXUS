<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import type { ConnectedAccountUserResultDTO } from '#lib/types';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		Search,
		User,
		ShieldAlert,
		Lock,
		CheckCircle,
		Mail,
		Tag
	} from 'lucide-svelte';

	let query = $state<string>('');
	let loading = $state<boolean>(false);
	let results = $state<ConnectedAccountUserResultDTO[]>([]);
	let hasSearched = $state<boolean>(false);

	async function handleLookup(): Promise<void> {
		if (!query.trim()) return;
		loading = true;
		hasSearched = true;

		try {
			results = await portalStaffStore.lookup(query.trim());
		} catch {
			toast.error('Failed to lookup account or user.');
		} finally {
			loading = false;
		}
	}

	onMount(() => {
		const acc = page.url.searchParams.get('accountName');
		if (acc) {
			query = acc;
			handleLookup();
		}
	});
</script>

<DashboardContainer
	title={t('dashboard.staff.lookUpAccountOrUser.title')}
	description="Search by account name, user name, or email to inspect related sub-accounts and active status."
	{loading}
>
	<!-- Search Box -->
	<div class="glass-card-inner search-card mb-4">
		<h3 class="card-subtitle">{t('dashboard.staff.lookUpAccountOrUser.lookup')}</h3>
		<form class="search-form" onsubmit={(e) => { e.preventDefault(); handleLookup(); }}>
			<div class="input-with-icon">
				<Search size={18} class="input-icon" />
				<input
					type="text"
					bind:value={query}
					placeholder={t('dashboard.staff.lookUpAccountOrUser.fuzzyIdentifier')}
					class="kongor-field"
					required
				/>
			</div>
			<button type="submit" class="btn-filled" disabled={loading || !query.trim()}>
				{t('dashboard.staff.lookUpAccountOrUser.lookup')}
			</button>
		</form>
	</div>

	<!-- Results List -->
	{#if hasSearched}
		<div class="glass-card-inner results-card">
			<div class="results-header">
				<h3 class="card-subtitle">{t('dashboard.staff.lookUpAccountOrUser.result')}</h3>
				<span class="badge-chip">{results.length} Found</span>
			</div>

			{#if results.length === 0}
				<div class="empty-state">
					<Search size={36} class="empty-icon" />
					<p class="empty-text">No accounts or users matched "{query}".</p>
				</div>
			{:else}
				<div class="entities-list">
					{#each results as entity}
						<div class="entity-item">
							<div class="entity-avatar">
								<User size={24} class="accent-icon" />
							</div>
							<div class="entity-content">
								<div class="entity-top">
									<h4 class="entity-name">{entity.nameWithClanTag || entity.name}</h4>
									<span class="entity-pill" class:pill-main={entity.main}>
										{entity.main ? 'Main Account' : 'Sub-Account'}
									</span>
									<span class="entity-type-badge">
										{entity.type}
									</span>
								</div>

								<div class="entity-details">
									<span class="detail-item">
										<Mail size={13} />
										{entity.maskedEmail}
									</span>
									<span class="detail-item">
										<Tag size={13} />
										Role: <strong>{entity.userRole}</strong>
									</span>
								</div>

								<div class="entity-actions">
									<a
										href="/dashboard/gm/suspend-a-player?accountName={encodeURIComponent(entity.name)}"
										class="action-btn-link text-danger"
									>
										<ShieldAlert size={15} />
										{t('dashboard.staff.lookUpAccountOrUser.lookupSuspensions')}
									</a>
									<a
										href="/dashboard/staff/enable-disable?accountName={encodeURIComponent(entity.name)}"
										class="action-btn-link"
									>
										<Lock size={15} />
										{t('dashboard.staff.lookUpAccountOrUser.enableDisable')}
									</a>
								</div>
							</div>
						</div>
					{/each}
				</div>
			{/if}
		</div>
	{/if}
</DashboardContainer>

<style>
	.search-card,
	.results-card {
		padding: 24px;
		border-radius: 10px;
	}

	.card-subtitle {
		margin: 0 0 16px 0;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.results-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 20px;
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

	.entities-list {
		display: flex;
		flex-direction: column;
		gap: 14px;
	}

	.entity-item {
		display: flex;
		gap: 16px;
		padding: 18px;
		border-radius: 8px;
		background: rgba(var(--kongor-surface-rgb), 0.35);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.12);
	}

	.entity-avatar {
		width: 44px;
		height: 44px;
		border-radius: 50%;
		background: rgba(var(--kongor-primary-rgb), 0.12);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	.entity-content {
		flex: 1;
		display: flex;
		flex-direction: column;
		gap: 8px;
	}

	.entity-top {
		display: flex;
		align-items: center;
		flex-wrap: wrap;
		gap: 10px;
	}

	.entity-name {
		margin: 0;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.entity-pill {
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		padding: 2px 8px;
		border-radius: 4px;
		background: rgba(255, 255, 255, 0.08);
		color: var(--kongor-text-muted);
	}

	.entity-pill.pill-main {
		background: rgba(var(--kongor-primary-rgb), 0.15);
		color: var(--kongor-primary);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.3);
	}

	.entity-type-badge {
		font-size: 0.72rem;
		font-weight: 700;
		padding: 2px 8px;
		border-radius: 4px;
		background: rgba(148, 163, 184, 0.12);
		color: #94a3b8;
	}

	.entity-details {
		display: flex;
		align-items: center;
		flex-wrap: wrap;
		gap: 16px;
		font-size: 0.84rem;
		color: var(--kongor-text-muted);
	}

	.detail-item {
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.entity-actions {
		display: flex;
		align-items: center;
		gap: 16px;
		margin-top: 4px;
		padding-top: 10px;
		border-top: 1px solid rgba(255, 255, 255, 0.06);
	}

	.action-btn-link {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 0.84rem;
		font-weight: 600;
		color: var(--kongor-primary);
		text-decoration: none;
	}

	.action-btn-link:hover {
		text-decoration: underline;
	}

	.action-btn-link.text-danger {
		color: #f87171;
	}

	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 36px 16px;
		gap: 10px;
		color: var(--kongor-text-muted);
	}

	.empty-icon { opacity: 0.4; }
	.empty-text { margin: 0; }
	.mb-4 { margin-bottom: 20px; }
</style>

