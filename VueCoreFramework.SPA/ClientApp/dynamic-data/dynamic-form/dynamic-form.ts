import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Api from '../../api';
import * as ErrorMsg from '../../error-msg';
import { FieldDefinition, Schema, VFGOptions } from '../../vfg/vfg';
import VueFormGenerator from 'vue-form-generator';
import { DataItem, Repository, OperationReply } from '../../store/repository';
import { permissionIncludesTarget, permissions, ShareData } from '../../store/userStore';

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

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    activity = false;
    canEdit = false;
    canShare = false;
    canShareAll = false;
    canShareGroup = false;
    errorMessage = '';
    formOptions: VFGOptions = {
        validateAfterLoad: true,
        validateAfterChanged: true
    };
    groupMembers: string[] = [];
    isValid = false;
    model: any = {};
    parentRepository: Repository = null;
    permissionOptions = [];
    repository: Repository = null;
    schema: Schema = {};
    selectedPermission = null;
    selectedShareGroup = null;
    selectedShareUsername = null;
    shareActivity = false;
    shareDialog = false;
    shareErrorMessage = '';
    shareGroups: string[] = [];
    shareGroup = '';
    shareGroupSuggestion = '';
    shareGroupTimeout = 0;
    shareSuccessMessage = '';
    shareUsernameSuggestion = '';
    shareUsernameTimeout = 0;
    shares: ShareData[] = [];
    shareUsername = '';
    shareWithAll = false;
    updateTimeout = 0;
    vmDefinition: Array<FieldDefinition>;

    @Watch('$route')
    onRouteChange(val: VueRouter.Route, oldVal: VueRouter.Route) {
        this.repository = this.$store.getters.getRepository(val.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    @Watch('shareDialog')
    onShareDialogChange(val: boolean, oldVal: boolean) {
        if (val) {
            this.updateShares();
        }
    }

    mounted() {
        this.repository = this.$store.getters.getRepository(this.$route.name);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        if (errors.find(e => e.error && e.error === "navigation success")) {
            this.updateForm();
        } else {
            this.isValid = isValid;
        }
    }

    addFieldToSchema(field: FieldDefinition) {
        let newField: FieldDefinition = Object.assign({}, field);
        if (field.groupName) {
            let group = this.schema.groups.find(g => g.legend == field.groupName);
            if (field.isName) {
                group.fields.unshift(newField);
            } else {
                group.fields.push(newField);
            }
        } else if (field.isName) {
            this.schema.fields.unshift(newField);
        } else {
            this.schema.fields.push(newField);
        }
    }

    onCancel() {
        this.activity = false;
        this.errorMessage = '';
        if (this.operation === 'add') {
            this.repository.remove(this.$route.fullPath, this.id)
                .then(response => {
                    this.activity = false;
                    this.$router.go(-1);
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred. The item could not be removed. ";
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        this.errorMessage += error.message.replace('CODE:', '');
                    }
                    ErrorMsg.logError("dynamic-form.onCancel", new Error(error));
                    this.activity = false;
                });
        } else {
            this.$router.go(-1);
        }
    }

    onEdit() {
        this.$router.push({ name: this.$route.name, params: { operation: 'edit', id: this.id } });
    }

    onHide(share: ShareData) {
        this.shareActivity = true;
        this.shareErrorMessage = '';
        this.shareSuccessMessage = '';
        let action: string;
        if (share.name === 'All Users') {
            action = 'HideDataFromAll';
        }
        if (share.type === 'user') {
            action = `HideDataFromUser/${share.name}`;
        } else {
            action = `HideDataFromGroup/${share.name}`;
        }
        Api.postApi(`api/Share/${action}/${this.$route.name}?operation=${share.level}&id=${this.id}`, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                ErrorMsg.logError('dynamic-form.onHide', error);
            });
    }

    onSave() {
        this.activity = true;
        this.errorMessage = '';
        let d = Object.assign({}, this.model);
        d[this.model.primaryKeyProperty] = this.id;
        // Remove unsupported or null properties from the ViewModel before sending for update,
        // to avoid errors when overwriting values with the placeholders.
        delete d.dataType;
        delete d.primaryKeyProperty;
        for (var prop in d) {
            if (d[prop] === "[None]" || d[prop] === "[...]") {
                delete d[prop];
            }
        }
        this.repository.update(this.$route.fullPath, d)
            .then(data => {
                this.$router.go(-1);
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be updated.";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError("dynamic-form.onSave", new Error(error));
            });
    }

    onSelectedShareGroupChange(val: string, oldVal: string) {
        this.shareGroup = val;
    }

    onSelectedShareUsernameChange(val: string, oldVal: string) {
        this.shareUsername = val;
    }

    onShare() {
        if (this.selectedPermission) {
            if (this.shareWithAll) {
                this.share('ShareDataWithAll');
            }
            if (this.shareGroup) {
                this.share('ShareDataWithGroup', this.shareGroup);
            }
            if (this.shareUsername) {
                this.share('ShareDataWithUser', this.shareUsername);
            }
        }
    }

    onShareGroupChange(val: string, oldVal: string) {
        if (this.shareGroupTimeout === 0) {
            this.shareGroupTimeout = setTimeout(this.suggestShareGroup, 500);
        }
    }

    onShareUsernameChange(val: string, oldVal: string) {
        if (this.shareUsernameTimeout === 0) {
            this.shareUsernameTimeout = setTimeout(this.suggestShareUsername, 500);
        }
    }

    share(action: string, target?: string) {
        this.shareActivity = true;
        this.shareErrorMessage = '';
        this.shareSuccessMessage = '';
        let url = `api/Share/${action}`;
        if (target) {
            url += `/${target}`;
        }
        url += `/${this.$route.name}?operation=${this.selectedPermission}&id=${this.id}`;
        Api.postApi(url, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.shareActivity = false;
                ErrorMsg.logError('dynamic-form.share', error);
            });
    }

    suggestShareGroup() {
        this.shareGroupTimeout = 0;
        if (this.shareGroup) {
            Api.getApi(`api/Share/GetShareableGroupCompletion/${this.shareGroup}`, this.$route.fullPath)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.shareGroupSuggestion = response.statusText;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('dynamic-form.suggestShareGroup', error);
                });
        }
    }

    suggestShareUsername() {
        this.shareUsernameTimeout = 0;
        if (this.shareUsername) {
            Api.getApi(`api/Share/GetShareableUsernameCompletion/${this.shareUsername}`, this.$route.fullPath)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.shareUsernameSuggestion = response.statusText;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('dynamic-form.suggestShareUsername', error);
                });
        }
    }

    updateForm() {
        this.activity = true;
        this.updateTimeout = 0;
        this.errorMessage = '';

        this.canShare = this.$store.getters.getSharePermission(this.$route.name, this.id);
        this.canShareAll = this.$store.state.userState.isAdmin;
        this.canShareGroup = this.$store.state.userState.isAdmin || this.$store.state.userState.managedGroups.length > 0;

        let permission = this.$store.getters.getPermission(this.$route.name, this.id);
        this.canEdit = permissionIncludesTarget(permission, permissions.permissionDataEdit);
        this.permissionOptions = [{ text: 'View', value: permissions.permissionDataView }];
        switch (permission) {
            case permissions.permissionDataAll:
            case permissions.permissionDataAdd:
            case permissions.permissionDataEdit:
                this.permissionOptions.push({ text: 'Edit', value: permissions.permissionDataEdit });
                break;
            default:
                break;
        }

        this.repository.find(this.$route.fullPath, this.id)
            .then(data => {
                this.model = { dataType: this.$route.name };
                this.schema = { fields: [] };
                this.repository.getFieldDefinitions(this.$route.fullPath)
                    .then(defData => {
                        this.vmDefinition = defData;
                        let groups = this.vmDefinition.filter(v => v.groupName !== undefined && v.groupName !== null).map(v => v.groupName);
                        if (groups.length) {
                            this.schema.groups = [];
                            for (var i = 0; i < groups.length; i++) {
                                this.schema.groups[i] = {
                                    legend: groups[i],
                                    fields: []
                                };
                            }
                        }
                        this.vmDefinition.forEach(field => {
                            this.model[field.model] = field.default || null;
                        });
                        for (var prop in data) {
                            this.model[prop] = data[prop];
                        }
                        this.vmDefinition.forEach(field => {
                            this.addFieldToSchema(field);
                        });
                        if (this.operation === 'view') {
                            this.schema.fields.forEach(f => f.disabled = true);
                        }
                        this.activity = false;
                    });
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while updating the data. ";
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError("dynamic-form.updateForm", new Error(error));
            });
    }

    updateShares() {
        Api.getApi(`api/Share/GetCurrentShares/${this.$route.name}?id=${this.id}`, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<Array<ShareData>>)
            .then(data => {
                this.shares = [];
                for (var i = 0; i < data.length; i++) {
                    this.shares[i] = data[i];
                    this.shares[i].id = i;
                }
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-form.updateShares', error);
            });
        Api.getApi(`api/Share/GetShareableGroupMembers`, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                this.groupMembers = data;
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-form.updateShares', error);
            });
        Api.getApi(`api/Share/GetShareableGroupSubset`, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                this.shareGroups = data;
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-form.updateShares', error);
            });
    }
}