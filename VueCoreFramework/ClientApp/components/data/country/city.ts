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

    otherCapitol = true;
    errorMessage = '';
    isCapitol = false;
    repository: Repository = null;
    updateTimeout = 0;

    @Watch('isCapitol')
    onIsCapitolChanged(newVal: boolean, oldVal: boolean) {
        this.errorMessage = '';
        this.repository.find(this.$route.fullPath, this.id)
            .then(data => {
                if (data['isCapitol'] !== this.isCapitol) {
                    data['isCapitol'] = this.isCapitol;
                    this.repository.update(this.$route.fullPath, data)
                        .then(data => {
                            if (data.error) {
                                this.errorMessage = data.error;
                            }
                        });
                }
            })
            .catch(error => {
                this.errorMessage = "A problem has occurred.";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                ErrorMsg.logError("city.onSetCapitol", new Error(error));
            });
    }

    @Watch('$route')
    onRouteChange(val: VueRouter.Route, oldVal: VueRouter.Route) {
        this.repository = this.$store.getters.getRepository(val.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    mounted() {
        this.repository = this.$store.getters.getRepository(this.$route.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    updateForm() {
        this.errorMessage = '';
        this.updateTimeout = 0;
        this.repository.getAll(this.$route.fullPath)
            .then(data => {
                let capitol = data.find(v => v['isCapitol']);
                this.isCapitol = capitol && capitol[capitol.primaryKeyProperty] === this.id;
                this.otherCapitol = capitol !== undefined && !this.isCapitol;
            })
            .catch(error => {
                this.otherCapitol = true; // Without reliable information, prevent setting a new capitol.
                this.errorMessage = "A problem has occurred.";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                ErrorMsg.logError("city.updateForm", new Error(error));
            });
    }
}