<script lang="ts">
	import { onMount } from 'svelte';
	import { Trophy, Shield, Gamepad2, Award, Flame } from 'lucide-svelte';
	import { t } from '#lib/i18n/index.svelte';
	import { apiFetch } from '#lib/api';
	import type { LeaderboardItem, QuestLeaderboardItem } from '#lib/types';

	let activeTab = $state<'ranked' | 'casual' | 'quests'>('ranked');
	let questEntries = $state<QuestLeaderboardItem[]>([]);
	let loadingQuests = $state<boolean>(false);

	const rankedEntries: LeaderboardItem[] = [
		{ rank: 1, playerName: 'MoonMeander', clanTag: 'tdM', rating: 2045.2, wins: 342, losses: 85 },
		{ rank: 2, playerName: 'AngryTestie', clanTag: 'EZ', rating: 2012.8, wins: 412, losses: 120 },
		{ rank: 3, playerName: 'KheZu', clanTag: 'BM', rating: 1998.4, wins: 290, losses: 78 },
		{ rank: 4, playerName: 'Mynuts', clanTag: 'tdM', rating: 1982.0, wins: 250, losses: 80 },
		{ rank: 5, playerName: 'bkid', clanTag: '5', rating: 1965.5, wins: 310, losses: 115 },
		{ rank: 6, playerName: 'Tralfamadore', clanTag: 'coL', rating: 1954.2, wins: 280, losses: 105 },
		{ rank: 7, playerName: 'Scandal', clanTag: 'RoX', rating: 1941.0, wins: 220, losses: 92 },
		{ rank: 8, playerName: 'Sender', clanTag: 'sG', rating: 1935.6, wins: 210, losses: 88 },
		{ rank: 9, playerName: 'slickz', clanTag: 'fntc', rating: 1928.1, wins: 195, losses: 75 },
		{ rank: 10, playerName: 'insania', clanTag: 'sG', rating: 1920.4, wins: 240, losses: 110 }
	];

	const casualEntries: LeaderboardItem[] = [
		{ rank: 1, playerName: 'TestieCasual', clanTag: 'EZ', rating: 1850.5, wins: 120, losses: 25 },
		{ rank: 2, playerName: 'MidWarsKing', clanTag: 'MW', rating: 1820.0, wins: 195, losses: 55 },
		{ rank: 3, playerName: 'CasualHero', clanTag: 'FUN', rating: 1795.3, wins: 140, losses: 48 }
	];

	async function loadQuestLeaderboard(): Promise<void> {
		if (loadingQuests) return;
		loadingQuests = true;
		try {
			const res = await apiFetch<QuestLeaderboardItem[]>('/Quests/Leaderboard?take=50');
			if (res.ok && res.data && res.data.length > 0) {
				questEntries = res.data;
			}
		} catch {
			// Retain existing or fallback
		} finally {
			loadingQuests = false;
		}
	}

	function switchTab(tab: 'ranked' | 'casual' | 'quests'): void {
		activeTab = tab;
		if (tab === 'quests' && questEntries.length === 0) {
			loadQuestLeaderboard();
		}
	}

	onMount(() => {
		loadQuestLeaderboard();
	});

	function getWinRate(wins: number, losses: number): number {
		const total = wins + losses;
		return total > 0 ? (wins / total) * 100 : 0;
	}

	const currentEntries = $derived(activeTab === 'ranked' ? rankedEntries : casualEntries);
</script>

<svelte:head>
	<title>{t('ladder.title')} - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	<!-- Page Header -->
	<header class="page-header animate-fade-in text-center">
		<div class="header-icon-wrap">
			<Trophy size={32} class="accent-icon" />
			<h1 class="section-header">{t('ladder.title')}</h1>
		</div>
		<p class="header-desc">
			The top Heroes Of Newerth competitors ranked by Season MMR and match performance.
		</p>
	</header>

	<!-- Tabs & Table Card -->
	<div class="card glass-card pa-4 mb-6 animate-fade-in-delay-1">
		<!-- Tab Switchers -->
		<div class="tabs-header">
			<button
				class="tab-btn"
				class:active={activeTab === 'ranked'}
				onclick={() => { switchTab('ranked'); }}
			>
				<Shield size={18} />
				<span>{t('ladder.gameModes.caldavar')}</span>
			</button>
			<button
				class="tab-btn"
				class:active={activeTab === 'casual'}
				onclick={() => { switchTab('casual'); }}
			>
				<Gamepad2 size={18} />
				<span>{t('ladder.gameModes.caldavarCasual')}</span>
			</button>
			<button
				class="tab-btn"
				class:active={activeTab === 'quests'}
				onclick={() => { switchTab('quests'); }}
			>
				<Flame size={18} />
				<span>{t('ladder.gameModes.warEffort')}</span>
			</button>
		</div>

		<!-- Table -->
		<div class="table-responsive">
			{#if activeTab === 'quests'}
				<table class="data-table">
					<thead>
						<tr>
							<th style="width: 90px;">Rank</th>
							<th>Player</th>
							<th style="width: 120px;">Clan</th>
							<th style="width: 160px; text-align: right;">Reputation</th>
							<th style="width: 140px; text-align: right;">Current Bag</th>
							<th style="width: 140px; text-align: right;">Completed</th>
						</tr>
					</thead>
					<tbody>
						{#if questEntries.length === 0}
							<tr>
								<td colspan="6" class="text-center" style="padding: 32px; opacity: 0.6;">
									{#if loadingQuests}
										Loading Quest Rankings...
									{:else}
										No quest rankings recorded for the current cycle yet.
									{/if}
								</td>
							</tr>
						{:else}
							{#each questEntries as item}
								<tr>
									<td data-label="Rank">
										{#if item.rank === 1}
											<span class="chip chip-gold">
												<Award size={14} /> #1
											</span>
										{:else if item.rank === 2}
											<span class="chip chip-silver">#2</span>
										{:else if item.rank === 3}
											<span class="chip chip-bronze">#3</span>
										{:else}
											<span class="rank-plain">#{item.rank}</span>
										{/if}
									</td>
									<td data-label="Player">
										<span class="player-name">{item.accountName}</span>
									</td>
									<td data-label="Clan">
										{#if item.clanTag}
											<span class="chip chip-tag">[{item.clanTag}]</span>
										{:else}
											<span class="dash">-</span>
										{/if}
									</td>
									<td data-label="Reputation" style="text-align: right;">
										<span class="rep-val">{item.totalReputation} RP</span>
									</td>
									<td data-label="Current Bag" style="text-align: right;">
										<span class="bag-val">Bag #{item.currentBagNumber}</span>
									</td>
									<td data-label="Completed" style="text-align: right;">
										<span>{item.completedBagsCount}</span>
									</td>
								</tr>
							{/each}
						{/if}
					</tbody>
				</table>
			{:else}
				<table class="data-table">
					<thead>
						<tr>
							<th style="width: 90px;">Rank</th>
							<th>Player</th>
							<th style="width: 120px;">Clan</th>
							<th style="width: 140px;">MMR</th>
							<th style="width: 120px;">{t('ladder.winRate')}</th>
							<th style="width: 150px;">Record (W/L)</th>
						</tr>
					</thead>
					<tbody>
						{#each currentEntries as item}
							{@const winRate = getWinRate(item.wins, item.losses)}
							<tr>
								<td data-label="Rank">
									{#if item.rank === 1}
										<span class="chip chip-gold">
											<Award size={14} /> #1
										</span>
									{:else if item.rank === 2}
										<span class="chip chip-silver">#2</span>
									{:else if item.rank === 3}
										<span class="chip chip-bronze">#3</span>
									{:else}
										<span class="rank-plain">#{item.rank}</span>
									{/if}
								</td>
								<td data-label="Player">
									<span class="player-name">{item.playerName}</span>
								</td>
								<td data-label="Clan">
									{#if item.clanTag}
										<span class="chip chip-tag">[{item.clanTag}]</span>
									{:else}
										<span class="dash">-</span>
									{/if}
								</td>
								<td data-label="MMR">
									<span class="mmr-val">{item.rating.toFixed(1)}</span>
								</td>
								<td data-label="Win Rate">
									<span class="winrate {winRate >= 50 ? 'winrate-high' : 'winrate-low'}">
										{winRate.toFixed(1)}%
									</span>
								</td>
								<td data-label="Record">
									<span>{item.wins} W - {item.losses} L</span>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			{/if}
		</div>
	</div>
</div>

<style>
	.page-container {
		max-width: 1200px;
		width: 100%;
		margin: 0 auto;
		padding: 0 24px;
	}

	.page-header {
		margin-bottom: 32px;
	}

	.header-icon-wrap {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 12px;
		margin-bottom: 8px;
	}

	.header-desc {
		max-width: 600px;
		margin: 0 auto;
		font-size: 1.0625rem;
		opacity: 0.85;
		line-height: 1.6;
	}

	.card {
		border-radius: 8px;
		overflow: hidden;
	}

	.tabs-header {
		display: flex;
		gap: 12px;
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.15);
		padding: 8px 12px;
		margin-bottom: 16px;
	}

	.tab-btn {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 10px 20px;
		border-radius: 6px;
		background: transparent;
		color: inherit;
		border: none;
		font-family: inherit;
		font-size: 0.9375rem;
		font-weight: 500;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.tab-btn:hover {
		background: rgba(var(--kongor-primary-rgb), 0.1);
	}

	.tab-btn.active {
		background: rgba(var(--kongor-primary-rgb), 0.2);
		color: var(--kongor-primary);
		font-weight: 600;
	}

	.table-responsive {
		width: 100%;
		overflow-x: auto;
	}

	.data-table {
		width: 100%;
		border-collapse: collapse;
		text-align: left;
		font-size: 0.9375rem;
	}

	.data-table th,
	.data-table td {
		padding: 14px 16px;
		border-bottom: 1px solid rgba(var(--kongor-primary-rgb), 0.1);
	}

	.data-table th {
		font-weight: 600;
		opacity: 0.75;
		font-size: 0.8125rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.data-table tbody tr:hover {
		background: rgba(var(--kongor-primary-rgb), 0.06);
	}

	.player-name {
		font-weight: 600;
		color: var(--kongor-primary);
	}

	.mmr-val {
		font-weight: 600;
	}

	.winrate-high {
		color: #66bb6a;
		font-weight: 500;
	}

	.winrate-low {
		color: #ef5350;
		font-weight: 500;
	}

	.chip {
		display: inline-flex;
		align-items: center;
		gap: 4px;
		padding: 3px 8px;
		border-radius: 4px;
		font-size: 0.8125rem;
		font-weight: 600;
	}

	.chip-gold {
		background: #ffa726;
		color: #1a1a1a;
	}

	.chip-silver {
		background: #ffd740;
		color: #1a1a1a;
	}

	.chip-bronze {
		background: #7c4dff;
		color: #ffffff;
	}

	.chip-tag {
		border: 1px solid rgba(38, 198, 218, 0.4);
		color: #26c6da;
		background: rgba(38, 198, 218, 0.1);
	}

	.rank-plain {
		font-weight: 600;
		opacity: 0.8;
	}

	.rep-val {
		color: #ffd700;
		font-weight: 600;
	}

	.bag-val {
		color: #26c6da;
		font-weight: 500;
	}

	.dash {
		opacity: 0.4;
	}

	.accent-icon {
		color: var(--kongor-primary);
	}

	.pa-4 { padding: 16px; }
	.mb-6 { margin-bottom: 24px; }
	.text-center { text-align: center; }
</style>

