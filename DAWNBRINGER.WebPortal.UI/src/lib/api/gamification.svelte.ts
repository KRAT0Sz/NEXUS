import { apiFetch } from '#lib/api';
import type {
	QuestDTO,
	ClaimQuestRewardResponse,
	DailyCheckInStatusDTO,
	ClaimCheckInResponseDTO,
	LuckySpinStatusDTO,
	LuckySpinResultDTO,
	WarEffortStatusDTO,
	OpenWarEffortBagResponseDTO
} from '#lib/types';

class GamificationManager {
	quests = $state<QuestDTO[]>([]);
	warEffort = $state<WarEffortStatusDTO | null>(null);
	checkInStatus = $state<DailyCheckInStatusDTO | null>(null);
	spinStatus = $state<LuckySpinStatusDTO | null>(null);
	loading = $state<boolean>(false);
	spinning = $state<boolean>(false);
	openingBag = $state<boolean>(false);

	async getQuests(): Promise<QuestDTO[]> {
		this.loading = true;
		try {
			const res = await apiFetch<QuestDTO[]>('/Quests');
			if (res.ok && res.data) {
				this.quests = res.data;
				return res.data;
			}
			return [];
		} finally {
			this.loading = false;
		}
	}

	async getWarEffortStatus(): Promise<WarEffortStatusDTO | null> {
		const res = await apiFetch<WarEffortStatusDTO>('/Quests/WarEffort');
		if (res.ok && res.data) {
			this.warEffort = res.data;
			return res.data;
		}
		return null;
	}

	async openWarEffortBag(bagNumber: number): Promise<{ ok: boolean; data?: OpenWarEffortBagResponseDTO; error?: string }> {
		this.openingBag = true;
		try {
			const res = await apiFetch<OpenWarEffortBagResponseDTO>(`/Quests/Bags/${bagNumber}/Open`, {
				method: 'POST'
			});
			if (res.ok && res.data) {
				await this.getWarEffortStatus();
				return { ok: true, data: res.data };
			}
			return { ok: false, error: res.error ?? 'Failed to open reward bag' };
		} finally {
			this.openingBag = false;
		}
	}

	async claimQuest(questId: number): Promise<{ ok: boolean; data?: ClaimQuestRewardResponse; error?: string }> {
		const res = await apiFetch<ClaimQuestRewardResponse>(`/Quests/${questId}/Claim`, {
			method: 'POST'
		});
		if (res.ok && res.data) {
			this.quests = this.quests.map((q) =>
				q.id === questId ? { ...q, isClaimed: true, claimedAt: new Date().toISOString() } : q
			);
			await this.getWarEffortStatus();
			return { ok: true, data: res.data };
		}
		return { ok: false, error: res.error ?? 'Failed to claim quest reward' };
	}

	async simulateProgress(targetType: string, increment = 1): Promise<QuestDTO[]> {
		const res = await apiFetch<QuestDTO[]>('/Quests/SimulateProgress', {
			method: 'POST',
			body: JSON.stringify({ TargetType: targetType, Increment: increment })
		});
		if (res.ok && res.data) {
			this.quests = res.data;
			await this.getWarEffortStatus();
			return res.data;
		}
		return this.quests;
	}

	async getCheckInStatus(): Promise<DailyCheckInStatusDTO | null> {
		const res = await apiFetch<DailyCheckInStatusDTO>('/MiniGames/CheckIn/Status');
		if (res.ok && res.data) {
			this.checkInStatus = res.data;
			return res.data;
		}
		return null;
	}

	async claimCheckIn(): Promise<{ ok: boolean; data?: ClaimCheckInResponseDTO; error?: string }> {
		const res = await apiFetch<ClaimCheckInResponseDTO>('/MiniGames/CheckIn/Claim', {
			method: 'POST'
		});
		if (res.ok && res.data) {
			await this.getCheckInStatus();
			return { ok: true, data: res.data };
		}
		return { ok: false, error: res.error ?? 'Failed to claim daily check-in' };
	}

	async getSpinStatus(): Promise<LuckySpinStatusDTO | null> {
		const res = await apiFetch<LuckySpinStatusDTO>('/MiniGames/Spin/Status');
		if (res.ok && res.data) {
			this.spinStatus = res.data;
			return res.data;
		}
		return null;
	}

	async playSpin(): Promise<{ ok: boolean; data?: LuckySpinResultDTO; error?: string }> {
		this.spinning = true;
		try {
			const res = await apiFetch<LuckySpinResultDTO>('/MiniGames/Spin/Play', {
				method: 'POST'
			});
			if (res.ok && res.data) {
				await this.getSpinStatus();
				return { ok: true, data: res.data };
			}
			return { ok: false, error: res.error ?? 'Failed to spin the wheel' };
		} finally {
			this.spinning = false;
		}
	}
}

export const gamification = new GamificationManager();
