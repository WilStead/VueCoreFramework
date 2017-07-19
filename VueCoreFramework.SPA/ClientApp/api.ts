import { store } from './store/store';
import { checkResponse } from './router';

export const urls = {
    apiUrl: "https://localhost:44371/",
    authUrl: "https://localhost:44329/",
    spaUrl: "https://localhost:44333/"
};

export function callApi(relUrl: string, init?: RequestInit): Promise<Response> {
    if (init) {
        return fetch(urls.apiUrl + relUrl, init);
    } else {
        return fetch(urls.apiUrl + relUrl);
    }
}

export function getApi(relUrl: string, returnPath?: string): Promise<Response> {
    let f = fetch(urls.apiUrl + relUrl,
        {
            method: 'GET',
            headers: {
                'Accept': `application/json;v=${store.state.apiVer}`,
                'Accept-Language': store.state.userState.culture,
                'Authorization': `bearer ${store.state.userState.user.access_token}`
            }
        });
    if (returnPath) {
        return f.then(response => checkResponse(response, returnPath));
    } else {
        return f;
    }
}

export function postApi(relUrl: string, returnPath?: string): Promise<Response> {
    let f = fetch(urls.apiUrl + relUrl,
        {
            method: 'POST',
            headers: {
                'Accept': `application/json;v=${store.state.apiVer}`,
                'Accept-Language': store.state.userState.culture,
                'Authorization': `bearer ${store.state.userState.user.access_token}`
            }
        });
    if (returnPath) {
        return f.then(response => checkResponse(response, returnPath));
    } else {
        return f;
    }
}
