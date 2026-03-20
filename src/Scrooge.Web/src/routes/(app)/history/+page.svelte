<script lang="ts">
	import { onMount } from 'svelte';
	import { getUsers, getExpenses } from '$lib/api';
	import type { UserDto, ExpenseDto } from '$lib/types';
	import ExpenseCard from '$lib/ExpenseCard.svelte';

	let users = $state<UserDto[]>([]);
	let expenses = $state<ExpenseDto[]>([]);
	let totalCount = $state(0);
	let currentPage = $state(1);
	let loading = $state(true);
	let filterUserId = $state<number | undefined>(undefined);

	const pageSize = 20;

	let totalPages = $derived(Math.max(1, Math.ceil(totalCount / pageSize)));

	onMount(async () => {
		users = await getUsers();
		await loadExpenses();
	});

	async function loadExpenses() {
		loading = true;
		const result = await getExpenses(currentPage, pageSize, filterUserId);
		expenses = result.items;
		totalCount = result.totalCount;
		loading = false;
	}

	async function setFilter(userId: number | undefined) {
		filterUserId = userId;
		currentPage = 1;
		await loadExpenses();
	}

	async function goToPage(p: number) {
		if (p < 1 || p > totalPages) return;
		currentPage = p;
		await loadExpenses();
	}
</script>

<h4 class="mb-3">History</h4>

<div class="btn-group mb-3" role="group">
	<button
		class="btn btn-sm"
		class:btn-primary={filterUserId === undefined}
		class:btn-outline-primary={filterUserId !== undefined}
		onclick={() => setFilter(undefined)}
	>
		All
	</button>
	{#each users as user (user.id)}
		<button
			class="btn btn-sm"
			class:btn-primary={filterUserId === user.id}
			class:btn-outline-primary={filterUserId !== user.id}
			onclick={() => setFilter(user.id)}
		>
			{user.name}
		</button>
	{/each}
</div>

{#if loading}
	<div class="text-center py-4">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{:else if expenses.length === 0}
	<p class="text-muted">No expenses found.</p>
{:else}
	{#each expenses as expense (expense.id)}
		<ExpenseCard {expense} />
	{/each}

	{#if totalPages > 1}
		<nav class="mt-3">
			<ul class="pagination justify-content-center">
				<li class="page-item" class:disabled={currentPage === 1}>
					<button class="page-link" onclick={() => goToPage(currentPage - 1)}>Previous</button>
				</li>
				{#each Array.from({ length: totalPages }, (_, i) => i + 1) as p}
					<li class="page-item" class:active={p === currentPage}>
						<button class="page-link" onclick={() => goToPage(p)}>{p}</button>
					</li>
				{/each}
				<li class="page-item" class:disabled={currentPage === totalPages}>
					<button class="page-link" onclick={() => goToPage(currentPage + 1)}>Next</button>
				</li>
			</ul>
		</nav>
	{/if}
{/if}
