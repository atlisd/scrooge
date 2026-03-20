<script lang="ts">
	import type { Snippet } from 'svelte';
	import NavMenu from '$lib/NavMenu.svelte';

	let { children }: { children: Snippet } = $props();
</script>

<div class="d-flex flex-column min-vh-100">
	<NavMenu />
	<main class="container flex-grow-1 py-3">
		{@render children()}
	</main>
</div>
