<script lang="ts">
	import {
		Download,
		Rocket,
		Gamepad2,
		Server,
		Monitor,
		Terminal,
		Apple
	} from 'lucide-svelte';
</script>

<svelte:head>
	<title>Downloads - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	<!-- Page Header -->
	<header class="page-header animate-fade-in text-center">
		<div class="header-icon-wrap">
			<Download size={32} class="accent-icon" />
			<h1 class="section-header">Downloads</h1>
		</div>
		<p class="header-desc">
			Everything you need to play Heroes Of Newerth or host your own match servers.
			Choose the tools and distributions for your platform.
		</p>
	</header>

	<!-- Launcher Section -->
	<div class="card glass-card pa-6 mb-6 animate-fade-in-delay-1">
		<div class="card-header">
			<Rocket size={24} class="accent-icon" />
			<h2 class="card-title">Launcher</h2>
		</div>
		<p class="card-text">
			Lightweight tools for launching the game client or match server. The launcher handles updates,
			configuration, and starting the application. Requires the corresponding distribution to be installed.
		</p>
		<div class="btn-group">
			<a
				href="https://github.com/Project-KONGOR-Open-Source/WILLOWMAKER/releases/latest"
				target="_blank"
				rel="noreferrer"
				class="btn btn-primary glow-button"
			>
				<Gamepad2 size={18} />
				<span>Game Client Launcher</span>
			</a>
			<a
				href="https://github.com/Project-KONGOR-Open-Source/COMPEL/releases/latest"
				target="_blank"
				rel="noreferrer"
				class="btn btn-primary glow-button"
			>
				<Server size={18} />
				<span>Match Server Launcher</span>
			</a>
		</div>
	</div>

	<hr class="section-divider" />

	<!-- Distributions Row -->
	<div class="grid-two animate-fade-in-delay-2">
		<!-- Game Client Distribution -->
		<div class="card glass-card pa-6">
			<div class="card-header">
				<Gamepad2 size={24} class="accent-icon" />
				<h2 class="card-title">Game Client Distribution</h2>
			</div>
			<p class="card-text">
				The complete game client files required to play Heroes Of Newerth.
				Download the distribution for your operating system.
			</p>
			<div class="btn-group flex-wrap">
				<a
					href="https://drive.proton.me/urls/TGJHBP4QV0#EZhpvXMk2YFB"
					target="_blank"
					rel="noreferrer"
					class="btn btn-primary glow-button btn-sm"
				>
					<Monitor size={16} />
					<span>Windows</span>
				</a>
				<a
					href="https://drive.proton.me/urls/WF9BS2PCHC#55DTLzEbSM7b"
					target="_blank"
					rel="noreferrer"
					class="btn btn-primary glow-button btn-sm"
				>
					<Terminal size={16} />
					<span>Linux</span>
				</a>
				<a
					href="https://drive.proton.me/urls/95EWAJN9HW#IQnQkLNEmPwu"
					target="_blank"
					rel="noreferrer"
					class="btn btn-primary glow-button btn-sm"
				>
					<Apple size={16} />
					<span>macOS</span>
				</a>
			</div>
		</div>

		<!-- Match Server Distribution -->
		<div class="card glass-card pa-6">
			<div class="card-header">
				<Server size={24} class="accent-icon" />
				<h2 class="card-title">Match Server Distribution</h2>
			</div>
			<p class="card-text">
				The complete match server files required to host game servers.
				Download the distribution for your operating system.
			</p>
			<div class="btn-group flex-wrap">
				<a
					href="https://drive.proton.me/urls/YRDWW35GDG#1SiknIwCH7rn"
					target="_blank"
					rel="noreferrer"
					class="btn btn-primary glow-button btn-sm"
				>
					<Monitor size={16} />
					<span>Windows</span>
				</a>
				<a
					href="https://drive.proton.me/urls/46F7CDK4J8#4W1x0JYEvtBG"
					target="_blank"
					rel="noreferrer"
					class="btn btn-primary glow-button btn-sm"
				>
					<Terminal size={16} />
					<span>Linux</span>
				</a>
			</div>
		</div>
	</div>
</div>

<style>
	.page-container {
		max-width: 1200px;
		width: 100%;
		margin: 0 auto;
		padding: 0 24px;
	}

	.page-header {
		margin-bottom: 32px;
	}

	.header-icon-wrap {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 12px;
		margin-bottom: 8px;
	}

	.header-desc {
		max-width: 600px;
		margin: 0 auto;
		font-size: 1.0625rem;
		opacity: 0.85;
		line-height: 1.6;
	}

	.grid-two {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 24px;
	}

	.card {
		border-radius: 8px;
		display: flex;
		flex-direction: column;
	}

	.card-header {
		display: flex;
		align-items: center;
		gap: 12px;
		margin-bottom: 12px;
	}

	.card-title {
		margin: 0;
		font-size: 1.375rem;
		font-weight: 500;
	}

	.card-text {
		flex-grow: 1;
		font-size: 1rem;
		line-height: 1.6;
		margin: 0 0 20px;
		opacity: 0.9;
	}

	.btn-group {
		display: flex;
		gap: 12px;
	}

	.accent-icon {
		color: var(--kongor-primary);
	}

	.btn {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 20px;
		border-radius: 6px;
		font-family: inherit;
		font-size: 0.9375rem;
		font-weight: 500;
		text-decoration: none;
		border: none;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.btn-sm {
		padding: 8px 16px;
		font-size: 0.875rem;
	}

	.btn-primary {
		background: var(--kongor-primary);
		color: #0a0f0d;
	}

	.pa-6 { padding: 24px; }
	.mb-6 { margin-bottom: 24px; }
	.text-center { text-align: center; }

	@media (max-width: 960px) {
		.grid-two {
			grid-template-columns: 1fr;
		}

		.btn-group {
			flex-direction: column;
		}
	}
</style>

