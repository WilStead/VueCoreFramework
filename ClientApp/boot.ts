import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';
import { router } from './router';

import Vue_Responsive from './vue-responsive-custom';
Vue.directive('responsiveness', Vue_Responsive);

import VueFormGenerator from 'vue-form-generator';
Vue.use(VueFormGenerator);

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
