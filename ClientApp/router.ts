import VueRouter from 'vue-router';
import { store } from './store/store';

const routes = [
    { path: '/', component: require('./components/home/home.vue') },
    { path: '/signin', component: require('./components/user/signin.vue') },
    { path: '/register', component: require('./components/user/register.vue') },
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

export function checkAuthentication(to, from, next) {
    let passed: boolean = false;

    // TODO: check credentials

    if (passed) {
        next();
    } else {
        next('/signin');
    }
}