import { store } from './store/store';
import { checkResponse } from './router';

export const urls = {
    apiUrl: "https://localhost:44325/",
    authUrl: "https://localhost:44300/",
    spaUrl: "https://localhost:44350/"
};

export function callApi(relUrl: string, init?: RequestInit): Promise<Response> {
    if (init) {
        return fetch(urls.apiUrl + relUrl, init);
    } else {
        return fetch(urls.apiUrl + relUrl);
    }
}

export function callSpa(relUrl: string, init?: RequestInit): Promise<Response> {
    if (init) {
        return fetch(urls.spaUrl + relUrl, init);
    } else {
        return fetch(urls.spaUrl + relUrl);
    }
}

function invokeHost(hostUrl: string, relUrl: string, method: string, returnPath?: string) {
    let headers = {
        'Accept': `application/json;v=${store.state.apiVer}`,
        'Accept-Language': store.state.userState.culture
    };
    if (store.state.userState.user) {
        headers['Authorization'] = `bearer ${store.state.userState.user.access_token}`;
    }
    let f = fetch(hostUrl + relUrl,
        {
            method,
            headers
        });
    if (returnPath) {
        return f.then(response => checkResponse(response, returnPath));
    } else {
        return f;
    }
}

export function getApi(relUrl: string, returnPath?: string): Promise<Response> {
    return invokeHost(urls.apiUrl, relUrl, 'GET', returnPath);
}

export function getSpa(relUrl: string, returnPath?: string): Promise<Response> {
    return invokeHost(urls.spaUrl, relUrl, 'GET', returnPath);
}

export function postApi(relUrl: string, returnPath?: string): Promise<Response> {
    return invokeHost(urls.apiUrl, relUrl, 'POST', returnPath);
}

export function postSpa(relUrl: string, returnPath?: string): Promise<Response> {
    return invokeHost(urls.spaUrl, relUrl, 'POST', returnPath);
}
