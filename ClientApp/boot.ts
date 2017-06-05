import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';
import { router } from './router';

import Vuetify from 'vuetify';
Vue.use(Vuetify);

import VueFormGenerator from 'vue-form-generator';

import autoVuetifyTextField from './dynamic-data/dynamic-form/auto-vuetify-text-field';
Vue.component("autoVuetifyTextField", autoVuetifyTextField);

Vue.use(VueFormGenerator);

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
