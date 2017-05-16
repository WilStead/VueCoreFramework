import VueRouter from 'vue-router';
import { store } from './store/store';

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
        component: require('./components/user/manage.vue'),
        meta: { requiresAuth: true }
    },
    { path: '/counter', component: resolve => require(['./components/counter/counter.vue'], resolve) },
    { path: '/fetchdata', component: resolve => require(['./components/fetchdata/fetchdata.vue'], resolve) },
    {
        path: '/countries',
        component: require('./components/countries/dashboard.vue'),
        children: [
            { path: 'list/:num', component: require('./components/countries/list.vue') },
            { path: ':name', component: require('./components/countries/details.vue') }
        ]
    },
    { path: '*', component: resolve => require(['./components/error/notfound.vue'], resolve) }
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
router.afterEach((to, from) => {
    store.commit('toggleVerticalMenu', { onOff: false });
});
router.beforeEach((to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuth)) {
        checkAuthorization(to)
            .then(auth => {
                if (auth) {
                    next();
                } else {
                    next({ path: '/login', query: { returnUrl: to.fullPath } });
                }
            })
    } else {
        next();
    }
});

export interface ApiResponseViewModel {
    response: string
}

interface AuthorizationViewModel {
    authorization: string
}
export function checkAuthorization(to): Promise<boolean> {
    return fetch('/api/Account/Authorize')
        .then(response => response.json() as Promise<AuthorizationViewModel>)
        .then(data => {
            if (data.authorization === "authorized") {
                return true;
            } else {
                return false;
            }
        });
}