import Vue from 'vue';
import Component from 'vue-class-component';
import { Country, CountryData } from '../../viewmodels/country';

@Component
export default class FetchDataComponent extends Vue {
    countries: Array<Country> = [];
    deleteError = '';
    deleteId = '';
    isDeleting = false;

    mounted() {
        this.$store.state.countryData.getAll()
            .then(data => this.countries = data);
    }

    cancelDelete() {
        this.isDeleting = false;
        this.deleteId = '';
    }

    createCountry() {
        let tempKey = Date.now().toString();
        let c: Country = {
            id: tempKey,
            name: "",
            epiIndex: 0
        };
        this.countries.push(c); // optimistic add
        this.$store.state.countryData.add(c)
            .catch(error => {
                this.countries.splice(this.countries.findIndex(b => b.id == c.id), 1); // remove on failure
            });
    }

    showCountryDetail(id: number) {

    }

    editCountry(id: number) {

    }

    deleteCountryQuestion(id: number) {

    }
}
