import { auth } from './auth.svelte';

export interface ApiResponse<T = unknown> {
	ok: boolean;
	status: number;
	data: T | null;
	error: string | null;
}

export async function apiFetch<T = unknown>(
	endpoint: string,
	options: RequestInit = {}
): Promise<ApiResponse<T>> {
	const headers: Record<string, string> = {
		'Content-Type': 'application/json',
		...(options.headers as Record<string, string>)
	};

	if (auth.token) {
		headers['Authorization'] = `Bearer ${auth.token}`;
	}

	try {
		const response = await fetch(endpoint, {
			...options,
			headers
		});

		const text = await response.text();
		let data: T | null = null;

		if (text) {
			try {
				data = JSON.parse(text) as T;
			} catch {
				data = text as unknown as T;
			}
		}

		if (!response.ok) {
			let errorMsg = typeof data === 'string' ? data : `Request failed with status ${response.status}`;
			if (typeof data === 'object' && data !== null && 'message' in data) {
				errorMsg = String((data as { message: unknown }).message);
			}
			return {
				ok: false,
				status: response.status,
				data: null,
				error: errorMsg
			};
		}

		return {
			ok: true,
			status: response.status,
			data,
			error: null
		};
	} catch {
		return {
			ok: false,
			status: 0,
			data: null,
			error: 'Unable to reach the server. Please try again later.'
		};
	}
}

