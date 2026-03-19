<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { getUsers, getExpense, updateExpense, deleteExpense } from '$lib/api';
	import { toMinorUnits, formatDisplayValue, parseAmountInput, isoToDisplayDate, parseDisplayDate } from '$lib/currency';
	import type { UserDto, SplitType } from '$lib/types';
	import ExpenseForm from '$lib/ExpenseForm.svelte';

	let users = $state<UserDto[]>([]);
	let loading = $state(true);
	let submitting = $state(false);
	let error = $state<string | null>(null);
	let notFound = $state(false);
	let showDeleteConfirm = $state(false);
	let deleting = $state(false);

	let model = $state({
		merchant: '',
		description: '',
		amount: '',
		paidById: 0,
		splitType: 'Equal' as SplitType,
		date: ''
	});

	let expenseId = $derived(parseInt($page.params.id, 10));

	onMount(async () => {
		const [u, exp] = await Promise.all([getUsers(), getExpense(expenseId)]);
		users = u;

		if (!exp) {
			notFound = true;
			loading = false;
			return;
		}

		model = {
			merchant: exp.merchant || '',
			description: exp.description || '',
			amount: formatDisplayValue(exp.amount),
			paidById: exp.paidById,
			splitType: exp.splitType,
			date: isoToDisplayDate(exp.date)
		};
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
			await updateExpense(expenseId, {
				merchant: model.merchant || null,
				description: model.description || null,
				amount: toMinorUnits(parsedAmount),
				splitType: model.splitType,
				paidById: model.paidById,
				date: isoDate
			});
			await goto('/');
		} catch (err: any) {
			error = err.message || 'Failed to update expense.';
		} finally {
			submitting = false;
		}
	}

	async function handleDelete() {
		deleting = true;
		try {
			await deleteExpense(expenseId);
			await goto('/');
		} catch (err: any) {
			error = err.message || 'Failed to delete expense.';
			showDeleteConfirm = false;
		} finally {
			deleting = false;
		}
	}
</script>

<h4 class="mb-3">Edit Expense</h4>

{#if loading}
	<div class="text-center py-4">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{:else if notFound}
	<div class="alert alert-warning">Expense not found.</div>
{:else}
	<ExpenseForm
		bind:model
		{users}
		buttonText="Update Expense"
		{submitting}
		{error}
		onSubmit={handleSubmit}
	/>

	<hr />

	{#if showDeleteConfirm}
		<div class="alert alert-danger">
			<p class="mb-2">Are you sure you want to delete this expense?</p>
			<button class="btn btn-danger btn-sm me-2" onclick={handleDelete} disabled={deleting}>
				{#if deleting}
					<span class="spinner-border spinner-border-sm me-1" role="status"></span>
				{/if}
				Yes, delete
			</button>
			<button class="btn btn-secondary btn-sm" onclick={() => (showDeleteConfirm = false)}>
				Cancel
			</button>
		</div>
	{:else}
		<button class="btn btn-outline-danger w-100" onclick={() => (showDeleteConfirm = true)}>
			Delete Expense
		</button>
	{/if}
{/if}
