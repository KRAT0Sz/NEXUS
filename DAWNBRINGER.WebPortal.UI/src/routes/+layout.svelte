<script lang="ts">
	import { onMount } from 'svelte';
	import { theme } from '#lib/theme.svelte';
	import { auth } from '#lib/auth.svelte';
	import Navbar from '#lib/components/Navbar.svelte';
	import Footer from '#lib/components/Footer.svelte';
	import Toaster from '#lib/components/Toaster.svelte';

	let { children } = $props();

	onMount(() => {
		theme.init();
		auth.init();
	});
</script>

<div class={theme.isDarkMode ? 'dark-mode' : 'light-mode'} id="app-theme-root">
	<div class="layout-wrapper" class:dark={theme.isDarkMode} class:light={!theme.isDarkMode}>
		<Navbar />

		<main class="page-body">
			{@render children?.()}
		</main>

		<Footer />
		<Toaster />
	</div>
</div>

<style>
	:global(html),
	:global(body) {
		margin: 0;
		padding: 0;
		font-family: 'Ubuntu', sans-serif;
		min-height: 100vh;
	}

	:global(*) {
		box-sizing: border-box;
	}

	:global(.accent-icon) {
		color: var(--kongor-primary);
	}

	:global(.spin-icon) {
		animation: spin 1s linear infinite;
	}

	@keyframes spin {
		from { transform: rotate(0deg); }
		to { transform: rotate(360deg); }
	}

	:global(.input-icon),
	:global(.search-icon) {
		position: absolute;
		left: 14px;
		color: var(--kongor-primary);
		pointer-events: none;
	}

	:global(.map-icon) {
		color: var(--kongor-secondary);
	}

	:global(.member-icon) {
		color: var(--kongor-primary);
	}

	#app-theme-root {
		min-height: 100vh;
		display: flex;
		flex-direction: column;
	}

	.layout-wrapper {
		display: flex;
		flex-direction: column;
		min-height: 100vh;
		transition: background 0.3s ease, color 0.3s ease;
		color: #e8f5e9;
	}

	.layout-wrapper.dark {
		background: linear-gradient(rgba(10, 15, 13, 0.82), rgba(10, 15, 13, 0.92)),
			url('/images/background.jpg') center/cover no-repeat fixed;
		color: #e8f5e9;
	}

	.layout-wrapper.light {
		background: linear-gradient(rgba(248, 250, 252, 0.88), rgba(248, 250, 252, 0.94)),
			url('/images/background.jpg') center/cover no-repeat fixed;
		color: #0a1929;
	}

	.page-body {
		flex: 1 0 auto;
		padding-top: 84px;
		padding-bottom: 40px;
		display: flex;
		flex-direction: column;
	}
</style>
