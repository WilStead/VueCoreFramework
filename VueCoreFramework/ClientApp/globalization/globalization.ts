import _t from '../globalization/translate';
import * as ErrorMsg from '../error-msg';

export const defaultCulture = "en-US";

export function setCulture(culture: string) {
    fetch(`dist/globalization/messages/${culture}.json`)
        .then(response => response.json())
        .then(data => {
            _t.setTranslation(data);
        })
        .catch(error => {
            ErrorMsg.logError('globalization.setCulture', error);
        });
}
setCulture(defaultCulture);