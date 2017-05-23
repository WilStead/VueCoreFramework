import { store } from '../../store/store';

export function logError(error: Error) {
    console.log(`${error.name}\n${error.message}\n\n${error.stack}`);
}

export function showErrorMsg(message: string) {
    store.state.error.message = message;
    store.state.error.dialogShown = true;
}

export function showErrorMsgAndLog(message: string, error: Error) {
    logError(error);
    showErrorMsg(message);
}