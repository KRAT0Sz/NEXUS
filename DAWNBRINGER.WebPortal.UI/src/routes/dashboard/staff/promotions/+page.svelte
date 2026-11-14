<script lang="ts">
	import { onMount } from 'svelte';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import { portalStaffStore } from '#lib/api/staff.svelte';
	import type { StaffRosterMemberDTO } from '#lib/types';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { Award, UserCheck, UserMinus, Shield, CheckCircle, RefreshCw } from 'lucide-svelte';

	let userName = $state<string>('');
	let targetRole = $state<string>('CUSTODIAN');
	let loading = $state<boolean>(true);
	let processing = $state<boolean>(false);
	let roster = $state<StaffRosterMemberDTO[]>([]);

	let successMessage = $state<string>('');

	const availableRoles = [
		{ label: 'Custodian / Staff (Elevated Privileges)', value: 'CUSTODIAN' },
		{ label: 'Administrator', value: 'ADMINISTRATOR' }
	];

	async function loadRoster(): Promise<void> {
		loading = true;
		try {
			roster = await portalStaffStore.getRoster();
		} finally {
			loading = false;
		}
	}

	onMount(() => {
		loadRoster();
	});

	async function handlePromote(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		if (!userName.trim()) {
			toast.warning('Please enter a user email address or user name.');
			return;
		}

		processing = true;
		successMessage = '';

		try {
			const res = await portalStaffStore.promoteUserRole(userName.trim(), targetRole);
			if (res.ok) {
				successMessage = t('dashboard.staff.promotions.promoteSuccess', {
					userName: userName.trim()
				});
				toast.success(`Promoted ${userName.trim()} to ${targetRole}.`);
				userName = '';
				await loadRoster();
			} else {
				toast.error(res.error || 'Failed to promote user.');
			}
		} finally {
			processing = false;
		}
	}

	async function handleDemote(email: string): Promise<void> {
		if (!confirm(`Are you sure you want to demote ${email} back to standard USER role?`)) {
			return;
		}

		processing = true;
		try {
			const res = await portalStaffStore.promoteUserRole(email, 'USER');
			if (res.ok) {
				toast.success(`Demoted ${email} to USER.`);
				await loadRoster();
			} else {
				toast.error(res.error || 'Failed to demote user.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<DashboardContainer
	title={t('dashboard.staff.promotions.title')}
	description="Promote qualified community members to staff roles or demote inactive staff."
	{loading}
>
	{#if successMessage}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{successMessage}</div>
		</div>
	{/if}

	<!-- Promote User Form -->
	<div class="glass-card-inner form-card mb-4">
		<h3 class="card-subtitle">{t('dashboard.staff.promotions.promoteToGameMaster')}</h3>
		<form class="promote-form" onsubmit={handlePromote}>
			<div class="form-row-2">
				<div class="form-group">
					<label for="user-input" class="field-label">{t('dashboard.staff.promotions.userName')}</label>
					<input
						id="user-input"
						type="text"
						bind:value={userName}
						placeholder="user@example.com"
						class="kongor-field"
						required
					/>
				</div>
				<div class="form-group">
					<label for="role-select" class="field-label">Target Staff Role</label>
					<select
						id="role-select"
						bind:value={targetRole}
						class="kongor-field kongor-select"
					>
						{#each availableRoles as r}
							<option value={r.value}>{r.label}</option>
						{/each}
					</select>
				</div>
			</div>

			<button
				type="submit"
				class="btn-filled"
				disabled={processing || !userName.trim()}
			>
				<UserCheck size={18} />
				{t('dashboard.staff.promotions.promote')}
			</button>
		</form>
	</div>

	<!-- Staff Roster Table -->
	<div class="glass-card-inner roster-card">
		<div class="roster-top">
			<h3 class="card-subtitle">Active Staff Roster</h3>
			<button type="button" class="btn-outlined btn-sm" onclick={loadRoster} disabled={loading}>
				<RefreshCw size={14} class={loading ? 'spin-icon' : ''} />
				Refresh Roster
			</button>
		</div>

		{#if roster.length === 0}
			<p class="empty-text">No elevated staff members found.</p>
		{:else}
			<div class="table-responsive">
				<table class="kongor-table">
					<thead>
						<tr>
							<th>Account / User</th>
							<th>Email Address</th>
							<th>Assigned Role</th>
							<th>Action</th>
						</tr>
					</thead>
					<tbody>
						{#each roster as member (member.id)}
							<tr>
								<td class="name-cell">
									<strong>{member.name}</strong>
								</td>
								<td>{member.email}</td>
								<td>
									<span class="role-badge" class:badge-admin={member.role === 'ADMINISTRATOR'}>
										{member.role}
									</span>
								</td>
								<td>
									<button
										type="button"
										class="btn-demote"
										onclick={() => handleDemote(member.email)}
										disabled={processing}
										title="Demote to USER"
									>
										<UserMinus size={15} />
										Demote
									</button>
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
	.form-card,
	.roster-card {
		padding: 24px;
		border-radius: 10px;
	}

	.card-subtitle {
		margin: 0 0 16px 0;
		font-size: 1.1rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.roster-top {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 16px;
	}

	.form-row-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
		margin-bottom: 18px;
	}

	@media (max-width: 640px) {
		.form-row-2 {
			grid-template-columns: 1fr;
		}
	}

	.form-group {
		display: flex;
		flex-direction: column;
	}

	.field-label {
		margin-bottom: 6px;
		font-size: 0.84rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.kongor-select {
		cursor: pointer;
	}

	.table-responsive {
		overflow-x: auto;
	}

	.kongor-table {
		width: 100%;
		border-collapse: collapse;
		font-size: 0.88rem;
		text-align: left;
	}

	.kongor-table th {
		padding: 12px 16px;
		font-weight: 700;
		text-transform: uppercase;
		font-size: 0.76rem;
		letter-spacing: 0.75px;
		color: var(--kongor-text-muted);
		background: rgba(var(--kongor-surface-rgb), 0.5);
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.15);
	}

	.kongor-table td {
		padding: 12px 16px;
		border-bottom: 1px solid rgba(255, 255, 255, 0.05);
	}

	.role-badge {
		font-size: 0.72rem;
		font-weight: 700;
		padding: 3px 8px;
		border-radius: 4px;
		background: rgba(var(--kongor-primary-rgb), 0.15);
		color: var(--kongor-primary);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.3);
	}

	.role-badge.badge-admin {
		background: rgba(245, 158, 11, 0.15);
		color: #f59e0b;
		border-color: rgba(245, 158, 11, 0.3);
	}

	.btn-demote {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 5px 12px;
		font-size: 0.8rem;
		border-radius: 4px;
		background: rgba(239, 68, 68, 0.12);
		border: 1px solid rgba(239, 68, 68, 0.3);
		color: #f87171;
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.btn-demote:hover {
		background: rgba(239, 68, 68, 0.22);
	}

	.btn-sm {
		padding: 6px 14px;
		font-size: 0.82rem;
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.empty-text {
		color: var(--kongor-text-muted);
		font-size: 0.9rem;
		margin: 16px 0;
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

