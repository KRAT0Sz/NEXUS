import { apiFetch } from '#lib/api';
import type {
	StaffActionLogDTO,
	ConnectedAccountUserResultDTO,
	StaffRosterMemberDTO,
	AccountTypeResponseDTO
} from '#lib/types';

class PortalStaffManager {
	actions = $state<StaffActionLogDTO[]>([]);
	roster = $state<StaffRosterMemberDTO[]>([]);
	loading = $state<boolean>(false);

	async getActions(count = 50): Promise<StaffActionLogDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<StaffActionLogDTO[]>(`/Staff/Actions?count=${count}`);
			if (res.ok && res.data) {
				this.actions = res.data;
				return res.data;
			}
			return [];
		} finally {
			this.loading = false;
		}
	}

	async issueCurrency(
		accountName: string,
		amount: number,
		currencyType: 'gold' | 'silver' | 'tickets'
	): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch(`/Staff/Currency/Issue?currencyType=${currencyType}`, {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName, Amount: amount })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async promoteUserRole(userName: string, role: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Staff/Promote', {
			method: 'POST',
			body: JSON.stringify({ UserName: userName, Role: role })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async lookup(query: string): Promise<ConnectedAccountUserResultDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<ConnectedAccountUserResultDTO[]>('/Staff/Lookup', {
				method: 'POST',
				body: JSON.stringify({ Query: query })
			});
			if (res.ok && res.data) {
				return res.data;
			}
			return [];
		} finally {
			this.loading = false;
		}
	}

	async getAccountType(accountName: string): Promise<AccountTypeResponseDTO | null> {
		const res = await apiFetch<AccountTypeResponseDTO>(`/Staff/Account/${encodeURIComponent(accountName)}/Type`);
		if (res.ok && res.data) {
			return res.data;
		}
		return null;
	}

	async enableAccount(accountName: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Staff/Account/Enable', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async disableAccount(accountName: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Staff/Account/Disable', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async promoteToHost(accountName: string): Promise<{ ok: boolean; error?: string }> {
		const res = await apiFetch('/Staff/Account/PromoteToHost', {
			method: 'POST',
			body: JSON.stringify({ AccountName: accountName })
		});
		return { ok: res.ok, error: res.error ?? undefined };
	}

	async getRoster(): Promise<StaffRosterMemberDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<StaffRosterMemberDTO[]>('/Staff/Roster');
			if (res.ok && res.data) {
				this.roster = res.data;
				return res.data;
			}
			return [];
		} finally {
			this.loading = false;
		}
	}
}

export const portalStaffStore = new PortalStaffManager();

