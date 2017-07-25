import { abstractField } from 'vue-form-generator';
import * as ErrorMsg from '../../error-msg';

export default {
    mixins: [abstractField],
    data() {
        return {
            activity: false,
            editDialogShown: false,
            editErrorMessage: '',
            errorMessage: '',
            repository: this.$store.getters.getRepository(this.schema.parentType),
            selected: [],
            selectedChildren: []
        };
    },
    methods: {
        async onAddSelect() {
            this.activity = true;
            this.editErrorMessage = '';
            try {
                await this.repository.addChildrenToCollection(this.$route.fullPath,
                    this.model[this.model.primaryKeyProperty],
                    this.schema.model,
                    this.selected.map(c => c[c.primaryKeyProperty]));
                this.selected.splice(0);
                this.refresh();
            } catch (error) {
                ErrorMsg.logError("fieldCollection.onAddSelect", error);
                this.editErrorMessage = "A problem occurred. The item could not be removed.";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.editErrorMessage += error.message.replace('CODE:', '');
                }
            }
            this.activity = false;
        },

        async onRemoveSelect() {
            this.activity = true;
            this.editErrorMessage = '';
            try {
                await this.repository.removeChildrenFromCollection(this.$route.fullPath,
                    this.model[this.model.primaryKeyProperty],
                    this.schema.model,
                    this.selectedChildren.map(c => c[c.primaryKeyProperty]));
                this.selectedChildren.splice(0);
                this.refresh();
            } catch (error) {
                ErrorMsg.logError("fieldCollection.onRemoveSelect", error);
                this.editErrorMessage = "A problem occurred. The item could not be removed.";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.editErrorMessage += error.message.replace('CODE:', '');
                }
            }
            this.activity = false;
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