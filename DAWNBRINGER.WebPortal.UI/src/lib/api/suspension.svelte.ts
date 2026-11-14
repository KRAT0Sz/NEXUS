import { apiFetch } from '#lib/api';
import type { SuspensionDTO, SuspendPlayerRequest } from '#lib/types';

class PortalSuspensionManager {
	suspensions = $state<SuspensionDTO[]>([]);
	loading = $state<boolean>(false);

	async getSuspensionsFor(accountName: string): Promise<SuspensionDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<SuspensionDTO[]>(`/Suspensions/Account/${encodeURIComponent(accountName)}`);
			if (res.ok && res.data) {
				this.suspensions = res.data;
				return res.data;
			}
			this.suspensions = [];
			return [];
		} finally {
			this.loading = false;
		}
	}

	async getAllSuspensions(): Promise<SuspensionDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<SuspensionDTO[]>('/Suspensions');
			if (res.ok && res.data) {
				this.suspensions = res.data;
				return res.data;
			}
			return [];
		} finally {
			this.loading = false;
		}
	}

	async suspend(payload: SuspendPlayerRequest): Promise<{ ok: boolean; data?: SuspensionDTO; error?: string }> {
		const res = await apiFetch<SuspensionDTO>('/Suspensions', {
			method: 'POST',
			body: JSON.stringify(payload)
		});
		return { ok: res.ok, data: res.data ?? undefined, error: res.error ?? undefined };
	}

	async overrideSuspension(id: number): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch(`/Suspensions/${id}/Override`, {
			method: 'POST'
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async changeDuration(id: number, duration: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch(`/Suspensions/${id}/Duration`, {
			method: 'POST',
			body: JSON.stringify({ Duration: duration })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}
}

export const portalSuspensionStore = new PortalSuspensionManager();

