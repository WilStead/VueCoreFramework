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
Vue.component("fieldVuetifyCheckbox", require('./dynamic-data/dynamic-form/fieldVuetifyCheckbox.vue'));
Vue.component("fieldVuetifySelect", require('./dynamic-data/dynamic-form/fieldVuetifySelect.vue'));
Vue.component("fieldVuetifyDateTime", require('./dynamic-data/dynamic-form/fieldVuetifyDateTime.vue'));
Vue.component("fieldVuetifyTimespan", require('./dynamic-data/dynamic-form/fieldVuetifyTimespan.vue'));
Vue.component("fieldVuetifyColor", require('./dynamic-data/dynamic-form/fieldVuetifyColor.vue'));
Vue.component("fieldNavigation", require('./dynamic-data/dynamic-form/fieldNavigation.vue'));

Vue.use(VueFormGenerator);

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue'))
});
