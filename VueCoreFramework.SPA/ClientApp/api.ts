import { store } from './store/store';
import { checkResponse } from './router';

export const urls = {
    apiUrl: "https://localhost:44325/",
    authUrl: "https://localhost:44300/",
    spaUrl: "https://localhost:44350/"
};

export function callFetch(url: string, method?: string, body?: any, cred?: boolean): Promise<Response> {
    if (!method) {
        method = 'GET';
    }
    let init: RequestInit = {
        method,
        headers: {
            'Accept': `application/json;v=${store.state.apiVer}`,
            'Accept-Language': store.state.userState.culture
        }
    };
    if (cred) {
        init.credentials = 'include';
        init.mode = 'cors';
    }
    if (store.state.userState.user) {
        init.headers['Authorization'] = `bearer ${store.state.userState.user.access_token}`;
    }
    if (body) {
        init.body = body;
        init.headers['Content-Type'] = `application/json;v=${store.state.apiVer}`;
    }
    return fetch(url, init);
}

function invokeHost(hostUrl: string, relUrl: string, returnPath: string, method: string, body: any, cred?: boolean) {
    let f = callFetch(hostUrl + relUrl, method, body, cred);
    if (returnPath) {
        return f.then(response => checkResponse(response, returnPath));
    } else {
        return f;
    }
}

export function getApi(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.apiUrl, relUrl, returnPath, 'GET', body);
}

export function getAuth(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.authUrl, relUrl, returnPath, 'GET', body, true);
}

export function getSpa(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.spaUrl, relUrl, returnPath, 'GET', body);
}

export function postApi(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.apiUrl, relUrl, returnPath, 'POST', body);
}

export function postAuth(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.authUrl, relUrl, returnPath, 'POST', body, true);
}

export function postSpa(relUrl: string, returnPath?: string, body?: any): Promise<Response> {
    return invokeHost(urls.spaUrl, relUrl, returnPath, 'POST', body);
}
