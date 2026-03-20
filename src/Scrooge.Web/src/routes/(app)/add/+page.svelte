<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { getUsers, createExpense } from '$lib/api';
	import { toMinorUnits, parseAmountInput, isoToDisplayDate, parseDisplayDate, todayString } from '$lib/currency';
	import type { UserDto, SplitType } from '$lib/types';
	import ExpenseForm from '$lib/ExpenseForm.svelte';

	let users = $state<UserDto[]>([]);
	let loading = $state(true);
	let submitting = $state(false);
	let error = $state<string | null>(null);

	let model = $state({
		merchant: '',
		description: '',
		amount: '',
		paidById: 0,
		splitType: 'Equal' as SplitType,
		date: isoToDisplayDate(todayString())
	});

	onMount(async () => {
		users = await getUsers();
		const lastPaidById = localStorage.getItem('lastPaidById');
		if (lastPaidById) {
			const id = parseInt(lastPaidById, 10);
			if (users.some((u) => u.id === id)) {
				model.paidById = id;
			} else if (users.length > 0) {
				model.paidById = users[0].id;
			}
		} else if (users.length > 0) {
			model.paidById = users[0].id;
		}
		loading = false;
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
			await createExpense({
				merchant: model.merchant || null,
				description: model.description || null,
				amount: toMinorUnits(parsedAmount),
				splitType: model.splitType,
				paidById: model.paidById,
				date: isoDate
			});
			localStorage.setItem('lastPaidById', String(model.paidById));
			await goto('/');
		} catch (err: any) {
			error = err.message || 'Failed to create expense.';
		} finally {
			submitting = false;
		}
	}
</script>

<h4 class="mb-3">Add Expense</h4>

{#if loading}
	<div class="text-center py-4">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{:else}
	<ExpenseForm
		bind:model
		{users}
		buttonText="Add Expense"
		{submitting}
		{error}
		onSubmit={handleSubmit}
	/>
{/if}
