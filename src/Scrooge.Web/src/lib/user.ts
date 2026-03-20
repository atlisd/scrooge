import { writable } from 'svelte/store';
import type { UserDto } from './types';

export const activeUser = writable<UserDto | null>(null);
let _activeUser: UserDto | null = null;

export function initUsers(users: UserDto[]) {
	localStorage.setItem('users', JSON.stringify(users));
	const savedId = localStorage.getItem('activeUserId');
	const saved = savedId ? users.find((u) => u.id === parseInt(savedId)) : null;
	const user = saved ?? users[0] ?? null;
	_activeUser = user;
	activeUser.set(user);
	if (user) localStorage.setItem('activeUserId', String(user.id));
}

export function setActiveUser(user: UserDto) {
	_activeUser = user;
	activeUser.set(user);
	localStorage.setItem('activeUserId', String(user.id));
}

export function getActiveUser(): UserDto | null {
	return _activeUser;
}

export function getCachedUsers(): UserDto[] {
	try {
		const s = typeof localStorage !== 'undefined' ? localStorage.getItem('users') : null;
		return s ? JSON.parse(s) : [];
	} catch {
		return [];
	}
}
