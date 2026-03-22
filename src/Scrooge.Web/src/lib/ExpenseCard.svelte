<script lang="ts">
	import type { ExpenseDto } from '$lib/types';
	import { formatAmount, formatDate } from '$lib/currency';
	import { goto } from '$app/navigation';
	import { activeUser } from '$lib/user';

	let { expense }: { expense: ExpenseDto } = $props();

	function click() {
		goto(`/edit/${expense.id}`);
	}

	let displayName = $derived(expense.merchant || expense.description || '—');
	let secondaryText = $derived(
		expense.merchant && expense.description ? expense.description : null
	);
	let isYou = $derived($activeUser !== null && expense.paidById === $activeUser.id);
	let paidByLabel = $derived($activeUser ? (isYou ? 'You' : expense.paidByName) : expense.paidByName);
	let lentAmount = $derived(
		expense.splitType === 'FullOther' ? expense.amount :
		expense.splitType === 'Equal'     ? Math.round(expense.amount / 2) :
		0
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
					{paidByLabel} paid {formatAmount(expense.amount)} &middot; {formatDate(expense.date)}
				</div>
			</div>
			<div class="text-end">
				{#if $activeUser && lentAmount > 0}
					{#if isYou}
						<small class="text-success">you lent</small>
					{:else}
						<small class="text-danger">you borrowed</small>
					{/if}
					<div class="fw-semibold" class:text-success={isYou} class:text-danger={!isYou}>{formatAmount(lentAmount)}</div>
				{/if}
			</div>
		</div>
	</div>
</div>
