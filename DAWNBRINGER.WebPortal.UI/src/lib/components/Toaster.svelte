<script lang="ts">
	import { toast } from '#lib/toast.svelte';
	import { CheckCircle2, AlertTriangle, AlertCircle, Info, X } from 'lucide-svelte';
</script>

<div class="toast-container" aria-live="polite">
	{#each toast.messages as item (item.id)}
		<div class="toast toast-{item.type} animate-slide-in">
			<div class="toast-icon">
				{#if item.type === 'success'}
					<CheckCircle2 size={18} />
				{:else if item.type === 'error'}
					<AlertCircle size={18} />
				{:else if item.type === 'warning'}
					<AlertTriangle size={18} />
				{:else}
					<Info size={18} />
				{/if}
			</div>
			<div class="toast-message">{item.message}</div>
			<button class="toast-close" onclick={() => toast.remove(item.id)} aria-label="Close">
				<X size={14} />
			</button>
		</div>
	{/each}
</div>

<style>
	.toast-container {
		position: fixed;
		bottom: 24px;
		right: 24px;
		display: flex;
		flex-direction: column;
		gap: 10px;
		z-index: 10000;
		max-width: 420px;
		width: calc(100vw - 48px);
		pointer-events: none;
	}

	.toast {
		pointer-events: auto;
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 12px 16px;
		border-radius: 8px;
		color: #ffffff;
		font-family: 'Ubuntu', sans-serif;
		font-size: 0.9375rem;
		box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
		backdrop-filter: blur(12px);
		-webkit-backdrop-filter: blur(12px);
		transition: all 0.3s ease;
	}

	.toast-success {
		background: rgba(46, 125, 50, 0.92);
		border-left: 4px solid #00e676;
	}

	.toast-error {
		background: rgba(198, 40, 40, 0.92);
		border-left: 4px solid #ef5350;
	}

	.toast-warning {
		background: rgba(230, 81, 0, 0.92);
		border-left: 4px solid #ffa726;
	}

	.toast-info {
		background: rgba(2, 119, 189, 0.92);
		border-left: 4px solid #26c6da;
	}

	.toast-icon {
		display: flex;
		align-items: center;
		flex-shrink: 0;
	}

	.toast-message {
		flex-grow: 1;
		line-height: 1.4;
		word-break: break-word;
	}

	.toast-close {
		background: transparent;
		border: none;
		color: rgba(255, 255, 255, 0.7);
		cursor: pointer;
		display: flex;
		align-items: center;
		padding: 4px;
		border-radius: 4px;
		transition: color 0.2s ease, background-color 0.2s ease;
	}

	.toast-close:hover {
		color: #ffffff;
		background: rgba(255, 255, 255, 0.15);
	}

	@keyframes toast-slide-in {
		from {
			opacity: 0;
			transform: translateX(30px);
		}
		to {
			opacity: 1;
			transform: translateX(0);
		}
	}

	.animate-slide-in {
		animation: toast-slide-in 0.3s cubic-bezier(0.2, 0.8, 0.2, 1) forwards;
	}
</style>
