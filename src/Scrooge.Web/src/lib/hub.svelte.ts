import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import type { HubConnection } from '@microsoft/signalr';

export const hub = $state({ expenseRevision: 0 });

let connection: HubConnection | null = null;

export async function startHub(): Promise<void> {
	if (connection) return;

	connection = new HubConnectionBuilder()
		.withUrl('/hubs/expenses')
		.withAutomaticReconnect()
		.build();

	connection.on('ExpenseChanged', () => {
		hub.expenseRevision++;
	});

	await tryConnect();
	document.addEventListener('visibilitychange', onVisibilityChange);
}

export async function stopHub(): Promise<void> {
	document.removeEventListener('visibilitychange', onVisibilityChange);
	if (connection) {
		await connection.stop();
		connection = null;
	}
}

async function tryConnect(): Promise<void> {
	try {
		await connection!.start();
	} catch {
		// silently ignore — live updates won't work but the app still functions
	}
}

async function onVisibilityChange(): Promise<void> {
	if (document.visibilityState === 'visible' && connection?.state === HubConnectionState.Disconnected) {
		await tryConnect();
	}
}
