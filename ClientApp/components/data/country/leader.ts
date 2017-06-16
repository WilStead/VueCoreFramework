import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../../error-msg';
import { Repository } from '../../../store/repository';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    id: string;

    @Prop()
    operation: string;

    @Watch('id')
    onIdChanged(val: string, oldVal: string) {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    @Watch('operation')
    onOperationChanged(val: string, oldVal: string) {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    name = "this country's leader";
    repository: Repository = null;
    updateTimeout = 0;

    beforeRouteUpdate(to: VueRouter.Route, from: VueRouter.Route, next: Function) {
        this.repository = new Repository(this.$route.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
        next();
    }

    mounted() {
        this.repository = new Repository(this.$route.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    updateForm() {
        this.updateTimeout = 0;
        this.repository.find(this.$route.fullPath, this.id)
            .then(data => {
                if (data.error) {
                    this.name = "this country's leader";
                } else {
                    this.name = data.data['name'];
                }
            })
            .catch(error => {
                ErrorMsg.logError("leader.updateForm", new Error(error));
            });
    }
}