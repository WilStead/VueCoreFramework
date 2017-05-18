import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { Country, countryFieldDefinitions } from '../../viewmodels/country';
import * as ErrorMsg from '../error/error-msg';
import { FieldDefinition } from '../../dynamic-forms/field-definition';

@Component
export default class DetailsComponent extends Vue {
    @Prop()
    id: string;

    @Prop()
    operation: string;

    country: Country;
    countryDefinition = countryFieldDefinitions;
    errorMessage: string;

    mounted() {
        if (this.operation === 'create') {
            this.country = { id: Date.now().toString(), name: "", epiIndex: null, creationTimestamp: Date.now() };
        } else {
            this.$store.state.countryData.find(this.id)
                .then((data: Country) => this.country = data)
                .catch(error => ErrorMsg.showErrorMsgAndLog("A problem occurred. The information could not be retrieved.", error));
        }
    }

    createCountry(country: Country) {
        country.creationTimestamp = Date.now();
        this.errorMessage = null;
        this.$store.state.countryData.add(country)
            .then(() => this.$router.push('/countries/maintenance'))
            .catch(error => {
                this.errorMessage = "Error creating country";
                ErrorMsg.logError(error);
            });
    }

    updateCountry(country: Country) {
        this.errorMessage = null;
        this.$store.state.countryData.update(country)
            .then((data: Country) => this.country = data)
            .catch(error => {
                this.errorMessage = "Error updating country";
                ErrorMsg.logError(error);
            });
    }
}