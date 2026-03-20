<script lang="ts">
	import { onMount, type Snippet } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { getSetupStatus, getAuthStatus, getUsers } from '$lib/api';
	import { setCurrency } from '$lib/currency';
	import { initUsers } from '$lib/user';
	import '../app.css';

	let { children }: { children: Snippet } = $props();

	let loaded = $state(false);

	onMount(async () => {
		try {
			const status = await getSetupStatus();
			setCurrency(status.currencyCode, status.currencyDecimals);

			if (!status.isComplete) {
				await goto('/setup');
				return;
			}

			const authenticated = await getAuthStatus();

			if (!authenticated) {
				await goto('/login');
				return;
			}

			const users = await getUsers();
			initUsers(users);

			const path = $page.url.pathname;
			if (path === '/setup' || path === '/login') {
				await goto('/');
			}
		} catch {
			// fall through to finally
		} finally {
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
