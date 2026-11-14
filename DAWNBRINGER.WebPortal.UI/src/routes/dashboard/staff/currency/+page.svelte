<script lang="ts">
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		Plus,
		Trash2,
		Play,
		Coins,
		CheckCircle,
		AlertCircle,
		Info,
		Loader2
	} from 'lucide-svelte';

	interface CurrencyRow {
		id: string;
		userName: string;
		gold: number;
		silver: number;
		tickets: number;
	}

	let rows = $state<CurrencyRow[]>([
		{ id: '1', userName: '', gold: 0, silver: 0, tickets: 0 }
	]);

	let isRunning = $state<boolean>(false);
	let batchLogs = $state<{ text: string; success: boolean }[]>([]);

	function addRow(): void {
		rows = [
			...rows,
			{
				id: Math.random().toString(36).slice(2, 9),
				userName: '',
				gold: 0,
				silver: 0,
				tickets: 0
			}
		];
	}

	function removeRow(id: string): void {
		if (rows.length <= 1) return;
		rows = rows.filter((r) => r.id !== id);
	}

	async function runBatchJob(): Promise<void> {
		const validRows = rows.filter((r) => r.userName.trim().length > 0);
		if (validRows.length === 0) {
			toast.warning('Please enter at least one user name to grant currency.');
			return;
		}

		isRunning = true;
		batchLogs = [];

		for (const row of validRows) {
			const accName = row.userName.trim();
			let rowSuccess = true;
			const grantedParts: string[] = [];

			if (row.gold > 0) {
				const gRes = await portalStaffStore.issueCurrency(accName, row.gold, 'gold');
				if (gRes.ok) grantedParts.push(`${row.gold} Gold`);
				else rowSuccess = false;
			}

			if (row.silver > 0) {
				const sRes = await portalStaffStore.issueCurrency(accName, row.silver, 'silver');
				if (sRes.ok) grantedParts.push(`${row.silver} Silver`);
				else rowSuccess = false;
			}

			if (row.tickets > 0) {
				const tRes = await portalStaffStore.issueCurrency(accName, row.tickets, 'tickets');
				if (tRes.ok) grantedParts.push(`${row.tickets} Tickets`);
				else rowSuccess = false;
			}

			if (grantedParts.length === 0) {
				batchLogs.push({
					text: `Skipped ${accName} (Amounts were zero)`,
					success: false
				});
				continue;
			}

			if (rowSuccess) {
				batchLogs.push({
					text: t('dashboard.staff.currency.batchLogMessage', {
						gold: row.gold,
						silver: row.silver,
						tickets: row.tickets,
						userName: accName
					}),
					success: true
				});
			} else {
				batchLogs.push({
					text: t('dashboard.staff.currency.batchLogErrorMessage', { userName: accName }),
					success: false
				});
			}
		}

		isRunning = false;
		toast.success('Batch job completed.');
	}
</script>

<DashboardContainer
	title={t('dashboard.staff.currency.title')}
	description="Distribute Gold Coins, Silver Coins, and Plinko Tickets to one or multiple accounts in a single batch operation."
	loading={isRunning}
>
	<div class="alert-box alert-info mb-4">
		<Info size={18} class="alert-icon" />
		<div class="alert-text">
			{t('dashboard.staff.currency.instructions')}
		</div>
	</div>

	<!-- Batch Rows Table / Card -->
	<div class="glass-card-inner batch-card">
		<div class="card-top-row">
			<h3 class="card-subtitle">{t('dashboard.staff.currency.handleBatchRows')}</h3>
			<button type="button" class="btn-outlined btn-sm" onclick={addRow}>
				<Plus size={16} />
				{t('dashboard.staff.currency.addRow')}
			</button>
		</div>

		<div class="rows-container">
			{#each rows as row, index (row.id)}
				<div class="currency-row">
					<div class="row-num">#{index + 1}</div>
					<div class="field-item flex-2">
						<label class="compact-label">{t('dashboard.staff.currency.userName')}</label>
						<input
							type="text"
							bind:value={row.userName}
							placeholder="Account Name"
							class="kongor-field"
							disabled={isRunning}
						/>
					</div>
					<div class="field-item">
						<label class="compact-label text-gold">{t('dashboard.staff.currency.gold')}</label>
						<input
							type="number"
							min="0"
							bind:value={row.gold}
							class="kongor-field"
							disabled={isRunning}
						/>
					</div>
					<div class="field-item">
						<label class="compact-label text-silver">{t('dashboard.staff.currency.silver')}</label>
						<input
							type="number"
							min="0"
							bind:value={row.silver}
							class="kongor-field"
							disabled={isRunning}
						/>
					</div>
					<div class="field-item">
						<label class="compact-label text-tickets">{t('dashboard.staff.currency.tickets')}</label>
						<input
							type="number"
							min="0"
							bind:value={row.tickets}
							class="kongor-field"
							disabled={isRunning}
						/>
					</div>
					{#if rows.length > 1}
						<button
							type="button"
							class="btn-delete"
							onclick={() => removeRow(row.id)}
							title="Remove row"
							disabled={isRunning}
						>
							<Trash2 size={16} />
						</button>
					{/if}
				</div>
			{/each}
		</div>

		<div class="batch-actions-row">
			<button
				type="button"
				class="btn-filled btn-batch"
				onclick={runBatchJob}
				disabled={isRunning}
			>
				{#if isRunning}
					<Loader2 size={18} class="spin-icon" />
					Processing Batch...
				{:else}
					<Play size={18} />
					{t('dashboard.staff.currency.runBatchJob')}
				{/if}
			</button>
		</div>
	</div>

	<!-- Execution Results Log -->
	{#if batchLogs.length > 0}
		<div class="glass-card-inner log-card mt-4">
			<h3 class="card-subtitle">{t('dashboard.staff.currency.result')}</h3>
			<ul class="logs-list">
				{#each batchLogs as log}
					<li class="log-item" class:log-success={log.success} class:log-error={!log.success}>
						{#if log.success}
							<CheckCircle size={16} class="text-success" />
						{:else}
							<AlertCircle size={16} class="text-danger" />
						{/if}
						<span>{log.text}</span>
					</li>
				{/each}
			</ul>
		</div>
	{/if}
</DashboardContainer>

<style>
	.batch-card,
	.log-card {
		padding: 24px;
		border-radius: 10px;
	}

	.card-top-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 18px;
	}

	.card-subtitle {
		margin: 0;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.rows-container {
		display: flex;
		flex-direction: column;
		gap: 12px;
		margin-bottom: 22px;
	}

	.currency-row {
		display: flex;
		align-items: flex-end;
		gap: 12px;
		padding: 12px 14px;
		border-radius: 8px;
		background: rgba(var(--kongor-surface-rgb), 0.35);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.1);
	}

	@media (max-width: 800px) {
		.currency-row {
			flex-wrap: wrap;
		}
	}

	.row-num {
		font-size: 0.82rem;
		font-weight: 700;
		color: var(--kongor-text-muted);
		padding-bottom: 10px;
		width: 24px;
	}

	.field-item {
		flex: 1;
		min-width: 90px;
	}

	.field-item.flex-2 {
		flex: 2;
		min-width: 140px;
	}

	.compact-label {
		display: block;
		font-size: 0.76rem;
		font-weight: 700;
		margin-bottom: 5px;
		text-transform: uppercase;
		letter-spacing: 0.5px;
	}

	.text-gold { color: #f59e0b; }
	.text-silver { color: #94a3b8; }
	.text-tickets { color: #38bdf8; }

	.btn-delete {
		background: none;
		border: 1px solid rgba(239, 68, 68, 0.3);
		color: #ef4444;
		border-radius: 6px;
		padding: 8px;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 2px;
		transition: background 0.15s ease;
	}

	.btn-delete:hover {
		background: rgba(239, 68, 68, 0.15);
	}

	.btn-sm {
		padding: 6px 14px;
		font-size: 0.84rem;
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.batch-actions-row {
		display: flex;
		justify-content: flex-end;
	}

	.btn-batch {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 24px;
		font-size: 0.95rem;
	}

	.mt-4 { margin-top: 20px; }
	.mb-4 { margin-bottom: 20px; }

	/* Alert Box */
	.alert-box {
		display: flex;
		align-items: flex-start;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
		font-size: 0.88rem;
	}

	.alert-info {
		background: rgba(var(--kongor-primary-rgb), 0.1);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		color: var(--kongor-text-heading);
	}

	.alert-icon { flex-shrink: 0; }

	/* Log List */
	.logs-list {
		list-style: none;
		padding: 0;
		margin: 16px 0 0 0;
		display: flex;
		flex-direction: column;
		gap: 8px;
	}

	.log-item {
		display: flex;
		align-items: center;
		gap: 10px;
		padding: 10px 14px;
		border-radius: 6px;
		font-size: 0.86rem;
		background: rgba(var(--kongor-surface-rgb), 0.3);
		border-left: 3px solid transparent;
	}

	.log-item.log-success {
		border-left-color: #22c55e;
		background: rgba(34, 197, 94, 0.08);
	}

	.log-item.log-error {
		border-left-color: #ef4444;
		background: rgba(239, 68, 68, 0.08);
	}

	.text-success { color: #22c55e; }
	.text-danger { color: #ef4444; }
</style>

