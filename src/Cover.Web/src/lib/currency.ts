import { writable } from 'svelte/store';

export const currencyCode = writable('ISK');
export const currencyDecimals = writable(0);

let _code = 'ISK';
let _decimals = 0;

const currencyLocaleMap: Record<string, string> = {
	ISK: 'is-IS',
	DKK: 'da-DK',
	NOK: 'nb-NO',
	SEK: 'sv-SE',
	EUR: 'de-DE',
	GBP: 'en-GB',
	USD: 'en-US',
	CAD: 'en-CA',
	AUD: 'en-AU',
	CHF: 'de-CH',
	JPY: 'ja-JP'
};

export function setCurrency(code: string, decimals: number) {
	_code = code;
	_decimals = decimals;
	currencyCode.set(code);
	currencyDecimals.set(decimals);
}

export function getCurrencyCode(): string {
	return _code;
}

export function getCurrencyDecimals(): number {
	return _decimals;
}

export function formatAmount(minorUnits: number): string {
	const locale = currencyLocaleMap[_code] || 'en-US';
	const divisor = Math.pow(10, _decimals);
	const value = minorUnits / divisor;

	const formatted = new Intl.NumberFormat(locale, {
		minimumFractionDigits: _decimals,
		maximumFractionDigits: _decimals,
		useGrouping: true
	}).format(value);

	return `${formatted} ${_code}`;
}

export function formatDate(dateStr: string): string {
	const date = new Date(dateStr + 'T00:00:00');
	const d = date.getDate();
	const m = date.getMonth() + 1;
	const y = date.getFullYear();

	const locale = currencyLocaleMap[_code] || 'en-US';

	if (locale === 'is-IS') return `${d}.${m}.${y}`;
	if (locale === 'da-DK' || locale === 'nb-NO' || locale === 'sv-SE' || locale === 'de-DE' || locale === 'de-CH')
		return `${d}.${m}.${y}`;
	if (locale === 'en-GB' || locale === 'en-AU') return `${d}/${m}/${y}`;
	if (locale === 'en-US' || locale === 'en-CA') return `${m}/${d}/${y}`;
	if (locale === 'ja-JP') return `${y}/${m}/${d}`;

	return `${d}.${m}.${y}`;
}

export function toMinorUnits(displayValue: number): number {
	const divisor = Math.pow(10, _decimals);
	return Math.round(displayValue * divisor);
}

export function toDisplayValue(minorUnits: number): number {
	const divisor = Math.pow(10, _decimals);
	return minorUnits / divisor;
}

export function getDecimalSeparator(): string {
	const locale = currencyLocaleMap[_code] || 'en-US';
	// Most European locales use comma
	if (['is-IS', 'da-DK', 'nb-NO', 'sv-SE', 'de-DE', 'de-CH'].includes(locale)) return ',';
	return '.';
}

export function parseAmountInput(input: string): number | null {
	if (!input || !input.trim()) return null;
	// Accept both comma and dot as decimal separator, normalize to dot
	const normalized = input.trim().replace(',', '.');
	const value = parseFloat(normalized);
	if (isNaN(value) || value <= 0) return null;
	return value;
}

export function formatDisplayValue(minorUnits: number): string {
	const divisor = Math.pow(10, _decimals);
	const value = minorUnits / divisor;
	const sep = getDecimalSeparator();
	if (_decimals === 0) return String(value);
	return value.toFixed(_decimals).replace('.', sep);
}

export function todayString(): string {
	const now = new Date();
	const y = now.getFullYear();
	const m = String(now.getMonth() + 1).padStart(2, '0');
	const d = String(now.getDate()).padStart(2, '0');
	return `${y}-${m}-${d}`;
}

/** Converts ISO date (yyyy-mm-dd) to display format based on currency locale */
export function isoToDisplayDate(iso: string): string {
	if (!iso) return '';
	const [y, m, d] = iso.split('-').map(Number);
	const locale = currencyLocaleMap[_code] || 'en-US';

	if (locale === 'en-US' || locale === 'en-CA') return `${m}/${d}/${y}`;
	if (locale === 'en-GB' || locale === 'en-AU') return `${d}/${m}/${y}`;
	if (locale === 'ja-JP') return `${y}/${m}/${d}`;
	// Default European: d.m.yyyy
	return `${d}.${m}.${y}`;
}

/** Parses a display-format date back to ISO yyyy-mm-dd. Accepts d.m.yyyy, d/m/yyyy, etc. */
export function parseDisplayDate(input: string): string | null {
	if (!input || !input.trim()) return null;
	const trimmed = input.trim();

	// Try d.m.yyyy or d/m/yyyy or d-m-yyyy
	const match = trimmed.match(/^(\d{1,2})[./\-](\d{1,2})[./\-](\d{4})$/);
	if (match) {
		const locale = currencyLocaleMap[_code] || 'en-US';
		let day: number, month: number, year: number;

		if (locale === 'en-US' || locale === 'en-CA') {
			// m/d/yyyy
			month = parseInt(match[1], 10);
			day = parseInt(match[2], 10);
		} else if (locale === 'ja-JP') {
			// yyyy/m/d — but pattern expects d.m.yyyy, so this won't match yyyy first
			day = parseInt(match[1], 10);
			month = parseInt(match[2], 10);
		} else {
			// d.m.yyyy (European)
			day = parseInt(match[1], 10);
			month = parseInt(match[2], 10);
		}
		year = parseInt(match[3], 10);

		if (month >= 1 && month <= 12 && day >= 1 && day <= 31 && year >= 2000) {
			return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
		}
	}

	// Try yyyy-mm-dd (ISO) passthrough
	const isoMatch = trimmed.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/);
	if (isoMatch) {
		const year = parseInt(isoMatch[1], 10);
		const month = parseInt(isoMatch[2], 10);
		const day = parseInt(isoMatch[3], 10);
		if (month >= 1 && month <= 12 && day >= 1 && day <= 31) {
			return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
		}
	}

	return null;
}

/** Returns the date format hint for the current locale */
export function getDateFormatHint(): string {
	const locale = currencyLocaleMap[_code] || 'en-US';
	if (locale === 'en-US' || locale === 'en-CA') return 'm/d/yyyy';
	if (locale === 'en-GB' || locale === 'en-AU') return 'd/m/yyyy';
	if (locale === 'ja-JP') return 'yyyy/m/d';
	return 'd.m.yyyy';
}
