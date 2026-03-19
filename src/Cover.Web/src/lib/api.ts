import { goto } from '$app/navigation';
import type {
	UserDto,
	BalanceDto,
	ExpenseDto,
	PagedResult,
	SetupStatusDto,
	LoginRequest,
	SetupRequest,
	CreateExpenseRequest,
	UpdateExpenseRequest
} from './types';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
	const res = await fetch(url, {
		credentials: 'include',
		...options,
		headers: {
			'Content-Type': 'application/json',
			...options?.headers
		}
	});

	if (res.status === 401) {
		await goto('/login');
		throw new Error('Unauthorized');
	}

	if (!res.ok) {
		const text = await res.text().catch(() => '');
		throw new Error(text || `HTTP ${res.status}`);
	}

	if (res.status === 204 || res.headers.get('content-length') === '0') {
		return undefined as T;
	}

	return res.json();
}

async function requestNoContent(url: string, options?: RequestInit): Promise<Response> {
	const res = await fetch(url, {
		credentials: 'include',
		...options,
		headers: {
			'Content-Type': 'application/json',
			...options?.headers
		}
	});

	if (res.status === 401) {
		await goto('/login');
		throw new Error('Unauthorized');
	}

	return res;
}

export async function login(req: LoginRequest): Promise<Response> {
	return requestNoContent('/api/auth/login', {
		method: 'POST',
		body: JSON.stringify(req)
	});
}

export async function logout(): Promise<void> {
	await fetch('/api/auth/logout', {
		method: 'POST',
		credentials: 'include'
	});
	window.location.href = '/login';
}

export async function getAuthStatus(): Promise<boolean> {
	try {
		const res = await fetch('/api/auth/status', { credentials: 'include' });
		return res.ok;
	} catch {
		return false;
	}
}

export async function getSetupStatus(): Promise<SetupStatusDto> {
	return request<SetupStatusDto>('/api/setup/status');
}

export async function setup(req: SetupRequest): Promise<UserDto[]> {
	return request<UserDto[]>('/api/setup', {
		method: 'POST',
		body: JSON.stringify(req)
	});
}

export async function getUsers(): Promise<UserDto[]> {
	return request<UserDto[]>('/api/users');
}

export async function getBalance(): Promise<BalanceDto | null> {
	try {
		return await request<BalanceDto>('/api/balance');
	} catch {
		return null;
	}
}

export async function getExpenses(
	page: number = 1,
	pageSize: number = 20,
	paidById?: number
): Promise<PagedResult<ExpenseDto>> {
	let url = `/api/expenses?page=${page}&pageSize=${pageSize}`;
	if (paidById !== undefined) {
		url += `&paidById=${paidById}`;
	}
	return request<PagedResult<ExpenseDto>>(url);
}

export async function getExpense(id: number): Promise<ExpenseDto | null> {
	try {
		return await request<ExpenseDto>(`/api/expenses/${id}`);
	} catch {
		return null;
	}
}

export async function getMerchants(query: string): Promise<string[]> {
	try {
		return await request<string[]>(
			`/api/expenses/merchants?q=${encodeURIComponent(query)}`
		);
	} catch {
		return [];
	}
}

export async function createExpense(req: CreateExpenseRequest): Promise<ExpenseDto> {
	return request<ExpenseDto>('/api/expenses', {
		method: 'POST',
		body: JSON.stringify(req)
	});
}

export async function updateExpense(id: number, req: UpdateExpenseRequest): Promise<ExpenseDto> {
	return request<ExpenseDto>(`/api/expenses/${id}`, {
		method: 'PUT',
		body: JSON.stringify(req)
	});
}

export async function deleteExpense(id: number): Promise<void> {
	await request<void>(`/api/expenses/${id}`, { method: 'DELETE' });
}
