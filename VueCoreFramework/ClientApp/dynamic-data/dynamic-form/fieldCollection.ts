import { abstractField } from 'vue-form-generator';
import { DataItem, Repository } from '../../store/repository';
import * as ErrorMsg from '../../error-msg';

export default {
    mixins: [abstractField],
    data() {
        return {
            activity: false,
            editDialogShown: false,
            editErrorMessage: '',
            errorMessage: '',
            repository: this.$store.getters.getRepository(this.schema.parentType) as Repository,
            selected: [] as DataItem[],
            selectedChildren: [] as DataItem[]
        };
    },
    methods: {
        onAddSelect() {
            this.activity = true;
            this.editErrorMessage = '';
            this.repository.addChildrenToCollection(this.$route.fullPath,
                this.model[this.model.primaryKeyProperty],
                this.schema.model,
                this.selected.map(c => c[c.primaryKeyProperty]))
                .then(data => {
                    if (data.error) {
                        this.editErrorMessage = data.error;
                    }
                    else {
                        this.selected.splice(0);
                        this.refresh();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.activity = false;
                    this.editErrorMessage = "A problem occurred. The item could not be removed.";
                    ErrorMsg.logError("fieldCollection.onAddSelect", new Error(error));
                });
        },

        onRemoveSelect() {
            this.activity = true;
            this.editErrorMessage = '';
            this.repository.removeChildrenFromCollection(this.$route.fullPath,
                this.model[this.model.primaryKeyProperty],
                this.schema.model,
                this.selectedChildren.map(c => c[c.primaryKeyProperty]))
                .then(data => {
                    if (data.error) {
                        this.editErrorMessage = data.error;
                    }
                    else {
                        this.selectedChildren.splice(0);
                        this.refresh();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.activity = false;
                    this.editErrorMessage = "A problem occurred. The item could not be removed.";
                    ErrorMsg.logError("fieldCollection.onRemoveSelect", new Error(error));
                });
        },

        onEditError(error: string) {
            this.editErrorMessage = error;
        },

        onError(error: string) {
            this.errorMessage = error;
        },

        refresh() {
            this.$children.forEach(child => {
                if (typeof child['refresh'] === "function") {
                    let f: Function = child['refresh'];
                    f();
                }
            });
        }
    }
};