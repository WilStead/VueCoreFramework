import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';
import { router } from './router';

import Vue_Responsive from './vue-responsive-custom';
Vue.directive('responsiveness', Vue_Responsive);

import VueForm from 'vue-form';
Vue.use(VueForm, {
    inputClasses: {
        valid: 'form-control-success',
        invalid: 'form-control-danger'
    }
});

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
