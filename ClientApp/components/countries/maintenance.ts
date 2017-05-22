import Vue from 'vue';
import Component from 'vue-class-component';
import { countryFieldDefinitions } from '../../viewmodels/country';
import { FieldDefinition } from '../../dynamic-data/field-definition';

@Component({
    components: {
        DynamicTableComponent: require('../../dynamic-data/dynamic-table/dynamic-table.vue')
    }
})
export default class MaintenanceComponent extends Vue {
    countryDefs: Array<FieldDefinition> = countryFieldDefinitions;
}
