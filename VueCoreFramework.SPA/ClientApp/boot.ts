import Vue from 'vue';
import VueRouter from 'vue-router';
Vue.use(VueRouter);
import { store } from './store/store';
import { router } from './router';

import Vuetify from 'vuetify';
Vue.use(Vuetify);

import VueFormGenerator from 'vue-form-generator';

Vue.component("dynamic-data-table", require('./dynamic-data/dynamic-table/dynamic-data-table.vue').default);
Vue.component("fieldVuetifyText", require('./dynamic-data/dynamic-form/fieldVuetifyText.vue').default);
Vue.component("fieldVuetifyCheckbox", require('./dynamic-data/dynamic-form/fieldVuetifyCheckbox.vue').default);
Vue.component("fieldVuetifySelect", require('./dynamic-data/dynamic-form/fieldVuetifySelect.vue').default);
Vue.component("fieldVuetifyDateTime", require('./dynamic-data/dynamic-form/fieldVuetifyDateTime.vue').default);
Vue.component("fieldVuetifyTimespan", require('./dynamic-data/dynamic-form/fieldVuetifyTimespan.vue').default);
Vue.component("fieldVuetifyColor", require('./dynamic-data/dynamic-form/fieldVuetifyColor.vue').default);
Vue.component("fieldNavigation", require('./dynamic-data/dynamic-form/fieldNavigation.vue').default);
Vue.component("fieldCollection", require('./dynamic-data/dynamic-form/fieldCollection.vue').default);

Vue.use(VueFormGenerator);

new Vue({
    el: '#app-root',
    store,
    router,
    render: h => h(require('./components/app/app.vue').default)
});
