import { apiFetch } from '#lib/api';
import type { HostAccountAuthorisationTokenResponse, BroadcastMessageRequest } from '#lib/types';

class PortalAdminManager {
	accountUpgrades = $state<string[]>([]);
	loading = $state<boolean>(false);

	async renameAccount(accountName: string, newAccountName: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Administration/Account/Rename', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName, NewAccountName: newAccountName })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async getAccountUpgrades(accountName: string): Promise<string[]> {
		this.loading = true;
		try {
			const res = await apiFetch<string[]>(`/Administration/Account/${encodeURIComponent(accountName)}/Upgrades`);
			if (res.ok && res.data) {
				this.accountUpgrades = res.data;
				return res.data;
			}
			this.accountUpgrades = [];
			return [];
		} finally {
			this.loading = false;
		}
	}

	async grantUpgrade(accountName: string, upgradeCode: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Administration/Upgrade/Grant', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName, UpgradeCode: upgradeCode })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async revokeUpgrade(accountName: string, upgradeCode: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Administration/Upgrade/Revoke', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName, UpgradeCode: upgradeCode })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async issueHostToken(): Promise<{ ok: boolean; data?: HostAccountAuthorisationTokenResponse; error?: string }> {
		const res = await apiFetch<HostAccountAuthorisationTokenResponse>('/Administration/Authorise/Host', {
			method: 'POST'
		});
		return { ok: res.ok, data: res.data ?? undefined, error: res.error ?? undefined };
	}

	async broadcastMessage(payload: BroadcastMessageRequest): Promise<{ ok: boolean; count?: number; error?: string }> {
		const res = await apiFetch<{ accountsMessaged: number }>('/Administration/Broadcast', {
			method: 'POST',
			body: JSON.stringify({
				Subject: payload.subject,
				Body: payload.body,
				Subtitle: payload.subtitle || null,
				BodyTitle: payload.bodyTitle || null,
				Footer: payload.footer || null
			})
		});
		return { ok: res.ok, count: res.data?.accountsMessaged, error: res.error ?? undefined };
	}
}

export const portalAdminStore = new PortalAdminManager();

