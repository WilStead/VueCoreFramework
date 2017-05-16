<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Manage your account:</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <div v-if="changeSuccess" class="text-success">
                    <span>{{ successMessage }}</span>
                </div>
                <div v-if="changingEmail">
                    <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.email)">
                        <input type="email" class="form-control" name="email" placeholder="Email" v-model.trim.lazy="model.email" required />
                        <field-messages name="email" show="$invalid && $dirty">
                            <div slot="required">a valid email address is required</div>
                            <div slot="email">a valid email address is required</div>
                        </field-messages>
                    </validate>
                </div>
                <div v-if="changingPassword || settingPassword">
                    <div v-if="changingPassword">
                        <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.oldPassword)">
                            <input type="password" class="form-control" name="oldPassword" placeholder="Password" v-model.trim.lazy="model.oldPassword" required />
                            <field-messages name="oldPassword" show="$invalid && $dirty">
                                <div slot="required">a password is required</div>
                            </field-messages>
                        </validate>
                    </div>
                    <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.newPassword)">
                        <input type="password" class="form-control" name="newPassword" placeholder="Password" v-model.trim.lazy="model.newPassword"
                               required minlength="6" maxlength="100" pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])" />
                        <field-messages name="newPassword" show="$invalid && $dirty">
                            <div slot="required">a password is required</div>
                        </field-messages>
                    </validate>
                    <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.confirmPassword)" :custom="{ customValidator: passwordMatch }">
                        <input type="password" class="form-control" name="confirmPassword" placeholder="Confirm Password" v-model.trim.lazy="model.confirmPassword" required />
                        <field-messages name="confirmPassword" show="$invalid && $dirty">
                            <div slot="required">you must confirm your password</div>
                            <div slot="passwordMatch">your passwords must match</div>
                        </field-messages>
                    </validate>
                </div>
                <ul class="text-danger">
                    <li v-for="error in model.errors">{{ error }}</li>
                </ul>
                <div v-if="changingEmail || changingPassword || settingPassword" class="submit-row">
                    <button class="btn btn-default" @click.stop.prevent="cancelChange">Cancel</button>
                    <button type="submit" class="btn btn-primary">Submit</button>
                </div>
            </vue-form>
            <div v-if="!changingEmail && !changingPassword && !settingPassword">
                <a href="#" @click.stop.prevent="changeEmail">Change email</a>
            </div>
            <div v-if="pendingEmailChange && !changingEmail && !changingPassword && !settingPassword">
                <a href="#" @click.stop.prevent="cancelEmailChange">Cancel pending email change</a>
            </div>
            <div v-if="hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="changePassword">Change password</a>
            </div>
            <div v-if="!hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="setPassword">Add a local password</a>
            </div>
            <div>
                <!--TODO: manage external logins-->
            </div>
        </div>
    </div>
</template>

<script src="./manage.ts"></script>

<style src="./user.scss" lang="scss"></style>