import { store } from './store/store';
import { JL } from 'jsnlog';

export function logError(origin: string, error: Error) {
    JL(origin).error(`${error.name}\n${error.message}\n\n${error.stack}`);
}

export function showErrorMsg(message: string) {
    store.state.error.message = message;
    store.state.error.dialogShown = true;
}

export function showErrorMsgAndLog(origin: string, message: string, error: Error) {
    logError(origin, error);
    showErrorMsg(message);
}