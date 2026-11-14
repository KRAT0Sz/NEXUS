import { browser } from '$app/env';
import en from './locales/en';
import th from './locales/th';
import ru from './locales/ru';
import es from './locales/es';
import fr from './locales/fr';

export type LocaleCode = 'en' | 'th' | 'ru' | 'es' | 'fr';

export interface LocaleOption {
	code: LocaleCode;
	name: string;
	flag: string;
}

export const LOCALES: readonly LocaleOption[] = [
	{ code: 'en', name: 'English', flag: '🇬🇧' },
	{ code: 'th', name: 'ไทย', flag: '🇹🇭' },
	{ code: 'ru', name: 'Русский', flag: '🇷🇺' },
	{ code: 'es', name: 'Español', flag: '🇪🇸' },
	{ code: 'fr', name: 'Français', flag: '🇫🇷' }
] as const;

type TranslationTree = Record<string, any>;

const translations: Record<LocaleCode, TranslationTree> = {
	en,
	th,
	ru,
	es,
	fr
};

const navFallbacks: Record<LocaleCode, Record<string, string>> = {
	en: {
		home: 'Home',
		community: 'Community',
		leaderboard: 'Leaderboard',
		replays: 'Replays',
		clans: 'Clans',
		register: 'Register',
		login: 'Log In',
		logout: 'Log Out',
		account: 'Account'
	},
	th: {
		home: 'หน้าหลัก',
		community: 'คอมมูนิตี้',
		leaderboard: 'อันดับผู้เล่น',
		replays: 'รีเพลย์',
		clans: 'แคลน',
		register: 'ลงทะเบียน',
		login: 'เข้าสู่ระบบ',
		logout: 'ออกจากระบบ',
		account: 'บัญชีผู้ใช้'
	},
	ru: {
		home: 'Главная',
		community: 'Сообщество',
		leaderboard: 'Таблица лидеров',
		replays: 'Повторы',
		clans: 'Кланы',
		register: 'Регистрация',
		login: 'Войти',
		logout: 'Выйти',
		account: 'Аккаунт'
	},
	es: {
		home: 'Inicio',
		community: 'Comunidad',
		leaderboard: 'Clasificación',
		replays: 'Repeticiones',
		clans: 'Clanes',
		register: 'Registrarse',
		login: 'Iniciar Sesión',
		logout: 'Cerrar Sesión',
		account: 'Cuenta'
	},
	fr: {
		home: 'Accueil',
		community: 'Communauté',
		leaderboard: 'Classement',
		replays: 'Replays',
		clans: 'Clans',
		register: 'Inscription',
		login: 'Connexion',
		logout: 'Déconnexion',
		account: 'Compte'
	}
};

class I18nManager {
	currentLocale = $state<LocaleCode>('en');

	constructor() {
		if (browser) {
			const saved = localStorage.getItem('kongor_locale') as LocaleCode | null;
			if (saved && saved in translations) {
				this.currentLocale = saved;
			} else {
				const browserLang = navigator.language?.slice(0, 2).toLowerCase() as LocaleCode;
				if (browserLang && browserLang in translations) {
					this.currentLocale = browserLang;
				}
			}
			document.documentElement.lang = this.currentLocale;
		}
	}

	setLocale(code: LocaleCode): void {
		if (code in translations) {
			this.currentLocale = code;
			if (browser) {
				localStorage.setItem('kongor_locale', code);
				document.documentElement.lang = code;
			}
		}
	}

	get localeName(): string {
		return LOCALES.find((l) => l.code === this.currentLocale)?.name ?? 'English';
	}

	get localeFlag(): string {
		return LOCALES.find((l) => l.code === this.currentLocale)?.flag ?? '🌐';
	}

	t(key: string, params?: Record<string, string | number>): string {
		const locale = this.currentLocale;

		// Check navFallbacks for "nav.*" keys
		if (key.startsWith('nav.')) {
			const navKey = key.slice(4);
			const localizedNav = navFallbacks[locale]?.[navKey] ?? navFallbacks.en[navKey];
			if (localizedNav) return localizedNav;
		}

		const keys = key.split('.');
		let value = this.resolveKey(translations[locale], keys);

		// Fallback to English if not found in current locale
		if (value === undefined && locale !== 'en') {
			value = this.resolveKey(translations.en, keys);
		}

		if (value === undefined) {
			return key;
		}

		if (typeof value !== 'string') {
			return String(value);
		}

		if (params) {
			return value.replace(/\{(\w+)\}/g, (_, token) => {
				return token in params ? String(params[token]) : `{${token}}`;
			});
		}

		return value;
	}

	private resolveKey(tree: TranslationTree | undefined, keys: string[]): any {
		let current = tree;
		for (const k of keys) {
			if (current === undefined || current === null) return undefined;
			current = current[k];
		}
		return current;
	}
}

export const i18n = new I18nManager();
export const t = (key: string, params?: Record<string, string | number>): string => i18n.t(key, params);

