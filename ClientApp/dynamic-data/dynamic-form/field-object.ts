import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import VueFormGenerator from 'vue-form-generator';
import { Repository } from '../../store/repository';

class abstractField extends Vue {
    schema: any;
    model: any;
}

@Component({ mixins: [VueFormGenerator.abstractfield] })
export default class FieldObjectComponent extends abstractField {
    @Prop()
    repository: Repository;

    deleteAsking = false;
    deletePending = false;
    errorMessage = '';
    hasValue = false;

    mounted() {
        if (typeof this.schema["childProp"] !== undefined && typeof this.schema["childIdProp"] !== undefined) {
            this.hasValue = true;
        }
    }

    onDelete() {
        if (typeof this.schema["childProp"] !== undefined && typeof this.schema["childIdProp"] !== undefined) {
            this.deletePending = true;
            this.repository.removeChild(this.$route.fullPath, this.model["id"], this.schema.childProp, this.model[this.schema.childIdProp])
                .then(data => {
                    if (data.error) {
                        this.errorMessage = data.error;
                    }
                    else {
                        this.hasValue = false;
                    }
                    this.deletePending = false;
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred. The item could not be removed.";
                    this.deletePending = false;
                    ErrorMsg.logError("field-object.onDelete", error);
                });
        }
    }
}