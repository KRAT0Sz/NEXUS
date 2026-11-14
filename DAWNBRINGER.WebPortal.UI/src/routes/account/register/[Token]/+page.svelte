<script lang="ts">
	import { page } from '$app/state';
	import { goto } from '$app/navigation';
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import { UserPlus, User, Eye, EyeOff, Loader2 } from 'lucide-svelte';

	let name = $state<string>('');
	let password = $state<string>('');
	let confirmPassword = $state<string>('');
	let passwordVisible = $state<boolean>(false);
	let confirmPasswordVisible = $state<boolean>(false);
	let processing = $state<boolean>(false);

	const token = $derived(page.params.Token || '');

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();

		if (password !== confirmPassword) {
			toast.warning('Passwords do not match.');
			return;
		}

		if (password.length < 8) {
			toast.warning('Password must be at least 8 characters long.');
			return;
		}

		processing = true;

		try {
			const res = await apiFetch('/User/Register', {
				method: 'POST',
				body: JSON.stringify({
					Token: token,
					Name: name.trim(),
					Password: password,
					ConfirmPassword: confirmPassword
				})
			});

			if (res.ok) {
				toast.success('Account created successfully.');
				goto('/login');
			} else {
				toast.error(res.error || 'Failed to create account.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<svelte:head>
	<title>Create Account - Project KONGOR</title>
</svelte:head>

<div class="auth-page-container animate-fade-in">
	<div class="card glass-card-form pa-6 form-card">
		<div class="form-header text-center">
			<div class="header-icon-wrap">
				<UserPlus size={32} class="accent-icon" />
				<h1 class="form-title">Create Account</h1>
			</div>
			<p class="form-subtitle">Choose an account name and password to complete your registration.</p>
		</div>

		<form onsubmit={handleSubmit} class="auth-form">
			<div class="form-group">
				<label for="acc-name" class="form-label">Account Name</label>
				<div class="input-wrapper">
					<User size={18} class="input-icon" />
					<input
						id="acc-name"
						type="text"
						bind:value={name}
						placeholder="Account name (4-12 characters)"
						class="form-input"
						required
						minlength={4}
						maxlength={12}
						disabled={processing}
					/>
				</div>
				<span class="helper-text">Between 4 and 12 characters. Letters, numbers, underscores, and hyphens only.</span>
			</div>

			<div class="form-group">
				<label for="acc-pw" class="form-label">Password</label>
				<div class="input-wrapper">
					<input
						id="acc-pw"
						type={passwordVisible ? 'text' : 'password'}
						bind:value={password}
						placeholder="At least 8 characters long"
						class="form-input form-input-adorned"
						required
						minlength={8}
						disabled={processing}
					/>
					<button
						type="button"
						class="toggle-pw-btn"
						onclick={() => { passwordVisible = !passwordVisible; }}
						aria-label="Toggle password visibility"
					>
						{#if passwordVisible}
							<Eye size={18} />
						{:else}
							<EyeOff size={18} />
						{/if}
					</button>
				</div>
				<span class="helper-text">At least 8 characters long.</span>
			</div>

			<div class="form-group">
				<label for="confirm-pw" class="form-label">Confirm Password</label>
				<div class="input-wrapper">
					<input
						id="confirm-pw"
						type={confirmPasswordVisible ? 'text' : 'password'}
						bind:value={confirmPassword}
						placeholder="Enter the same password again for confirmation"
						class="form-input form-input-adorned"
						required
						minlength={8}
						disabled={processing}
					/>
					<button
						type="button"
						class="toggle-pw-btn"
						onclick={() => { confirmPasswordVisible = !confirmPasswordVisible; }}
						aria-label="Toggle confirm password visibility"
					>
						{#if confirmPasswordVisible}
							<Eye size={18} />
						{:else}
							<EyeOff size={18} />
						{/if}
					</button>
				</div>
				<span class="helper-text">Enter the same password again for confirmation.</span>
			</div>

			<div class="form-actions-end">
				<button type="submit" class="btn btn-primary glow-button" disabled={processing}>
					{#if processing}
						<Loader2 size={18} class="spin-icon" />
						<span>Creating account...</span>
					{:else}
						<span>Create Account</span>
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

	.form-actions-end {
		display: flex;
		justify-content: flex-end;
		margin-top: 8px;
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

