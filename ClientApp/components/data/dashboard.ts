import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class DashboardComponent extends Vue {
    @Prop()
    title: string;
}