import { store } from './store/store';
import { JL } from 'jsnlog';

/**
 * Logs the provided error to JSNLog.
 * @param {string} origin The site of the error, used as the logger name.
 * @param error The error to be logged.
 */
export function logError(origin: string, error: Error) {
    JL(origin).error(`${error.name}\n${error.message}\n\n${error.stack}`);
}

/**
 * Displays an error message to the user as a modal dialog.
 * @param {string} message The message to display.
 */
export function showErrorMsg(message: string) {
    store.state.error.message = message;
    store.state.error.dialogShown = true;
}

/**
 * Displays an error message to the user as a modal dialog, and logs an error to JSNLog.
 * @param {string} origin The site of the error, used as the logger name.
 * @param {string} message The message to display.
 * @param error The error to be logged.
 */
export function showErrorMsgAndLog(origin: string, message: string, error: Error) {
    logError(origin, error);
    showErrorMsg(message);
}