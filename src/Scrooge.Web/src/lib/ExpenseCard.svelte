<script lang="ts">
	import type { ExpenseDto } from '$lib/types';
	import { formatAmount, formatDate } from '$lib/currency';
	import { goto } from '$app/navigation';

	let { expense }: { expense: ExpenseDto } = $props();

	function click() {
		goto(`/edit/${expense.id}`);
	}

	let displayName = $derived(expense.merchant || expense.description || '—');
	let secondaryText = $derived(
		expense.merchant && expense.description ? expense.description : null
	);
</script>

<div
	class="card mb-2 expense-card"
	onclick={click}
	onkeydown={(e) => e.key === 'Enter' && click()}
	role="button"
	tabindex="0"
>
	<div class="card-body py-2 px-3">
		<div class="d-flex justify-content-between align-items-start">
			<div>
				<div class="fw-semibold">{displayName}</div>
				{#if secondaryText}
					<small class="text-muted">{secondaryText}</small>
				{/if}
				<div class="text-muted small">
					{expense.paidByName} paid &middot; {formatDate(expense.date)}
				</div>
			</div>
			<div class="text-end">
				<div class="fw-semibold">{formatAmount(expense.amount)}</div>
				{#if expense.splitType === 'FullOther'}
					<span class="badge bg-warning text-dark split-badge">100% other</span>
				{/if}
			</div>
		</div>
	</div>
</div>
