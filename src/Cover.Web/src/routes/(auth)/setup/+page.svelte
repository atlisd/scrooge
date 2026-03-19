<script lang="ts">
	import { goto } from '$app/navigation';
	import { setup } from '$lib/api';

	let name1 = $state('');
	let name2 = $state('');
	let currency = $state('ISK');
	let currencyDecimals = $state(0);
	let username = $state('');
	let password = $state('');
	let error = $state('');
	let submitting = $state(false);

	const currencies = [
		{ code: 'ISK', label: 'ISK – Icelandic Króna' },
		{ code: 'USD', label: 'USD – US Dollar' },
		{ code: 'EUR', label: 'EUR – Euro' },
		{ code: 'GBP', label: 'GBP – British Pound' },
		{ code: 'DKK', label: 'DKK – Danish Krone' },
		{ code: 'NOK', label: 'NOK – Norwegian Krone' },
		{ code: 'SEK', label: 'SEK – Swedish Krona' },
		{ code: 'CAD', label: 'CAD – Canadian Dollar' },
		{ code: 'AUD', label: 'AUD – Australian Dollar' },
		{ code: 'CHF', label: 'CHF – Swiss Franc' },
		{ code: 'JPY', label: 'JPY – Japanese Yen' }
	];

	async function handleSubmit(e: Event) {
		e.preventDefault();
		error = '';

		if (!name1.trim() || !name2.trim()) {
			error = 'Both names are required.';
			return;
		}
		if (!username.trim() || !password.trim()) {
			error = 'Username and password are required.';
			return;
		}
		if (password.length < 12) {
			error = 'Password must be at least 12 characters.';
			return;
		}
		if (!/[A-Z]/.test(password)) {
			error = 'Password must contain an uppercase letter.';
			return;
		}
		if (!/[a-z]/.test(password)) {
			error = 'Password must contain a lowercase letter.';
			return;
		}
		if (!/[0-9]/.test(password)) {
			error = 'Password must contain a digit.';
			return;
		}
		if (!/[^A-Za-z0-9]/.test(password)) {
			error = 'Password must contain a special character.';
			return;
		}

		submitting = true;
		try {
			await setup({
				name1: name1.trim(),
				name2: name2.trim(),
				username: username.trim(),
				password,
				currency,
				currencyDecimals
			});
			await goto('/login');
		} catch (err: any) {
			error = err.message || 'Setup failed.';
		} finally {
			submitting = false;
		}
	}
</script>

<div class="card mt-4">
	<div class="card-body">
		<h3 class="card-title mb-3">Setup Cover</h3>

		{#if error}
			<div class="alert alert-danger">{error}</div>
		{/if}

		<form onsubmit={handleSubmit}>
			<div class="mb-3">
				<label for="name1" class="form-label">Person 1</label>
				<input id="name1" type="text" class="form-control" bind:value={name1} required />
			</div>

			<div class="mb-3">
				<label for="name2" class="form-label">Person 2</label>
				<input id="name2" type="text" class="form-control" bind:value={name2} required />
			</div>

			<div class="mb-3">
				<label for="currency" class="form-label">Currency</label>
				<select id="currency" class="form-select" bind:value={currency}>
					{#each currencies as c}
						<option value={c.code}>{c.label}</option>
					{/each}
				</select>
			</div>

			<div class="mb-3">
				<label for="decimals" class="form-label">Decimal places</label>
				<select id="decimals" class="form-select" bind:value={currencyDecimals}>
					<option value={0}>0 (e.g. ISK, JPY)</option>
					<option value={2}>2 (e.g. USD, EUR)</option>
					<option value={3}>3</option>
				</select>
			</div>

			<hr />

			<div class="mb-3">
				<label for="username" class="form-label">Username</label>
				<input id="username" type="text" class="form-control" bind:value={username} required />
			</div>

			<div class="mb-3">
				<label for="password" class="form-label">Password</label>
				<input id="password" type="password" class="form-control" bind:value={password} required />
				<div class="form-text">
					Min 12 characters with uppercase, lowercase, digit, and special character.
				</div>
			</div>

			<button type="submit" class="btn btn-primary w-100" disabled={submitting}>
				{#if submitting}
					<span class="spinner-border spinner-border-sm me-1" role="status"></span>
				{/if}
				Complete Setup
			</button>
		</form>
	</div>
</div>
