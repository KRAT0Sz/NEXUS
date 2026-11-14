<script lang="ts">
	import { Shield, Users } from 'lucide-svelte';
	import type { ClanItem } from '#lib/types';

	const registeredClans: ClanItem[] = [
		{ tag: 'tdM', name: 'team discover More', leaderName: 'MoonMeander', memberCount: 15, createdDate: '2022-10-15' },
		{ tag: 'EZ', name: 'Easy Mode Gaming', leaderName: 'AngryTestie', memberCount: 24, createdDate: '2022-11-01' },
		{ tag: 'sG', name: 'stay Green', leaderName: 'insania', memberCount: 18, createdDate: '2023-01-20' },
		{ tag: 'coL', name: 'Complexity Gaming', leaderName: 'Tralfamadore', memberCount: 12, createdDate: '2023-03-10' },
		{ tag: 'BM', name: 'Bad Monkey Gaming', leaderName: 'KheZu', memberCount: 10, createdDate: '2023-05-04' }
	];
</script>

<svelte:head>
	<title>Clan Hub - Project KONGOR</title>
</svelte:head>

<div class="page-container">
	<!-- Page Header -->
	<header class="page-header animate-fade-in text-center">
		<div class="header-icon-wrap">
			<Shield size={32} class="accent-icon" />
			<h1 class="section-header">Clan Hub</h1>
		</div>
		<p class="header-desc">
			Explore registered clans, roster rankings, and competitive teams in Newerth.
		</p>
	</header>

	<!-- Clan Directory Table Card -->
	<div class="card glass-card pa-4 mb-6 animate-fade-in-delay-1">
		<div class="table-responsive">
			<table class="data-table">
				<thead>
					<tr>
						<th style="width: 110px;">Tag</th>
						<th>Clan Name</th>
						<th style="width: 170px;">Leader</th>
						<th style="width: 140px;">Members</th>
						<th style="width: 160px;">Created Date</th>
					</tr>
				</thead>
				<tbody>
					{#each registeredClans as item}
						<tr>
							<td data-label="Tag">
								<span class="chip chip-outlined">[{item.tag}]</span>
							</td>
							<td data-label="Clan Name">
								<span class="clan-name">{item.name}</span>
							</td>
							<td data-label="Leader">
								<span class="leader-name">{item.leaderName}</span>
							</td>
							<td data-label="Members">
								<div class="member-cell">
									<Users size={16} class="member-icon" />
									<span>{item.memberCount}</span>
								</div>
							</td>
							<td data-label="Created Date">
								<span class="date-text">{item.createdDate}</span>
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

	.clan-name {
		font-weight: 600;
	}

	.leader-name,
	.date-text {
		color: var(--kongor-secondary);
	}

	.member-cell {
		display: flex;
		align-items: center;
		gap: 6px;
	}

	.member-icon {
		color: var(--kongor-primary);
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

	.accent-icon {
		color: var(--kongor-primary);
	}

	.pa-4 { padding: 16px; }
	.mb-6 { margin-bottom: 24px; }
	.text-center { text-align: center; }
</style>

