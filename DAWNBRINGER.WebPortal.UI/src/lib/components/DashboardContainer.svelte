<script lang="ts">
	import type { Snippet } from 'svelte';
	import { Loader2 } from 'lucide-svelte';

	interface Props {
		title: string;
		description?: string;
		loading?: boolean;
		children?: Snippet;
	}

	let { title, description = '', loading = false, children }: Props = $props();
</script>

<div class="dashboard-page-container glass-card">
	<div class="page-header">
		<div class="header-titles">
			<h1 class="page-title">{title}</h1>
			{#if description}
				<p class="page-desc">{description}</p>
			{/if}
		</div>
		{#if loading}
			<div class="header-loader">
				<Loader2 size={22} class="spin-icon text-primary" />
			</div>
		{/if}
	</div>

	<hr class="header-divider" />

	<div class="page-content" class:is-loading={loading}>
		{@render children?.()}
	</div>
</div>

<style>
	.dashboard-page-container {
		border-radius: 12px;
		padding: 24px 28px;
		position: relative;
		overflow: hidden;
	}

	.page-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 16px;
	}

	.header-titles {
		flex: 1;
	}

	.page-title {
		margin: 0;
		font-size: 1.5rem;
		font-weight: 600;
		letter-spacing: 0.5px;
		color: var(--kongor-text-heading);
	}

	.page-desc {
		margin: 6px 0 0 0;
		font-size: 0.88rem;
		color: var(--kongor-text-muted);
	}

	.header-divider {
		margin: 20px 0 24px 0;
		border: 0;
		height: 1px;
		background: linear-gradient(90deg, rgba(var(--kongor-primary-rgb), 0.35), transparent);
	}

	.page-content {
		transition: opacity 0.2s ease;
	}

	.page-content.is-loading {
		opacity: 0.65;
		pointer-events: none;
	}

	.header-loader {
		display: flex;
		align-items: center;
		padding-top: 4px;
	}

	.text-primary {
		color: var(--kongor-primary);
	}
</style>

