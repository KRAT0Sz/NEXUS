<script lang="ts">
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import { KeyRound, Mail, Loader2 } from 'lucide-svelte';

	let emailAddress = $state<string>('');
	let confirmEmailAddress = $state<string>('');
	let processing = $state<boolean>(false);

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();

		if (emailAddress !== confirmEmailAddress) {
			toast.warning('Email addresses do not match.');
			return;
		}

		processing = true;

		try {
			const res = await apiFetch('/Password/Reset/Request', {
				method: 'POST',
				body: JSON.stringify({ EmailAddress: emailAddress.trim() })
			});

			if (res.ok) {
				const sentEmail = emailAddress;
				emailAddress = '';
				confirmEmailAddress = '';
				toast.success('Password reset requested successfully.');
				toast.info(`A new password and confirmation link have been sent to ${sentEmail}.`);
			} else {
				toast.error(res.error || 'Failed to request password reset.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<svelte:head>
	<title>Forgotten Password - Project KONGOR</title>
</svelte:head>

<div class="auth-page-container animate-fade-in">
	<div class="card glass-card-form pa-6 form-card">
		<div class="form-header text-center">
			<div class="header-icon-wrap">
				<KeyRound size={32} class="accent-icon" />
				<h1 class="form-title">Forgotten Password</h1>
			</div>
			<p class="form-subtitle">Enter your email address to receive a new password. A confirmation link will be sent to your inbox.</p>
		</div>

		<form onsubmit={handleSubmit} class="auth-form">
			<div class="form-group">
				<label for="recover-email" class="form-label">Email Address</label>
				<div class="input-wrapper">
					<Mail size={18} class="input-icon" />
					<input
						id="recover-email"
						type="email"
						bind:value={emailAddress}
						placeholder="The email address associated with your account"
						class="form-input"
						required
						disabled={processing}
					/>
				</div>
				<span class="helper-text">The email address associated with your account.</span>
			</div>

			<div class="form-group">
				<label for="confirm-recover-email" class="form-label">Confirm Email Address</label>
				<div class="input-wrapper">
					<Mail size={18} class="input-icon" />
					<input
						id="confirm-recover-email"
						type="email"
						bind:value={confirmEmailAddress}
						placeholder="Enter the same email address again for confirmation"
						class="form-input"
						required
						disabled={processing}
					/>
				</div>
				<span class="helper-text">Enter the same email address again for confirmation.</span>
			</div>

			<div class="form-footer-row">
				<a href="/login" class="text-link">Back to log in</a>
				<button type="submit" class="btn btn-primary glow-button" disabled={processing}>
					{#if processing}
						<Loader2 size={18} class="spin-icon" />
						<span>Requesting...</span>
					{:else}
						<span>Request Password Reset</span>
					{/if}
				</button>
			</div>
		</form>
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

	.auth-form {
		display: flex;
		flex-direction: column;
		gap: 18px;
	}

	.form-group {
		display: flex;
		flex-direction: column;
		gap: 6px;
	}

	.form-label {
		font-size: 0.875rem;
		font-weight: 500;
	}

	.input-wrapper {
		position: relative;
		display: flex;
		align-items: center;
	}

	.input-icon {
		position: absolute;
		left: 14px;
		color: var(--kongor-primary);
		pointer-events: none;
	}

	.form-input {
		width: 100%;
		padding: 12px 14px 12px 42px;
		border-radius: 6px;
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		background: rgba(var(--kongor-surface-rgb), 0.6);
		color: inherit;
		font-family: inherit;
		font-size: 0.9375rem;
		outline: none;
		transition: border-color 0.2s ease, box-shadow 0.2s ease;
	}

	.form-input:focus {
		border-color: var(--kongor-primary);
		box-shadow: 0 0 12px rgba(var(--kongor-primary-rgb), 0.25);
	}

	.helper-text {
		font-size: 0.75rem;
		opacity: 0.65;
	}

	.form-footer-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-top: 8px;
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
	.text-center { text-align: center; }
</style>

