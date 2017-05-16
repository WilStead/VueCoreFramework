<template>
    <div class="user-container">
        <div class="user-form">
            <div class="logo-container"></div>
            <h4>Manage your account:</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <div v-if="changeSuccess" class="text-success">
                    <span>Success!</span>
                </div>
                <div v-if="getModelError('*')" class="text-danger">
                    <span>{{ getModelError('*') }}</span>
                </div>
                <div v-if="changingEmail">
                    <validate :custom="{customValidator: emailModelErrorValidator}" auto-label class="form-group" v-bind:class="fieldClassName(formstate.email)">
                        <input type="email" class="form-control" name="email" placeholder="Email" required v-model.trim.lazy="model.email" />
                        <field-messages name="email" show="$invalid && $dirty">
                            <div slot="required">a valid email address is required</div>
                            <div slot="email">a valid email address is required</div>
                            <div slot="emailModelErrorValidator">{{ getModelError('Email') }}</div>
                        </field-messages>
                    </validate>
                </div>
                <div v-if="changingPassword || settingPassword">
                    <div v-if="changingPassword">
                        <validate :custom="{customValidator: oldPasswordModelErrorValidator}" auto-label class="form-group" v-bind:class="fieldClassName(formstate.oldPassword)">
                            <input type="password" class="form-control" name="oldPassword" placeholder="Password" required minlength="6" maxlength="100" v-model.trim.lazy="model.oldPassword" />
                            <field-messages name="oldPassword" show="$invalid && $dirty">
                                <div slot="required">a password is required</div>
                                <div slot="passwordModelErrorValidator">{{ getModelError('OldPassword') }}</div>
                            </field-messages>
                        </validate>
                    </div>
                    <validate :custom="{customValidator: newPasswordModelErrorValidator}" auto-label class="form-group" v-bind:class="fieldClassName(formstate.newPassword)">
                        <input type="password" class="form-control" name="newPassword" placeholder="Password" required minlength="6" maxlength="100" v-model.trim.lazy="model.newPassword" />
                        <field-messages name="newPassword" show="$invalid && $dirty">
                            <div slot="required">a password is required</div>
                            <div slot="passwordModelErrorValidator">{{ getModelError('NewPassword') }}</div>
                        </field-messages>
                    </validate>
                    <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.confirmPassword)" :custom="{ customValidator: passwordMatch }">
                        <input type="password" class="form-control" name="confirmPassword" placeholder="Confirm Password" required v-model.trim.lazy="model.confirmPassword" />
                        <field-messages name="confirmPassword" show="$invalid && $dirty">
                            <div slot="required">you must confirm your password</div>
                            <div slot="passwordMatch">your passwords must match</div>
                        </field-messages>
                    </validate>
                </div>
            </vue-form>
            <div v-if="changingEmail || changingPassword || settingPassword">
                <button class="btn btn-default" @click.stop.prevent="cancelChange">Cancel</button>
                <button type="submit" class="btn btn-primary">Submit</button>
            </div>
            <div v-if="!changingEmail && !changingPassword && !settingPassword">
                <a href="#" @click.stop.prevent="changeEmail">Change email</a>
            </div>
            <div v-if="hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="changePassword">Change password</a>
            </div>
            <div v-if="!hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="setPassword">Add password</a>
            </div>
            <div>
                <!--TODO: manage external logins-->
            </div>
        </div>
    </div>
</template>

<script src="./manage.ts"></script>

<style src="./user.scss" lang="scss"></style>