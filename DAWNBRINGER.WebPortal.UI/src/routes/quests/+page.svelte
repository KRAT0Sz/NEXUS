<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { gamification } from '#lib/api/gamification.svelte';
	import { auth } from '#lib/auth.svelte';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import type { WarEffortBagDTO, OpenWarEffortBagResponseDTO } from '#lib/types';
	import {
		Trophy,
		Coins,
		Ticket,
		CheckCircle2,
		Sparkles,
		Flame,
		Clock,
		Swords,
		Shield,
		Crosshair,
		RefreshCw,
		Lock,
		Gift,
		Package,
		Award,
		Zap,
		Crown,
		X
	} from 'lucide-svelte';

	let claimingQuestId = $state<number | null>(null);
	let openingBagNumber = $state<number | null>(null);
	let simulating = $state<boolean>(false);
	let celebrationModal = $state<OpenWarEffortBagResponseDTO | null>(null);
	let selectedBagForDetails = $state<WarEffortBagDTO | null>(null);
	let timeRemaining = $state<string>('');
	let timerInterval: ReturnType<typeof setInterval> | null = null;

	onMount(async () => {
		if (auth.isAuthenticated) {
			await loadAllData();
			startCycleCountdown();
		}
	});

	onDestroy(() => {
		if (timerInterval) {
			clearInterval(timerInterval);
		}
	});

	async function loadAllData() {
		await Promise.all([
			gamification.getQuests(),
			gamification.getWarEffortStatus()
		]);
	}

	function startCycleCountdown() {
		updateCountdown();
		timerInterval = setInterval(updateCountdown, 1000);
	}

	function updateCountdown() {
		const we = gamification.warEffort;
		if (!we || !we.cycleEndDate) {
			timeRemaining = '--:--:--';
			return;
		}

		const diffMs = new Date(we.cycleEndDate).getTime() - Date.now();
		if (diffMs <= 0) {
			timeRemaining = 'Cycle Ending...';
			return;
		}

		const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
		const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
		const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
		const seconds = Math.floor((diffMs % (1000 * 60)) / 1000);

		if (days > 0) {
			timeRemaining = `${days}d ${hours.toString().padStart(2, '0')}h ${minutes.toString().padStart(2, '0')}m`;
		} else {
			timeRemaining = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
		}
	}

	async function handleClaim(questId: number) {
		claimingQuestId = questId;
		const res = await gamification.claimQuest(questId);
		claimingQuestId = null;

		if (res.ok && res.data) {
			toast.success(
				`Claimed: +${res.data.grantedSilver} Silver, +${res.data.grantedGold} Gold, +${res.data.grantedTickets} Tickets!`
			);
		} else {
			toast.error(res.error || 'Failed to claim reward');
		}
	}

	async function handleOpenBag(bagNumber: number) {
		openingBagNumber = bagNumber;
		const res = await gamification.openWarEffortBag(bagNumber);
		openingBagNumber = null;

		if (res.ok && res.data) {
			celebrationModal = res.data;
			toast.success(`Unlocked ${res.data.message}`);
		} else {
			toast.error(res.error || 'Failed to open reward bag');
		}
	}

	async function handleSimulate(targetType: string, inc: number = 1) {
		simulating = true;
		await gamification.simulateProgress(targetType, inc);
		simulating = false;
		toast.info(`Simulated match progress: +${inc} ${targetType}`);
	}

	function getTargetIcon(targetType: string) {
		switch (targetType) {
			case 'MatchWin':
				return Trophy;
			case 'Kills':
				return Crosshair;
			case 'Assists':
				return Swords;
			case 'WardsPlaced':
				return Shield;
			default:
				return Flame;
		}
	}

	function getBagStatus(bag: WarEffortBagDTO) {
		const we = gamification.warEffort;
		if (!we) return { isOpened: false, isReady: false, isActive: false, isLocked: true };

		const completed = we.completedBags?.find((cb) => cb.bagNumber === bag.bagNumber);
		if (completed) {
			return {
				isOpened: completed.isOpened,
				isReady: !completed.isOpened,
				isActive: false,
				isLocked: false
			};
		}

		if (bag.bagNumber === we.currentBagNumber) {
			return {
				isOpened: false,
				isReady: false,
				isActive: true,
				isLocked: false
			};
		}

		return {
			isOpened: false,
			isReady: false,
			isActive: false,
			isLocked: bag.bagNumber > we.currentBagNumber
		};
	}

	function getBagProgressPercentage(bag: WarEffortBagDTO): number {
		const we = gamification.warEffort;
		if (!we) return 0;
		const status = getBagStatus(bag);
		if (status.isOpened || status.isReady) return 100;
		if (status.isActive) {
			const req = Math.max(1, bag.requiredReputation);
			return Math.min(100, Math.round((we.currentBagReputation / req) * 100));
		}
		return 0;
	}
</script>

<div class="quests-container">
	<!-- Page Header -->
	<div class="quests-header">
		<div class="header-icon-wrapper">
			<Trophy class="header-icon" />
		</div>
		<div>
			<h1 class="page-title">{t('quests.warEffortTitle')}</h1>
			<p class="page-subtitle">{t('quests.warEffortSubtitle')}</p>
		</div>
	</div>

	{#if !auth.isAuthenticated}
		<div class="login-prompt">
			<p>Please log in to participate in The War Effort and track your active quests.</p>
			<a href="/login" class="login-btn">Log In</a>
		</div>
	{:else if gamification.loading && !gamification.warEffort && gamification.quests.length === 0}
		<div class="loading-state">
			<RefreshCw class="spin-icon" />
			<p>Loading War Effort progression and quests...</p>
		</div>
	{:else}
		<!-- ========================================================================= -->
		<!-- THE WAR EFFORT PROGRESSION TRACK (14 Tiers)                               -->
		<!-- ========================================================================= -->
		{#if gamification.warEffort}
			{@const we = gamification.warEffort}
			{@const currentBag = we.bags.find((b) => b.bagNumber === we.currentBagNumber) || we.bags[0]}
			<section class="war-effort-section">
				<!-- War Effort Overview Stats Card -->
				<div class="war-effort-banner">
					<div class="banner-left">
						<div class="cycle-emblem">
							<Shield class="emblem-shield" />
							<span class="cycle-label">CYCLE</span>
							<span class="cycle-num">{we.currentCycle}</span>
						</div>
						<div class="banner-titles">
							<div class="banner-tag">
								<Sparkles class="tag-sparkle" />
								<span>{t('quests.warEffortCycle', { cycle: we.currentCycle })}</span>
							</div>
							<h2 class="banner-heading">The 14 Bag Campaign</h2>
							<p class="banner-desc">
								Earn Reputation Points from matches and daily objectives to unlock up to 14 campaign bags!
							</p>
						</div>
					</div>

					<div class="banner-stats">
						<div class="stat-pill">
							<Clock class="stat-icon timer" />
							<div class="stat-details">
								<span class="stat-label">{t('quests.cycleEndsIn')}</span>
								<span class="stat-value countdown">{timeRemaining}</span>
							</div>
						</div>

						<div class="stat-pill">
							<Award class="stat-icon rep" />
							<div class="stat-details">
								<span class="stat-label">{t('quests.totalReputation')}</span>
								<span class="stat-value rep-value">{we.totalCycleReputation} RP</span>
							</div>
						</div>

						<div class="stat-pill">
							<Package class="stat-icon bag" />
							<div class="stat-details">
								<span class="stat-label">{t('quests.activeTier')}</span>
								<span class="stat-value">{we.currentBagNumber} / 14</span>
							</div>
						</div>
					</div>
				</div>

				<!-- Active Milestone Highlight Progress -->
				{#if currentBag}
					{@const activePct = Math.min(
						100,
						Math.round((we.currentBagReputation / Math.max(1, currentBag.requiredReputation)) * 100)
					)}
					<div class="active-progress-card">
						<div class="active-progress-header">
							<div class="tier-info">
								<span class="tier-badge">Tier {currentBag.bagNumber} Active</span>
								<h3 class="tier-name">{currentBag.name}</h3>
							</div>
							<div class="reputation-ratio">
								<span class="current-rp">{we.currentBagReputation}</span>
								<span class="max-rp">/ {currentBag.requiredReputation} RP</span>
								<span class="pct-pill">{activePct}%</span>
							</div>
						</div>

						<div class="campaign-progress-bar">
							<div class="campaign-bar-fill" style="width: {activePct}%">
								<div class="bar-glow"></div>
							</div>
						</div>
					</div>
				{/if}

				<!-- 14-Bag Milestone Track -->
				<div class="track-header-row">
					<div class="track-title-group">
						<Gift class="track-title-icon" />
						<h3 class="track-title">Campaign Milestones (14 Tiers)</h3>
					</div>
					<span class="track-note">Click any unlocked bag to claim your spoils</span>
				</div>

				<div class="milestones-track">
					{#each we.bags as bag (bag.bagNumber)}
						{@const status = getBagStatus(bag)}
						{@const progressPct = getBagProgressPercentage(bag)}
						{@const isSatchel = bag.bagNumber <= 5}
						{@const isChest = bag.bagNumber > 5 && !bag.isFinalTier}
						{@const isFinal = bag.isFinalTier}

						<div
							class="milestone-node"
							class:opened={status.isOpened}
							class:ready={status.isReady}
							class:active={status.isActive}
							class:locked={status.isLocked}
							class:final-tier={isFinal}
							tabindex="0"
							role="button"
							onclick={() => (selectedBagForDetails = bag)}
							onkeydown={(e) => {
								if (e.key === 'Enter' || e.key === ' ') {
									e.preventDefault();
									selectedBagForDetails = bag;
								}
							}}
						>
							<!-- Tier Marker -->
							<div class="node-tier-label">
								{#if isFinal}
									<Crown class="crown-icon" />
									<span>FINAL</span>
								{:else}
									<span>T{bag.bagNumber}</span>
								{/if}
							</div>

							<!-- Graphic Container -->
							<div class="node-graphic">
								{#if isSatchel}
									<div class="bag-art satchel-art">
										<Package class="art-icon" />
									</div>
								{:else if isChest}
									<div class="bag-art chest-art">
										<Gift class="art-icon" />
									</div>
								{:else}
									<div class="bag-art champion-art">
										<Crown class="art-icon gold" />
										<div class="aura-glow"></div>
									</div>
								{/if}

								<!-- Overlay Status Badges -->
								{#if status.isOpened}
									<div class="node-status-badge opened">
										<CheckCircle2 class="status-icon" />
									</div>
								{:else if status.isReady}
									<div class="node-status-badge ready animate-pulse">
										<Sparkles class="status-icon" />
									</div>
								{:else if status.isLocked}
									<div class="node-status-badge locked">
										<Lock class="status-icon" />
									</div>
								{/if}
							</div>

							<!-- Bag Info -->
							<div class="node-info">
								<span class="node-name" title={bag.name}>{bag.name}</span>
								<span class="node-rp">{bag.requiredReputation} RP</span>
							</div>

							<!-- Rewards Tiny Strip -->
							<div class="node-rewards">
								{#if bag.rewardSilver > 0}
									<span class="reward-mini silver" title="{bag.rewardSilver} Silver">
										<Coins class="mini-icon" />
										{bag.rewardSilver}
									</span>
								{/if}
								{#if bag.rewardGold > 0}
									<span class="reward-mini gold" title="{bag.rewardGold} Gold">
										<Sparkles class="mini-icon" />
										{bag.rewardGold}
									</span>
								{/if}
								{#if bag.rewardTickets > 0}
									<span class="reward-mini ticket" title="{bag.rewardTickets} Ticket(s)">
										<Ticket class="mini-icon" />
										{bag.rewardTickets}
									</span>
								{/if}
							</div>

							<!-- Action Button for Ready Bags -->
							{#if status.isReady}
								<button
									class="open-bag-btn"
									disabled={openingBagNumber === bag.bagNumber}
									onclick={(e) => {
										e.stopPropagation();
										handleOpenBag(bag.bagNumber);
									}}
								>
									{#if openingBagNumber === bag.bagNumber}
										<RefreshCw class="btn-icon spin-icon" />
									{:else}
										<Sparkles class="btn-icon" />
									{/if}
									{t('quests.openBag')}
								</button>
							{:else if status.isOpened}
								<div class="node-opened-tag">
									<CheckCircle2 class="tag-icon" />
									{t('quests.opened')}
								</div>
							{:else if status.isActive}
								<div class="node-progress-strip">
									<div class="strip-fill" style="width: {progressPct}%"></div>
								</div>
							{/if}
						</div>
					{/each}
				</div>
			</section>
		{/if}

		<!-- ========================================================================= -->
		<!-- ACTIVE DAILY OBJECTIVES (Quests)                                          -->
		<!-- ========================================================================= -->
		<section class="quests-section">
			<div class="section-title-bar">
				<div class="title-with-badge">
					<Flame class="section-flame-icon" />
					<h2 class="section-title">{t('quests.dailySlots')}</h2>
				</div>
				<div class="reset-timer-tag">
					<Clock class="clock-icon" />
					<span>Resets daily at 00:00 UTC</span>
				</div>
			</div>

			<div class="quests-grid">
				{#each gamification.quests as quest (quest.id)}
					{@const Icon = getTargetIcon(quest.targetType)}
					{@const progressPct = Math.min(
						100,
						Math.round((quest.currentCount / Math.max(1, quest.targetCount)) * 100)
					)}
					<div class="quest-card" class:completed={quest.isCompleted} class:claimed={quest.isClaimed}>
						<div class="quest-top">
							<div class="quest-icon-badge">
								<Icon class="quest-icon" />
							</div>
							<div class="quest-meta">
								<div class="category-row">
									<span class="category-badge">{quest.category}</span>
									<span class="rp-badge">
										<Zap class="rp-icon" />
										+{quest.reputationReward || 25} RP
									</span>
								</div>
								<h3 class="quest-title">{quest.title}</h3>
							</div>
						</div>

						<p class="quest-desc">{quest.description}</p>

						<div class="progress-section">
							<div class="progress-labels">
								<span>{t('quests.progress')}</span>
								<span class="count-text">{quest.currentCount} / {quest.targetCount}</span>
							</div>
							<div class="progress-track">
								<div class="progress-fill" style="width: {progressPct}%"></div>
							</div>
						</div>

						<div class="rewards-section">
							<span class="rewards-label">{t('quests.rewards')}:</span>
							<div class="reward-badges">
								{#if quest.rewardSilver > 0}
									<span class="reward-badge silver">
										<Coins class="badge-icon" />
										{quest.rewardSilver}
									</span>
								{/if}
								{#if quest.rewardGold > 0}
									<span class="reward-badge gold">
										<Sparkles class="badge-icon" />
										{quest.rewardGold}
									</span>
								{/if}
								{#if quest.rewardTickets > 0}
									<span class="reward-badge ticket">
										<Ticket class="badge-icon" />
										{quest.rewardTickets} Ticket{quest.rewardTickets > 1 ? 's' : ''}
									</span>
								{/if}
								<span class="reward-badge reputation">
									<Zap class="badge-icon" />
									+{quest.reputationReward || 25} RP
								</span>
							</div>
						</div>

						<div class="quest-action">
							{#if quest.isClaimed}
								<button class="action-btn claimed" disabled>
									<CheckCircle2 class="btn-icon" />
									{t('quests.claimed')}
								</button>
							{:else if quest.isCompleted}
								<button
									class="action-btn claim-active"
									disabled={claimingQuestId === quest.id}
									onclick={() => handleClaim(quest.id)}
								>
									{#if claimingQuestId === quest.id}
										<RefreshCw class="btn-icon spin-icon" />
									{:else}
										<Sparkles class="btn-icon" />
									{/if}
									{t('quests.claimReward')}
								</button>
							{:else}
								<div class="in-progress-badge">
									<Clock class="badge-icon" />
									In Progress ({quest.currentCount} / {quest.targetCount})
								</div>
							{/if}
						</div>
					</div>
				{/each}
			</div>
		</section>

		<!-- ========================================================================= -->
		<!-- SIMULATION TESTING PANEL                                                  -->
		<!-- ========================================================================= -->
		<div class="simulation-panel">
			<div class="sim-header">
				<Sparkles class="sim-icon" />
				<div>
					<h4>{t('quests.simulateMatch')}</h4>
					<p>Test match triggers: advances both quest completion and The War Effort reputation points in real-time!</p>
				</div>
			</div>
			<div class="sim-buttons">
				<button class="sim-btn" disabled={simulating} onclick={() => handleSimulate('MatchWin', 1)}>
					{t('quests.simulateWin')}
				</button>
				<button class="sim-btn" disabled={simulating} onclick={() => handleSimulate('Kills', 1)}>
					{t('quests.simulateKill')}
				</button>
				<button class="sim-btn" disabled={simulating} onclick={() => handleSimulate('WardsPlaced', 5)}>
					{t('quests.simulateWard')}
				</button>
				<button class="sim-btn" disabled={simulating} onclick={() => handleSimulate('Assists', 5)}>
					{t('quests.simulateAssist')}
				</button>
				<button class="sim-btn" disabled={simulating} onclick={() => handleSimulate('MatchesPlayed', 1)}>
					{t('quests.simulateMatchPlayed')}
				</button>
			</div>
		</div>
	{/if}
</div>

<!-- ============================================================================= -->
<!-- CELEBRATION MODAL: BAG OPENED                                                 -->
<!-- ============================================================================= -->
{#if celebrationModal}
	<div
		class="modal-backdrop"
		tabindex="0"
		role="button"
		onclick={() => (celebrationModal = null)}
		onkeydown={(e) => {
			if (e.key === 'Escape') celebrationModal = null;
		}}
	>
		<div
			class="celebration-card"
			tabindex="0"
			role="dialog"
			aria-labelledby="celebration-title"
			onclick={(e) => e.stopPropagation()}
			onkeydown={(e) => e.stopPropagation()}
		>
			<button class="close-modal-btn" onclick={() => (celebrationModal = null)}>
				<X class="x-icon" />
			</button>

			<div class="celebration-header">
				<div class="chest-glow-icon">
					<Gift class="loot-icon" />
					<div class="burst-glow"></div>
				</div>
				<h2 id="celebration-title" class="celebration-title">{t('quests.bagOpenedTitle')}</h2>
				<p class="celebration-subtitle">{celebrationModal.message}</p>
			</div>

			<div class="loot-prizes-grid">
				{#if celebrationModal.grantedSilver > 0}
					<div class="loot-item silver">
						<Coins class="loot-badge-icon" />
						<span class="loot-qty">+{celebrationModal.grantedSilver}</span>
						<span class="loot-name">Silver Coins</span>
					</div>
				{/if}
				{#if celebrationModal.grantedGold > 0}
					<div class="loot-item gold">
						<Sparkles class="loot-badge-icon" />
						<span class="loot-qty">+{celebrationModal.grantedGold}</span>
						<span class="loot-name">Gold Coins</span>
					</div>
				{/if}
				{#if celebrationModal.grantedTickets > 0}
					<div class="loot-item ticket">
						<Ticket class="loot-badge-icon" />
						<span class="loot-qty">+{celebrationModal.grantedTickets}</span>
						<span class="loot-name">Plinko Tickets</span>
					</div>
				{/if}
				{#if celebrationModal.grantedUpgrade}
					<div class="loot-item item">
						<Award class="loot-badge-icon" />
						<span class="loot-qty">UNLOCK</span>
						<span class="loot-name">{celebrationModal.grantedUpgrade}</span>
					</div>
				{/if}
			</div>

			<button class="claim-loot-btn" onclick={() => (celebrationModal = null)}>
				<Sparkles class="btn-icon" />
				Claim & Continue
			</button>
		</div>
	</div>
{/if}

<!-- ============================================================================= -->
<!-- BAG DETAILS PREVIEW MODAL                                                     -->
<!-- ============================================================================= -->
{#if selectedBagForDetails}
	{@const bag = selectedBagForDetails}
	{@const status = getBagStatus(bag)}
	<div
		class="modal-backdrop"
		tabindex="0"
		role="button"
		onclick={() => (selectedBagForDetails = null)}
		onkeydown={(e) => {
			if (e.key === 'Escape') selectedBagForDetails = null;
		}}
	>
		<div
			class="details-card"
			tabindex="0"
			role="dialog"
			aria-labelledby="details-title"
			onclick={(e) => e.stopPropagation()}
			onkeydown={(e) => e.stopPropagation()}
		>
			<button class="close-modal-btn" onclick={() => (selectedBagForDetails = null)}>
				<X class="x-icon" />
			</button>

			<div class="details-top">
				<div class="details-badge">Tier {bag.bagNumber} / 14</div>
				<h3 id="details-title" class="details-title">{bag.name}</h3>
				<span class="details-req">Requires {bag.requiredReputation} Reputation Points</span>
			</div>

			<div class="details-rewards-box">
				<h4>Contents & Rewards:</h4>
				<div class="reward-badges-large">
					{#if bag.rewardSilver > 0}
						<div class="reward-large silver">
							<Coins class="large-icon" />
							<span>{bag.rewardSilver} Silver Coins</span>
						</div>
					{/if}
					{#if bag.rewardGold > 0}
						<div class="reward-large gold">
							<Sparkles class="large-icon" />
							<span>{bag.rewardGold} Gold Coins</span>
						</div>
					{/if}
					{#if bag.rewardTickets > 0}
						<div class="reward-large ticket">
							<Ticket class="large-icon" />
							<span>{bag.rewardTickets} Plinko Ticket{bag.rewardTickets > 1 ? 's' : ''}</span>
						</div>
					{/if}
					{#if bag.rewardProductName}
						<div class="reward-large item">
							<Award class="large-icon" />
							<span>Exclusive: {bag.rewardProductName}</span>
						</div>
					{/if}
				</div>
			</div>

			<div class="details-action-area">
				{#if status.isReady}
					<button
						class="open-bag-btn large"
						onclick={() => {
							selectedBagForDetails = null;
							handleOpenBag(bag.bagNumber);
						}}
					>
						<Sparkles class="btn-icon" />
						{t('quests.openBag')}
					</button>
				{:else if status.isOpened}
					<div class="status-text opened">
						<CheckCircle2 class="text-icon" />
						{t('quests.opened')}
					</div>
				{:else if status.isActive}
					<div class="status-text active">
						<Zap class="text-icon" />
						Currently Progressing ({gamification.warEffort?.currentBagReputation || 0} / {bag.requiredReputation} RP)
					</div>
				{:else}
					<div class="status-text locked">
						<Lock class="text-icon" />
						{t('quests.locked')}
					</div>
				{/if}
			</div>
		</div>
	</div>
{/if}

<style>
	.quests-container {
		max-width: 1280px;
		margin: 0 auto;
		padding: 30px 20px 80px;
	}

	.quests-header {
		display: flex;
		align-items: center;
		gap: 20px;
		margin-bottom: 28px;
	}

	.header-icon-wrapper {
		width: 56px;
		height: 56px;
		border-radius: 14px;
		background: linear-gradient(135deg, rgba(234, 179, 8, 0.25), rgba(234, 179, 8, 0.05));
		border: 1px solid rgba(234, 179, 8, 0.5);
		display: flex;
		align-items: center;
		justify-content: center;
		box-shadow: 0 0 20px rgba(234, 179, 8, 0.2);
	}

	:global(.header-icon) {
		width: 30px;
		height: 30px;
		color: #facc15;
	}

	.page-title {
		font-size: 2rem;
		font-weight: 800;
		color: #ffffff;
		margin: 0 0 4px 0;
		letter-spacing: -0.5px;
	}

	.page-subtitle {
		font-size: 0.95rem;
		color: #94a3b8;
		margin: 0;
	}

	.login-prompt,
	.loading-state {
		text-align: center;
		padding: 60px 20px;
		background: rgba(15, 23, 42, 0.7);
		border: 1px solid rgba(255, 255, 255, 0.08);
		border-radius: 16px;
		color: #94a3b8;
	}

	.login-btn {
		display: inline-block;
		margin-top: 16px;
		padding: 10px 28px;
		background: #eab308;
		color: #0f172a;
		font-weight: 700;
		border-radius: 8px;
		text-decoration: none;
	}

	/* ========================================================================= */
	/* THE WAR EFFORT PROGRESSION TRACK                                          */
	/* ========================================================================= */
	.war-effort-section {
		background: radial-gradient(ellipse at top, #1e293b 0%, #0f172a 100%);
		border: 1px solid rgba(234, 179, 8, 0.25);
		border-radius: 20px;
		padding: 24px;
		margin-bottom: 36px;
		box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5), inset 0 1px 0 rgba(255, 255, 255, 0.1);
	}

	.war-effort-banner {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 24px;
		flex-wrap: wrap;
		padding-bottom: 20px;
		border-bottom: 1px solid rgba(255, 255, 255, 0.08);
		margin-bottom: 20px;
	}

	.banner-left {
		display: flex;
		align-items: center;
		gap: 20px;
	}

	.cycle-emblem {
		position: relative;
		width: 64px;
		height: 72px;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	:global(.emblem-shield) {
		position: absolute;
		width: 100%;
		height: 100%;
		color: #d97706;
		fill: rgba(217, 119, 6, 0.2);
		filter: drop-shadow(0 0 10px rgba(234, 179, 8, 0.4));
	}

	.cycle-label {
		position: relative;
		font-size: 0.65rem;
		font-weight: 800;
		letter-spacing: 1px;
		color: #fde047;
		margin-top: 4px;
	}

	.cycle-num {
		position: relative;
		font-size: 1.5rem;
		font-weight: 900;
		color: #ffffff;
		line-height: 1;
	}

	.banner-tag {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 0.75rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.8px;
		color: #facc15;
		margin-bottom: 4px;
	}

	:global(.tag-sparkle) {
		width: 14px;
		height: 14px;
		color: #facc15;
	}

	.banner-heading {
		font-size: 1.45rem;
		font-weight: 800;
		color: #f8fafc;
		margin: 0 0 4px 0;
	}

	.banner-desc {
		font-size: 0.85rem;
		color: #94a3b8;
		margin: 0;
		max-width: 480px;
	}

	.banner-stats {
		display: flex;
		gap: 14px;
		flex-wrap: wrap;
	}

	.stat-pill {
		background: rgba(15, 23, 42, 0.6);
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 12px;
		padding: 10px 16px;
		display: flex;
		align-items: center;
		gap: 12px;
	}

	:global(.stat-icon) {
		width: 24px;
		height: 24px;
	}

	:global(.stat-icon.timer) {
		color: #38bdf8;
	}
	:global(.stat-icon.rep) {
		color: #facc15;
	}
	:global(.stat-icon.bag) {
		color: #a78bfa;
	}

	.stat-details {
		display: flex;
		flex-direction: column;
	}

	.stat-label {
		font-size: 0.7rem;
		color: #64748b;
		text-transform: uppercase;
		font-weight: 700;
		letter-spacing: 0.5px;
	}

	.stat-value {
		font-size: 1.05rem;
		font-weight: 800;
		color: #f8fafc;
	}

	.stat-value.countdown {
		color: #38bdf8;
		font-variant-numeric: tabular-nums;
	}

	.stat-value.rep-value {
		color: #facc15;
	}

	/* Active Progress Bar */
	.active-progress-card {
		background: rgba(15, 23, 42, 0.7);
		border: 1px solid rgba(234, 179, 8, 0.3);
		border-radius: 14px;
		padding: 16px 20px;
		margin-bottom: 24px;
	}

	.active-progress-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 10px;
	}

	.tier-info {
		display: flex;
		align-items: center;
		gap: 10px;
	}

	.tier-badge {
		background: rgba(234, 179, 8, 0.2);
		border: 1px solid rgba(234, 179, 8, 0.5);
		color: #fde047;
		font-size: 0.75rem;
		font-weight: 800;
		text-transform: uppercase;
		padding: 2px 8px;
		border-radius: 6px;
	}

	.tier-name {
		font-size: 1.05rem;
		font-weight: 700;
		color: #ffffff;
		margin: 0;
	}

	.reputation-ratio {
		display: flex;
		align-items: baseline;
		gap: 6px;
	}

	.current-rp {
		font-size: 1.25rem;
		font-weight: 800;
		color: #facc15;
	}

	.max-rp {
		font-size: 0.85rem;
		color: #94a3b8;
	}

	.pct-pill {
		background: rgba(56, 189, 248, 0.15);
		color: #38bdf8;
		font-size: 0.75rem;
		font-weight: 800;
		padding: 2px 8px;
		border-radius: 999px;
		margin-left: 6px;
	}

	.campaign-progress-bar {
		width: 100%;
		height: 12px;
		background: rgba(255, 255, 255, 0.08);
		border-radius: 999px;
		overflow: hidden;
		position: relative;
	}

	.campaign-bar-fill {
		height: 100%;
		background: linear-gradient(90deg, #ca8a04, #eab308, #facc15);
		border-radius: 999px;
		transition: width 0.5s ease;
		position: relative;
	}

	.bar-glow {
		position: absolute;
		right: 0;
		top: 0;
		bottom: 0;
		width: 20px;
		background: #ffffff;
		opacity: 0.6;
		filter: blur(3px);
	}

	/* Milestones Track */
	.track-header-row {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 14px;
	}

	.track-title-group {
		display: flex;
		align-items: center;
		gap: 8px;
	}

	:global(.track-title-icon) {
		width: 18px;
		height: 18px;
		color: #facc15;
	}

	.track-title {
		font-size: 1rem;
		font-weight: 700;
		color: #e2e8f0;
		margin: 0;
	}

	.track-note {
		font-size: 0.75rem;
		color: #64748b;
	}

	.milestones-track {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
		gap: 12px;
	}

	.milestone-node {
		background: rgba(15, 23, 42, 0.8);
		border: 1px solid rgba(255, 255, 255, 0.08);
		border-radius: 14px;
		padding: 12px 10px;
		display: flex;
		flex-direction: column;
		align-items: center;
		text-align: center;
		cursor: pointer;
		position: relative;
		transition: all 0.25s ease;
		outline: none;
	}

	.milestone-node:hover {
		transform: translateY(-3px);
		border-color: rgba(234, 179, 8, 0.4);
		box-shadow: 0 8px 20px rgba(0, 0, 0, 0.4);
	}

	.milestone-node.active {
		border-color: #eab308;
		background: linear-gradient(180deg, rgba(234, 179, 8, 0.15) 0%, rgba(15, 23, 42, 0.9) 100%);
		box-shadow: 0 0 15px rgba(234, 179, 8, 0.25);
	}

	.milestone-node.ready {
		border-color: #10b981;
		background: linear-gradient(180deg, rgba(16, 185, 129, 0.2) 0%, rgba(15, 23, 42, 0.9) 100%);
		box-shadow: 0 0 20px rgba(16, 185, 129, 0.35);
		animation: readyPulse 2s infinite ease-in-out;
	}

	.milestone-node.opened {
		border-color: rgba(255, 255, 255, 0.06);
		opacity: 0.7;
	}

	.milestone-node.locked {
		opacity: 0.65;
	}

	.milestone-node.final-tier {
		border-color: rgba(234, 179, 8, 0.6);
		background: linear-gradient(180deg, rgba(217, 119, 6, 0.2) 0%, rgba(15, 23, 42, 0.9) 100%);
	}

	@keyframes readyPulse {
		0%,
		100% {
			border-color: #10b981;
			box-shadow: 0 0 15px rgba(16, 185, 129, 0.3);
		}
		50% {
			border-color: #34d399;
			box-shadow: 0 0 25px rgba(52, 211, 153, 0.6);
		}
	}

	.node-tier-label {
		display: inline-flex;
		align-items: center;
		gap: 4px;
		font-size: 0.65rem;
		font-weight: 800;
		color: #94a3b8;
		letter-spacing: 0.5px;
		margin-bottom: 6px;
	}

	.milestone-node.active .node-tier-label {
		color: #facc15;
	}
	.milestone-node.final-tier .node-tier-label {
		color: #ffd700;
	}

	:global(.crown-icon) {
		width: 12px;
		height: 12px;
		color: #ffd700;
	}

	.node-graphic {
		position: relative;
		width: 52px;
		height: 52px;
		display: flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 8px;
	}

	.bag-art {
		width: 46px;
		height: 46px;
		border-radius: 12px;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: transform 0.2s;
	}

	.milestone-node:hover .bag-art {
		transform: scale(1.08);
	}

	.satchel-art {
		background: linear-gradient(135deg, #78350f 0%, #451a03 100%);
		border: 1px solid rgba(217, 119, 6, 0.4);
		color: #fbbf24;
	}

	.chest-art {
		background: linear-gradient(135deg, #1e3a8a 0%, #172554 100%);
		border: 1px solid rgba(59, 130, 246, 0.4);
		color: #60a5fa;
	}

	.champion-art {
		background: linear-gradient(135deg, #854d0e 0%, #3b0764 100%);
		border: 1px solid rgba(234, 179, 8, 0.7);
		color: #ffd700;
		position: relative;
	}

	:global(.art-icon) {
		width: 24px;
		height: 24px;
	}

	:global(.art-icon.gold) {
		color: #ffd700;
		filter: drop-shadow(0 0 8px #f59e0b);
	}

	.node-status-badge {
		position: absolute;
		bottom: -2px;
		right: -2px;
		width: 20px;
		height: 20px;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		border: 2px solid #0f172a;
	}

	.node-status-badge.opened {
		background: #10b981;
		color: #ffffff;
	}

	.node-status-badge.ready {
		background: #eab308;
		color: #0f172a;
	}

	.node-status-badge.locked {
		background: #475569;
		color: #94a3b8;
	}

	:global(.status-icon) {
		width: 12px;
		height: 12px;
	}

	.node-info {
		display: flex;
		flex-direction: column;
		gap: 2px;
		margin-bottom: 8px;
		width: 100%;
	}

	.node-name {
		font-size: 0.75rem;
		font-weight: 700;
		color: #f1f5f9;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	.node-rp {
		font-size: 0.65rem;
		color: #94a3b8;
	}

	.node-rewards {
		display: flex;
		gap: 4px;
		justify-content: center;
		flex-wrap: wrap;
		margin-bottom: 6px;
	}

	.reward-mini {
		display: inline-flex;
		align-items: center;
		gap: 3px;
		font-size: 0.65rem;
		font-weight: 700;
		padding: 2px 5px;
		border-radius: 4px;
	}

	.reward-mini.silver {
		background: rgba(148, 163, 184, 0.15);
		color: #cbd5e1;
	}
	.reward-mini.gold {
		background: rgba(234, 179, 8, 0.15);
		color: #fde047;
	}
	.reward-mini.ticket {
		background: rgba(59, 130, 246, 0.15);
		color: #93c5fd;
	}

	:global(.mini-icon) {
		width: 10px;
		height: 10px;
	}

	.open-bag-btn {
		width: 100%;
		padding: 6px 4px;
		border-radius: 6px;
		background: linear-gradient(135deg, #10b981 0%, #059669 100%);
		color: #ffffff;
		border: none;
		font-size: 0.7rem;
		font-weight: 800;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 4px;
		box-shadow: 0 2px 8px rgba(16, 185, 129, 0.4);
		transition: transform 0.15s;
	}

	.open-bag-btn:hover:not(:disabled) {
		transform: scale(1.03);
	}

	.node-opened-tag {
		font-size: 0.65rem;
		font-weight: 700;
		color: #10b981;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 3px;
	}

	:global(.tag-icon) {
		width: 12px;
		height: 12px;
	}

	.node-progress-strip {
		width: 100%;
		height: 4px;
		background: rgba(255, 255, 255, 0.1);
		border-radius: 999px;
		overflow: hidden;
	}

	.strip-fill {
		height: 100%;
		background: #eab308;
		border-radius: 999px;
	}

	/* ========================================================================= */
	/* DAILY OBJECTIVES GRID                                                     */
	/* ========================================================================= */
	.quests-section {
		margin-bottom: 36px;
	}

	.section-title-bar {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 20px;
		flex-wrap: wrap;
		gap: 12px;
	}

	.title-with-badge {
		display: flex;
		align-items: center;
		gap: 10px;
	}

	:global(.section-flame-icon) {
		width: 24px;
		height: 24px;
		color: #f97316;
	}

	.section-title {
		font-size: 1.35rem;
		font-weight: 800;
		color: #ffffff;
		margin: 0;
	}

	.reset-timer-tag {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 0.8rem;
		color: #94a3b8;
		background: rgba(15, 23, 42, 0.6);
		padding: 6px 12px;
		border-radius: 8px;
		border: 1px solid rgba(255, 255, 255, 0.08);
	}

	:global(.clock-icon) {
		width: 14px;
		height: 14px;
		color: #38bdf8;
	}

	.quests-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
		gap: 20px;
	}

	.quest-card {
		background: #0f172a;
		border: 1px solid rgba(255, 255, 255, 0.1);
		border-radius: 16px;
		padding: 22px;
		display: flex;
		flex-direction: column;
		justify-content: space-between;
		position: relative;
		transition: all 0.25s ease;
	}

	.quest-card:hover {
		border-color: rgba(234, 179, 8, 0.4);
		transform: translateY(-2px);
		box-shadow: 0 12px 24px -10px rgba(0, 0, 0, 0.5);
	}

	.quest-card.completed {
		border-color: rgba(234, 179, 8, 0.5);
		background: linear-gradient(180deg, #131d36 0%, #0f172a 100%);
	}

	.quest-card.claimed {
		opacity: 0.75;
		border-color: rgba(16, 185, 129, 0.3);
	}

	.quest-top {
		display: flex;
		align-items: flex-start;
		gap: 14px;
		margin-bottom: 12px;
	}

	.quest-icon-badge {
		width: 44px;
		height: 44px;
		border-radius: 12px;
		background: rgba(255, 255, 255, 0.05);
		border: 1px solid rgba(255, 255, 255, 0.1);
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	:global(.quest-icon) {
		width: 22px;
		height: 22px;
		color: #eab308;
	}

	.quest-meta {
		flex: 1;
	}

	.category-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 8px;
		margin-bottom: 4px;
	}

	.category-badge {
		font-size: 0.7rem;
		text-transform: uppercase;
		font-weight: 800;
		letter-spacing: 0.5px;
		color: #38bdf8;
	}

	.rp-badge {
		display: inline-flex;
		align-items: center;
		gap: 3px;
		font-size: 0.7rem;
		font-weight: 800;
		background: rgba(234, 179, 8, 0.15);
		color: #facc15;
		padding: 2px 7px;
		border-radius: 6px;
		border: 1px solid rgba(234, 179, 8, 0.3);
	}

	:global(.rp-icon) {
		width: 12px;
		height: 12px;
	}

	.quest-title {
		font-size: 1.1rem;
		font-weight: 700;
		color: #f8fafc;
		margin: 0;
	}

	.quest-desc {
		font-size: 0.88rem;
		color: #94a3b8;
		line-height: 1.45;
		margin: 0 0 18px 0;
		min-height: 38px;
	}

	.progress-section {
		margin-bottom: 18px;
	}

	.progress-labels {
		display: flex;
		justify-content: space-between;
		font-size: 0.85rem;
		color: #94a3b8;
		margin-bottom: 8px;
		font-weight: 600;
	}

	.count-text {
		color: #f8fafc;
	}

	.progress-track {
		width: 100%;
		height: 8px;
		background: rgba(255, 255, 255, 0.08);
		border-radius: 999px;
		overflow: hidden;
	}

	.progress-fill {
		height: 100%;
		background: linear-gradient(90deg, #eab308, #f59e0b);
		border-radius: 999px;
		transition: width 0.4s ease;
	}

	.quest-card.completed .progress-fill {
		background: linear-gradient(90deg, #10b981, #34d399);
	}

	.rewards-section {
		display: flex;
		align-items: center;
		gap: 10px;
		margin-bottom: 18px;
		padding-top: 14px;
		border-top: 1px solid rgba(255, 255, 255, 0.06);
	}

	.rewards-label {
		font-size: 0.8rem;
		color: #64748b;
		font-weight: 600;
	}

	.reward-badges {
		display: flex;
		gap: 6px;
		flex-wrap: wrap;
	}

	.reward-badge {
		display: inline-flex;
		align-items: center;
		gap: 4px;
		font-size: 0.75rem;
		font-weight: 700;
		padding: 3px 8px;
		border-radius: 6px;
	}

	.reward-badge.silver {
		background: rgba(148, 163, 184, 0.15);
		color: #cbd5e1;
		border: 1px solid rgba(148, 163, 184, 0.3);
	}

	.reward-badge.gold {
		background: rgba(234, 179, 8, 0.15);
		color: #fde047;
		border: 1px solid rgba(234, 179, 8, 0.4);
	}

	.reward-badge.ticket {
		background: rgba(59, 130, 246, 0.15);
		color: #93c5fd;
		border: 1px solid rgba(59, 130, 246, 0.4);
	}

	.reward-badge.reputation {
		background: rgba(245, 158, 11, 0.15);
		color: #fbbf24;
		border: 1px solid rgba(245, 158, 11, 0.4);
	}

	:global(.badge-icon) {
		width: 13px;
		height: 13px;
	}

	.action-btn {
		width: 100%;
		padding: 10px 16px;
		border-radius: 10px;
		font-weight: 700;
		font-size: 0.9rem;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		transition: all 0.2s ease;
		border: none;
	}

	.action-btn.claim-active {
		background: linear-gradient(135deg, #eab308 0%, #ca8a04 100%);
		color: #0f172a;
		box-shadow: 0 4px 15px rgba(234, 179, 8, 0.35);
	}

	.action-btn.claim-active:hover {
		transform: translateY(-1px);
		box-shadow: 0 6px 20px rgba(234, 179, 8, 0.5);
	}

	.action-btn.claimed {
		background: rgba(16, 185, 129, 0.1);
		border: 1px solid rgba(16, 185, 129, 0.3);
		color: #34d399;
		cursor: default;
	}

	.in-progress-badge {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 6px;
		padding: 9px;
		border-radius: 8px;
		background: rgba(255, 255, 255, 0.04);
		color: #64748b;
		font-size: 0.8rem;
		font-weight: 600;
	}

	:global(.btn-icon) {
		width: 16px;
		height: 16px;
	}

	:global(.spin-icon) {
		animation: spin 1s linear infinite;
	}

	@keyframes spin {
		from {
			transform: rotate(0deg);
		}
		to {
			transform: rotate(360deg);
		}
	}

	/* ========================================================================= */
	/* SIMULATION PANEL                                                          */
	/* ========================================================================= */
	.simulation-panel {
		background: rgba(30, 41, 59, 0.5);
		border: 1px dashed rgba(234, 179, 8, 0.4);
		border-radius: 16px;
		padding: 22px;
	}

	.sim-header {
		display: flex;
		align-items: center;
		gap: 12px;
		margin-bottom: 16px;
	}

	:global(.sim-icon) {
		width: 22px;
		height: 22px;
		color: #eab308;
	}

	.sim-header h4 {
		margin: 0;
		font-size: 1rem;
		color: #f8fafc;
	}

	.sim-header p {
		margin: 2px 0 0 0;
		font-size: 0.82rem;
		color: #94a3b8;
	}

	.sim-buttons {
		display: flex;
		gap: 10px;
		flex-wrap: wrap;
	}

	.sim-btn {
		padding: 8px 14px;
		border-radius: 8px;
		background: rgba(255, 255, 255, 0.08);
		border: 1px solid rgba(255, 255, 255, 0.15);
		color: #e2e8f0;
		font-size: 0.82rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.2s;
	}

	.sim-btn:hover:not(:disabled) {
		background: rgba(234, 179, 8, 0.2);
		border-color: #eab308;
		color: #ffd700;
	}

	.sim-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	/* ========================================================================= */
	/* MODALS                                                                    */
	/* ========================================================================= */
	.modal-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.75);
		backdrop-filter: blur(8px);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 999;
		padding: 20px;
		animation: fadeIn 0.2s ease;
	}

	.celebration-card {
		background: radial-gradient(ellipse at top, #1e293b 0%, #090d16 100%);
		border: 2px solid #eab308;
		border-radius: 24px;
		padding: 36px 30px;
		max-width: 440px;
		width: 100%;
		text-align: center;
		position: relative;
		box-shadow: 0 0 50px rgba(234, 179, 8, 0.4);
		animation: popIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
	}

	.details-card {
		background: radial-gradient(ellipse at top, #1e293b 0%, #090d16 100%);
		border: 1px solid rgba(255, 255, 255, 0.2);
		border-radius: 20px;
		padding: 30px 26px;
		max-width: 420px;
		width: 100%;
		position: relative;
		box-shadow: 0 20px 40px rgba(0, 0, 0, 0.6);
		animation: popIn 0.25s ease;
	}

	.close-modal-btn {
		position: absolute;
		top: 14px;
		right: 14px;
		background: rgba(255, 255, 255, 0.1);
		border: none;
		border-radius: 50%;
		width: 32px;
		height: 32px;
		display: flex;
		align-items: center;
		justify-content: center;
		color: #94a3b8;
		cursor: pointer;
		transition: all 0.2s;
	}

	.close-modal-btn:hover {
		background: rgba(255, 255, 255, 0.2);
		color: #ffffff;
	}

	:global(.x-icon) {
		width: 18px;
		height: 18px;
	}

	.chest-glow-icon {
		position: relative;
		width: 80px;
		height: 80px;
		margin: 0 auto 16px;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	:global(.loot-icon) {
		width: 52px;
		height: 52px;
		color: #ffd700;
		filter: drop-shadow(0 0 15px rgba(234, 179, 8, 0.8));
	}

	.burst-glow {
		position: absolute;
		inset: -10px;
		background: radial-gradient(circle, rgba(234, 179, 8, 0.4) 0%, transparent 70%);
		border-radius: 50%;
		animation: pulseGlow 2s infinite ease-in-out;
	}

	@keyframes pulseGlow {
		0%,
		100% {
			transform: scale(0.9);
			opacity: 0.6;
		}
		50% {
			transform: scale(1.15);
			opacity: 1;
		}
	}

	.celebration-title {
		font-size: 1.5rem;
		font-weight: 800;
		color: #ffffff;
		margin: 0 0 6px 0;
	}

	.celebration-subtitle {
		font-size: 0.9rem;
		color: #cbd5e1;
		margin: 0 0 24px 0;
	}

	.loot-prizes-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
		gap: 12px;
		margin-bottom: 24px;
	}

	.loot-item {
		background: rgba(15, 23, 42, 0.8);
		border-radius: 12px;
		padding: 14px 10px;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 4px;
		border: 1px solid rgba(255, 255, 255, 0.1);
	}

	.loot-item.silver {
		border-color: rgba(148, 163, 184, 0.4);
		color: #cbd5e1;
	}
	.loot-item.gold {
		border-color: rgba(234, 179, 8, 0.5);
		color: #fde047;
	}
	.loot-item.ticket {
		border-color: rgba(59, 130, 246, 0.5);
		color: #93c5fd;
	}
	.loot-item.item {
		border-color: rgba(168, 85, 247, 0.5);
		color: #c084fc;
	}

	:global(.loot-badge-icon) {
		width: 22px;
		height: 22px;
		margin-bottom: 2px;
	}

	.loot-qty {
		font-size: 1.15rem;
		font-weight: 800;
	}

	.loot-name {
		font-size: 0.72rem;
		color: #94a3b8;
	}

	.claim-loot-btn {
		width: 100%;
		padding: 12px;
		border-radius: 12px;
		background: linear-gradient(135deg, #eab308 0%, #ca8a04 100%);
		color: #0f172a;
		font-size: 1rem;
		font-weight: 800;
		border: none;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		box-shadow: 0 4px 20px rgba(234, 179, 8, 0.4);
		transition: all 0.2s;
	}

	.claim-loot-btn:hover {
		transform: translateY(-2px);
		box-shadow: 0 6px 25px rgba(234, 179, 8, 0.6);
	}

	/* Details card */
	.details-top {
		text-align: center;
		margin-bottom: 20px;
	}

	.details-badge {
		display: inline-block;
		font-size: 0.75rem;
		font-weight: 800;
		color: #facc15;
		text-transform: uppercase;
		letter-spacing: 0.5px;
		margin-bottom: 6px;
	}

	.details-title {
		font-size: 1.35rem;
		font-weight: 800;
		color: #ffffff;
		margin: 0 0 6px 0;
	}

	.details-req {
		font-size: 0.85rem;
		color: #94a3b8;
	}

	.details-rewards-box {
		background: rgba(15, 23, 42, 0.7);
		border-radius: 12px;
		padding: 16px;
		margin-bottom: 20px;
	}

	.details-rewards-box h4 {
		margin: 0 0 12px 0;
		font-size: 0.85rem;
		color: #cbd5e1;
		font-weight: 700;
	}

	.reward-badges-large {
		display: flex;
		flex-direction: column;
		gap: 8px;
	}

	.reward-large {
		display: flex;
		align-items: center;
		gap: 10px;
		padding: 8px 12px;
		border-radius: 8px;
		font-size: 0.88rem;
		font-weight: 700;
	}

	.reward-large.silver {
		background: rgba(148, 163, 184, 0.15);
		color: #e2e8f0;
	}
	.reward-large.gold {
		background: rgba(234, 179, 8, 0.15);
		color: #fde047;
	}
	.reward-large.ticket {
		background: rgba(59, 130, 246, 0.15);
		color: #93c5fd;
	}
	.reward-large.item {
		background: rgba(168, 85, 247, 0.15);
		color: #d8b4fe;
	}

	:global(.large-icon) {
		width: 18px;
		height: 18px;
	}

	.open-bag-btn.large {
		padding: 12px 20px;
		font-size: 0.95rem;
		border-radius: 10px;
	}

	.status-text {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		padding: 12px;
		border-radius: 10px;
		font-size: 0.9rem;
		font-weight: 700;
	}

	.status-text.opened {
		background: rgba(16, 185, 129, 0.15);
		color: #34d399;
	}
	.status-text.active {
		background: rgba(234, 179, 8, 0.15);
		color: #fde047;
	}
	.status-text.locked {
		background: rgba(100, 116, 139, 0.15);
		color: #94a3b8;
	}

	:global(.text-icon) {
		width: 18px;
		height: 18px;
	}

	@keyframes fadeIn {
		from {
			opacity: 0;
		}
		to {
			opacity: 1;
		}
	}

	@keyframes popIn {
		from {
			transform: scale(0.92);
			opacity: 0;
		}
		to {
			transform: scale(1);
			opacity: 1;
		}
	}
</style>
