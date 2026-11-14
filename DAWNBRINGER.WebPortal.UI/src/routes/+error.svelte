<script lang="ts">
	import { page } from '$app/state';
	import { SearchX } from 'lucide-svelte';
</script>

<svelte:head>
	<title>Page Not Found - Project KONGOR</title>
</svelte:head>

<div class="auth-page-container animate-fade-in">
	<div class="card glass-card-form pa-6 form-card text-center">
		<div class="form-header">
			<div class="header-icon-wrap">
				<SearchX size={36} class="accent-icon" />
				<h1 class="form-title">
					{#if page.status === 404}
						Page Not Found
					{:else}
						Error {page.status}
					{/if}
				</h1>
			</div>
			<p class="form-subtitle">
				{#if page.status === 404}
					The portal has no route mapped to this address.
				{:else}
					{page.error?.message || 'An unexpected error occurred.'}
				{/if}
			</p>
		</div>

		<div class="form-action-center mt-6">
			<a href="/" class="btn btn-primary glow-button">
				Return Home
			</a>
		</div>
	</div>
</div>

<style>
	.auth-page-container {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 24px;
		width: 100%;
		max-width: 640px;
		margin: 0 auto;
	}

	.form-card {
		width: 100%;
		border-radius: 8px;
	}

	.form-header {
		margin-bottom: 24px;
	}

	.header-icon-wrap {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 10px;
		margin-bottom: 8px;
	}

	.form-title {
		font-size: 1.5rem;
		font-weight: 500;
		margin: 0;
	}

	.form-subtitle {
		font-size: 0.9375rem;
		opacity: 0.8;
		margin: 0;
	}

	.form-action-center {
		display: flex;
		justify-content: center;
	}

	.btn {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 22px;
		border-radius: 6px;
		font-family: inherit;
		font-size: 0.9375rem;
		font-weight: 500;
		text-decoration: none;
		border: none;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.btn-primary {
		background: var(--kongor-primary);
		color: #0a0f0d;
	}

	.accent-icon {
		color: var(--kongor-primary);
	}

	.pa-6 { padding: 24px; }
	.mt-6 { margin-top: 24px; }
	.text-center { text-align: center; }
</style>

