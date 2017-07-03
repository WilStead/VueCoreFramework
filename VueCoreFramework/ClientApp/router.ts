import VueRouter from 'vue-router';
import { authenticate, AuthorizationViewModel, checkAuthorization } from './authorization';
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
        authorize(to)
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
 * Calls an API endpoint which authorizes the current user for the route being navigated to.
 * @param to The Route being navigated to.
 * @returns {string} Either 'authorized' or 'unauthorized' or 'login' if the user must sign in.
 */
export function authorize(to: VueRouter.Route): Promise<string> {
    let dataType = to.name;
    let op: string;
    let id: string;
    if (dataType.endsWith("DataTable")) {
        dataType = dataType.substring(0, dataType.length - 9);
    } else {
        op = to.params.operation;
        id = to.params.id;
    }
    return checkAuthorization(dataType, op, id);
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