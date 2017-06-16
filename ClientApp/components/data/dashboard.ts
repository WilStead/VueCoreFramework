import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class DashboardComponent extends Vue {
    @Prop()
    fontAwesome: boolean;

    @Prop()
    iconClass: string;

    @Prop()
    title: string;
}