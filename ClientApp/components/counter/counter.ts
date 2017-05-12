import Vue from 'vue';
import Component from 'vue-class-component';
import { State, Mutation } from 'vuex-class';

@Component
export default class CounterComponent extends Vue {
    @State count;
    @Mutation increment;
}
