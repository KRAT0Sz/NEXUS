<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { ArrowDown } from 'lucide-svelte';
	import { apiFetch } from '#lib/api';
	import { t } from '#lib/i18n/index.svelte';

	interface ServerStatsResponse {
		totalPlayers?: number;
		totalAccounts?: number;
		totalUsers?: number;
		totalClans?: number;
		onlinePlayers?: number;
		matchesInProgress?: number;
		TotalPlayers?: number;
		OnlinePlayers?: number;
		MatchesInProgress?: number;
	}

	interface ServerStats {
		TotalPlayers: string;
		OnlinePlayers: string;
		MatchesInProgress: string;
	}

	let stats = $state<ServerStats>({
		TotalPlayers: '—',
		OnlinePlayers: '—',
		MatchesInProgress: '—'
	});

	let refreshTimer: ReturnType<typeof setInterval> | null = null;

	async function updateStats(): Promise<void> {
		try {
			const res = await apiFetch<ServerStatsResponse>('/Statistics/Portal');
			if (res.ok && res.data) {
				const accounts = res.data.totalAccounts ?? res.data.totalPlayers ?? res.data.TotalPlayers ?? 0;
				const online = res.data.onlinePlayers ?? res.data.OnlinePlayers ?? 0;
				const matches = res.data.matchesInProgress ?? res.data.MatchesInProgress ?? 0;

				stats.TotalPlayers = Number(accounts).toLocaleString();
				stats.OnlinePlayers = Number(online).toLocaleString();
				stats.MatchesInProgress = Number(matches).toLocaleString();
			}
		} catch {
			if (stats.TotalPlayers === '—') stats.TotalPlayers = '0';
			if (stats.OnlinePlayers === '—') stats.OnlinePlayers = '0';
			if (stats.MatchesInProgress === '—') stats.MatchesInProgress = '0';
		}
	}

	onMount(() => {
		void updateStats();
		refreshTimer = setInterval(() => {
			void updateStats();
		}, 15000);
	});

	onDestroy(() => {
		if (refreshTimer) clearInterval(refreshTimer);
	});

	let timeLine = $derived([
		{
			date: t('fp.timeline.e.date'),
			title: t('fp.timeline.e.title'),
			text: t('fp.timeline.e.text')
		},
		{
			date: t('fp.timeline.d.date'),
			title: t('fp.timeline.d.title'),
			text: t('fp.timeline.d.text')
		},
		{
			date: t('fp.timeline.c.date'),
			title: t('fp.timeline.c.title'),
			text: t('fp.timeline.c.text')
		},
		{
			date: t('fp.timeline.b.date'),
			title: t('fp.timeline.b.title'),
			text: t('fp.timeline.b.text')
		},
		{
			date: t('fp.timeline.a.date'),
			title: t('fp.timeline.a.title'),
			text: t('fp.timeline.a.text')
		}
	]);

	function scrollToTimeline(): void {
		const el = document.getElementById('timeline');
		if (el) {
			el.scrollIntoView({ behavior: 'smooth' });
		}
	}
</script>

<svelte:head>
	<title>{t('fp.title')} - Project KONGOR</title>
</svelte:head>

<div class="kongor-frontpage">
	<!-- Parallax Hero Section (caldavar_dark.jpg) -->
	<section class="hero-parallax">
		<div class="parallax-overlay"></div>

		<div class="hero-content">
			<!-- Title & CTA -->
			<div class="hero-header animate-fade-in">
				<h1 class="ff-scurlock ff-4x hero-brand">Project KONGOR</h1>
				<a href="/email/register" class="btn-register-hero">
					{t('fp.register')}
				</a>
			</div>

			<!-- Stats Row -->
			<div class="stats-card custom-bg animate-fade-in-delay-1">
				<div class="stats-grid">
					<div class="stat-box">
						<div class="stat-value">{stats.TotalPlayers}</div>
						<div class="stat-title">{t('fp.registeredAccounts')}</div>
					</div>
					<div class="stat-box">
						<div class="stat-value">{stats.OnlinePlayers}</div>
						<div class="stat-title">{t('fp.playersOnline')}</div>
					</div>
					<div class="stat-box">
						<div class="stat-value">{stats.MatchesInProgress}</div>
						<div class="stat-title">{t('fp.matchesInProgress')}</div>
					</div>
				</div>
			</div>

			<!-- Scroll to Timeline CTA -->
			<div class="timeline-cta animate-fade-in-delay-2">
				<button type="button" class="btn-timeline-scroll" onclick={scrollToTimeline}>
					<span>{t('fp.timeline.title')}</span>
					<ArrowDown size={20} class="bounce-icon" />
				</button>
			</div>
		</div>
	</section>

	<!-- Timeline Section -->
	<section id="timeline" class="timeline-section">
		<div class="timeline-container">
			<h2 class="timeline-main-title">{t('fp.timeline.title')}</h2>

			<div class="timeline-tree">
				{#each timeLine as item, index}
					<div class="timeline-row {index % 2 === 0 ? 'row-left' : 'row-right'}">
						<!-- Opposite Date (Desktop) -->
						<div class="timeline-opposite">
							<span class="opposite-date">{item.date}</span>
						</div>

						<!-- Center Node Dot -->
						<div class="timeline-node">
							<div class="node-circle"></div>
						</div>

						<!-- Timeline Content Card -->
						<div class="timeline-card-wrapper">
							<div class="timeline-card glass-card elevation-2">
								<div class="mobile-date">{item.date}</div>
								<h3 class="card-milestone-title">{item.title}</h3>
								<p class="card-milestone-text">{item.text}</p>
							</div>
						</div>
					</div>
				{/each}
			</div>
		</div>
	</section>
</div>

<style>
	.kongor-frontpage {
		width: 100%;
		display: flex;
		flex-direction: column;
		margin-top: -84px; /* Pull up behind navbar for seamless parallax */
	}

	/* Hero Parallax */
	.hero-parallax {
		position: relative;
		width: 100%;
		min-height: 100vh;
		background: url('/images/caldavar_dark.jpg') center top / cover no-repeat fixed;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 120px 24px 60px;
		overflow: hidden;
	}

	.parallax-overlay {
		position: absolute;
		inset: 0;
		background: rgba(10, 15, 12, 0.45);
		pointer-events: none;
	}

	.hero-content {
		position: relative;
		z-index: 1;
		width: 100%;
		max-width: 1100px;
		margin: 0 auto;
		display: flex;
		flex-direction: column;
		align-items: center;
		text-align: center;
	}

	.hero-header {
		margin-bottom: 50px;
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	.ff-scurlock {
		font-family: 'Scurlock', sans-serif !important;
		text-transform: lowercase;
	}

	.ff-4x {
		font-size: 4.5rem;
		line-height: 1.1;
		margin: 0 0 24px;
		letter-spacing: 2px;
		color: #ffffff;
		transition: text-shadow 0.25s ease-in-out;
		cursor: default;
	}

	.ff-4x:hover {
		text-shadow: 0 0 15px #00E676, 0 0 35px #00E676;
	}

	/* Register Button (matching Vuetify tile primary button) */
	.btn-register-hero {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 225px;
		height: 65px;
		background: #d32f2f;
		color: #ffffff;
		font-family: 'Ubuntu', sans-serif;
		font-size: 1.25rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 1px;
		text-decoration: none;
		border-radius: 4px;
		box-shadow: 0 4px 14px rgba(0, 0, 0, 0.5);
		transition: all 0.25s ease;
		border: 1px solid rgba(255, 255, 255, 0.15);
	}

	.btn-register-hero:hover {
		background: #b71c1c;
		transform: translateY(-2px);
		box-shadow: 0 6px 20px rgba(211, 47, 47, 0.45);
	}

	/* Stats Card (custom-bg: #0c00006e) */
	.stats-card {
		width: 100%;
		max-width: 960px;
		background-color: rgba(12, 0, 0, 0.55);
		backdrop-filter: blur(10px);
		-webkit-backdrop-filter: blur(10px);
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 8px;
		padding: 28px 24px;
		margin-bottom: 50px;
		box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
	}

	.stats-grid {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 24px;
		align-items: center;
	}

	.stat-box {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.stat-value {
		font-size: 2.85rem;
		font-weight: 900;
		color: #ffffff;
		line-height: 1.1;
		margin-bottom: 8px;
		letter-spacing: -0.5px;
		text-shadow: 0 2px 8px rgba(0, 0, 0, 0.8);
	}

	.stat-title {
		font-size: 1.1rem;
		font-weight: 400;
		color: rgba(255, 255, 255, 0.85);
		text-transform: uppercase;
		letter-spacing: 0.5px;
	}

	/* Scroll to timeline button */
	.timeline-cta {
		display: flex;
		justify-content: center;
		margin-top: 10px;
	}

	.btn-timeline-scroll {
		background: transparent;
		border: none;
		color: #ffffff;
		font-family: inherit;
		font-size: 1.15rem;
		font-weight: 500;
		display: inline-flex;
		align-items: center;
		gap: 10px;
		cursor: pointer;
		padding: 14px 28px;
		border-radius: 4px;
		transition: all 0.25s ease;
		text-shadow: 0 2px 6px rgba(0, 0, 0, 0.8);
	}

	.btn-timeline-scroll:hover {
		background: rgba(255, 255, 255, 0.08);
		color: var(--kongor-primary);
	}

	:global(.bounce-icon) {
		animation: bounce 2s infinite ease-in-out;
	}

	@keyframes bounce {
		0%, 20%, 50%, 80%, 100% {
			transform: translateY(0);
		}
		40% {
			transform: translateY(6px);
		}
		60% {
			transform: translateY(3px);
		}
	}

	/* Timeline Section */
	.timeline-section {
		width: 100%;
		padding: 60px 24px 80px;
	}

	.timeline-container {
		max-width: 1000px;
		margin: 0 auto;
	}

	.timeline-main-title {
		text-align: center;
		font-size: 2.75rem;
		font-weight: 300;
		letter-spacing: 1px;
		margin: 0 0 50px;
		color: inherit;
	}

	/* Timeline Tree Structure */
	.timeline-tree {
		position: relative;
		width: 100%;
		padding: 20px 0;
	}

	/* Center vertical line */
	.timeline-tree::before {
		content: '';
		position: absolute;
		top: 0;
		bottom: 0;
		left: 50%;
		width: 2px;
		background: rgba(255, 255, 255, 0.2);
		transform: translateX(-50%);
	}

	.timeline-row {
		display: flex;
		align-items: flex-start;
		position: relative;
		margin-bottom: 40px;
	}

	.timeline-row:last-child {
		margin-bottom: 0;
	}

	.timeline-opposite {
		width: 50%;
		padding: 14px 36px;
		box-sizing: border-box;
	}

	.opposite-date {
		font-size: 1rem;
		font-weight: 500;
		color: var(--kongor-secondary);
	}

	.timeline-node {
		position: absolute;
		left: 50%;
		top: 18px;
		transform: translate(-50%, -50%);
		z-index: 2;
	}

	.node-circle {
		width: 16px;
		height: 16px;
		border-radius: 50%;
		background: #d32f2f;
		border: 3px solid rgba(255, 255, 255, 0.85);
		box-shadow: 0 0 10px rgba(211, 47, 47, 0.7);
	}

	.timeline-card-wrapper {
		width: 50%;
		padding: 0 36px;
		box-sizing: border-box;
	}

	/* Alternating sides */
	.row-left .timeline-opposite {
		order: 2;
		text-align: left;
	}

	.row-left .timeline-card-wrapper {
		order: 1;
		text-align: right;
	}

	.row-right .timeline-opposite {
		order: 1;
		text-align: right;
	}

	.row-right .timeline-card-wrapper {
		order: 2;
		text-align: left;
	}

	/* Card styling */
	.timeline-card {
		background: rgba(20, 26, 22, 0.75);
		backdrop-filter: blur(8px);
		-webkit-backdrop-filter: blur(8px);
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 8px;
		padding: 24px;
		box-shadow: 0 4px 18px rgba(0, 0, 0, 0.35);
		text-align: left;
		transition: transform 0.2s ease, box-shadow 0.2s ease;
	}

	.timeline-card:hover {
		transform: translateY(-3px);
		box-shadow: 0 8px 26px rgba(0, 0, 0, 0.5);
		border-color: rgba(255, 255, 255, 0.2);
	}

	.mobile-date {
		display: none;
		font-size: 0.875rem;
		color: var(--kongor-secondary);
		margin-bottom: 6px;
		font-weight: 500;
	}

	.card-milestone-title {
		margin: 0 0 10px;
		font-size: 1.35rem;
		font-weight: 600;
		color: #ffffff;
	}

	.card-milestone-text {
		margin: 0;
		font-size: 0.95rem;
		line-height: 1.6;
		color: rgba(255, 255, 255, 0.85);
	}

	/* Responsive design */
	@media (max-width: 960px) {
		.hero-parallax {
			min-height: auto;
			padding-top: 120px;
			padding-bottom: 60px;
			background-attachment: scroll; /* Mobile smooth scrolling */
		}

		.ff-4x {
			font-size: 3rem;
		}

		.stats-grid {
			grid-template-columns: 1fr;
			gap: 24px;
		}

		.timeline-main-title {
			font-size: 2.2rem;
		}

		/* Collapsible dense timeline for mobile */
		.timeline-tree::before {
			left: 20px;
		}

		.timeline-row {
			flex-direction: column;
			padding-left: 45px;
			margin-bottom: 30px;
		}

		.timeline-opposite {
			display: none;
		}

		.mobile-date {
			display: block;
		}

		.timeline-node {
			left: 20px;
			top: 24px;
		}

		.timeline-card-wrapper {
			width: 100%;
			padding: 0;
		}
	}
</style>
