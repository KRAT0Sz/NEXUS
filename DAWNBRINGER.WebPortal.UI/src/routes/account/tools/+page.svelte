<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { auth } from '#lib/auth.svelte';
	import { toast } from '#lib/toast.svelte';
	import { apiFetch } from '#lib/api';
	import type { HostAccountAuthorisationTokenResponse } from '#lib/types';
	import {
		ArrowLeft,
		Search,
		Server,
		ShieldAlert,
		Key,
		Copy,
		Check,
		X,
		Loader2
	} from 'lucide-svelte';

	let searchQuery = $state<string>('');

	// Add Host Account Modal
	let addHostDialogOpen = $state<boolean>(false);
	let hostToken = $state<string>('');
	let hostAccountName = $state<string>('');
	let addHostProcessing = $state<boolean>(false);

	// Issue Host Token Modal
	let issueDialogOpen = $state<boolean>(false);
	let issueProcessing = $state<boolean>(false);
	let issuedTokenData = $state<HostAccountAuthorisationTokenResponse | null>(null);
	let copied = $state<boolean>(false);

	onMount(() => {
		if (!auth.isAuthenticated) {
			goto('/login');
		}
	});

	interface Tool {
		id: string;
		name: string;
		description: string;
		actionLabel: string;
		icon: typeof Server;
		visible: boolean;
		action: () => void;
	}

	const allTools = $derived<Tool[]>([
		{
			id: 'add-host',
			name: 'Add Host Account',
			description: 'Redeem an authorisation token to add a match server host account as a sub-account of your user.',
			actionLabel: 'Add',
			icon: Server,
			visible: true,
			action: () => {
				hostToken = '';
				hostAccountName = '';
				addHostDialogOpen = true;
			}
		},
		{
			id: 'issue-host',
			name: 'Issue Host Account Authorisation Token',
			description: 'Generate a single-use authorisation token with a 24-hour expiration period, that allows a user to create a match server host account.',
			actionLabel: 'Issue',
			icon: ShieldAlert,
			visible: auth.isAdministrator || auth.isCustodian,
			action: () => {
				issuedTokenData = null;
				copied = false;
				issueDialogOpen = true;
			}
		}
	]);

	const filteredTools = $derived.by(() => {
		const visible = allTools.filter((t) => t.visible);
		if (!searchQuery.trim()) return visible;
		const q = searchQuery.toLowerCase();
		return visible.filter(
			(t) => t.name.toLowerCase().includes(q) || t.description.toLowerCase().includes(q)
		);
	});

	async function handleAddHostSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		if (!hostToken.trim() || !hostAccountName.trim()) {
			toast.warning('Please enter both the authorisation token and account name.');
			return;
		}

		addHostProcessing = true;

		try {
			const res = await apiFetch(`/Accounts/${encodeURIComponent(hostToken.trim())}`, {
				method: 'POST',
				body: JSON.stringify({ Name: hostAccountName.trim() })
			});

			if (res.ok) {
				toast.success(`Host account "${hostAccountName.trim()}" was created successfully.`);
				addHostDialogOpen = false;
			} else {
				toast.error(res.error || 'Failed to create host account.');
			}
		} finally {
			addHostProcessing = false;
		}
	}

	async function handleIssueToken(): Promise<void> {
		issueProcessing = true;

		try {
			const res = await apiFetch<HostAccountAuthorisationTokenResponse>('/Administration/Authorise/Host', {
				method: 'POST'
			});

			if (res.ok && res.data) {
				issuedTokenData = res.data;
				toast.success('Authorisation token issued successfully.');
			} else {
				toast.error(res.error || 'Failed to issue authorisation token.');
			}
		} finally {
			issueProcessing = false;
		}
	}

	async function copyToken(): Promise<void> {
		if (!issuedTokenData?.token) return;
		try {
			await navigator.clipboard.writeText(issuedTokenData.token);
			copied = true;
			toast.info('Token copied to clipboard.');
			setTimeout(() => { copied = false; }, 2000);
		} catch {
			toast.warning('Unable to access clipboard. Please copy manually.');
		}
	}
</script>

<svelte:head>
	<title>Account Tools - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	<!-- Page Header Card -->
	<div class="card glass-card pa-6 mb-6 animate-fade-in relative-wrap">
		<a href="/account" class="btn-outlined back-btn">
			<ArrowLeft size={16} />
			<span>Back</span>
		</a>

		<span class="sub-label">Account</span>
		<h1 class="page-title">Tools</h1>
		<p class="page-desc">
			Tools available on your account. Some tools require elevated permissions, and do not display for accounts without those permissions.
		</p>
	</div>

	<!-- Filter Card -->
	<div class="card glass-card pa-4 mb-6 animate-fade-in-delay-1">
		<div class="search-wrapper">
			<Search size={18} class="search-icon" />
			<input
				type="text"
				bind:value={searchQuery}
				placeholder="Filter Tools - Type to filter by name or description..."
				class="search-input"
			/>
		</div>
	</div>

	<!-- Tools Grid -->
	{#if filteredTools.length > 0}
		<div class="grid-two">
			{#each filteredTools as tool}
				{@const IconComponent = tool.icon}
				<div class="card glass-card pa-6 tool-card animate-fade-in-delay-2">
					<div class="tool-header">
						<IconComponent size={24} class="accent-icon" />
						<h2 class="tool-title">{tool.name}</h2>
					</div>
					<p class="tool-desc">{tool.description}</p>
					<div class="tool-action-end">
						<button class="btn btn-primary glow-button" onclick={tool.action}>
							{tool.actionLabel}
						</button>
					</div>
				</div>
			{/each}
		</div>
	{:else}
		<div class="card glass-card pa-6 text-center">
			<p class="empty-text">No tools match your filter.</p>
		</div>
	{/if}
</div>

<!-- Add Host Account Modal -->
{#if addHostDialogOpen}
	<div class="modal-backdrop" onclick={() => { if (!addHostProcessing) addHostDialogOpen = false; }} role="presentation"></div>
	<div class="modal-card glass-card pa-6" role="dialog" aria-modal="true">
		<div class="modal-header">
			<h2 class="modal-title">Add Host Account</h2>
			<button class="icon-btn" onclick={() => { addHostDialogOpen = false; }} disabled={addHostProcessing} aria-label="Close modal">
				<X size={20} />
			</button>
		</div>

		<form onsubmit={handleAddHostSubmit} class="form-stack mt-4">
			<p class="modal-desc">
				Enter the authorisation token provided by an administrator and choose a name for your new match server host account.
				The account will be created as a sub-account under your current user.
			</p>

			<div class="form-group">
				<label for="host-token" class="form-label">Authorisation Token</label>
				<div class="input-wrapper">
					<Key size={18} class="input-icon" />
					<input
						id="host-token"
						type="text"
						bind:value={hostToken}
						placeholder="The GUID issued to you by an administrator"
						class="form-input"
						required
						disabled={addHostProcessing}
					/>
				</div>
			</div>

			<div class="form-group">
				<label for="host-name" class="form-label">Account Name</label>
				<div class="input-wrapper">
					<Server size={18} class="input-icon" />
					<input
						id="host-name"
						type="text"
						bind:value={hostAccountName}
						placeholder="Between 4 and 15 characters"
						class="form-input"
						required
						minlength={4}
						maxlength={15}
						disabled={addHostProcessing}
					/>
				</div>
			</div>

			<div class="modal-actions mt-4">
				<button type="button" class="btn btn-secondary" onclick={() => { addHostDialogOpen = false; }} disabled={addHostProcessing}>
					Cancel
				</button>
				<button type="submit" class="btn btn-primary glow-button" disabled={addHostProcessing}>
					{#if addHostProcessing}
						<Loader2 size={16} class="spin-icon" />
						<span>Creating...</span>
					{:else}
						<span>Create</span>
					{/if}
				</button>
			</div>
		</form>
	</div>
{/if}

<!-- Issue Host Account Token Modal (Admin/Custodian) -->
{#if issueDialogOpen}
	<div class="modal-backdrop" onclick={() => { if (!issueProcessing) issueDialogOpen = false; }} role="presentation"></div>
	<div class="modal-card glass-card pa-6" role="dialog" aria-modal="true">
		<div class="modal-header">
			<h2 class="modal-title">Issue Host Account Authorisation Token</h2>
			<button class="icon-btn" onclick={() => { issueDialogOpen = false; }} disabled={issueProcessing} aria-label="Close modal">
				<X size={20} />
			</button>
		</div>

		{#if !issuedTokenData}
			<div class="form-stack mt-4">
				<p class="modal-desc">
					Issue a single-use authorisation token with a 24-hour expiration period, that allows a user to create a match server host account.
					The token must be shared with the recipient out-of-band.
				</p>

				<div class="modal-actions mt-4">
					<button type="button" class="btn btn-secondary" onclick={() => { issueDialogOpen = false; }} disabled={issueProcessing}>
						Cancel
					</button>
					<button type="button" class="btn btn-primary glow-button" onclick={handleIssueToken} disabled={issueProcessing}>
						{#if issueProcessing}
							<Loader2 size={16} class="spin-icon" />
							<span>Issuing...</span>
						{:else}
							<span>Issue Token</span>
						{/if}
					</button>
				</div>
			</div>
		{:else}
			<div class="form-stack mt-4">
				<div class="alert alert-success">
					Token issued successfully. Copy it now and share it with the recipient out-of-band.
				</div>

				<div class="form-group">
					<label for="issued-token" class="form-label">Authorisation Token</label>
					<div class="input-wrapper">
						<input
							id="issued-token"
							type="text"
							readonly
							value={issuedTokenData.token}
							class="form-input form-input-adorned font-mono"
						/>
						<button type="button" class="copy-btn" onclick={copyToken} aria-label="Copy token">
							{#if copied}
								<Check size={18} class="accent-icon" />
							{:else}
								<Copy size={18} />
							{/if}
						</button>
					</div>
				</div>

				<div class="modal-actions mt-4">
					<button type="button" class="btn btn-primary glow-button" onclick={() => { issueDialogOpen = false; }}>
						Done
					</button>
				</div>
			</div>
		{/if}
	</div>
{/if}

<style>
	.page-container {
		max-width: 1200px;
		width: 100%;
		margin: 0 auto;
		padding: 0 24px;
	}

	.relative-wrap {
		position: relative;
	}

	.back-btn {
		position: absolute;
		top: 24px;
		right: 24px;
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.sub-label {
		font-size: 0.8125rem;
		color: var(--kongor-secondary);
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.page-title {
		font-size: 2rem;
		margin: 4px 0 8px;
		color: var(--kongor-primary);
		font-weight: 600;
	}

	.page-desc {
		font-size: 0.9375rem;
		opacity: 0.85;
		margin: 0;
		max-width: 700px;
		line-height: 1.5;
	}

	.card {
		border-radius: 8px;
	}

	.search-wrapper {
		position: relative;
		display: flex;
		align-items: center;
	}

	.search-icon {
		position: absolute;
		left: 14px;
		color: var(--kongor-primary);
		pointer-events: none;
	}

	.search-input {
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

	.search-input:focus {
		border-color: var(--kongor-primary);
		box-shadow: 0 0 12px rgba(var(--kongor-primary-rgb), 0.25);
	}

	.grid-two {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 24px;
	}

	.tool-card {
		display: flex;
		flex-direction: column;
		height: 220px;
	}

	.tool-header {
		display: flex;
		align-items: center;
		gap: 10px;
		margin-bottom: 8px;
	}

	.tool-title {
		margin: 0;
		font-size: 1.125rem;
		font-weight: 600;
	}

	.tool-desc {
		flex-grow: 1;
		font-size: 0.9375rem;
		opacity: 0.85;
		line-height: 1.5;
		margin: 0 0 16px;
	}

	.tool-action-end {
		display: flex;
		justify-content: flex-end;
	}

	.empty-text {
		color: var(--kongor-secondary);
		margin: 0;
		font-size: 1rem;
	}

	/* Modal */
	.modal-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.7);
		backdrop-filter: blur(4px);
		z-index: 2000;
	}

	.modal-card {
		position: fixed;
		top: 50%;
		left: 50%;
		transform: translate(-50%, -50%);
		width: 100%;
		max-width: 520px;
		z-index: 2010;
		border-radius: 8px;
		animation: modal-pop 0.2s ease-out;
	}

	@keyframes modal-pop {
		from { opacity: 0; transform: translate(-50%, -46%); }
		to { opacity: 1; transform: translate(-50%, -50%); }
	}

	.modal-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding-bottom: 12px;
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.15);
	}

	.modal-title {
		margin: 0;
		font-size: 1.25rem;
		font-weight: 600;
	}

	.modal-desc {
		font-size: 0.875rem;
		opacity: 0.85;
		line-height: 1.5;
		margin: 0 0 12px;
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

	.font-mono {
		font-family: monospace;
		font-size: 0.875rem;
	}

	.copy-btn {
		position: absolute;
		right: 12px;
		background: transparent;
		border: none;
		color: inherit;
		opacity: 0.8;
		cursor: pointer;
		display: flex;
		align-items: center;
		padding: 4px;
	}

	.copy-btn:hover {
		opacity: 1;
		color: var(--kongor-primary);
	}

	.modal-actions {
		display: flex;
		justify-content: flex-end;
		gap: 12px;
	}

	.alert {
		padding: 12px 16px;
		border-radius: 6px;
		font-size: 0.875rem;
	}

	.alert-success {
		background: rgba(46, 125, 50, 0.2);
		border: 1px solid #66bb6a;
		color: #a5d6a7;
	}

	.icon-btn {
		background: transparent;
		border: none;
		color: inherit;
		cursor: pointer;
		padding: 4px;
		border-radius: 4px;
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

	.btn-secondary {
		background: rgba(255, 255, 255, 0.1);
		color: inherit;
	}

	.btn-secondary:hover {
		background: rgba(255, 255, 255, 0.18);
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

	.pa-4 { padding: 16px; }
	.pa-6 { padding: 24px; }
	.mb-6 { margin-bottom: 24px; }
	.mt-4 { margin-top: 16px; }
	.text-center { text-align: center; }

	@media (max-width: 960px) {
		.grid-two {
			grid-template-columns: 1fr;
		}

		.tool-card {
			height: auto;
		}
	}
</style>

