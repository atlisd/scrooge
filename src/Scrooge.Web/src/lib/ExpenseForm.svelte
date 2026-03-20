<script lang="ts">
	import type { UserDto, SplitType } from '$lib/types';
	import { getMerchants } from '$lib/api';
	import { getDateFormatHint, isoToDisplayDate, parseDisplayDate } from '$lib/currency';

	interface ExpenseFormModel {
		merchant: string;
		description: string;
		amount: string;
		paidById: number;
		splitType: SplitType;
		date: string;
	}

	let {
		model = $bindable(),
		users,
		buttonText = 'Save',
		submitting = false,
		error = null,
		showDescription = true,
		onSubmit
	}: {
		model: ExpenseFormModel;
		users: UserDto[];
		buttonText?: string;
		submitting?: boolean;
		error?: string | null;
		showDescription?: boolean;
		onSubmit: () => void;
	} = $props();

	let merchants = $state<string[]>([]);
	let showMerchants = $state(false);
	let blurTimeout: ReturnType<typeof setTimeout> | null = null;

	function autofocus(node: HTMLInputElement) {
		node.focus();
	}

	async function onMerchantInput() {
		if (model.merchant.length >= 1) {
			merchants = await getMerchants(model.merchant);
			showMerchants = merchants.length > 0;
		} else {
			showMerchants = false;
		}
	}

	function selectMerchant(m: string) {
		model.merchant = m;
		showMerchants = false;
	}

	function onMerchantBlur() {
		blurTimeout = setTimeout(() => {
			showMerchants = false;
		}, 150);
	}

	function onMerchantFocus() {
		if (blurTimeout) {
			clearTimeout(blurTimeout);
			blurTimeout = null;
		}
	}

	let datePickerInput: HTMLInputElement;

	function openDatePicker() {
		// Set the hidden date input to the current model value so the picker opens on the right date
		const iso = parseDisplayDate(model.date);
		if (iso) datePickerInput.value = iso;
		// iOS Safari requires .focus() — showPicker() is not supported there
		datePickerInput.focus();
		try {
			datePickerInput.showPicker();
		} catch {
			// showPicker not supported (iOS Safari) — focus() above is sufficient
		}
	}

	function onDatePicked(e: Event) {
		const target = e.target as HTMLInputElement;
		if (target.value) {
			model.date = isoToDisplayDate(target.value);
		}
	}

	function handleSubmit(e: Event) {
		e.preventDefault();
		onSubmit();
	}
</script>

<form onsubmit={handleSubmit}>
	{#if error}
		<div class="alert alert-danger">{error}</div>
	{/if}

	<div class="mb-3">
		<label for="amount" class="form-label">Amount</label>
		<input
			id="amount"
			type="text"
			inputmode="decimal"
			class="form-control"
			maxlength="20"
			bind:value={model.amount}
			use:autofocus
			required
		/>
	</div>

	<div class="mb-3 position-relative">
		<label for="merchant" class="form-label">Merchant</label>
		<input
			id="merchant"
			type="text"
			class="form-control"
			maxlength="200"
			bind:value={model.merchant}
			oninput={onMerchantInput}
			onblur={onMerchantBlur}
			onfocus={onMerchantFocus}
			autocomplete="off"
			autocapitalize="sentences"
		/>
		{#if showMerchants}
			<div class="autocomplete-dropdown">
				{#each merchants as m}
					<div
						class="dropdown-item"
						onmousedown={() => selectMerchant(m)}
						role="option"
						aria-selected="false"
						tabindex="-1"
					>
						{m}
					</div>
				{/each}
			</div>
		{/if}
	</div>

	{#if showDescription}
		<div class="mb-3">
			<label for="description" class="form-label">Description</label>
			<input
				id="description"
				type="text"
				class="form-control"
				maxlength="500"
				bind:value={model.description}
				autocapitalize="sentences"
			/>
		</div>
	{/if}

	<div class="mb-3">
		<label for="paidBy" class="form-label">Paid by</label>
		<select id="paidBy" class="form-select" bind:value={model.paidById}>
			{#each users as user}
				<option value={user.id}>{user.name}</option>
			{/each}
		</select>
	</div>

	<div class="mb-3">
		<span class="form-label d-block">Split</span>
		<div>
			<div class="form-check form-check-inline">
				<input
					class="form-check-input"
					type="radio"
					id="splitEqual"
					value="Equal"
					bind:group={model.splitType}
				/>
				<label class="form-check-label" for="splitEqual">50/50</label>
			</div>
			<div class="form-check form-check-inline">
				<input
					class="form-check-input"
					type="radio"
					id="splitFull"
					value="FullOther"
					bind:group={model.splitType}
				/>
				<label class="form-check-label" for="splitFull">100% other</label>
			</div>
		</div>
	</div>

	<div class="mb-3">
		<label for="date" class="form-label">Date</label>
		<div class="input-group">
			<input
				id="date"
				type="text"
				inputmode="numeric"
				class="form-control"
				placeholder={getDateFormatHint()}
				bind:value={model.date}
				required
			/>
			<div class="position-relative">
				<button type="button" class="btn btn-outline-secondary" onclick={openDatePicker} aria-label="Open calendar">
					&#128197;
				</button>
				<input
					type="date"
					bind:this={datePickerInput}
					onchange={onDatePicked}
					tabindex={-1}
					aria-hidden="true"
					style="position:absolute;top:0;left:0;width:100%;height:100%;opacity:0;"
				/>
			</div>
		</div>
	</div>

	<button type="submit" class="btn btn-primary w-100" disabled={submitting}>
		{#if submitting}
			<span class="spinner-border spinner-border-sm me-1" role="status"></span>
		{/if}
		{buttonText}
	</button>
</form>
