<script lang="ts">
	import type { BalanceDto } from '$lib/types';
	import { formatAmount } from '$lib/currency';

	let { balance }: { balance: BalanceDto | null } = $props();
</script>

{#if balance}
	<div class="card mb-3 balance-card" class:owes={balance.owedAmount > 0}>
		<div class="card-body">
			{#if balance.owedAmount === 0}
				<p class="card-text mb-0 fw-semibold text-success">All settled!</p>
			{:else}
				{@const owerName = balance.owesUserId === balance.user1.userId
					? balance.user1.name
					: balance.user2.name}
				{@const owedName = balance.isOwedUserId === balance.user1.userId
					? balance.user1.name
					: balance.user2.name}
				<p class="card-text mb-0 fw-semibold">
					{owerName} owes {owedName}
					<span class="text-danger">{formatAmount(balance.owedAmount)}</span>
				</p>
			{/if}
		</div>
	</div>
{/if}
