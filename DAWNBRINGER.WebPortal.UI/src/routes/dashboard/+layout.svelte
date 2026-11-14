<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { auth } from '#lib/auth.svelte';
	import { t } from '#lib/i18n/index.svelte';
	import type { Snippet } from 'svelte';
	import {
		LayoutDashboard,
		ShieldAlert,
		History,
		Gavel,
		Coins,
		Search,
		Lock,
		Server,
		Award,
		Sparkles,
		Edit3,
		Megaphone,
		Settings,
		Wrench,
		ChevronRight,
		User
	} from 'lucide-svelte';

	let { children }: { children?: Snippet } = $props();

	onMount(() => {
		if (!auth.isAuthenticated) {
			goto('/login');
		}
	});

	interface NavItem {
		href: string;
		label: string;
		icon: typeof LayoutDashboard;
		visible: boolean;
	}

	interface NavGroup {
		title: string;
		items: NavItem[];
		visible: boolean;
	}

	const navGroups = $derived<NavGroup[]>([
		{
			title: t('dashboard.title'),
			visible: true,
			items: [
				{
					href: '/dashboard',
					label: t('dashboard.title'),
					icon: LayoutDashboard,
					visible: true
				},
				{
					href: '/account',
					label: t('dashboard.mySettingsBtn'),
					icon: Settings,
					visible: true
				},
				{
					href: '/account/tools',
					label: 'Tools & Host Tokens',
					icon: Wrench,
					visible: true
				}
			]
		},
		{
			title: t('dashboard.menuTitles.gmTools'),
			visible: auth.isGameMaster,
			items: [
				{
					href: '/dashboard/gm/suspend-a-player',
					label: t('dashboard.gm.suspension.title'),
					icon: ShieldAlert,
					visible: auth.isGameMaster
				}
			]
		},
		{
			title: t('dashboard.menuTitles.sgmTools'),
			visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges,
			items: [
				{
					href: '/dashboard/staff/actions',
					label: t('dashboard.staff.actions.title'),
					icon: History,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/suspensions',
					label: t('dashboard.staff.suspensions.title'),
					icon: Gavel,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/currency',
					label: t('dashboard.staff.currency.title'),
					icon: Coins,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/look-up-account-or-user',
					label: t('dashboard.staff.lookUpAccountOrUser.title'),
					icon: Search,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/enable-disable',
					label: t('dashboard.staff.enableDisable.title'),
					icon: Lock,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/promote-to-tmm-host',
					label: t('dashboard.staff.promoteToTMMHost.title'),
					icon: Server,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				},
				{
					href: '/dashboard/staff/promotions',
					label: t('dashboard.staff.promotions.title'),
					icon: Award,
					visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges
				}
			]
		},
		{
			title: t('dashboard.menuTitles.adminTools'),
			visible: auth.isAdministrator || auth.isCustodian,
			items: [
				{
					href: '/dashboard/admin/upgrades',
					label: t('dashboard.admin.upgrades.title'),
					icon: Sparkles,
					visible: auth.isAdministrator || auth.isCustodian
				},
				{
					href: '/dashboard/admin/rename',
					label: t('dashboard.admin.renameAccount.title'),
					icon: Edit3,
					visible: auth.isAdministrator || auth.isCustodian
				},
				{
					href: '/dashboard/admin/broadcast',
					label: 'Global Broadcast',
					icon: Megaphone,
					visible: auth.isAdministrator || auth.isCustodian
				}
			]
		}
	]);

	function isCurrent(href: string): boolean {
		return page.url.pathname === href;
	}
</script>

<div class="dashboard-shell container">
	<div class="dashboard-grid">
		<!-- Sidebar -->
		<aside class="dashboard-sidebar">
			<!-- User Summary Card -->
			<div class="user-profile-card glass-card">
				<div class="user-avatar-wrap">
					<User size={28} class="accent-icon" />
				</div>
				<div class="user-info-wrap">
					<div class="user-display-name">{auth.userName || 'Staff Member'}</div>
					<div class="user-role-badge">
						{auth.userRole || 'USER'}
					</div>
				</div>
			</div>

			<!-- Navigation Links -->
			<nav class="sidebar-nav glass-card" aria-label="Dashboard navigation">
				{#each navGroups as group}
					{#if group.visible && group.items.some(i => i.visible)}
						<div class="nav-group-section">
							<div class="nav-group-title">{group.title}</div>
							<div class="nav-items-list">
								{#each group.items as item}
									{#if item.visible}
										{@const Icon = item.icon}
										<a
											href={item.href}
											class="sidebar-link"
											class:active={isCurrent(item.href)}
										>
											<Icon size={18} class="sidebar-icon" />
											<span class="link-label">{item.label}</span>
											{#if isCurrent(item.href)}
												<ChevronRight size={16} class="active-chevron" />
											{/if}
										</a>
									{/if}
								{/each}
							</div>
						</div>
					{/if}
				{/each}
			</nav>
		</aside>

		<!-- Main Workspace Area -->
		<section class="dashboard-workspace">
			{@render children?.()}
		</section>
	</div>
</div>

<style>
	.dashboard-shell {
		max-width: 1400px;
		margin: 0 auto;
		padding: 24px 20px 48px 20px;
	}

	.dashboard-grid {
		display: grid;
		grid-template-columns: 290px 1fr;
		gap: 28px;
		align-items: start;
	}

	@media (max-width: 960px) {
		.dashboard-grid {
			grid-template-columns: 1fr;
		}
	}

	/* Sidebar Styles */
	.dashboard-sidebar {
		display: flex;
		flex-direction: column;
		gap: 20px;
		position: sticky;
		top: 100px;
	}

	.user-profile-card {
		display: flex;
		align-items: center;
		gap: 16px;
		padding: 16px 20px;
		border-radius: 12px;
	}

	.user-avatar-wrap {
		width: 48px;
		height: 48px;
		border-radius: 50%;
		background: rgba(var(--kongor-primary-rgb), 0.15);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.3);
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	.user-info-wrap {
		min-width: 0;
		flex: 1;
	}

	.user-display-name {
		font-weight: 600;
		font-size: 1rem;
		color: var(--kongor-text-heading);
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	.user-role-badge {
		display: inline-block;
		margin-top: 4px;
		font-size: 0.72rem;
		text-transform: uppercase;
		letter-spacing: 0.75px;
		font-weight: 700;
		color: var(--kongor-primary);
		background: rgba(var(--kongor-primary-rgb), 0.12);
		padding: 2px 8px;
		border-radius: 4px;
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
	}

	.sidebar-nav {
		border-radius: 12px;
		padding: 16px 12px;
		display: flex;
		flex-direction: column;
		gap: 18px;
	}

	.nav-group-section {
		display: flex;
		flex-direction: column;
		gap: 6px;
	}

	.nav-group-title {
		font-size: 0.72rem;
		text-transform: uppercase;
		letter-spacing: 1px;
		color: var(--kongor-text-muted);
		padding: 0 12px;
		font-weight: 700;
	}

	.nav-items-list {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.sidebar-link {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 9px 12px;
		border-radius: 8px;
		color: var(--kongor-text-body);
		text-decoration: none;
		font-size: 0.88rem;
		font-weight: 500;
		transition: background 0.15s ease, color 0.15s ease, transform 0.15s ease;
	}

	.sidebar-link:hover {
		background: rgba(var(--kongor-primary-rgb), 0.1);
		color: var(--kongor-primary);
		transform: translateX(3px);
	}

	.sidebar-link.active {
		background: linear-gradient(90deg, rgba(var(--kongor-primary-rgb), 0.2), rgba(var(--kongor-primary-rgb), 0.05));
		border-left: 3px solid var(--kongor-primary);
		color: var(--kongor-primary);
		font-weight: 600;
	}

	:global(.sidebar-icon) {
		flex-shrink: 0;
	}

	.link-label {
		flex: 1;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	:global(.active-chevron) {
		color: var(--kongor-primary);
		margin-left: auto;
	}

	/* Workspace Area */
	.dashboard-workspace {
		min-width: 0;
	}
</style>
