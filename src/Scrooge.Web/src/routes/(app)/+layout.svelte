<script lang="ts">
	import { onMount, onDestroy, type Snippet } from 'svelte';
	import NavMenu from '$lib/NavMenu.svelte';
	import { startHub, stopHub } from '$lib/hub.svelte';

	let { children }: { children: Snippet } = $props();

	onMount(() => startHub());
	onDestroy(() => stopHub());
</script>

<div class="d-flex flex-column min-vh-100">
	<NavMenu />
	<main class="container flex-grow-1 py-3">
		{@render children()}
	</main>
</div>
