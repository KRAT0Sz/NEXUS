<script lang="ts">
	import { auth } from '#lib/auth.svelte';
	import { t } from '#lib/i18n/index.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import {
		ShieldAlert,
		History,
		Coins,
		Search,
		Gavel,
		Sparkles,
		Edit3,
		Megaphone,
		ArrowRight
	} from 'lucide-svelte';

	interface QuickAction {
		title: string;
		desc: string;
		href: string;
		icon: typeof ShieldAlert;
		visible: boolean;
		badge?: string;
	}

	const quickActions = $derived<QuickAction[]>([
		{
			title: t('dashboard.gm.suspension.title'),
			desc: 'Issue new player suspensions, check penalty history, and enforce Code of Conduct.',
			href: '/dashboard/gm/suspend-a-player',
			icon: ShieldAlert,
			visible: auth.isGameMaster,
			badge: 'GM'
		},
		{
			title: t('dashboard.staff.suspensions.title'),
			desc: 'Override active suspensions or adjust remaining ban durations.',
			href: '/dashboard/staff/suspensions',
			icon: Gavel,
			visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges,
			badge: 'SGM / Staff'
		},
		{
			title: t('dashboard.staff.currency.title'),
			desc: 'Distribute Gold Coins, Silver Coins, and Plinko Tickets in batch to users.',
			href: '/dashboard/staff/currency',
			icon: Coins,
			visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges,
			badge: 'Staff'
		},
		{
			title: t('dashboard.staff.lookUpAccountOrUser.title'),
			desc: 'Search accounts, find connected sub-accounts, and view associated identities.',
			href: '/dashboard/staff/look-up-account-or-user',
			icon: Search,
			visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges,
			badge: 'Staff'
		},
		{
			title: t('dashboard.staff.actions.title'),
			desc: 'Audit log of moderation, promotions, suspensions, and staff actions.',
			href: '/dashboard/staff/actions',
			icon: History,
			visible: auth.isSeniorGameMaster || auth.hasElevatedPrivileges,
			badge: 'Staff'
		},
		{
			title: t('dashboard.admin.upgrades.title'),
			desc: 'Grant or revoke store upgrades, avatars, announcers, and account cosmetics.',
			href: '/dashboard/admin/upgrades',
			icon: Sparkles,
			visible: auth.isAdministrator || auth.isCustodian,
			badge: 'Admin'
		},
		{
			title: t('dashboard.admin.renameAccount.title'),
			desc: 'Administratively rename an account without requiring a name reset token.',
			href: '/dashboard/admin/rename',
			icon: Edit3,
			visible: auth.isAdministrator || auth.isCustodian,
			badge: 'Admin'
		},
		{
			title: 'Global Broadcast',
			desc: 'Send high-priority system announcements directly to all players’ in-game inboxes.',
			href: '/dashboard/admin/broadcast',
			icon: Megaphone,
			visible: auth.isAdministrator || auth.isCustodian,
			badge: 'Admin'
		}
	]);
</script>

<DashboardContainer
	title={t('dashboard.title')}
	description={t('dashboard.welcome', { name: auth.userName || 'Staff Member' })}
>
	<div class="welcome-banner glass-card-inner">
		<div class="banner-text">
			<h2 class="banner-title">Management & Moderation Control Hub</h2>
			<p class="banner-sub">
				Welcome to Project KONGOR’s unified staff and administration center. Access moderation tools, suspension systems, currency distribution, and server controls below.
			</p>
		</div>
	</div>

	<div class="quick-actions-section">
		<h3 class="section-title">Quick Actions</h3>
		<div class="actions-grid">
			{#each quickActions as action}
				{#if action.visible}
					{@const Icon = action.icon}
					<a href={action.href} class="action-card glass-card-inner">
						<div class="card-top">
							<div class="icon-bubble">
								<Icon size={22} class="accent-icon" />
							</div>
							{#if action.badge}
								<span class="role-pill">{action.badge}</span>
							{/if}
						</div>
						<div class="card-body">
							<h4 class="card-title">{action.title}</h4>
							<p class="card-desc">{action.desc}</p>
						</div>
						<div class="card-footer">
							<span class="open-link">Open tool <ArrowRight size={14} /></span>
						</div>
					</a>
				{/if}
			{/each}
		</div>
	</div>
</DashboardContainer>

<style>
	.welcome-banner {
		padding: 24px;
		border-radius: 10px;
		margin-bottom: 28px;
		background: linear-gradient(135deg, rgba(var(--kongor-primary-rgb), 0.12), rgba(var(--kongor-surface-rgb), 0.5)) !important;
		border-left: 4px solid var(--kongor-primary) !important;
	}

	.banner-title {
		margin: 0 0 8px 0;
		font-size: 1.25rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.banner-sub {
		margin: 0;
		font-size: 0.92rem;
		color: var(--kongor-text-muted);
		line-height: 1.5;
	}

	.section-title {
		margin: 0 0 16px 0;
		font-size: 1.05rem;
		font-weight: 600;
		text-transform: uppercase;
		letter-spacing: 0.5px;
		color: var(--kongor-text-heading);
	}

	.actions-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
		gap: 18px;
	}

	.action-card {
		padding: 20px;
		border-radius: 10px;
		text-decoration: none;
		color: inherit;
		display: flex;
		flex-direction: column;
		justify-content: space-between;
		transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
	}

	.action-card:hover {
		transform: translateY(-3px);
		border-color: rgba(var(--kongor-primary-rgb), 0.4) !important;
		box-shadow: 0 8px 24px rgba(var(--kongor-primary-rgb), 0.15);
	}

	.card-top {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 14px;
	}

	.icon-bubble {
		width: 42px;
		height: 42px;
		border-radius: 8px;
		background: rgba(var(--kongor-primary-rgb), 0.12);
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.role-pill {
		font-size: 0.7rem;
		font-weight: 700;
		text-transform: uppercase;
		padding: 3px 8px;
		border-radius: 4px;
		background: rgba(var(--kongor-primary-rgb), 0.15);
		color: var(--kongor-primary);
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
	}

	.card-title {
		margin: 0 0 6px 0;
		font-size: 1.05rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.card-desc {
		margin: 0;
		font-size: 0.84rem;
		color: var(--kongor-text-muted);
		line-height: 1.45;
	}

	.card-footer {
		margin-top: 16px;
		padding-top: 12px;
		border-top: 1px solid rgba(var(--kongor-primary-rgb), 0.08);
	}

	.open-link {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 0.82rem;
		font-weight: 600;
		color: var(--kongor-primary);
		transition: gap 0.15s ease;
	}

	.action-card:hover .open-link {
		gap: 10px;
	}
</style>
