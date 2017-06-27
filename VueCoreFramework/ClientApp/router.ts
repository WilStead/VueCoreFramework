import VueRouter from 'vue-router';
import * as Store from './store/store';
import { PermissionData } from './store/userStore';
import * as ErrorMsg from './error-msg';

const routes: Array<VueRouter.RouteConfig> = [
    { path: '/', component: require('./components/home/home.vue') },
    {
        path: '/login',
        component: require('./components/user/login.vue'),
        props: (route) => ({ returnUrl: route.query.returnUrl })
    },
    {
        path: '/register',
        component: require('./components/user/register.vue'),
        props: (route) => ({ returnUrl: route.query.returnUrl })
    },
    {
        path: '/user/manage',
        meta: { requiresAuthenticate: true },
        component: require('./components/user/manage.vue')
    },
    {
        path: '/user/email/confirm',
        component: require('./components/user/email/confirm.vue')
    },
    {
        path: '/user/email/restore',
        component: require('./components/user/email/restore.vue')
    },
    {
        path: '/user/reset/:code',
        component: require('./components/user/password/reset.vue'),
        props: true
    },
    {
        path: '/group/manage',
        meta: { requiresAuthenticate: true },
        component: require('./components/group/manage.vue')
    },
    { path: '/error/notfound', component: resolve => require(['./components/error/notfound.vue'], resolve) },
    { path: '/error/:code', component: resolve => require(['./components/error/error.vue'], resolve), props: true }
];

/**
 * The SPA framework's VueRouter instance.
 */
export const router = new VueRouter({
    mode: 'history',
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition; // return to last place if using back/forward
        } else if (to.hash) {
            return { selector: to.hash }; // scroll to anchor if provided
        } else {
            return { x: 0, y: 0 }; // reset to top-left
        }
    }
});
router.beforeEach((to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuthorize)) {
        checkAuthorization(to)
            .then(auth => {
                if (auth === "login") {
                    next({ path: '/login', query: { returnUrl: to.fullPath } });
                } else if (auth === "unauthorized") {
                    next({ path: '/error/401' });
                } else {
                    next();
                }
            })
            .catch(error => {
                ErrorMsg.logError("router.beforeEach", error);
                next({ path: '/login', query: { returnUrl: to.fullPath } });
            });
    } else if (to.matched.some(record => record.meta.requiresAuthenticate)) {
        authenticate()
            .then(auth => {
                if (auth === "authorized") {
                    next();
                } else {
                    next({ path: '/login', query: { returnUrl: to.fullPath } });
                }
            });
    } else {
        next();
    }
});

/**
 * A ViewModel used to receive a response from an API call, with an error and response string.
 */
export interface ApiResponseViewModel {
    error: string,
    response: string
}

/**
 * A ViewModel used to transfer information during user account authorization tasks.
 */
interface AuthorizationViewModel {
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
 * Calls an API endpoint which authenticates the current user.
 * @returns {string} Either 'authorized' or 'login' if the user must sign in.
 */
export function authenticate(): Promise<string> {
    let url = '/api/Authorization/Authenticate/';
    let full = false;
    if (!Store.store.state.userState.email
        || Store.store.state.userState.email === "user@example.com") {
        full = true;
        url += '?full=true';
    }
    return fetch(url,
        {
            headers: {
                'Authorization': `bearer ${Store.store.state.userState.token}`
            }
        })
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
                return "login";
            } else {
                return "authorized";
            }
        })
        .catch(error => {
            if (error.message !== "login") {
                ErrorMsg.logError("router.checkAuthorization", new Error(error));
            }
            return "login";
        });
}

/**
 * Calls an API endpoint which authorizes the current user for the route being navigated to.
 * @param to The Route being navigated to.
 * @returns {string} Either 'authorized' or 'unauthorized' or 'login' if the user must sign in.
 */
export function checkAuthorization(to: VueRouter.Route): Promise<string> {
    let url = '/api/Authorization/Authorize/';
    let dataType = to.name;
    let op: string;
    let id: string;
    if (dataType.endsWith("DataTable")) {
        dataType = dataType.substring(0, dataType.length - 9);
    } else {
        op = to.params.operation;
        id = to.params.id;
    }
    url += dataType;
    if (op) url += `?operation=${op}`;
    if (id) {
        if (op) {
            url += '&';
        } else {
            url += '?';
        }
        url += `id=${id}`;
    }
    return fetch(url,
        {
            headers: {
                'Authorization': `bearer ${Store.store.state.userState.token}`
            }
        })
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
                ErrorMsg.logError("router.checkAuthorization", new Error(error));
            }
            return "unauthorized";
        });
}

/**
 * Verifies that the response of an API call was an OK response. If not, redirects to the login
 * page on 401, and throws an error otherwise.
 * @param response
 * @param {string} returnPath The page to redirect to after a successful login, if required.
 */
export function checkResponse(response: Response, returnPath: string) {
    if (!response.ok) {
        if (response.status === 401) {
            router.push({ path: '/login', query: { returnUrl: returnPath } });
        }
        throw Error(response.statusText);
    }
    return response;
}