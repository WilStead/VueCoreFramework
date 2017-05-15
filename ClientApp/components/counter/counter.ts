import Vue from 'vue';
import Component from 'vue-class-component';

@Component
export default class CounterComponent extends Vue {
    increment() {
        this.$store.commit('increment');
    }
}
