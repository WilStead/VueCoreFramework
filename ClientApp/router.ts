import VueRouter from 'vue-router';
import { store } from './store/store';
import * as ErrorMsg from './error-msg';

const routes = [
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
        meta: { requiresAuth: true },
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
    { path: '/fetchdata', component: resolve => require(['./components/fetchdata/fetchdata.vue'], resolve) },
    { path: '/error/notfound', component: resolve => require(['./components/error/notfound.vue'], resolve) },
    { path: '/error/:code', component: resolve => require(['./components/error/error.vue'], resolve) }
];

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
    if (to.matched.some(record => record.meta.requiresAuth)) {
        checkAuthorization(to, to.fullPath)
            .then(auth => {
                if (auth) {
                    next();
                } else {
                    next({ path: '/login', query: { returnUrl: to.fullPath } });
                }
            })
            .catch(error => {
                ErrorMsg.logError("router.beforeEach", error);
                next({ path: '/login', query: { returnUrl: to.fullPath } });
            });
    } else {
        next();
    }
});

export interface ApiResponseViewModel {
    response: string
}

interface AuthorizationViewModel {
    email: string,
    authorization: string
}
export function checkAuthorization(to, returnPath): Promise<boolean> {
    return fetch('/api/Account/Authorize',
        {
            headers: {
                'Authorization': `bearer ${store.state.token}`
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
            if (data.authorization === "authorized") {
                store.state.email = data.email;
                return true;
            } else {
                return false;
            }
        })
        .catch(error => {
            if (error.message !== "unauthorized") {
                ErrorMsg.logError("router.checkAuthorization", new Error(error));
            }
            return false;
        });
}

export function checkResponse(response, returnPath) {
    if (!response.ok) {
        if (response.status === 401) {
            router.push({ path: '/login', query: { returnUrl: returnPath } });
        }
        throw Error(response.statusText);
    }
    return response;
}