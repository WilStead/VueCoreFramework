import * as Api from '../api';
import _t from '../globalization/translate';
import * as ErrorMsg from '../error-msg';

export const defaultCulture = "en-US";

export async function setCulture(culture: string) {
    try {
        let response = await Api.callFetch(`dist/globalization/messages/${culture}.json`);
        let data = await response.json();
        _t.setTranslation(data);
    } catch (error) {
        ErrorMsg.logError('globalization.setCulture', error);
    }
}