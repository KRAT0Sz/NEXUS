<script lang="ts">
	import { auth } from '#lib/auth.svelte';
	import { theme } from '#lib/theme.svelte';
	import { i18n, t, LOCALES, type LocaleCode } from '#lib/i18n/index.svelte';
	import { goto } from '$app/navigation';
	import {
		Menu,
		X,
		ChevronDown,
		Trophy,
		Video,
		Shield,
		UserPlus,
		LogIn,
		LogOut,
		User,
		Sun,
		Moon,
		Wand2,
		Users,
		Globe,
		Dices
	} from 'lucide-svelte';

	let drawerOpen = $state<boolean>(false);
	let communityMenuOpen = $state<boolean>(false);
	let languageMenuOpen = $state<boolean>(false);

	function toggleDrawer(): void {
		drawerOpen = !drawerOpen;
	}

	function closeDrawer(): void {
		drawerOpen = false;
	}

	function toggleCommunityMenu(): void {
		communityMenuOpen = !communityMenuOpen;
		if (communityMenuOpen) languageMenuOpen = false;
	}

	function closeCommunityMenu(): void {
		communityMenuOpen = false;
	}

	function toggleLanguageMenu(): void {
		languageMenuOpen = !languageMenuOpen;
		if (languageMenuOpen) communityMenuOpen = false;
	}

	function closeLanguageMenu(): void {
		languageMenuOpen = false;
	}

	function selectLocale(code: LocaleCode): void {
		i18n.setLocale(code);
		languageMenuOpen = false;
	}

	function handleLogOut(): void {
		auth.clearToken();
		closeDrawer();
		goto('/');
	}
</script>

<svelte:window onclick={() => {
	if (communityMenuOpen) communityMenuOpen = false;
	if (languageMenuOpen) languageMenuOpen = false;
}} />

<header class="kongor-appbar navbar-root">
	<div class="nav-container">
		<!-- Mobile Hamburger -->
		<button class="icon-btn mobile-menu-btn" onclick={toggleDrawer} aria-label="Toggle navigation menu">
			<Menu size={24} />
		</button>

		<!-- Home / Logo -->
		<a href="/" class="kongor-home-button home-brand">
			<img src="/images/home.png" alt="Project KONGOR" class="logo-glow home-logo" width="40" height="40" />
			<span class="home-text">{t('nav.home')}</span>
		</a>

		<div class="nav-spacer"></div>

		<!-- Desktop Navigation Links -->
		<nav class="desktop-nav" aria-label="Main Navigation">
			<!-- Community Dropdown -->
			<div class="dropdown-wrapper">
				<button class="nav-link dropdown-trigger" onclick={(e) => { e.stopPropagation(); toggleCommunityMenu(); }}>
					<span>{t('nav.community')}</span>
					<ChevronDown size={16} class={communityMenuOpen ? 'chevron-rotated' : ''} />
				</button>

				{#if communityMenuOpen}
					<div class="dropdown-menu glass-card">
						<a href="https://discord.com/invite/N6pKzGDqUH" target="_blank" rel="noreferrer" class="dropdown-item" onclick={closeCommunityMenu}>
							Discord
						</a>
						<a href="https://app.element.io/#/room/#newerth:matrix.org" target="_blank" rel="noreferrer" class="dropdown-item" onclick={closeCommunityMenu}>
							Element
						</a>
						<a href="https://www.reddit.com/r/newerth/" target="_blank" rel="noreferrer" class="dropdown-item" onclick={closeCommunityMenu}>
							Reddit
						</a>
					</div>
				{/if}
			</div>

			<a href="/leaderboard" class="nav-link">{t('nav.leaderboard')}</a>
			<a href="/replays" class="nav-link">{t('nav.replays')}</a>
			<a href="/clans" class="nav-link">{t('nav.clans')}</a>
			<a href="/quests" class="nav-link">{t('quests.title')}</a>
			<a href="/minigames/spin" class="nav-link">{t('minigames.title')}</a>
		</nav>

		<div class="nav-spacer"></div>

		<!-- Auth Buttons -->
		<div class="desktop-auth">
			{#if !auth.isAuthenticated}
				<a href="/email/register" class="btn-outlined">{t('nav.register')}</a>
				<a href="/login" class="btn-outlined">{t('nav.login')}</a>
			{:else}
				{#if auth.isStaff || auth.isGameMaster}
					<a href="/dashboard" class="btn-outlined btn-dashboard">
						<Shield size={15} />
						<span>Dashboard</span>
					</a>
				{/if}
				<a href="/account" class="btn-outlined">{t('nav.account')}</a>
				<button class="btn-outlined" onclick={handleLogOut}>{t('nav.logout')}</button>
			{/if}
		</div>

		<!-- Utility Icons: Language + Cursor Trail + Theme -->
		<div class="util-actions">
			<!-- Language Dropdown -->
			<div class="dropdown-wrapper">
				<button
					class="icon-btn"
					onclick={(e) => { e.stopPropagation(); toggleLanguageMenu(); }}
					title="Change language ({i18n.localeName})"
					aria-label="Change language"
				>
					<Globe size={20} />
				</button>

				{#if languageMenuOpen}
					<div class="dropdown-menu dropdown-menu-right glass-card">
						{#each LOCALES as loc}
							<button
								class="dropdown-item language-item {i18n.currentLocale === loc.code ? 'active-language' : ''}"
								onclick={() => selectLocale(loc.code)}
							>
								<span class="flag-icon">{loc.flag}</span>
								<span class="lang-name">{loc.name}</span>
							</button>
						{/each}
					</div>
				{/if}
			</div>

			<button
				class="icon-btn"
				onclick={() => theme.toggleCursorTrail()}
				title={theme.cursorTrailEnabled ? 'Disable Cursor Trail' : 'Enable Cursor Trail'}
				aria-label="Toggle Cursor Trail"
			>
				<Wand2 size={20} class={theme.cursorTrailEnabled ? 'active-icon' : ''} />
			</button>

			<button
				class="icon-btn"
				onclick={() => theme.toggleTheme()}
				title={theme.isDarkMode ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
				aria-label="Toggle Theme"
			>
				{#if theme.isDarkMode}
					<Sun size={20} />
				{:else}
					<Moon size={20} />
				{/if}
			</button>
		</div>
	</div>
</header>

<!-- Mobile Navigation Drawer -->
{#if drawerOpen}
	<!-- Backdrop -->
	<div class="drawer-backdrop" onclick={closeDrawer} role="presentation"></div>

	<!-- Drawer Panel -->
	<aside class="drawer-panel glass-card" aria-label="Mobile Navigation">
		<div class="drawer-header">
			<a href="/" class="home-brand" onclick={closeDrawer}>
				<img src="/images/home.png" alt="Project KONGOR" class="logo-glow" width="36" height="36" />
				<span class="home-text">Project KONGOR</span>
			</a>
			<button class="icon-btn" onclick={closeDrawer} aria-label="Close menu">
				<X size={22} />
			</button>
		</div>

		<div class="drawer-content">
			<!-- Language Selector in Drawer -->
			<div class="drawer-section-title">Language ({i18n.localeName})</div>
			<div class="drawer-language-grid">
				{#each LOCALES as loc}
					<button
						class="drawer-lang-btn {i18n.currentLocale === loc.code ? 'active-lang' : ''}"
						onclick={() => selectLocale(loc.code)}
					>
						<span>{loc.flag}</span>
						<span>{loc.name}</span>
					</button>
				{/each}
			</div>

			<hr class="drawer-divider" />

			<div class="drawer-section-title">{t('nav.community')}</div>
			<a href="https://discord.com/invite/N6pKzGDqUH" target="_blank" rel="noreferrer" class="drawer-link" onclick={closeDrawer}>
				<Users size={18} /> Discord
			</a>
			<a href="https://app.element.io/#/room/#newerth:matrix.org" target="_blank" rel="noreferrer" class="drawer-link" onclick={closeDrawer}>
				<Users size={18} /> Element
			</a>
			<a href="https://www.reddit.com/r/newerth/" target="_blank" rel="noreferrer" class="drawer-link" onclick={closeDrawer}>
				<Users size={18} /> Reddit
			</a>

			<hr class="drawer-divider" />

			<div class="drawer-section-title">{t('fp.title')}</div>
			<a href="/leaderboard" class="drawer-link" onclick={closeDrawer}>
				<Trophy size={18} /> {t('nav.leaderboard')}
			</a>
			<a href="/replays" class="drawer-link" onclick={closeDrawer}>
				<Video size={18} /> {t('nav.replays')}
			</a>
			<a href="/clans" class="drawer-link" onclick={closeDrawer}>
				<Shield size={18} /> {t('nav.clans')}
			</a>
			<a href="/quests" class="drawer-link" onclick={closeDrawer}>
				<Trophy size={18} /> {t('quests.title')}
			</a>
			<a href="/minigames/spin" class="drawer-link" onclick={closeDrawer}>
				<Dices size={18} /> {t('minigames.title')}
			</a>

			<hr class="drawer-divider" />

			<div class="drawer-section-title">{t('nav.account')}</div>
			{#if !auth.isAuthenticated}
				<a href="/email/register" class="drawer-link" onclick={closeDrawer}>
					<UserPlus size={18} /> {t('nav.register')}
				</a>
				<a href="/login" class="drawer-link" onclick={closeDrawer}>
					<LogIn size={18} /> {t('nav.login')}
				</a>
			{:else}
				{#if auth.isStaff || auth.isGameMaster}
					<a href="/dashboard" class="drawer-link drawer-dashboard-link" onclick={closeDrawer}>
						<Shield size={18} class="accent-icon" /> Dashboard
					</a>
				{/if}
				<a href="/account" class="drawer-link" onclick={closeDrawer}>
					<User size={18} /> {t('nav.account')}
				</a>
				<button class="drawer-link drawer-btn" onclick={handleLogOut}>
					<LogOut size={18} /> {t('nav.logout')}
				</button>
			{/if}

			<hr class="drawer-divider" />

			<button class="drawer-link drawer-btn" onclick={() => { theme.toggleCursorTrail(); }}>
				<Wand2 size={18} /> {theme.cursorTrailEnabled ? 'Disable Cursor Trail' : 'Enable Cursor Trail'}
			</button>
			<button class="drawer-link drawer-btn" onclick={() => { theme.toggleTheme(); }}>
				{#if theme.isDarkMode}
					<Sun size={18} /> Light Mode
				{:else}
					<Moon size={18} /> Dark Mode
				{/if}
			</button>
		</div>
	</aside>
{/if}

<style>
	.navbar-root {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		height: 64px;
		z-index: 1000;
		display: flex;
		align-items: center;
	}

	.nav-container {
		max-width: 1200px;
		width: 100%;
		margin: 0 auto;
		padding: 0 24px;
		display: flex;
		align-items: center;
		gap: 8px;
		height: 100%;
	}

	.nav-spacer {
		flex-grow: 1;
	}

	.home-brand {
		display: flex;
		align-items: center;
		gap: 8px;
		text-decoration: none;
		color: inherit;
		font-weight: 500;
		font-size: 1.1rem;
		cursor: pointer;
	}

	.home-text {
		color: var(--kongor-primary);
		font-weight: 600;
	}

	.desktop-nav {
		display: flex;
		align-items: center;
		gap: 6px;
	}

	.nav-link {
		color: inherit;
		text-decoration: none;
		padding: 8px 12px;
		border-radius: 6px;
		font-size: 0.9375rem;
		font-weight: 500;
		transition: background-color 0.2s ease, color 0.2s ease;
		background: transparent;
		border: none;
		cursor: pointer;
		display: flex;
		align-items: center;
		gap: 4px;
	}

	.nav-link:hover {
		background: rgba(var(--kongor-primary-rgb), 0.1);
		color: var(--kongor-primary);
	}

	.dropdown-wrapper {
		position: relative;
	}

	.dropdown-trigger {
		font-family: inherit;
	}

	:global(.chevron-rotated) {
		transform: rotate(180deg);
		transition: transform 0.2s ease;
	}

	.dropdown-menu {
		position: absolute;
		top: calc(100% + 8px);
		left: 0;
		min-width: 160px;
		display: flex;
		flex-direction: column;
		padding: 8px 0;
		border-radius: 8px;
		box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
		z-index: 100;
	}

	.dropdown-item {
		padding: 10px 18px;
		color: inherit;
		text-decoration: none;
		font-size: 0.9375rem;
		transition: background 0.2s ease, color 0.2s ease;
	}

	.dropdown-item:hover {
		background: rgba(var(--kongor-primary-rgb), 0.15);
		color: var(--kongor-primary);
	}

	.dropdown-menu-right {
		left: auto;
		right: 0;
		min-width: 170px;
	}

	.language-item {
		display: flex;
		align-items: center;
		gap: 10px;
		background: transparent;
		border: none;
		width: 100%;
		text-align: left;
		cursor: pointer;
		font-family: inherit;
	}

	.language-item.active-language {
		color: var(--kongor-primary);
		font-weight: 600;
		background: rgba(var(--kongor-primary-rgb), 0.12);
	}

	.flag-icon {
		font-size: 1.15rem;
		line-height: 1;
	}

	.drawer-language-grid {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 8px;
		margin-bottom: 12px;
	}

	.drawer-lang-btn {
		display: flex;
		align-items: center;
		gap: 8px;
		padding: 8px 12px;
		background: rgba(255, 255, 255, 0.05);
		border: 1px solid var(--kongor-border);
		border-radius: 6px;
		color: inherit;
		font-family: inherit;
		font-size: 0.875rem;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.drawer-lang-btn:hover,
	.drawer-lang-btn.active-lang {
		background: rgba(var(--kongor-primary-rgb), 0.15);
		border-color: var(--kongor-primary);
		color: var(--kongor-primary);
	}

	.desktop-auth {
		display: flex;
		align-items: center;
		gap: 8px;
	}

	.btn-dashboard {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		border-color: rgba(var(--kongor-primary-rgb), 0.4);
		color: var(--kongor-primary);
	}

	.btn-outlined {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 6px 16px;
		border-radius: 6px;
		border: 1px solid var(--kongor-primary);
		color: var(--kongor-primary);
		background: transparent;
		font-family: inherit;
		font-size: 0.875rem;
		font-weight: 500;
		text-decoration: none;
		cursor: pointer;
		transition: background-color 0.2s ease, transform 0.15s ease;
	}

	.btn-outlined:hover {
		background: rgba(var(--kongor-primary-rgb), 0.12);
		transform: translateY(-1px);
	}

	.util-actions {
		display: flex;
		align-items: center;
		gap: 6px;
	}

	.icon-btn {
		background: transparent;
		border: none;
		color: inherit;
		cursor: pointer;
		padding: 8px;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: transform 0.2s ease, color 0.2s ease;
	}

	.icon-btn:hover {
		transform: scale(1.15);
		color: var(--kongor-primary);
	}

	:global(.active-icon) {
		color: var(--kongor-primary);
	}

	.mobile-menu-btn {
		display: none;
	}

	/* Drawer Styles */
	.drawer-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.65);
		backdrop-filter: blur(4px);
		z-index: 1050;
	}

	.drawer-panel {
		position: fixed;
		top: 0;
		left: 0;
		bottom: 0;
		width: 280px;
		z-index: 1060;
		display: flex;
		flex-direction: column;
		padding: 16px;
		overflow-y: auto;
		animation: slide-in 0.25s ease-out;
	}

	@keyframes slide-in {
		from { transform: translateX(-100%); }
		to { transform: translateX(0); }
	}

	.drawer-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding-bottom: 12px;
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.15);
		margin-bottom: 12px;
	}

	.drawer-content {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.drawer-section-title {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		color: var(--kongor-secondary);
		padding: 8px 12px 4px;
		font-weight: 600;
	}

	.drawer-link {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 10px 14px;
		border-radius: 6px;
		color: inherit;
		text-decoration: none;
		font-size: 0.9375rem;
		font-family: inherit;
		transition: background 0.2s ease, color 0.2s ease;
	}

	.drawer-btn {
		background: transparent;
		border: none;
		width: 100%;
		text-align: left;
		cursor: pointer;
	}

	.drawer-link:hover {
		background: rgba(var(--kongor-primary-rgb), 0.12);
		color: var(--kongor-primary);
	}

	.drawer-divider {
		border: none;
		height: 1px;
		background: rgba(var(--kongor-primary-rgb), 0.15);
		margin: 8px 0;
	}

	@media (max-width: 960px) {
		.desktop-nav,
		.desktop-auth {
			display: none;
		}

		.mobile-menu-btn {
			display: flex;
		}
	}
</style>
