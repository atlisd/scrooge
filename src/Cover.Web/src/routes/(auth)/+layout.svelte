<script lang="ts">
	import type { Snippet } from 'svelte';

	let { children }: { children: Snippet } = $props();
</script>

<main class="container">
	<div class="row justify-content-center mt-5">
		<div class="col-md-6 col-lg-5">
			{@render children()}
		</div>
	</div>
</main>
