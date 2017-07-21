import Oidc from 'oidc-client';
import * as Api from './api';
import * as Store from './store/store';
import { PermissionData } from './store/userStore';
import { JL } from 'jsnlog';
import * as ErrorMsg from './error-msg';

export let authMgr: Oidc.UserManager;
let config: Oidc.UserManagerSettings;
export function configureOidc() {
    Oidc.Log.level = Oidc.Log.WARN;
    Oidc.Log.logger = JL("OIDC");
    config = {
        authority: Api.urls.authUrl,
        client_id: "vue.client",
        redirect_uri: `${Api.urls.spaUrl}oidc/callback`,
        silent_redirect_uri: `${Api.urls.spaUrl}oidc/silent_callback`,
        response_type: "id_token token",
        scope: "openid email profile vcfapi",
        post_logout_redirect_uri: Api.urls.spaUrl,
        automaticSilentRenew: true
    };
    authMgr = new Oidc.UserManager(config);
}

function nonce() {
    var guidHolder = 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx';
    var hex = '0123456789abcdef';
    var r = 0;
    var guidResponse = "";
    for (var i = 0; i < guidHolder.length; i++) {
        if (guidHolder[i] !== '-' && guidHolder[i] !== '4') {
            // each x and y needs to be random
            r = Math.random() * 16 | 0;
        }

        if (guidHolder[i] === 'x') {
            guidResponse += hex[r];
        } else if (guidHolder[i] === 'y') {
            // clock-seq-and-reserved first hex is filtered and remaining hex values are random
            r &= 0x3; // bit and with 0011 to set pos 2 to zero ?0??
            r |= 0x8; // set pos 3 to 1 as 1???
            guidResponse += hex[r];
        } else {
            guidResponse += guidHolder[i];
        }
    }
    return guidResponse;
}

export function login(returnUrl = '') {
    let url = `connect/authorize?client_id=${config.client_id}&scope=${config.scope}&redirect_uri=${config.redirect_uri}&state=${returnUrl}&response_type=${config.response_type}&nonce=${nonce()}&prompt=none`;
    Api.callAuth(url, {
        credentials: 'include'
    });
}

export function logout() {
    authMgr.removeUser()
        .then(() => {
            Api.callAuth('Account/Logout');
        });
}

/**
 * Authenticates the current user.
 * @returns {string} Either 'authorized' or 'login' if the user must sign in.
 */
export function authenticate(full?: boolean): Promise<string> {
    authMgr.getUser()
        .then(user => {
            Store.store.commit(Store.setUser, user);
            if (user) {
                return "authorized";
            } else {
                return "login";
            }
        });

    let url = 'Authorization/Authenticate/';
    if (full || !Store.store.state.userState.user) {
        full = true;
        url += '?full=true';
    }
    return Api.getAuth(url)
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw Error("login");
                }
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<AuthorizationViewModel>)
        .then(data => {
            if (data.token) {
                Store.store.commit(Store.setToken, data.token);
            }
            if (full) {
                Store.store.commit(Store.setUsername, data.username);
                Store.store.commit(Store.setEmail, data.email);
                Store.store.commit(Store.setIsAdmin, data.isAdmin);
                Store.store.commit(Store.setIsSiteAdmin, data.isSiteAdmin);
            }
            if (data.authorization === "login") {
                if (Store.store.state.userState.user) {
                    Store.store.commit(Store.setToken, '');
                }
                return "login";
            } else {
                return "authorized";
            }
        })
        .catch(error => {
            if (Store.store.state.userState.user) {
                Store.store.commit(Store.setToken, '');
            }
            if (error.message !== "login") {
                ErrorMsg.logError("router.checkAuthorization", new Error(error));
            }
            return "login";
        });
}

/**
 * A ViewModel used to transfer information during user account authorization tasks.
 */
export interface AuthorizationViewModel {
    /**
     * A value indicating whether the user is authorized for the requested action or not.
     */
    authorization: string;

    /**
     * Indicates that the user is authorized to share/hide the requested data.
     */
    canShare: string;

    /**
     * The email of the user account.
     */
    email: string;

    /**
     * Indicates that the current user is an administrator.
     */
    isAdmin: boolean;

    /**
     * Indicates that the current user is the site administrator.
     */
    isSiteAdmin: boolean;

    /**
     * A JWT bearer token.
     */
    token: string;

    /**
     * The username of the user account.
     */
    username: string;
}

/**
 * Calls an API endpoint which authorizes the current user for the data indicated.
 * @param {string} dataType The type of data requested.
 * @param {string} operation The type of operation to be performed on the data.
 * @param {string} id The primary key of the data item requested.
 * @returns {string} Either 'authorized' or 'unauthorized' or 'login' if the user must sign in.
 */
export function checkAuthorization(dataType: string, operation = '', id = ''): Promise<string> {
    let url = `Authorization/Authorize/${dataType}`;
    if (operation) url += `?operation=${operation}`;
    if (id) {
        if (operation) {
            url += '&';
        } else {
            url += '?';
        }
        url += `id=${id}`;
    }
    return Api.getAuth(url)
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    throw Error("unauthorized");
                }
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<AuthorizationViewModel>)
        .then(data => {
            if (data.authorization === "login") {
                if (Store.store.state.userState.user) {
                    Store.store.commit(Store.setToken, '');
                }
                return "login";
            }
            Store.store.commit(Store.setEmail, data.email);
            Store.store.commit(Store.setIsAdmin, data.isAdmin);
            Store.store.commit(Store.setIsSiteAdmin, data.isSiteAdmin);
            Store.store.commit(Store.setToken, data.token);
            Store.store.commit(Store.setUsername, data.username);
            let permission: PermissionData = { dataType };
            if (id) {
                permission.id = id;
            }
            permission.canShare = data.canShare;
            if (data.authorization !== "authorized"
                && data.authorization !== "unauthorized") {
                permission.permission = data.authorization;
            }
            Store.store.commit(Store.updatePermission, permission);
            return data.authorization;
        })
        .catch(error => {
            if (error.message !== "unauthorized") {
                ErrorMsg.logError("authorization.checkAuthorization", new Error(error));
            }
            return "unauthorized";
        });
}
