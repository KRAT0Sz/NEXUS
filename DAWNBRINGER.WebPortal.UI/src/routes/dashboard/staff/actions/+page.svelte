<script lang="ts">
	import { onMount } from 'svelte';
	import { t } from '#lib/i18n/index.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import type { StaffActionLogDTO } from '#lib/types';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		History,
		Search,
		Calendar,
		ArrowRight,
		Info,
		RefreshCw
	} from 'lucide-svelte';

	let loading = $state<boolean>(true);
	let filterQuery = $state<string>('');
	let actions = $state<StaffActionLogDTO[]>([]);

	async function loadActions(): Promise<void> {
		loading = true;
		try {
			actions = await portalStaffStore.getActions(100);
		} finally {
			loading = false;
		}
	}

	onMount(() => {
		loadActions();
	});

	const filteredActions = $derived.by(() => {
		if (!filterQuery.trim()) return actions;
		const q = filterQuery.toLowerCase();
		return actions.filter(
			(a) =>
				a.targetID.toLowerCase().includes(q) ||
				a.action.toLowerCase().includes(q) ||
				a.details.toLowerCase().includes(q)
		);
	});

	function formatDate(ts: string): string {
		try {
			return new Date(ts).toLocaleString();
		} catch {
			return ts;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.staff.actions.title')}
	description="Audit history of administrative actions, moderation commands, and currency grants."
	{loading}
>
	<!-- Search & Filter Bar -->
	<div class="filter-bar glass-card-inner mb-4">
		<div class="input-with-icon flex-1">
			<Search size={18} class="input-icon" />
			<input
				type="text"
				bind:value={filterQuery}
				placeholder="Filter by target account, action type, or details..."
				class="kongor-field"
			/>
		</div>
		<button type="button" class="btn-outlined btn-refresh" onclick={loadActions} disabled={loading}>
			<RefreshCw size={16} class={loading ? 'spin-icon' : ''} />
			Refresh
		</button>
	</div>

	<!-- Actions Table -->
	<div class="glass-card-inner table-card">
		{#if filteredActions.length === 0}
			<div class="empty-state">
				<History size={40} class="empty-icon text-muted" />
				<p class="empty-text">No staff actions recorded yet.</p>
			</div>
		{:else}
			<div class="table-responsive">
				<table class="kongor-table">
					<thead>
						<tr>
							<th>{t('dashboard.staff.actions.targetName')}</th>
							<th>{t('dashboard.staff.actions.action')}</th>
							<th>{t('dashboard.staff.actions.details')}</th>
							<th>{t('dashboard.staff.actions.timestamp')}</th>
						</tr>
					</thead>
					<tbody>
						{#each filteredActions as act (act.id)}
							<tr>
								<td class="target-cell">
									<a
										href="/dashboard/staff/look-up-account-or-user?accountName={encodeURIComponent(act.targetID)}"
										class="target-link"
										title="Lookup account"
									>
										{act.targetID}
										<ArrowRight size={13} />
									</a>
								</td>
								<td>
									<span class="action-tag">{act.action}</span>
								</td>
								<td class="details-cell">
									<span class="details-text" title={act.details}>{act.details}</span>
								</td>
								<td class="timestamp-cell">
									<span class="time-stamp">
										<Calendar size={13} />
										{formatDate(act.timestamp)}
									</span>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</div>
</DashboardContainer>

<style>
	.filter-bar {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
	}

	.flex-1 { flex: 1; }

	.input-with-icon {
		position: relative;
	}

	.input-with-icon input {
		padding-left: 42px;
	}

	.btn-refresh {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 18px;
		white-space: nowrap;
	}

	.table-card {
		padding: 0;
		border-radius: 10px;
		overflow: hidden;
	}

	.table-responsive {
		width: 100%;
		overflow-x: auto;
	}

	.kongor-table {
		width: 100%;
		border-collapse: collapse;
		font-size: 0.88rem;
		text-align: left;
	}

	.kongor-table th {
		padding: 14px 18px;
		font-weight: 700;
		text-transform: uppercase;
		font-size: 0.76rem;
		letter-spacing: 0.75px;
		color: var(--kongor-text-muted);
		background: rgba(var(--kongor-surface-rgb), 0.5);
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.15);
	}

	.kongor-table td {
		padding: 14px 18px;
		border-bottom: 1px solid rgba(255, 255, 255, 0.05);
		color: var(--kongor-text-body);
	}

	.kongor-table tr:hover td {
		background: rgba(var(--kongor-primary-rgb), 0.06);
	}

	.target-link {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-weight: 600;
		color: var(--kongor-primary);
		text-decoration: none;
	}

	.target-link:hover {
		text-decoration: underline;
	}

	.action-tag {
		display: inline-block;
		font-size: 0.76rem;
		font-weight: 600;
		padding: 3px 8px;
		border-radius: 4px;
		background: rgba(var(--kongor-primary-rgb), 0.12);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		color: var(--kongor-text-heading);
	}

	.details-cell {
		max-width: 320px;
	}

	.details-text {
		display: block;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		color: var(--kongor-text-muted);
	}

	.timestamp-cell {
		white-space: nowrap;
	}

	.time-stamp {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 0.82rem;
		color: var(--kongor-text-muted);
	}

	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 48px 24px;
		gap: 12px;
	}

	.empty-icon { opacity: 0.4; }
	.empty-text { margin: 0; color: var(--kongor-text-muted); font-size: 0.95rem; }
	.mb-4 { margin-bottom: 20px; }
</style>

