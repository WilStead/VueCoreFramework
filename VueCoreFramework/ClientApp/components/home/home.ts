import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import _t from '../../globalization/translate';

@Component({
    filters: {
        t: function (value: string) {
            return _t(value, "home");
        }
    }
})
export default class HomeComponent extends Vue { }