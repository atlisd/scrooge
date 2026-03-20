<script lang="ts">
	import type { BalanceDto } from '$lib/types';
	import { formatAmount } from '$lib/currency';
	import { activeUser } from '$lib/user';

	let { balance }: { balance: BalanceDto | null } = $props();
</script>

{#if balance}
	<div class="card mb-3 balance-card" class:owes={balance.owedAmount > 0}>
		<div class="card-body">
			{#if balance.owedAmount === 0}
				<p class="card-text mb-0 fw-semibold text-success">All settled!</p>
			{:else if $activeUser}
				{@const otherUser =
					$activeUser.id === balance.user1.userId ? balance.user2 : balance.user1}
				{#if $activeUser.id === balance.owesUserId}
					<p class="card-text mb-0 fw-semibold">
						You owe {otherUser.name}
						<span class="text-danger">{formatAmount(balance.owedAmount)}</span>
					</p>
				{:else}
					<p class="card-text mb-0 fw-semibold">
						{otherUser.name} owes you
						<span class="text-success">{formatAmount(balance.owedAmount)}</span>
					</p>
				{/if}
			{:else}
				{@const owerName =
					balance.owesUserId === balance.user1.userId ? balance.user1.name : balance.user2.name}
				{@const owedName =
					balance.isOwedUserId === balance.user1.userId ? balance.user1.name : balance.user2.name}
				<p class="card-text mb-0 fw-semibold">
					{owerName} owes {owedName}
					<span class="text-danger">{formatAmount(balance.owedAmount)}</span>
				</p>
			{/if}
		</div>
	</div>
{/if}
