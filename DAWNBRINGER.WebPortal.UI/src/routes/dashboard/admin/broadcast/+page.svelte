<script lang="ts">
	import { toast } from '#lib/toast.svelte';
	import { portalAdminStore } from '#lib/api/admin.svelte';
	import DashboardContainer from '#lib/components/DashboardContainer.svelte';
	import { Megaphone, CheckCircle, Send, AlertCircle } from 'lucide-svelte';

	let subject = $state<string>('');
	let subtitle = $state<string>('');
	let bodyTitle = $state<string>('');
	let body = $state<string>('');
	let footer = $state<string>('');

	let processing = $state<boolean>(false);
	let successMsg = $state<string>('');

	async function handleBroadcast(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		if (!subject.trim() || !body.trim()) {
			toast.warning('Subject and Body are required fields.');
			return;
		}

		if (!confirm('Are you sure you want to broadcast this message to ALL players in the game?')) {
			return;
		}

		processing = true;
		successMsg = '';

		try {
			const res = await portalAdminStore.broadcastMessage({
				subject: subject.trim(),
				body: body.trim(),
				subtitle: subtitle.trim() || undefined,
				bodyTitle: bodyTitle.trim() || undefined,
				footer: footer.trim() || undefined
			});

			if (res.ok) {
				const count = res.count ?? 0;
				successMsg = `Broadcast successfully delivered to ${count} account inbox(es).`;
				toast.success(`Broadcast sent to ${count} accounts.`);
				subject = '';
				subtitle = '';
				bodyTitle = '';
				body = '';
				footer = '';
			} else {
				toast.error(res.error || 'Failed to broadcast message.');
			}
		} finally {
			processing = false;
		}
	}
</script>

<DashboardContainer
	title="Global System Message Broadcast"
	description="Broadcast important announcements, patch notes, or tournament alerts directly to all in-game inboxes."
>
	{#if successMsg}
		<div class="alert-box alert-success mb-4">
			<CheckCircle size={20} class="alert-icon" />
			<div class="alert-text">{successMsg}</div>
		</div>
	{/if}

	<div class="glass-card-inner broadcast-card">
		<div class="card-header-block">
			<Megaphone size={26} class="accent-icon" />
			<div>
				<h3 class="card-title">Compose Global In-Game Broadcast</h3>
				<p class="card-desc">
					This message will be injected into every existing account’s in-game mail tab. Messages are batched automatically.
				</p>
			</div>
		</div>

		<form class="broadcast-form" onsubmit={handleBroadcast}>
			<div class="form-group">
				<label for="b-subject" class="field-label">Subject *</label>
				<input
					id="b-subject"
					type="text"
					bind:value={subject}
					placeholder="e.g. Patch 4.10.2 Maintenance Notice"
					class="kongor-field"
					required
				/>
			</div>

			<div class="form-row-2">
				<div class="form-group">
					<label for="b-subtitle" class="field-label">Subtitle (Optional)</label>
					<input
						id="b-subtitle"
						type="text"
						bind:value={subtitle}
						placeholder="e.g. Scheduled Downtime"
						class="kongor-field"
					/>
				</div>
				<div class="form-group">
					<label for="b-bodytitle" class="field-label">Body Title (Optional)</label>
					<input
						id="b-bodytitle"
						type="text"
						bind:value={bodyTitle}
						placeholder="e.g. Server Maintenance Alert"
						class="kongor-field"
					/>
				</div>
			</div>

			<div class="form-group">
				<label for="b-body" class="field-label">Body Content *</label>
				<textarea
					id="b-body"
					bind:value={body}
					placeholder="Main announcement message details..."
					class="kongor-field kongor-textarea"
					rows="6"
					required
				></textarea>
			</div>

			<div class="form-group">
				<label for="b-footer" class="field-label">Footer Note (Optional)</label>
				<input
					id="b-footer"
					type="text"
					bind:value={footer}
					placeholder="e.g. — The Project KONGOR Development Team"
					class="kongor-field"
				/>
			</div>

			<button
				type="submit"
				class="btn-filled"
				disabled={processing || !subject.trim() || !body.trim()}
			>
				<Send size={18} />
				{processing ? 'Broadcasting to All Inboxes...' : 'Send Global Broadcast'}
			</button>
		</form>
	</div>
</DashboardContainer>

<style>
	.broadcast-card {
		padding: 26px;
		border-radius: 10px;
		max-width: 800px;
	}

	.card-header-block {
		display: flex;
		align-items: flex-start;
		gap: 16px;
		margin-bottom: 22px;
	}

	.card-title {
		margin: 0 0 6px 0;
		font-size: 1.15rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.card-desc {
		margin: 0;
		font-size: 0.86rem;
		color: var(--kongor-text-muted);
		line-height: 1.5;
	}

	.form-group {
		margin-bottom: 18px;
	}

	.form-row-2 {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
	}

	@media (max-width: 600px) {
		.form-row-2 {
			grid-template-columns: 1fr;
		}
	}

	.field-label {
		display: block;
		margin-bottom: 6px;
		font-size: 0.84rem;
		font-weight: 600;
		color: var(--kongor-text-heading);
	}

	.kongor-textarea {
		resize: vertical;
		font-family: inherit;
	}

	.mb-4 { margin-bottom: 20px; }

	.alert-box {
		display: flex;
		align-items: flex-start;
		gap: 12px;
		padding: 14px 18px;
		border-radius: 8px;
		font-size: 0.88rem;
	}

	.alert-success {
		background: rgba(34, 197, 94, 0.12);
		border: 1px solid rgba(34, 197, 94, 0.35);
		color: #86efac;
	}

	.alert-icon { flex-shrink: 0; }
</style>

