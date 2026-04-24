<script lang="ts">
	import { onMount } from 'svelte';
	import { getBalance, getExpenses, getUsers, createExpense } from '$lib/api';
	import { hub } from '$lib/hub.svelte';
	import { toMinorUnits, parseAmountInput, isoToDisplayDate, parseDisplayDate, todayString, formatAmount } from '$lib/currency';
	import { getActiveUser } from '$lib/user';
	import type { BalanceDto, ExpenseDto, UserDto, SplitType } from '$lib/types';
	import BalanceSummary from '$lib/BalanceSummary.svelte';
	import ExpenseCard from '$lib/ExpenseCard.svelte';
	import ExpenseForm from '$lib/ExpenseForm.svelte';

	let balance = $state<BalanceDto | null>(null);
	let expenses = $state<ExpenseDto[]>([]);
	let totalCount = $state(0);
	let users = $state<UserDto[]>([]);
	let loading = $state(true);
	let submitting = $state(false);
	let error = $state<string | null>(null);
	let successMessage = $state<string | null>(null);
	let successVisible = $state(false);
	let successTimeout: ReturnType<typeof setTimeout> | null = null;
	let formKey = $state(0);
	let inited = false;

	let model = $state({
		merchant: '',
		description: '',
		amount: '',
		paidById: 0,
		splitType: 'Equal' as SplitType,
		date: isoToDisplayDate(todayString())
	});

	async function loadData() {
		const [b, e] = await Promise.all([getBalance(), getExpenses(1, 3)]);
		balance = b;
		expenses = e.items;
		totalCount = e.totalCount;
	}

	onMount(async () => {
		const [, , fetched] = await Promise.all([
			getBalance().then((b) => (balance = b)),
			getExpenses(1, 3).then((e) => {
				expenses = e.items;
				totalCount = e.totalCount;
			}),
			getUsers()
		]);

		const activeUser = getActiveUser();
		if (activeUser) {
			users = [
				...fetched.filter((u) => u.id === activeUser.id),
				...fetched.filter((u) => u.id !== activeUser.id)
			];
			model.paidById = activeUser.id;
		} else {
			users = fetched;
			if (fetched.length > 0) model.paidById = fetched[0].id;
		}

		loading = false;
		inited = true;
	});

	$effect(() => {
		hub.expenseRevision;
		if (inited) loadData();
	});

	async function handleSubmit() {
		const parsedAmount = parseAmountInput(model.amount);
		if (parsedAmount === null) {
			error = 'Please enter a valid amount.';
			return;
		}

		const isoDate = parseDisplayDate(model.date);
		if (!isoDate) {
			error = 'Please enter a valid date.';
			return;
		}

		error = null;
		submitting = true;

		try {
			const minorAmount = toMinorUnits(parsedAmount);
			const merchant = model.merchant;

			await createExpense({
				merchant: merchant || null,
				description: model.description || null,
				amount: minorAmount,
				splitType: model.splitType,
				paidById: model.paidById,
				date: isoDate
			});

			const activeUser = getActiveUser();
			const defaultPaidById = activeUser?.id ?? users[0]?.id ?? model.paidById;
			model = {
				merchant: '',
				description: '',
				amount: '',
				paidById: defaultPaidById,
				splitType: 'Equal',
				date: isoToDisplayDate(todayString())
			};

			formKey += 1;

			if (successTimeout) clearTimeout(successTimeout);
			successMessage = merchant ? `${merchant} · ${formatAmount(minorAmount)} added` : `${formatAmount(minorAmount)} added`;
			successVisible = true;
			successTimeout = setTimeout(() => {
				successVisible = false;
				setTimeout(() => (successMessage = null), 400);
			}, 3000);

			await loadData();
		} catch (err: any) {
			error = err.message || 'Failed to create expense.';
		} finally {
			submitting = false;
		}
	}
</script>

{#if loading}
	<div class="text-center py-4">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{:else}
	<BalanceSummary {balance} />

	{#key formKey}
		<ExpenseForm
			bind:model
			{users}
			buttonText="Add Expense"
			{submitting}
			{error}
			showDescription={false}
			collapsibleMore={true}
			onSubmit={handleSubmit}
		/>
	{/key}

	<h5 class="mt-4 mb-3">Recent Expenses</h5>

	{#if expenses.length === 0}
		<p class="text-muted">No expenses yet.</p>
	{:else}
		{#each expenses as expense (expense.id)}
			<ExpenseCard {expense} />
		{/each}

		<div class="text-center mt-2">
			<a href="/history" class="btn btn-outline-secondary btn-sm">View all</a>
		</div>
	{/if}
{/if}

{#if successMessage}
	<div class="notify-overlay" class:notify-visible={successVisible}>
		<div class="notify-card">
			<div class="notify-icon">✓</div>
			<div class="notify-text">{successMessage}</div>
		</div>
	</div>
{/if}

<style>
	.notify-overlay {
		position: fixed;
		inset: 0;
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1050;
		pointer-events: none;
		opacity: 0;
		transition: opacity 0.3s ease;
	}

	.notify-visible {
		opacity: 1;
	}

	.notify-card {
		background: #fff;
		border-radius: 1rem;
		box-shadow: 0 8px 40px rgba(0, 0, 0, 0.18);
		padding: 2rem 2.5rem;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.75rem;
		min-width: 240px;
		max-width: 90vw;
		transform: scale(0.92);
		transition: transform 0.3s ease;
	}

	.notify-visible .notify-card {
		transform: scale(1);
	}

	.notify-icon {
		width: 3rem;
		height: 3rem;
		border-radius: 50%;
		background: #22c55e;
		color: #fff;
		font-size: 1.5rem;
		font-weight: 700;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.notify-text {
		font-size: 1.1rem;
		font-weight: 500;
		color: #1a1a2e;
		text-align: center;
	}
</style>
