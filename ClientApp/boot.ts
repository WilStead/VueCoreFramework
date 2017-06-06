import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';
import { router } from './router';

import Vuetify from 'vuetify';
Vue.use(Vuetify);

import VueFormGenerator from 'vue-form-generator';

Vue.component("fieldVuetifyText", require('./dynamic-data/dynamic-form/fieldVuetifyText.vue'));

Vue.use(VueFormGenerator);

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
