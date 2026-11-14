<script lang="ts">
	import { Video, Search, MapPin, Download } from 'lucide-svelte';
	import type { ReplayItem } from '#lib/types';

	let searchTerm = $state<string>('');

	const allReplays: ReplayItem[] = [
		{ matchID: 100452, mapName: 'Forests of Caldavar', gameMode: 'All Pick', duration: '42:15', recordedDate: '2026-11-13', downloadURL: 'https://replays.projectkongor.com/replays/M100452.honreplay' },
		{ matchID: 100451, mapName: 'Mid Wars', gameMode: 'Blind Ban', duration: '28:40', recordedDate: '2026-11-13', downloadURL: 'https://replays.projectkongor.com/replays/M100451.honreplay' },
		{ matchID: 100450, mapName: "Grimm's Crossing (3v3)", gameMode: 'Lock Pick', duration: '35:10', recordedDate: '2026-11-12', downloadURL: 'https://replays.projectkongor.com/replays/M100450.honreplay' },
		{ matchID: 100449, mapName: 'Forests of Caldavar', gameMode: 'Captains Pick', duration: '51:02', recordedDate: '2026-11-12', downloadURL: 'https://replays.projectkongor.com/replays/M100449.honreplay' },
		{ matchID: 100448, mapName: 'Rift Wars', gameMode: 'All Random', duration: '22:18', recordedDate: '2026-11-11', downloadURL: 'https://replays.projectkongor.com/replays/M100448.honreplay' }
	];

	const filteredReplays = $derived(
		!searchTerm.trim()
			? allReplays
			: allReplays.filter(
					(r) =>
						r.matchID.toString().includes(searchTerm) ||
						r.mapName.toLowerCase().includes(searchTerm.toLowerCase()) ||
						r.gameMode.toLowerCase().includes(searchTerm.toLowerCase())
				)
	);
</script>

<svelte:head>
	<title>Match Replays - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	<!-- Page Header -->
	<header class="page-header animate-fade-in text-center">
		<div class="header-icon-wrap">
			<Video size={32} class="accent-icon" />
			<h1 class="section-header">Match Replays</h1>
		</div>
		<p class="header-desc">
			Search and download recorded match replays directly to view in your Heroes Of Newerth client.
		</p>
	</header>

	<!-- Search Card -->
	<div class="card glass-card pa-4 mb-6 animate-fade-in-delay-1">
		<div class="search-bar">
			<div class="search-input-wrapper">
				<Search size={18} class="search-icon" />
				<input
					type="text"
					bind:value={searchTerm}
					placeholder="Search by Match ID, Map or Hero..."
					class="search-input"
				/>
			</div>
			<button class="btn btn-primary glow-button">
				Search
			</button>
		</div>
	</div>

	<!-- Replays Table Card -->
	<div class="card glass-card pa-4 mb-6 animate-fade-in-delay-2">
		<div class="table-responsive">
			<table class="data-table">
				<thead>
					<tr>
						<th style="width: 130px;">Match ID</th>
						<th>Map</th>
						<th>Game Mode</th>
						<th style="width: 140px;">Duration</th>
						<th style="width: 160px;">Date</th>
						<th style="width: 160px;">Action</th>
					</tr>
				</thead>
				<tbody>
					{#each filteredReplays as item}
						<tr>
							<td data-label="Match ID">
								<span class="chip chip-outlined">#{item.matchID}</span>
							</td>
							<td data-label="Map">
								<div class="map-cell">
									<MapPin size={16} class="map-icon" />
									<span>{item.mapName}</span>
								</div>
							</td>
							<td data-label="Game Mode">
								<span>{item.gameMode}</span>
							</td>
							<td data-label="Duration">
								<span>{item.duration}</span>
							</td>
							<td data-label="Date">
								<span class="date-text">{item.recordedDate}</span>
							</td>
							<td data-label="Action">
								<a
									href={item.downloadURL}
									target="_blank"
									rel="noreferrer"
									class="btn btn-primary glow-button btn-sm"
								>
									<Download size={14} />
									<span>Download</span>
								</a>
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
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
	}

	.search-bar {
		display: flex;
		align-items: center;
		gap: 12px;
	}

	.search-input-wrapper {
		position: relative;
		flex-grow: 1;
		display: flex;
		align-items: center;
	}

	.search-icon {
		position: absolute;
		left: 14px;
		color: var(--kongor-primary);
		pointer-events: none;
	}

	.search-input {
		width: 100%;
		padding: 12px 14px 12px 42px;
		border-radius: 6px;
		border: 1px solid rgba(var(--kongor-primary-rgb), 0.25);
		background: rgba(var(--kongor-surface-rgb), 0.6);
		color: inherit;
		font-family: inherit;
		font-size: 0.9375rem;
		outline: none;
		transition: border-color 0.2s ease, box-shadow 0.2s ease;
	}

	.search-input:focus {
		border-color: var(--kongor-primary);
		box-shadow: 0 0 12px rgba(var(--kongor-primary-rgb), 0.2);
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

	.map-cell {
		display: flex;
		align-items: center;
		gap: 6px;
	}

	.map-icon {
		color: var(--kongor-secondary);
	}

	.date-text {
		color: var(--kongor-secondary);
	}

	.chip {
		display: inline-flex;
		align-items: center;
		padding: 3px 8px;
		border-radius: 4px;
		font-size: 0.8125rem;
		font-weight: 600;
	}

	.chip-outlined {
		border: 1px solid var(--kongor-primary);
		color: var(--kongor-primary);
		background: rgba(var(--kongor-primary-rgb), 0.08);
	}

	.btn {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 10px 20px;
		border-radius: 6px;
		font-family: inherit;
		font-size: 0.9375rem;
		font-weight: 500;
		text-decoration: none;
		border: none;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.btn-sm {
		padding: 6px 14px;
		font-size: 0.8125rem;
	}

	.btn-primary {
		background: var(--kongor-primary);
		color: #0a0f0d;
	}

	.accent-icon {
		color: var(--kongor-primary);
	}

	.pa-4 { padding: 16px; }
	.mb-6 { margin-bottom: 24px; }
	.text-center { text-align: center; }
</style>

