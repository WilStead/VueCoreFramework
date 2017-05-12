import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';

import Vue_Responsive from './vue-responsive-custom';
Vue.directive('responsiveness', Vue_Responsive);

const routes = [
    { path: '/', component: require('./components/home/home.vue') },
    { path: '/counter', component: resolve => require(['./components/counter/counter.vue'], resolve) },
    { path: '/fetchdata', component: resolve => require(['./components/fetchdata/fetchdata.vue'], resolve) },
    { path: '*', component: resolve => require(['./components/error/notfound.vue'], resolve) }
];

const router = new VueRouter({
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

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
