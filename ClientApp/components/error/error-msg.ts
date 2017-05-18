export function logError(error: Error) {
    console.log(`${error.name}\n${error.message}\n\n${error.stack}`);
}

export function showErrorMsg(message: string) {
    alert(message);
}

export function showErrorMsgAndLog(message: string, error: Error) {
    console.log(`${error.name}\n${error.message}\n\n${error.stack}`);
    showErrorMsg(message);
}