export interface SiblingAccount {
	name: string;
	accountType: string;
	isMain: boolean;
	clanName: string;
	clanTag: string;
}

export interface UserResponse {
	userID: number;
	currentAccountName: string;
	currentClanTag: string;
	emailAddress: string;
	role: string;
	timestampCreated: string;
	currentAccountLevel: number;
	accounts: SiblingAccount[];
}

export interface AuthenticationTokenResponse {
	token: string;
}

export interface HostAccountAuthorisationTokenResponse {
	token: string;
	expiresAt: string;
}

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastMessage {
	id: string;
	message: string;
	type: ToastType;
}

export interface LeaderboardItem {
	rank: number;
	playerName: string;
	clanTag: string;
	rating: number;
	wins: number;
	losses: number;
}

export interface ReplayItem {
	matchID: number;
	mapName: string;
	gameMode: string;
	duration: string;
	recordedDate: string;
	downloadURL: string;
}

export interface ClanItem {
	tag: string;
	name: string;
	leaderName: string;
	memberCount: number;
	createdDate: string;
}

// ==========================================
// Portal Moderation & Administration Types
// ==========================================

export interface SuspensionDTO {
	id: number;
	offenderAccountName: string;
	issuerAccountName: string;
	timestampStart: string;
	timestampEnd: string;
	reason: string;
	multiplier: number;
	override: boolean;
}

export interface SuspendPlayerRequest {
	offenderAccountName: string;
	duration: string;
	reason: string;
	internalDetails: string;
	suspendByMask?: number;
}

export interface StaffActionLogDTO {
	id: number;
	sourceID: string;
	targetID: string;
	action: string;
	details: string;
	timestamp: string;
}

export interface ConnectedAccountUserResultDTO {
	origin: string;
	name: string;
	main: boolean;
	type: string;
	nameWithClanTag: string;
	maskedEmail: string;
	userRole: string;
}

export interface StaffRosterMemberDTO {
	id: number;
	name: string;
	email: string;
	role: string;
}

export interface AccountTypeResponseDTO {
	accountName: string;
	accountType: string;
	typeId: number;
}

export interface BroadcastMessageRequest {
	subject: string;
	body: string;
	subtitle?: string;
	bodyTitle?: string;
	footer?: string;
}

// ==========================================
// Gamification, Quests & Mini-Games Types
// ==========================================

export interface QuestDTO {
	id: number;
	questKey: string;
	title: string;
	description: string;
	category: string;
	targetType: string;
	targetCount: number;
	currentCount: number;
	rewardSilver: number;
	rewardGold: number;
	rewardTickets: number;
	rewardUpgradeCode: string | null;
	isCompleted: boolean;
	isClaimed: boolean;
	claimedAt: string | null;
	resetAt: string;
	reputationReward?: number;
	triggerName?: string | null;
}

export interface ClaimQuestRewardResponse {
	success: boolean;
	message: string;
	questId: number;
	grantedSilver: number;
	grantedGold: number;
	grantedTickets: number;
	grantedUpgrade: string | null;
	newSilverBalance: number;
	newGoldBalance: number;
	newTicketBalance: number;
}

export interface DailyCheckInRewardTierDTO {
	day: number;
	rewardSilver: number;
	rewardGold: number;
	rewardTickets: number;
	isToday: boolean;
	isClaimed: boolean;
}

export interface DailyCheckInStatusDTO {
	currentStreak: number;
	hasCheckedInToday: boolean;
	lastCheckInDate: string | null;
	streakRewards: DailyCheckInRewardTierDTO[];
}

export interface ClaimCheckInResponseDTO {
	success: boolean;
	message: string;
	dayClaimed: number;
	newStreak: number;
	grantedSilver: number;
	grantedGold: number;
	grantedTickets: number;
	newSilverBalance: number;
	newGoldBalance: number;
	newTicketBalance: number;
}

export interface LuckySpinWheelSliceDTO {
	sliceIndex: number;
	label: string;
	rewardType: string;
	amount: number;
	upgradeCode: string | null;
	colorHex: string;
}

export interface LuckySpinStatusDTO {
	canFreeSpin: boolean;
	ticketCostPerSpin: number;
	availableTickets: number;
	totalSpinsPlayed: number;
	wheelSlices: LuckySpinWheelSliceDTO[];
	nextFreeSpinAvailableAt?: string | null;
	cooldownRemainingSeconds: number;
}

export interface LuckySpinResultDTO {
	success: boolean;
	winningSliceIndex: number;
	rewardLabel: string;
	rewardType: string;
	amount: number;
	upgradeCode: string | null;
	remainingTickets: number;
	newSilverBalance: number;
	newGoldBalance: number;
	newTicketBalance: number;
	message: string;
	nextFreeSpinAvailableAt?: string | null;
	cooldownRemainingSeconds: number;
}

export interface WarEffortBagDTO {
	bagNumber: number;
	name: string;
	requiredReputation: number;
	rewardType: string;
	rewardSilver: number;
	rewardGold: number;
	rewardTickets: number;
	rewardProductName: string | null;
	isFinalTier: boolean;
}

export interface AccountCompletedBagDTO {
	bagNumber: number;
	isOpened: boolean;
	completedAt: string;
	openedAt: string | null;
	grantedSilver: number;
	grantedGold: number;
	grantedTickets: number;
	grantedProductCode: string | null;
}

export interface WarEffortStatusDTO {
	currentCycle: number;
	currentBagNumber: number;
	currentBagReputation: number;
	totalCycleReputation: number;
	dailyResetsRemaining: number;
	cycleEndDate: string;
	bags: WarEffortBagDTO[];
	completedBags: AccountCompletedBagDTO[];
}

export interface OpenWarEffortBagResponseDTO {
	success: boolean;
	message: string;
	bagNumber: number;
	grantedSilver: number;
	grantedGold: number;
	grantedTickets: number;
	grantedUpgrade: string | null;
	newSilverBalance: number;
	newGoldBalance: number;
	newTicketBalance: number;
}




