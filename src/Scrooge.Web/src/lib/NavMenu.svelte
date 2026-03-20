<script lang="ts">
	import { page } from '$app/stores';
	import { logout } from '$lib/api';

	let collapsed = $state(true);

	function toggle() {
		collapsed = !collapsed;
	}

	let pathname = $derived($page.url.pathname);
</script>

<nav class="navbar navbar-expand-md navbar-dark bg-dark">
	<div class="container">
		<a class="navbar-brand" href="/">Scrooge</a>
		<button class="navbar-toggler" type="button" onclick={toggle} aria-label="Toggle navigation">
			<span class="navbar-toggler-icon"></span>
		</button>
		<div class="collapse navbar-collapse" class:show={!collapsed}>
			<ul class="navbar-nav me-auto">
				<li class="nav-item">
					<a class="nav-link" class:active={pathname === '/'} href="/">Dashboard</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" class:active={pathname === '/add'} href="/add">Add Expense</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" class:active={pathname === '/history'} href="/history">History</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" class:active={pathname === '/about'} href="/about">About</a>
				</li>
			</ul>
			<button class="btn btn-outline-light btn-sm" onclick={logout}>Logout</button>
		</div>
	</div>
</nav>
