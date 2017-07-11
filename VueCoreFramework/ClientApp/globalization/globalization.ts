import { store } from '../store/store';
import _t from '../globalization/translate';
import * as ErrorMsg from '../error-msg';

export const defaultCulture = "en-US";

export function setCulture(culture: string) {
    fetch(`dist/globalization/messages/${culture}.json`,
        {
            method: 'GET',
            headers: {
                'Accept': `application/json;v=${store.state.apiVer}`,
                'Accept-Language': culture
            }
        })
        .then(response => response.json())
        .then(data => {
            _t.setTranslation(data);
        })
        .catch(error => {
            ErrorMsg.logError('globalization.setCulture', error);
        });
}
setCulture(defaultCulture);