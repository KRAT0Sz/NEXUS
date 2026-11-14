<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import { KeyRound, Loader2 } from 'lucide-svelte';

	let processing = $state<boolean>(false);
	const token = $derived(page.params.Token || '');

	async function handleConfirm(): Promise<void> {
		processing = true;

		try {
			const res = await apiFetch('/Password/Reset/Confirm', {
				method: 'POST',
				body: JSON.stringify({ Token: token })
			});

			if (res.ok) {
				toast.success('Password reset confirmed successfully.');
				toast.info('You can now log in with your new password.');
				goto('/login');
			} else {
				toast.error(res.error || 'Failed to confirm password reset.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<svelte:head>
	<title>Confirm Password Reset - Project KONGOR</title>
</svelte:head>

<div class="auth-page-container animate-fade-in">
	<div class="card glass-card-form pa-6 form-card text-center">
		<div class="form-header">
			<div class="header-icon-wrap">
				<KeyRound size={32} class="accent-icon" />
				<h1 class="form-title">Confirm Password Reset</h1>
			</div>
			<p class="form-subtitle">Click the button below to activate the new password that was sent to your email address.</p>
		</div>

		<div class="form-action-center mt-6">
			<button class="btn btn-primary glow-button" onclick={handleConfirm} disabled={processing}>
				{#if processing}
					<Loader2 size={18} class="spin-icon" />
					<span>Confirming...</span>
				{:else}
					<span>Confirm Password Reset</span>
				{/if}
			</button>
		</div>
	</div>

	<p class="bottom-callout text-center">
		<a href="/login" class="text-link">Return to log in</a>
	</p>
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

	.text-link {
		color: var(--kongor-primary);
		text-decoration: none;
		font-size: 0.875rem;
		font-weight: 500;
	}

	.text-link:hover {
		text-decoration: underline;
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
		border: none;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.btn-primary {
		background: var(--kongor-primary);
		color: #0a0f0d;
	}

	.btn:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.bottom-callout {
		margin-top: 18px;
		font-size: 0.9375rem;
		opacity: 0.9;
	}

	.spin-icon {
		animation: spin 1s linear infinite;
	}

	@keyframes spin {
		from { transform: rotate(0deg); }
		to { transform: rotate(360deg); }
	}

	.accent-icon {
		color: var(--kongor-primary);
	}

	.pa-6 { padding: 24px; }
	.mt-6 { margin-top: 24px; }
	.text-center { text-align: center; }
</style>

