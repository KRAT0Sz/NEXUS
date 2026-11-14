<script lang="ts">
	import { goto } from '$app/navigation';
	import { auth } from '#lib/auth.svelte';
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import { t } from '#lib/i18n/index.svelte';
	import type { AuthenticationTokenResponse } from '#lib/types';
	import { LogIn, User, Eye, EyeOff, Loader2 } from 'lucide-svelte';

	let name = $state<string>('');
	let password = $state<string>('');
	let passwordVisible = $state<boolean>(false);
	let processing = $state<boolean>(false);

	function togglePasswordVisibility(): void {
		passwordVisible = !passwordVisible;
	}

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();

		if (!name.trim() || !password) {
			toast.warning('Please enter your account name and password.');
			return;
		}

		processing = true;

		try {
			const res = await apiFetch<AuthenticationTokenResponse>('/User/LogIn', {
				method: 'POST',
				body: JSON.stringify({ Name: name.trim(), Password: password })
			});

			if (res.ok && res.data?.token) {
				auth.setToken(res.data.token);
				toast.success('Logged in successfully.');
				goto('/account');
			} else {
				toast.error(res.error || 'Failed to log in.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<svelte:head>
	<title>{t('login.title')} - Project KONGOR</title>
</svelte:head>

<div class="auth-page-container animate-fade-in">
	<div class="card glass-card-form pa-6 form-card">
		<div class="form-header text-center">
			<div class="header-icon-wrap">
				<LogIn size={32} class="accent-icon" />
				<h1 class="form-title">{t('login.title')}</h1>
			</div>
			<p class="form-subtitle">Enter your account credentials to access the portal.</p>
		</div>

		<form onsubmit={handleSubmit} class="auth-form">
			<div class="form-group">
				<label for="account-name" class="form-label">{t('login.username')}</label>
				<div class="input-wrapper">
					<User size={18} class="input-icon" />
					<input
						id="account-name"
						type="text"
						bind:value={name}
						placeholder="Your account name (4-12 characters)"
						class="form-input"
						required
						minlength={4}
						maxlength={12}
						disabled={processing}
					/>
				</div>
				<span class="helper-text">Your account name, between 4 and 12 characters.</span>
			</div>

			<div class="form-group">
				<label for="account-password" class="form-label">{t('login.password')}</label>
				<div class="input-wrapper">
					<input
						id="account-password"
						type={passwordVisible ? 'text' : 'password'}
						bind:value={password}
						placeholder="Your account password"
						class="form-input form-input-adorned"
						required
						disabled={processing}
					/>
					<button
						type="button"
						class="toggle-pw-btn"
						onclick={togglePasswordVisibility}
						aria-label="Toggle password visibility"
					>
						{#if passwordVisible}
							<Eye size={18} />
						{:else}
							<EyeOff size={18} />
						{/if}
					</button>
				</div>
				<span class="helper-text">Your account password.</span>
			</div>

			<div class="form-footer-row">
				<a href="/password/recover" class="text-link">{t('login.forgotPassword')}</a>
				<button type="submit" class="btn btn-primary glow-button" disabled={processing}>
					{#if processing}
						<Loader2 size={18} class="spin-icon" />
						<span>Logging in...</span>
					{:else}
						<span>{t('login.login')}</span>
					{/if}
				</button>
			</div>
		</form>
	</div>

	<p class="bottom-callout text-center">
		Do not have an account? <a href="/email/register" class="text-link">{t('fp.register')}</a>
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

	.form-input-adorned {
		padding-left: 14px;
		padding-right: 42px;
	}

	.form-input:focus {
		border-color: var(--kongor-primary);
		box-shadow: 0 0 12px rgba(var(--kongor-primary-rgb), 0.25);
	}

	.toggle-pw-btn {
		position: absolute;
		right: 12px;
		background: transparent;
		border: none;
		color: inherit;
		opacity: 0.7;
		cursor: pointer;
		display: flex;
		align-items: center;
		padding: 4px;
	}

	.toggle-pw-btn:hover {
		opacity: 1;
		color: var(--kongor-primary);
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
	.text-center { text-align: center; }
</style>

