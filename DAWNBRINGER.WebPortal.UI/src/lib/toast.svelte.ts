import type { ToastMessage, ToastType } from './types';

class ToastManager {
	messages = $state<ToastMessage[]>([]);

	add(message: string, type: ToastType = 'info', durationMs = 5000): void {
		const id = Math.random().toString(36).substring(2, 9);
		this.messages.push({ id, message, type });

		setTimeout(() => {
			this.remove(id);
		}, durationMs);
	}

	success(message: string, durationMs = 5000): void {
		this.add(message, 'success', durationMs);
	}

	error(message: string, durationMs = 5000): void {
		this.add(message, 'error', durationMs);
	}

	info(message: string, durationMs = 5000): void {
		this.add(message, 'info', durationMs);
	}

	warning(message: string, durationMs = 5000): void {
		this.add(message, 'warning', durationMs);
	}

	remove(id: string): void {
		this.messages = this.messages.filter((m) => m.id !== id);
	}
}

export const toast = new ToastManager();

