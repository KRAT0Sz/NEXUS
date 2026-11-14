<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { auth } from '#lib/auth.svelte';
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import type { UserResponse } from '#lib/types';
	import { Wrench, Mail, Lock, Eye, EyeOff, Loader2 } from 'lucide-svelte';

	let loading = $state<boolean>(true);
	let accountOverview = $state<UserResponse | null>(null);

	// Update Email Form
	let newEmail = $state<string>('');
	let confirmNewEmail = $state<string>('');
	let emailPassword = $state<string>('');
	let emailPasswordVisible = $state<boolean>(false);
	let emailProcessing = $state<boolean>(false);

	// Update Password Form
	let currentPassword = $state<string>('');
	let newPassword = $state<string>('');
	let confirmNewPassword = $state<string>('');
	let currentPasswordVisible = $state<boolean>(false);
	let newPasswordVisible = $state<boolean>(false);
	let confirmNewPasswordVisible = $state<boolean>(false);
	let passwordProcessing = $state<boolean>(false);

	const displayName = $derived.by(() => {
		if (!accountOverview) return '';
		return accountOverview.currentClanTag
			? `[${accountOverview.currentClanTag}]${accountOverview.currentAccountName}`
			: accountOverview.currentAccountName;
	});

	function formatDate(timestamp: string): string {
		try {
			const d = new Date(timestamp);
			return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
		} catch {
			return timestamp;
		}
	}

	async function loadAccount(): Promise<void> {
		if (!auth.userID) return;
		loading = true;

		try {
			const res = await apiFetch<UserResponse>(`/User/${auth.userID}`);
			if (res.ok && res.data) {
				accountOverview = res.data;
			} else {
				toast.error(res.error || 'Unable to load account overview.');
			}
		} finally {
			loading = false;
		}
	}

	onMount(() => {
		if (!auth.isAuthenticated) {
			goto('/login');
			return;
		}
		loadAccount();
	});

	async function handleEmailUpdate(e: SubmitEvent): Promise<void> {
		e.preventDefault();

		if (newEmail !== confirmNewEmail) {
			toast.warning('Email addresses do not match.');
			return;
		}

		emailProcessing = true;

		try {
			const res = await apiFetch('/Email/Update/Request', {
				method: 'POST',
				body: JSON.stringify({
					EmailAddress: newEmail.trim(),
					Password: emailPassword
				})
			});

			if (res.ok) {
				const sentEmail = newEmail;
				newEmail = '';
				confirmNewEmail = '';
				emailPassword = '';
				toast.success('Email update requested successfully.');
				toast.info(`A verification link has been sent to ${sentEmail}.`);
			} else {
				toast.error(res.error || 'Failed to request email update.');
			}
		} finally {
			emailProcessing = false;
		}
	}

	async function handlePasswordUpdate(e: SubmitEvent): Promise<void> {
		e.preventDefault();

		if (newPassword !== confirmNewPassword) {
			toast.warning('New passwords do not match.');
			return;
		}

		if (newPassword.length < 8) {
			toast.warning('New password must be at least 8 characters long.');
			return;
		}

		passwordProcessing = true;

		try {
			const res = await apiFetch('/Password/Update/Request', {
				method: 'POST',
				body: JSON.stringify({
					CurrentPassword: currentPassword,
					Password: newPassword,
					ConfirmPassword: confirmNewPassword
				})
			});

			if (res.ok) {
				const email = accountOverview?.emailAddress || '';
				currentPassword = '';
				newPassword = '';
				confirmNewPassword = '';
				toast.success('Password update requested successfully.');
				toast.info(`A confirmation link has been sent to ${email}.`);
			} else {
				toast.error(res.error || 'Failed to request password update.');
			}
		} finally {
			passwordProcessing = false;
		}
	}
</script>

<svelte:head>
	<title>Account - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	{#if loading}
		<div class="loading-wrap py-16 text-center animate-fade-in">
			<Loader2 size={40} class="spin-icon accent-icon" />
			<p class="loading-text">Loading account overview...</p>
		</div>
	{:else if !accountOverview}
		<div class="alert alert-error animate-fade-in">
			Unable to load account overview. Please refresh the page and try again.
		</div>
	{:else}
		<!-- Account Header Card -->
		<div class="card glass-card pa-6 mb-6 account-header-card animate-fade-in">
			<div class="header-top-row">
				<div class="header-info">
					<span class="sub-label">Current Account</span>
					<h1 class="account-name-glow main-account-name">{displayName}</h1>
				</div>
				<a href="/account/tools" class="btn-outlined tools-btn">
					<Wrench size={16} />
					<span>Tools</span>
				</a>
			</div>

			<div class="account-stats-grid mt-6">
				<div class="stat-col">
					<span class="stat-label">Email Address</span>
					<span class="stat-val">{accountOverview.emailAddress}</span>
				</div>
				<div class="stat-col">
					<span class="stat-label">User Role</span>
					<span class="stat-val">{accountOverview.role}</span>
				</div>
				<div class="stat-col">
					<span class="stat-label">Member Since</span>
					<span class="stat-val">{formatDate(accountOverview.timestampCreated)}</span>
				</div>
				<div class="stat-col">
					<span class="stat-label">Account Level</span>
					<span class="stat-val">{accountOverview.currentAccountLevel}</span>
				</div>
			</div>
		</div>

		<!-- Sibling Accounts Card -->
		<div class="card glass-card pa-6 mb-6 animate-fade-in-delay-1">
			<div class="section-heading mb-4">
				<h2 class="section-title">Sibling Accounts</h2>
				<p class="section-sub">All accounts linked to this user, including clan membership.</p>
			</div>

			<div class="table-responsive">
				<table class="data-table">
					<thead>
						<tr>
							<th>Account Name</th>
							<th>Account Type</th>
							<th>Main Account</th>
							<th>Clan</th>
							<th>Clan Tag</th>
						</tr>
					</thead>
					<tbody>
						{#each accountOverview.accounts as acc}
							<tr>
								<td data-label="Account Name">
									<span class="font-bold">{acc.name}</span>
								</td>
								<td data-label="Account Type">
									<span>{acc.accountType.toUpperCase()}</span>
								</td>
								<td data-label="Main Account">
									{#if acc.isMain}
										<span class="chip chip-primary">Yes</span>
									{:else}
										<span class="chip chip-dim">No</span>
									{/if}
								</td>
								<td data-label="Clan">
									<span>{acc.clanName || '-'}</span>
								</td>
								<td data-label="Clan Tag">
									{#if acc.clanTag}
										<span class="chip chip-tag">[{acc.clanTag}]</span>
									{:else}
										<span class="dash">-</span>
									{/if}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		</div>

		<!-- Update Forms Grid -->
		<div class="grid-two">
			<!-- Update Email Form -->
			<div class="card glass-card pa-6 animate-fade-in-delay-2">
				<div class="section-heading mb-4">
					<h2 class="section-title">Update Email Address</h2>
					<p class="section-sub">A verification link will be sent to your new email address.</p>
				</div>

				<form onsubmit={handleEmailUpdate} class="form-stack">
					<div class="form-group">
						<label for="new-email" class="form-label">New Email Address</label>
						<div class="input-wrapper">
							<Mail size={18} class="input-icon" />
							<input
								id="new-email"
								type="email"
								bind:value={newEmail}
								placeholder="A valid email address you have access to"
								class="form-input"
								required
								disabled={emailProcessing}
							/>
						</div>
					</div>

					<div class="form-group">
						<label for="confirm-new-email" class="form-label">Confirm New Email Address</label>
						<div class="input-wrapper">
							<Mail size={18} class="input-icon" />
							<input
								id="confirm-new-email"
								type="email"
								bind:value={confirmNewEmail}
								placeholder="Enter the same email address again"
								class="form-input"
								required
								disabled={emailProcessing}
							/>
						</div>
					</div>

					<div class="form-group">
						<label for="email-pw" class="form-label">Current Password</label>
						<div class="input-wrapper">
							<input
								id="email-pw"
								type={emailPasswordVisible ? 'text' : 'password'}
								bind:value={emailPassword}
								placeholder="Your current password"
								class="form-input form-input-adorned"
								required
								disabled={emailProcessing}
							/>
							<button
								type="button"
								class="toggle-pw-btn"
								onclick={() => { emailPasswordVisible = !emailPasswordVisible; }}
								aria-label="Toggle password visibility"
							>
								{#if emailPasswordVisible}
									<Eye size={18} />
								{:else}
									<EyeOff size={18} />
								{/if}
							</button>
						</div>
					</div>

					<div class="form-actions-end">
						<button type="submit" class="btn btn-primary glow-button" disabled={emailProcessing}>
							{#if emailProcessing}
								<Loader2 size={16} class="spin-icon" />
								<span>Requesting...</span>
							{:else}
								<span>Request Email Update</span>
							{/if}
						</button>
					</div>
				</form>
			</div>

			<!-- Update Password Form -->
			<div class="card glass-card pa-6 animate-fade-in-delay-2">
				<div class="section-heading mb-4">
					<h2 class="section-title">Update Password</h2>
					<p class="section-sub">A confirmation link will be sent to your email address.</p>
				</div>

				<form onsubmit={handlePasswordUpdate} class="form-stack">
					<div class="form-group">
						<label for="curr-pw" class="form-label">Current Password</label>
						<div class="input-wrapper">
							<input
								id="curr-pw"
								type={currentPasswordVisible ? 'text' : 'password'}
								bind:value={currentPassword}
								placeholder="Your current password"
								class="form-input form-input-adorned"
								required
								disabled={passwordProcessing}
							/>
							<button
								type="button"
								class="toggle-pw-btn"
								onclick={() => { currentPasswordVisible = !currentPasswordVisible; }}
								aria-label="Toggle password visibility"
							>
								{#if currentPasswordVisible}
									<Eye size={18} />
								{:else}
									<EyeOff size={18} />
								{/if}
							</button>
						</div>
					</div>

					<div class="form-group">
						<label for="new-pw" class="form-label">New Password</label>
						<div class="input-wrapper">
							<input
								id="new-pw"
								type={newPasswordVisible ? 'text' : 'password'}
								bind:value={newPassword}
								placeholder="At least 8 characters long"
								class="form-input form-input-adorned"
								required
								minlength={8}
								disabled={passwordProcessing}
							/>
							<button
								type="button"
								class="toggle-pw-btn"
								onclick={() => { newPasswordVisible = !newPasswordVisible; }}
								aria-label="Toggle password visibility"
							>
								{#if newPasswordVisible}
									<Eye size={18} />
								{:else}
									<EyeOff size={18} />
								{/if}
							</button>
						</div>
					</div>

					<div class="form-group">
						<label for="confirm-new-pw" class="form-label">Confirm New Password</label>
						<div class="input-wrapper">
							<input
								id="confirm-new-pw"
								type={confirmNewPasswordVisible ? 'text' : 'password'}
								bind:value={confirmNewPassword}
								placeholder="Enter the same password again"
								class="form-input form-input-adorned"
								required
								minlength={8}
								disabled={passwordProcessing}
							/>
							<button
								type="button"
								class="toggle-pw-btn"
								onclick={() => { confirmNewPasswordVisible = !confirmNewPasswordVisible; }}
								aria-label="Toggle password visibility"
							>
								{#if confirmNewPasswordVisible}
									<Eye size={18} />
								{:else}
									<EyeOff size={18} />
								{/if}
							</button>
						</div>
					</div>

					<div class="form-actions-end">
						<button type="submit" class="btn btn-primary glow-button" disabled={passwordProcessing}>
							{#if passwordProcessing}
								<Loader2 size={16} class="spin-icon" />
								<span>Requesting...</span>
							{:else}
								<span>Request Password Update</span>
							{/if}
						</button>
					</div>
				</form>
			</div>
		</div>
	{/if}
</div>

<style>
	.page-container {
		max-width: 1200px;
		width: 100%;
		margin: 0 auto;
		padding: 0 24px;
	}

	.card {
		border-radius: 8px;
	}

	.account-header-card {
		position: relative;
	}

	.header-top-row {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
	}

	.sub-label {
		font-size: 0.8125rem;
		color: var(--kongor-secondary);
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.main-account-name {
		font-size: 2rem;
		margin: 4px 0 0;
		color: var(--kongor-primary);
		font-weight: 600;
	}

	.tools-btn {
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.account-stats-grid {
		display: grid;
		grid-template-columns: repeat(4, 1fr);
		gap: 16px;
	}

	.stat-col {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.stat-label {
		font-size: 0.75rem;
		color: var(--kongor-secondary);
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.stat-val {
		font-size: 1rem;
		font-weight: 500;
	}

	.section-heading {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.section-title {
		font-size: 1.25rem;
		margin: 0;
		font-weight: 600;
	}

	.section-sub {
		font-size: 0.875rem;
		opacity: 0.75;
		margin: 0;
	}

	.table-responsive {
		width: 100%;
		overflow-x: auto;
	}

	.data-table {
		width: 100%;
		border-collapse: collapse;
		text-align: left;
		font-size: 0.9375rem;
	}

	.data-table th,
	.data-table td {
		padding: 14px 16px;
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.1);
	}

	.data-table th {
		font-weight: 600;
		opacity: 0.75;
		font-size: 0.8125rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.grid-two {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 24px;
	}

	.form-stack {
		display: flex;
		flex-direction: column;
		gap: 16px;
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

	.form-actions-end {
		display: flex;
		justify-content: flex-end;
		margin-top: 6px;
	}

	.chip {
		display: inline-flex;
		align-items: center;
		padding: 3px 8px;
		border-radius: 4px;
		font-size: 0.8125rem;
		font-weight: 600;
	}

	.chip-primary {
		background: var(--kongor-primary);
		color: #0a0f0d;
	}

	.chip-dim {
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.3);
		color: inherit;
		opacity: 0.7;
	}

	.chip-tag {
		border: 1px solid rgba(38, 198, 218, 0.4);
		color: #26c6da;
		background: rgba(38, 198, 218, 0.1);
	}

	.dash {
		opacity: 0.4;
	}

	.font-bold {
		font-weight: 600;
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

	.btn-outlined {
		display: inline-flex;
		align-items: center;
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
		transition: all 0.2s ease;
	}

	.btn-outlined:hover {
		background: rgba(var(--kongor-primary-rgb), 0.12);
	}

	.loading-wrap {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 12px;
	}

	.loading-text {
		color: var(--kongor-secondary);
		font-size: 1rem;
	}

	.alert {
		padding: 16px;
		border-radius: 6px;
		font-size: 0.9375rem;
	}

	.alert-error {
		background: rgba(198, 40, 40, 0.9);
		color: #ffffff;
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
	.py-16 { padding-top: 64px; padding-bottom: 64px; }
	.mb-4 { margin-bottom: 16px; }
	.mb-6 { margin-bottom: 24px; }
	.mt-6 { margin-top: 24px; }
	.text-center { text-align: center; }

	@media (max-width: 960px) {
		.grid-two {
			grid-template-columns: 1fr;
		}

		.account-stats-grid {
			grid-template-columns: repeat(2, 1fr);
		}
	}
</style>

