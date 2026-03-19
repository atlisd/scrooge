<script lang="ts">
	import { onMount, type Snippet } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { getSetupStatus, getAuthStatus } from '$lib/api';
	import { setCurrency } from '$lib/currency';
	import '../app.css';

	let { children }: { children: Snippet } = $props();

	let loaded = $state(false);

	onMount(async () => {
		try {
			const status = await getSetupStatus();
			setCurrency(status.currencyCode, status.currencyDecimals);

			if (!status.isComplete) {
				loaded = true;
				await goto('/setup');
				return;
			}

			const authenticated = await getAuthStatus();
			loaded = true;

			if (!authenticated) {
				await goto('/login');
				return;
			}

			const path = $page.url.pathname;
			if (path === '/setup' || path === '/login') {
				await goto('/');
			}
		} catch {
			loaded = true;
		}
	});
</script>

{#if loaded}
	{@render children()}
{:else}
	<div class="spinner-overlay">
		<div class="spinner-border text-primary" role="status">
			<span class="visually-hidden">Loading...</span>
		</div>
	</div>
{/if}
