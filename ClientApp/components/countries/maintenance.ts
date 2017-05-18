import Vue from 'vue';
import Component from 'vue-class-component';
import { Country } from '../../viewmodels/country';
import * as ErrorMsg from '../error/error-msg';

@Component
export default class MaintenanceComponent extends Vue {
    countries: Array<Country> = [];
    deleteQuestionIds: Array<string> = [];
    deleteIds: Array<string> = [];

    mounted() {
        this.$store.state.countryData.getAll()
            .then(data => this.countries = data);
    }

    cancelDelete(id: string) {
        this.deleteQuestionIds.splice(this.deleteQuestionIds.indexOf(id), 1);
    }

    createCountry() {
        let timestamp = Date.now();
        let c: Country = {
            id: timestamp.toString(),
            creationTimestamp: timestamp,
            name: "",
            epiIndex: 0
        };
        this.countries.push(c); // optimistic add
        this.$store.state.countryData.add(c)
            .then((data: Country) => {
                c.id = data.id; // update optimistically added object with the permanent id
                // check for edits to the optimistically added object
                for (var prop in data) {
                    if (c[prop] !== data[prop]) {
                        // fire update to sync with data store if any are found
                        this.editCountry(c.id);
                        break;
                    }
                }
            })
            .catch(error => {
                // remove the optimistically added object on failure
                this.countries.splice(this.countries.findIndex(b => b.id == c.id), 1);
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The new item could not be added.", error);
            });
    }

    deleteCountry(id: string) {
        this.deleteIds.push(id);
        this.cancelDelete(id);
        this.$store.state.countryData.remove(id)
            .then(() => {
                this.countries.splice(this.countries.findIndex(c => c.id == id), 1);
                this.deleteIds.splice(this.deleteIds.indexOf(id), 1);
            })
            .catch(error => {
                this.deleteIds.splice(this.deleteIds.indexOf(id), 1);
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The item could not be removed.", error);
            });
    }

    deleteCountryQuestion(id: string) {
        this.deleteQuestionIds.push(id);
    }

    editCountry(id: string) {
        this.$router.push({ name: 'country', params: { id, operation: 'edit' } });
    }

    showCountryDetail(id: string) {
        this.$router.push({ name: 'country', params: { id, operation: 'details' } });
    }
}
