import adapter from '@sveltejs/adapter-auto';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const apiTarget = process.env.services__web_portal_api__https__0
	|| process.env.services__web_portal_api__http__0
	|| process.env.SERVICES__WEB_PORTAL_API__HTTPS__0
	|| process.env.SERVICES__WEB_PORTAL_API__HTTP__0
	|| 'https://localhost:5556';

export default defineConfig({
	plugins: [
		sveltekit({
			compilerOptions: {
				runes: ({ filename }) =>
					filename.split(/[/\\]/).includes('node_modules') ? undefined : true
			},
			adapter: adapter()
		})
	],
	resolve: {
		alias: {
			'$lib': path.resolve(__dirname, './src/lib'),
			'#lib': path.resolve(__dirname, './src/lib')
		}
	},
	server: {
		host: '0.0.0.0',
		port: process.env.PORT ? parseInt(process.env.PORT) : 5557,
		strictPort: false,
		proxy: {
			'/User': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Email': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Password': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Accounts': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Administration': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Statistics': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Staff': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Suspensions': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Quests': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/MiniGames': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Clan': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Reports': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/Community': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			},
			'/CaiUpload': {
				target: apiTarget,
				changeOrigin: true,
				secure: false
			}
		}
	}
});
