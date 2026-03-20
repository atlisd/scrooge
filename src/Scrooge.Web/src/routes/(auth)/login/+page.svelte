<script lang="ts">
	import { login } from '$lib/api';
	import { getCachedUsers, setActiveUser } from '$lib/user';

	let username = $state('');
	let password = $state('');
	let error = $state('');
	let submitting = $state(false);

	let cachedUsers = getCachedUsers();
	let selectedUserId = $state<number | null>(
		parseInt(localStorage.getItem('activeUserId') || '') || cachedUsers[0]?.id || null
	);

	async function handleSubmit(e: Event) {
		e.preventDefault();
		error = '';
		submitting = true;

		try {
			const res = await login({ username, password });
			if (res.ok) {
				const user = cachedUsers.find((u) => u.id === selectedUserId);
				if (user) setActiveUser(user);
				window.location.href = '/';
			} else if (res.status === 429) {
				error = 'Too many login attempts. Please try again later.';
			} else {
				error = 'Invalid username or password.';
			}
		} catch (err: any) {
			error = err.message || 'Login failed.';
		} finally {
			submitting = false;
		}
	}
</script>

<div class="card mt-4">
	<div class="card-body">
		<h3 class="card-title mb-3">Login</h3>

		{#if error}
			<div class="alert alert-danger">{error}</div>
		{/if}

		<form onsubmit={handleSubmit}>
			<div class="mb-3">
				<label for="username" class="form-label">Username</label>
				<input id="username" type="text" class="form-control" bind:value={username} required />
			</div>

			<div class="mb-3">
				<label for="password" class="form-label">Password</label>
				<input id="password" type="password" class="form-control" bind:value={password} required />
			</div>

			{#if cachedUsers.length > 0}
				<div class="mb-3">
					<label class="form-label">Who are you?</label>
					{#each cachedUsers as user}
						<div class="form-check">
							<input
								class="form-check-input"
								type="radio"
								name="activeUser"
								bind:group={selectedUserId}
								value={user.id}
							/>
							<label class="form-check-label">{user.name}</label>
						</div>
					{/each}
				</div>
			{/if}

			<button type="submit" class="btn btn-primary w-100" disabled={submitting}>
				{#if submitting}
					<span class="spinner-border spinner-border-sm me-1" role="status"></span>
				{/if}
				Login
			</button>
		</form>
	</div>
</div>
