<script lang="ts">
	import { onMount } from 'svelte';
	import { getBalance, getExpenses } from '$lib/api';
	import { hub } from '$lib/hub.svelte';
	import type { BalanceDto, ExpenseDto } from '$lib/types';
	import BalanceSummary from '$lib/BalanceSummary.svelte';
	import ExpenseCard from '$lib/ExpenseCard.svelte';

	let balance = $state<BalanceDto | null>(null);
	let expenses = $state<ExpenseDto[]>([]);
	let totalCount = $state(0);
	let loading = $state(true);
	let inited = false;

	async function loadData() {
		const [b, e] = await Promise.all([getBalance(), getExpenses(1, 5)]);
		balance = b;
		expenses = e.items;
		totalCount = e.totalCount;
	}

	onMount(async () => {
		await loadData();
		loading = false;
		inited = true;
	});

	$effect(() => {
		hub.expenseRevision;
		if (inited) loadData();
	});
</script>

<h4 class="mb-3">Dashboard</h4>

{#if loading}
	<div class="text-center py-4">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{:else}
	<BalanceSummary {balance} />

	<a href="/add" class="btn btn-primary w-100 mb-3">Add Expense</a>

	<h5 class="mt-4 mb-3">Recent Expenses</h5>

	{#if expenses.length === 0}
		<p class="text-muted">No expenses yet.</p>
	{:else}
		{#each expenses as expense (expense.id)}
			<ExpenseCard {expense} />
		{/each}

		{#if totalCount > 5}
			<div class="text-center mt-2">
				<a href="/history" class="btn btn-outline-secondary btn-sm">View all</a>
			</div>
		{/if}
	{/if}
{/if}
