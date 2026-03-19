export interface UserDto {
	id: number;
	name: string;
}

export interface ExpenseDto {
	id: number;
	merchant: string | null;
	description: string | null;
	amount: number;
	splitType: SplitType;
	paidById: number;
	paidByName: string;
	date: string;
	createdAt: string;
}

export interface CreateExpenseRequest {
	merchant: string | null;
	description: string | null;
	amount: number;
	splitType: SplitType;
	paidById: number;
	date: string;
}

export interface UpdateExpenseRequest {
	merchant: string | null;
	description: string | null;
	amount: number;
	splitType: SplitType;
	paidById: number;
	date: string;
}

export interface UserBalanceInfo {
	userId: number;
	name: string;
	totalPaid: number;
	credit: number;
}

export interface BalanceDto {
	user1: UserBalanceInfo;
	user2: UserBalanceInfo;
	netBalance: number;
	owesUserId: number;
	isOwedUserId: number;
	owedAmount: number;
}

export interface LoginRequest {
	username: string;
	password: string;
}

export interface SetupRequest {
	name1: string;
	name2: string;
	username: string;
	password: string;
	currency: string;
	currencyDecimals: number;
}

export interface SetupStatusDto {
	isComplete: boolean;
	currencyCode: string;
	currencyDecimals: number;
}

export interface PagedResult<T> {
	items: T[];
	totalCount: number;
	page: number;
	pageSize: number;
}

export type SplitType = 'Equal' | 'FullOther';
