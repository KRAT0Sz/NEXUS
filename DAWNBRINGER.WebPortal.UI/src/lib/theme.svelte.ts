import { browser } from '$app/env';

declare global {
	interface Window {
		CursorTrail?: {
			enable: () => void;
			disable: () => void;
			isEnabled: () => boolean;
		};
	}
}

class ThemeManager {
	isDarkMode = $state<boolean>(true);
	cursorTrailEnabled = $state<boolean>(false);
	isInitialised = $state<boolean>(false);

	init(): void {
		if (!browser || this.isInitialised) return;

		const storedTheme = localStorage.getItem('theme');
		if (storedTheme !== null) {
			this.isDarkMode = storedTheme === 'dark';
		} else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
			this.isDarkMode = false;
		}

		const storedTrail = localStorage.getItem('cursor-trail');
		if (storedTrail !== null) {
			this.cursorTrailEnabled = storedTrail === 'true';
		}

		if (this.cursorTrailEnabled && window.CursorTrail) {
			window.CursorTrail.enable();
		}

		this.isInitialised = true;
	}

	toggleTheme(): void {
		this.isDarkMode = !this.isDarkMode;
		if (browser) {
			localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
		}
	}

	toggleCursorTrail(): void {
		this.cursorTrailEnabled = !this.cursorTrailEnabled;
		if (browser) {
			localStorage.setItem('cursor-trail', String(this.cursorTrailEnabled));
			if (window.CursorTrail) {
				if (this.cursorTrailEnabled) {
					window.CursorTrail.enable();
				} else {
					window.CursorTrail.disable();
				}
			}
		}
	}
}

export const theme = new ThemeManager();

