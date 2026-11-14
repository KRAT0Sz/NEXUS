<script lang="ts">
	const currentYear = new Date().getFullYear();
</script>

<footer class="kongor-footer footer-root">
	<div class="footer-container">
		<hr class="section-divider" />

		<nav class="footer-links" aria-label="Footer Navigation">
			<a href="/" class="footer-link">Home</a>
			<a href="/downloads" class="footer-link">Downloads</a>
			<a href="https://codex.kongor.net" target="_blank" rel="noreferrer" class="footer-link">Knowledge Base</a>
			<a href="https://discord.com/invite/N6pKzGDqUH" target="_blank" rel="noreferrer" class="footer-link">Discord</a>
			<a href="https://app.element.io/#/room/#newerth:matrix.org" target="_blank" rel="noreferrer" class="footer-link">Element</a>
			<a href="https://www.reddit.com/r/newerth/" target="_blank" rel="noreferrer" class="footer-link">Reddit</a>
			<a href="https://github.com/Project-KONGOR-Open-Source" target="_blank" rel="noreferrer" class="footer-link">GitHub</a>
		</nav>

		<div class="footer-copy">
			<p class="copy-text">
				Copyright: 2022-{currentYear} Project KONGOR | Contact: <a href="mailto:project.kongor@proton.me">project.kongor@proton.me</a>
			</p>
			<p class="disclaimer-text">
				Any and all of the Heroes Of Newerth intellectual property is owned exclusively by Garena.
			</p>
		</div>
	</div>
</footer>

<style>
	.footer-root {
		width: 100%;
		padding-bottom: 24px;
	}

	.footer-container {
		max-width: 1200px;
		margin: 0 auto;
		padding: 0 24px;
	}

	.footer-links {
		display: flex;
		justify-content: center;
		flex-wrap: wrap;
		gap: 24px;
		padding-bottom: 12px;
	}

	.footer-link {
		color: inherit;
		text-decoration: none;
		font-size: 0.875rem;
		transition: color 0.2s ease;
	}

	.footer-link:hover {
		color: var(--kongor-primary);
	}

	.footer-copy {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding-top: 16px;
		gap: 4px;
		text-align: center;
	}

	.copy-text {
		margin: 0;
		font-size: 0.8125rem;
		color: inherit;
		opacity: 0.85;
	}

	.copy-text a {
		color: var(--kongor-primary);
		text-decoration: none;
	}

	.copy-text a:hover {
		text-decoration: underline;
	}

	.disclaimer-text {
		margin: 0;
		font-size: 0.75rem;
		color: var(--kongor-secondary);
		opacity: 0.8;
	}
</style>

