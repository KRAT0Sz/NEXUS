<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { gamification } from '#lib/api/gamification.svelte';
	import { auth } from '#lib/auth.svelte';
	import { t } from '#lib/i18n/index.svelte';
	import { toast } from '#lib/toast.svelte';
	import type { LuckySpinWheelSliceDTO } from '#lib/types';
	import {
		Dices,
		Sparkles,
		Ticket,
		Gift,
		ChevronDown
	} from 'lucide-svelte';

	// Authentic 6 sectors matching Garena GAS Mobile Heroes of Newerth Lucky Draw
	const DEFAULT_SLICES: LuckySpinWheelSliceDTO[] = [
		{ sliceIndex: 0, label: 'AVATAR', rewardType: 'Avatar', amount: 1, upgradeCode: 'avatar_skin', colorHex: '#f98e54' },
		{ sliceIndex: 1, label: 'COIN', rewardType: 'Coin', amount: 150, upgradeCode: null, colorHex: '#cca5df' },
		{ sliceIndex: 2, label: 'WARD', rewardType: 'Ward', amount: 1, upgradeCode: 'ward_skin', colorHex: '#7dd8db' },
		{ sliceIndex: 3, label: 'THANKS!', rewardType: 'Thanks', amount: 25, upgradeCode: null, colorHex: '#fde08b' },
		{ sliceIndex: 4, label: 'COURIER', rewardType: 'Courier', amount: 1, upgradeCode: 'courier_skin', colorHex: '#e86b79' },
		{ sliceIndex: 5, label: 'ICON', rewardType: 'Icon', amount: 1, upgradeCode: 'icon_custom', colorHex: '#7dd8db' }
	];

	let rotationDegrees = $state<number>(0);
	let isSpinning = $state<boolean>(false);
	let winningResult = $state<string | null>(null);
	let countdownSeconds = $state<number>(0);
	let timerInterval: ReturnType<typeof setInterval> | null = null;
	let remindMe = $state<boolean>(true);

	// Always guarantee the 6 slices are available even if API hasn't loaded or returned null
	const activeSlices = $derived(
		gamification.spinStatus?.wheelSlices && gamification.spinStatus.wheelSlices.length === 6
			? gamification.spinStatus.wheelSlices
			: DEFAULT_SLICES
	);

	// Derived readiness: when countdown reaches 0 or canFreeSpin is true, center "หมุน" button is displayed
	const isFreeSpinReady = $derived.by(() => {
		if (gamification.spinStatus && !gamification.spinStatus.canFreeSpin && countdownSeconds > 0) {
			return false;
		}
		return true;
	});

	function handleToggleReminder() {
		if (remindMe) {
			toast.success('ตั้งเตือนเมื่อถึงโอกาสหมุนครั้งถัดไปเรียบร้อยแล้ว');
			if (typeof window !== 'undefined' && 'Notification' in window && Notification.permission === 'default') {
				Notification.requestPermission();
			}
		}
	}

	function updateCountdown() {
		if (gamification.spinStatus?.nextFreeSpinAvailableAt) {
			const target = new Date(gamification.spinStatus.nextFreeSpinAvailableAt).getTime();
			const now = Date.now();
			const diff = Math.max(0, Math.floor((target - now) / 1000));
			countdownSeconds = diff;
			if (diff <= 0 && !gamification.spinStatus.canFreeSpin) {
				gamification.spinStatus.canFreeSpin = true;
			}
		} else if (gamification.spinStatus?.canFreeSpin) {
			countdownSeconds = 0;
		} else if (gamification.spinStatus?.cooldownRemainingSeconds !== undefined && gamification.spinStatus.cooldownRemainingSeconds > 0) {
			countdownSeconds = gamification.spinStatus.cooldownRemainingSeconds;
		} else {
			countdownSeconds = 0;
		}
	}

	$effect(() => {
		if (gamification.spinStatus) {
			updateCountdown();
		}
	});

	onMount(async () => {
		if (auth.isAuthenticated) {
			await gamification.getSpinStatus();
			updateCountdown();
		}

		timerInterval = setInterval(() => {
			if (countdownSeconds > 0) {
				countdownSeconds -= 1;
				if (countdownSeconds <= 0) {
					countdownSeconds = 0;
					if (gamification.spinStatus) {
						gamification.spinStatus.canFreeSpin = true;
					}
				}
			}
		}, 1000);
	});

	onDestroy(() => {
		if (timerInterval) {
			clearInterval(timerInterval);
		}
	});

	function formatCountdown(totalSec: number): string {
		const hours = Math.floor(totalSec / 3600);
		const minutes = Math.floor((totalSec % 3600) / 60);
		const seconds = totalSec % 60;
		const pad = (n: number) => n.toString().padStart(2, '0');
		return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
	}

	function handleCenterHubClick() {
		if (isSpinning) return;
		if (isFreeSpinReady) {
			handleSpin();
		} else if ((gamification.spinStatus?.availableTickets ?? 0) >= 1) {
			handleSpin();
		} else {
			toast.info(`โอกาสหมุนฟรีครั้งถัดไปในอีก ${formatCountdown(countdownSeconds)}`);
		}
	}

	async function handleSpin() {
		if (isSpinning) return;

		const res = await gamification.playSpin();
		if (!res.ok || !res.data) {
			toast.error(res.error || 'Spin failed');
			return;
		}

		const winningIndex = res.data.winningSliceIndex;
		winningResult = null;
		isSpinning = true;

		// 6 slices = 60 degrees per slice.
		// Slice 0 center is at 0 deg (top / 12 o'clock).
		const sliceAngle = 60;
		const extraTurns = 5 * 360;
		// Pointer needle is at top (0 deg). To point to slice N, wheel rotates so that slice N is at top.
		const targetAngle = 360 - (winningIndex * sliceAngle);
		const currentBase = Math.floor(rotationDegrees / 360) * 360;
		// Small natural jitter within the slice (-12 to +12 deg)
		const jitter = (Math.random() - 0.5) * 24;
		rotationDegrees = currentBase + extraTurns + targetAngle + jitter;

		// Animation takes 4.2 seconds
		setTimeout(() => {
			isSpinning = false;
			winningResult = res.data?.rewardLabel ?? 'Reward';
			toast.success(res.data?.message || `You won ${winningResult}!`);
			updateCountdown();
		}, 4200);
	}
</script>

<div class="minigames-container">
	<div class="page-header">
		<div class="header-icon-wrapper">
			<Dices class="header-icon" />
		</div>
		<div>
			<h1 class="page-title">{t('minigames.title')}</h1>
			<p class="page-subtitle">{t('minigames.subtitle')}</p>
		</div>
	</div>

	{#if !auth.isAuthenticated}
		<div class="login-prompt">
			<p>Please log in to spin the Lucky Wheel.</p>
			<a href="/login" class="login-btn">Log In</a>
		</div>
	{:else}
		<!-- GAS HoN Lucky Wheel of Fortune Section -->
		<div class="gas-lucky-draw-card">
			<!-- GAS App Top Red Navigation Bar -->
			<div class="gas-nav-bar">
				<div class="nav-left">
					<span class="back-arrow">←</span>
					<span class="nav-title">ลุ้นโชค</span>
				</div>
			</div>

			<!-- GAS App Game Selector Sub-bar -->
			<div class="gas-sub-bar">
				<div class="game-selector">
					<span class="game-name">Heroes of Newerth</span>
					<ChevronDown class="selector-arrow" />
				</div>
				<button class="rewards-info-btn" title="ของรางวัล">
					<Gift class="gift-icon" />
				</button>
			</div>

			<!-- Wheel Stage Container -->
			<div class="gas-wheel-wrapper">
				<div class="wheel-stage">
					<!-- Rotating SVG Wheel (Outer circle with 6 slices) -->
					<div
						class="wheel-disc-wrapper"
						style="transform: rotate({rotationDegrees}deg); transition: transform {isSpinning ? '4.2s cubic-bezier(0.12, 0.95, 0.2, 1)' : 'none'};"
					>
						<svg class="wheel-svg" viewBox="0 0 400 400">
							<defs>
								<filter id="soft-shadow" x="-20%" y="-20%" width="140%" height="140%">
									<feDropShadow dx="0" dy="2" stdDeviation="3" flood-color="#000000" flood-opacity="0.25"/>
								</filter>
							</defs>

							<!-- Outer Rim -->
							<circle cx="200" cy="200" r="197" fill="#ffffff" stroke="#e2e8f0" stroke-width="6"/>

							<!-- 6 Pastel Slices (60 deg each, slice 0 at top / -90 deg) -->
							{#each activeSlices as slice, i}
								{@const midDeg = i * 60 - 90}
								{@const startRad = (midDeg - 30) * (Math.PI / 180)}
								{@const endRad = (midDeg + 30) * (Math.PI / 180)}
								{@const x1 = 200 + 192 * Math.cos(startRad)}
								{@const y1 = 200 + 192 * Math.sin(startRad)}
								{@const x2 = 200 + 192 * Math.cos(endRad)}
								{@const y2 = 200 + 192 * Math.sin(endRad)}
								{@const midRad = midDeg * (Math.PI / 180)}
								{@const tx = 200 + 132 * Math.cos(midRad)}
								{@const ty = 200 + 132 * Math.sin(midRad)}
								{@const rotDeg = midDeg + 90}

								<!-- Sector Arc -->
								<path
									d="M 200 200 L {x1} {y1} A 192 192 0 0 1 {x2} {y2} Z"
									fill={slice.colorHex}
									stroke="#ffffff"
									stroke-width="2.5"
								/>

								<!-- Item Artwork + Badge (Scaled up to maximum sector capacity) -->
								<g transform="translate({tx}, {ty}) rotate({rotDeg})" filter="url(#soft-shadow)">
									{#if i === 0}
										<!-- AVATAR (Top) -->
										<g transform="scale(2.1)">
											<g transform="translate(0, -9)">
												<path d="M 0 -17 L 15 -9 L 13 9 L 0 17 L -13 9 L -15 -9 Z" fill="#b45309" stroke="#fef08a" stroke-width="1.5" />
												<path d="M 0 -14 L 12 -7 L 10 7 L 0 14 L -10 7 L -12 -7 Z" fill="#d97706" />
												<path d="M -7 -5 L 7 -5 L 5 4 L 0 7 L -5 4 Z" fill="#1e1b4b" />
												<circle cx="-3" cy="-1" r="1.5" fill="#fde047" />
												<circle cx="3" cy="-1" r="1.5" fill="#fde047" />
												<!-- NEW badge -->
												<rect x="7" y="-20" width="18" height="10" rx="3" fill="#ef4444" stroke="#ffffff" stroke-width="1"/>
												<text x="16" y="-12" fill="#ffffff" font-size="7" font-weight="900" text-anchor="middle">NEW</text>
											</g>
											<g transform="translate(0, 13)">
												<rect x="-30" y="-7" width="60" height="15" rx="3" fill="#1e1b4b" stroke="#eab308" stroke-width="1.5" />
												<text x="0" y="4.5" fill="#fde047" font-size="9.5" font-weight="900" text-anchor="middle" letter-spacing="0.5">AVATAR</text>
											</g>
										</g>
									{:else if i === 1}
										<!-- COIN (Top-Right) -->
										<g transform="scale(2.1)">
											<g transform="translate(0, -9)">
												<circle cx="-6" cy="2" r="11" fill="#94a3b8" stroke="#f1f5f9" stroke-width="1.5" />
												<circle cx="-6" cy="2" r="8" fill="#cbd5e1" />
												<circle cx="6" cy="-2" r="12" fill="#d97706" stroke="#fef08a" stroke-width="1.5" />
												<circle cx="6" cy="-2" r="9" fill="#f59e0b" />
												<polygon points="6,-6 7.5,-2 11,-2 8,0.5 9,4 6,2 3,4 4,0.5 1,-2 4.5,-2" fill="#fef08a" />
											</g>
											<g transform="translate(0, 13)">
												<rect x="-26" y="-7" width="52" height="15" rx="3" fill="#1e1b4b" stroke="#f1f5f9" stroke-width="1.5" />
												<text x="0" y="4.5" fill="#ffffff" font-size="9.5" font-weight="900" text-anchor="middle" letter-spacing="0.5">COIN</text>
											</g>
										</g>
									{:else if i === 2}
										<!-- WARD (Bottom-Right) -->
										<g transform="scale(2.1)">
											<g transform="translate(0, -9)">
												<path d="M -3 14 C -6 6, -10 -4, -3 -12 C 4 -12, 10 -4, 4 8 Z" fill="#047857" stroke="#6ee7b7" stroke-width="1.5" />
												<path d="M -6 4 C -12 2, -14 -6, -8 -8 C -4 -4, -3 0, -6 4 Z" fill="#10b981" />
												<path d="M 5 6 C 11 4, 13 -4, 7 -6 C 3 -2, 2 2, 5 6 Z" fill="#34d399" />
												<ellipse cx="0" cy="-3" rx="7" ry="5" fill="#fef08a" stroke="#047857" stroke-width="1" />
												<circle cx="0" cy="-3" r="2.5" fill="#065f46" />
											</g>
											<g transform="translate(0, 13)">
												<rect x="-26" y="-7" width="52" height="15" rx="3" fill="#064e3b" stroke="#34d399" stroke-width="1.5" />
												<text x="0" y="4.5" fill="#a7f3d0" font-size="9.5" font-weight="900" text-anchor="middle" letter-spacing="0.5">WARD</text>
											</g>
										</g>
									{:else if i === 3}
										<!-- THANKS! (Bottom) - Official GAS HoN Sticker from /images/luckywheel/thanks.png -->
										<image
											href="/images/luckywheel/thanks.png"
											xlink:href="/images/luckywheel/thanks.png"
											x="-67"
											y="-67"
											width="134"
											height="134"
											preserveAspectRatio="xMidYMid meet"
										/>
									{:else if i === 4}
										<!-- COURIER (Bottom-Left) -->
										<g transform="scale(2.1)">
											<g transform="translate(0, -9)">
												<path d="M -7 -4 C -15 -12, -18 2, -7 4 Z" fill="#e2e8f0" stroke="#94a3b8" stroke-width="1"/>
												<path d="M 7 -4 C 15 -12, 18 2, 7 4 Z" fill="#e2e8f0" stroke="#94a3b8" stroke-width="1"/>
												<rect x="-9" y="-6" width="18" height="13" rx="2" fill="#78350f" stroke="#fde047" stroke-width="1.5" />
												<line x1="-9" y1="0" x2="9" y2="0" stroke="#fde047" stroke-width="1.5" />
												<rect x="-2" y="-2" width="4" height="4" fill="#fef08a" />
											</g>
											<g transform="translate(0, 13)">
												<rect x="-32" y="-7" width="64" height="15" rx="3" fill="#450a0a" stroke="#f87171" stroke-width="1.5" />
												<text x="0" y="4.5" fill="#fecaca" font-size="9" font-weight="900" text-anchor="middle" letter-spacing="0.5">COURIER</text>
											</g>
										</g>
									{:else if i === 5}
										<!-- ICON (Top-Left) - Official GAS HoN Sticker from /images/luckywheel/icon.png -->
										<image
											href="/images/luckywheel/icon.png"
											xlink:href="/images/luckywheel/icon.png"
											x="-64"
											y="-64"
											width="128"
											height="128"
											preserveAspectRatio="xMidYMid meet"
										/>
									{/if}
								</g>
							{/each}
						</svg>
					</div>

					<!-- Center Hub Button with Needle Pointer Pointing UP -->
					<button
						class="center-hub-btn"
						class:can-spin={isFreeSpinReady && !isSpinning}
						class:spinning={isSpinning}
						disabled={isSpinning}
						onclick={handleCenterHubClick}
						aria-label={isFreeSpinReady ? 'หมุนวงล้อ' : 'เวลานับถอยหลังโอกาสถัดไป'}
					>
						<!-- Pointer Needle pointing UP into the top slice -->
						<div class="needle-pointer" class:needle-ready={isFreeSpinReady && !isSpinning}></div>

						{#if isSpinning}
							<span class="hub-spinning-text">กำลังหมุน...</span>
						{:else if isFreeSpinReady}
							<div class="hub-ready-content">
								<span class="hub-spin-text">หมุน</span>
								<span class="hub-spin-subtext">ฟรี</span>
							</div>
						{:else}
							<div class="hub-cooldown-content">
								<span class="hub-sublabel">โอกาสหน้า</span>
								<span class="hub-timer">{formatCountdown(countdownSeconds)}</span>
							</div>
						{/if}
					</button>
				</div>
			</div>

			<!-- GAS App "เตือนฉันเมื่อมีโอกาสอีกครั้ง" Checkbox -->
			<div class="gas-reminder-container">
				<button class="gas-reminder-btn" onclick={() => { remindMe = !remindMe; handleToggleReminder(); }}>
					<div class="checkbox-circle" class:checked={remindMe}>
						{#if remindMe}
							<span class="check-mark">✔</span>
						{/if}
					</div>
					<span class="reminder-text">เตือนฉันเมื่อมีโอกาสอีกครั้ง</span>
				</button>
			</div>

			<!-- Result Announcement & Extra Ticket Spin Controls -->
			<div class="spin-controls">
				{#if winningResult}
					<div class="result-banner">
						{#if winningResult === 'THANKS!'}
							<img src="/images/luckywheel/thanks.png" alt="Thanks!" class="winner-sticker" />
						{:else if winningResult === 'ICON'}
							<img src="/images/luckywheel/icon.png" alt="Icon!" class="winner-sticker" />
						{:else}
							<Sparkles class="banner-icon" />
						{/if}
						<span>{t('minigames.congratulations')} <strong>{winningResult}</strong></span>
					</div>
				{/if}

				<!-- If cooldown is active (free spin not ready) -->
				{#if !isFreeSpinReady}
					{#if (gamification.spinStatus?.availableTickets ?? 0) > 0}
						<div class="ticket-spin-row">
							<button
								class="spin-action-btn ticket"
								disabled={isSpinning}
								onclick={handleSpin}
							>
								<Ticket class="btn-icon" />
								{isSpinning ? t('minigames.spinning') : 'หมุนด้วยตั๋ว (ใช้ 1 ใบ)'}
							</button>

							<div class="ticket-balance-badge">
								<Ticket class="mini-icon" />
								<span>{gamification.spinStatus?.availableTickets ?? 0} {t('minigames.ticketBalance')}</span>
							</div>
						</div>
					{:else}
						<div class="ticket-empty-info">
							<Ticket class="mini-icon" />
							<span>ตั๋วหมุน: 0 ใบ (ทำเควสต์หรือเช็คอินเพื่อรับตั๋วเพิ่ม)</span>
						</div>
					{/if}
				{:else if (gamification.spinStatus?.availableTickets ?? 0) > 0}
					<div class="ticket-balance-badge">
						<Ticket class="mini-icon" />
						<span>มีตั๋วสำรอง {gamification.spinStatus?.availableTickets} ใบ</span>
					</div>
				{/if}

				<p class="spins-counter">
					{t('minigames.totalSpins')}: <strong>{gamification.spinStatus?.totalSpinsPlayed ?? 0}</strong>
				</p>
			</div>
		</div>
	{/if}
</div>

<style>
	.minigames-container {
		max-width: 1000px;
		margin: 0 auto;
		padding: 40px 20px 80px;
	}

	.page-header {
		display: flex;
		align-items: center;
		gap: 20px;
		margin-bottom: 36px;
	}

	.header-icon-wrapper {
		width: 60px;
		height: 60px;
		border-radius: 16px;
		background: linear-gradient(135deg, rgba(234, 179, 8, 0.2), rgba(234, 179, 8, 0.05));
		border: 1px solid rgba(234, 179, 8, 0.4);
		display: flex;
		align-items: center;
		justify-content: center;
	}

	:global(.header-icon) {
		width: 32px;
		height: 32px;
		color: #eab308;
	}

	.page-title {
		font-size: 2rem;
		font-weight: 800;
		color: #fff;
		margin: 0 0 6px 0;
	}

	.page-subtitle {
		font-size: 1rem;
		color: #94a3b8;
		margin: 0;
	}

	.login-prompt {
		text-align: center;
		padding: 60px 20px;
		background: rgba(15, 23, 42, 0.6);
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

	:global(.mini-icon) {
		width: 12px;
		height: 12px;
	}

	/* GAS HoN Style Lucky Draw Card */
	.gas-lucky-draw-card {
		background: #ffffff;
		border-radius: 20px;
		overflow: hidden;
		box-shadow: 0 10px 30px rgba(0, 0, 0, 0.12);
		border: 1px solid #e2e8f0;
		max-width: 480px;
		margin: 0 auto;
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	:global(.dark) .gas-lucky-draw-card {
		background: #111827;
		border-color: #1f2937;
		box-shadow: 0 10px 30px rgba(0, 0, 0, 0.4);
	}

	.gas-nav-bar {
		width: 100%;
		background: #ef4444;
		padding: 14px 20px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		color: #ffffff;
	}

	.nav-left {
		display: flex;
		align-items: center;
		gap: 12px;
	}

	.back-arrow {
		font-size: 1.3rem;
		font-weight: 700;
	}

	.nav-title {
		font-size: 1.25rem;
		font-weight: 800;
		letter-spacing: 0.5px;
	}

	.gas-sub-bar {
		width: 100%;
		background: #f8fafc;
		border-bottom: 1px solid #e2e8f0;
		padding: 10px 20px;
		display: flex;
		align-items: center;
		justify-content: space-between;
	}

	:global(.dark) .gas-sub-bar {
		background: #1f2937;
		border-color: #374151;
	}

	.game-selector {
		display: flex;
		align-items: center;
		gap: 6px;
		font-size: 0.95rem;
		font-weight: 700;
		color: #ef4444;
	}

	:global(.selector-arrow) {
		width: 16px;
		height: 16px;
		color: #ef4444;
	}

	.rewards-info-btn {
		background: none;
		border: none;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 4px;
	}

	:global(.gift-icon) {
		width: 22px;
		height: 22px;
		color: #84cc16;
	}

	.gas-wheel-wrapper {
		padding: 30px 10px 15px;
		display: flex;
		justify-content: center;
		width: 100%;
	}

	.wheel-stage {
		position: relative;
		width: 360px;
		height: 360px;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.wheel-disc-wrapper {
		width: 100%;
		height: 100%;
	}

	.wheel-svg {
		width: 100%;
		height: 100%;
		filter: drop-shadow(0 6px 16px rgba(0, 0, 0, 0.12));
	}

	/* Center Hub Button & Pointer Needle */
	.center-hub-btn {
		position: absolute;
		top: 50%;
		left: 50%;
		transform: translate(-50%, -50%);
		width: 140px;
		height: 140px;
		border-radius: 50%;
		background: #f1f5f9;
		border: 6px solid #ffffff;
		box-shadow: 0 4px 14px rgba(0, 0, 0, 0.15);
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		cursor: default;
		z-index: 25;
		transition: all 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
		padding: 0;
	}

	:global(.dark) .center-hub-btn {
		background: #1e293b;
		border-color: #334155;
	}

	.center-hub-btn.can-spin {
		cursor: pointer;
		background: linear-gradient(145deg, #ef4444 0%, #dc2626 55%, #b91c1c 100%);
		border-color: #ffffff;
		box-shadow: 0 6px 20px rgba(220, 38, 38, 0.45), 0 0 0 4px rgba(239, 68, 68, 0.3);
		animation: pulse-ring 2.2s infinite;
	}

	:global(.dark) .center-hub-btn.can-spin {
		background: linear-gradient(145deg, #ef4444 0%, #dc2626 55%, #991b1b 100%);
		border-color: #f87171;
		box-shadow: 0 6px 24px rgba(239, 68, 68, 0.55), 0 0 0 4px rgba(239, 68, 68, 0.35);
	}

	.center-hub-btn.can-spin:hover {
		transform: translate(-50%, -50%) scale(1.06);
		box-shadow: 0 8px 28px rgba(220, 38, 38, 0.6), 0 0 0 6px rgba(239, 68, 68, 0.4);
	}

	.center-hub-btn.can-spin:active {
		transform: translate(-50%, -50%) scale(0.96);
	}

	.needle-pointer {
		position: absolute;
		top: -19px;
		left: 50%;
		transform: translateX(-50%);
		width: 0;
		height: 0;
		border-left: 14px solid transparent;
		border-right: 14px solid transparent;
		border-bottom: 22px solid #ffffff;
		filter: drop-shadow(0 -3px 3px rgba(0, 0, 0, 0.18));
		z-index: 30;
		transition: border-bottom-color 0.25s ease;
	}

	:global(.dark) .needle-pointer {
		border-bottom-color: #334155;
	}

	.needle-pointer.needle-ready {
		border-bottom-color: #ef4444;
	}

	:global(.dark) .needle-pointer.needle-ready {
		border-bottom-color: #ef4444;
	}

	.hub-ready-content {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		line-height: 1;
		gap: 2px;
	}

	.hub-spin-text {
		font-size: 2.2rem;
		font-weight: 900;
		color: #ffffff;
		letter-spacing: 2px;
		text-shadow: 0 2px 6px rgba(0, 0, 0, 0.35);
		font-family: inherit;
	}

	.hub-spin-subtext {
		font-size: 0.85rem;
		font-weight: 800;
		color: #fef08a;
		letter-spacing: 1.5px;
		text-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
		text-transform: uppercase;
	}

	.hub-cooldown-content {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.hub-sublabel {
		font-size: 0.85rem;
		font-weight: 700;
		color: #64748b;
		margin-bottom: 3px;
	}

	:global(.dark) .hub-sublabel {
		color: #94a3b8;
	}

	.hub-timer {
		font-size: 1.25rem;
		font-weight: 800;
		color: #0f172a;
		font-variant-numeric: tabular-nums;
		letter-spacing: 0.5px;
	}

	:global(.dark) .hub-timer {
		color: #f8fafc;
	}

	.hub-spinning-text {
		font-size: 1rem;
		font-weight: 800;
		color: #ffffff;
	}

	.ticket-empty-info {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 6px;
		font-size: 0.82rem;
		color: #94a3b8;
		font-weight: 600;
		padding: 8px 14px;
		background: rgba(148, 163, 184, 0.08);
		border-radius: 8px;
		margin-top: 6px;
	}

	@keyframes pulse-ring {
		0% {
			box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.5);
		}
		70% {
			box-shadow: 0 0 0 10px rgba(239, 68, 68, 0);
		}
		100% {
			box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
		}
	}

	/* GAS Reminder Checkbox */
	.gas-reminder-container {
		margin-top: 15px;
		margin-bottom: 15px;
	}

	.gas-reminder-btn {
		background: none;
		border: none;
		cursor: pointer;
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 0.95rem;
		font-weight: 600;
		color: #475569;
	}

	:global(.dark) .gas-reminder-btn {
		color: #94a3b8;
	}

	.checkbox-circle {
		width: 22px;
		height: 22px;
		border-radius: 50%;
		border: 2px solid #cbd5e1;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: all 0.2s;
	}

	.checkbox-circle.checked {
		background: #84cc16;
		border-color: #84cc16;
		color: #ffffff;
	}

	.check-mark {
		font-size: 12px;
		font-weight: 900;
		color: #ffffff;
	}

	.spin-controls {
		text-align: center;
		width: 100%;
		max-width: 400px;
		padding: 0 20px 24px;
	}

	.result-banner {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 10px;
		background: linear-gradient(90deg, rgba(234, 179, 8, 0.15), rgba(234, 179, 8, 0.05));
		border: 1px solid rgba(234, 179, 8, 0.4);
		border-radius: 12px;
		padding: 12px 20px;
		color: #fde047;
		font-size: 1rem;
		margin-bottom: 16px;
	}

	.winner-sticker {
		width: 36px;
		height: 27px;
		object-fit: contain;
		filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.25));
	}

	:global(.banner-icon) {
		width: 20px;
		height: 20px;
		color: #eab308;
	}

	.ticket-spin-row {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 8px;
		width: 100%;
		margin-top: 8px;
	}

	.spin-action-btn {
		width: 100%;
		padding: 14px 28px;
		border-radius: 12px;
		font-weight: 800;
		font-size: 1rem;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		border: none;
		transition: all 0.2s;
	}

	.spin-action-btn.ticket {
		background: linear-gradient(135deg, #eab308 0%, #ca8a04 100%);
		color: #0f172a;
		box-shadow: 0 4px 16px rgba(234, 179, 8, 0.35);
	}

	.spin-action-btn:hover:not(:disabled) {
		transform: translateY(-2px);
		box-shadow: 0 6px 20px rgba(234, 179, 8, 0.45);
	}

	.spin-action-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
		transform: none;
	}

	.ticket-balance-badge {
		display: flex;
		align-items: center;
		gap: 6px;
		font-size: 0.85rem;
		color: #64748b;
		font-weight: 700;
	}

	.spins-counter {
		margin-top: 14px;
		font-size: 0.85rem;
		color: #64748b;
	}

	.spins-counter strong {
		color: #94a3b8;
	}

	:global(.btn-icon) {
		width: 18px;
		height: 18px;
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
</style>
