import VueRouter from 'vue-router';
import { authenticate, AuthorizationViewModel, checkAuthorization } from './authorization';
import * as ErrorMsg from './error-msg';

const routes: Array<VueRouter.RouteConfig> = [
    { path: '/', component: require('./components/home/home.vue').default },
    { path: '/home/index', redirect: '/' },
    {
        path: '/login',
        component: require('./components/user/login.vue').default,
        props: (route) => ({ returnUrl: route.query.returnUrl })
    },
    {
        path: '/register',
        component: require('./components/user/register.vue').default,
        props: (route) => ({ returnUrl: route.query.returnUrl })
    },
    {
        path: '/user/manage',
        meta: { requiresAuthenticate: true },
        component: require('./components/user/manage.vue').default
    },
    {
        path: '/user/email/confirm',
        component: require('./components/user/email/confirm.vue').default
    },
    {
        path: '/user/email/restore',
        component: require('./components/user/email/restore.vue').default
    },
    {
        path: '/user/reset/:code',
        component: require('./components/user/password/reset.vue').default,
        props: true
    },
    {
        path: '/group/manage',
        meta: { requiresAuthenticate: true },
        component: require('./components/group/manage.vue').default
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
router.beforeEach(async (to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuthorize)) {
        try {
            let auth = await authorize(to);
            if (auth === "login") {
                next({ path: '/login', query: { returnUrl: to.fullPath } });
            } else if (auth === "unauthorized") {
                next({ path: '/error/401' });
            } else {
                next();
            }
        } catch (error) {
            ErrorMsg.logError("router.beforeEach", error);
            next({ path: '/login', query: { returnUrl: to.fullPath } });
        }
    } else if (to.matched.some(record => record.meta.requiresAuthenticate)) {
        try {
            let auth = await authenticate();
            if (auth === "authorized") {
                next();
            } else {
                next({ path: '/login', query: { returnUrl: to.fullPath } });
            }
        } catch (error) {
            ErrorMsg.logError("router.beforeEach", error);
            next({ path: '/login', query: { returnUrl: to.fullPath } });
        };
    } else {
        next();
    }
});

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
 * When the response of an API call is a 401 status code, redirects to the login
 * page.
 * @param {Response} response The result of a fetch to the API.
 * @param {string} returnPath The page to redirect to after a successful login, if required.
 */
export function checkResponse(response: Response, returnPath: string) {
    if (!response.ok && response.status === 401) {
        router.push({ path: '/login', query: { returnUrl: returnPath } });
    }
    return response;
}