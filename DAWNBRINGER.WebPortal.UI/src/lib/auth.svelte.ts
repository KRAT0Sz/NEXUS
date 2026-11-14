import { browser } from '$app/env';

const TOKEN_KEY = 'authentication-token';

interface JWTPayload {
	user_id?: string | number;
	user_role?: string;
	unique_name?: string;
	exp?: number;
	[key: string]: unknown;
}

function parseJwt(token: string): JWTPayload | null {
	try {
		const base64Url = token.split('.')[1];
		if (!base64Url) return null;
		const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
		const jsonPayload = decodeURIComponent(
			atob(base64)
				.split('')
				.map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
				.join('')
		);
		return JSON.parse(jsonPayload);
	} catch {
		return null;
	}
}

class AuthManager {
	token = $state<string | null>(null);
	userID = $state<number | null>(null);
	userRole = $state<string | null>(null);
	userName = $state<string | null>(null);
	isInitialised = $state<boolean>(false);

	get isAuthenticated(): boolean {
		return !!this.token && !!this.userID;
	}

	get isAdministrator(): boolean {
		return this.userRole === 'ADMINISTRATOR';
	}

	get isCustodian(): boolean {
		return this.userRole === 'CUSTODIAN';
	}

	get isSeniorGameMaster(): boolean {
		return this.isAdministrator || this.isCustodian || this.userRole === 'SENIORGAMEMASTER';
	}

	get isGameMaster(): boolean {
		return this.isSeniorGameMaster || this.userRole === 'GAMEMASTER';
	}

	get isStaff(): boolean {
		return this.isGameMaster || this.userRole === 'STAFF';
	}

	get hasElevatedPrivileges(): boolean {
		return this.isAdministrator || this.isCustodian;
	}

	init(): void {
		if (!browser || this.isInitialised) return;

		const stored = localStorage.getItem(TOKEN_KEY);
		if (stored) {
			this.applyToken(stored);
		}
		this.isInitialised = true;
	}

	setToken(newToken: string): void {
		if (browser) {
			localStorage.setItem(TOKEN_KEY, newToken);
		}
		this.applyToken(newToken);
	}

	clearToken(): void {
		if (browser) {
			localStorage.removeItem(TOKEN_KEY);
		}
		this.token = null;
		this.userID = null;
		this.userRole = null;
		this.userName = null;
	}

	private applyToken(tokenString: string): void {
		const payload = parseJwt(tokenString);
		if (!payload) {
			this.clearToken();
			return;
		}

		if (payload.exp && payload.exp * 1000 <= Date.now()) {
			this.clearToken();
			return;
		}

		this.token = tokenString;
		this.userID = payload.user_id ? Number(payload.user_id) : null;
		this.userRole = payload.user_role ? String(payload.user_role) : null;
		this.userName = payload.unique_name ? String(payload.unique_name) : null;
	}
}

export const auth = new AuthManager();

