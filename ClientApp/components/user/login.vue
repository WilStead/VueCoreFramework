<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Sign In</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <validate auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.email)">
                    <input type="email" class="form-control" name="email" placeholder="Email" v-model.trim.lazy="model.email" required />
                    <field-messages name="email" show="$dirty && $touched">
                        <div slot="required">a valid email address is required</div>
                        <div slot="email">a valid email address is required</div>
                    </field-messages>
                </validate>
                <div v-if="!forgottenPassword">
                    <validate auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.password)">
                        <input type="password" class="form-control" name="password" placeholder="Password" v-model.trim.lazy="model.password" required />
                        <field-messages name="password" show="$dirty && $touched">
                            <div slot="required">a password is required</div>
                        </field-messages>
                    </validate>
                    <div class="checkbox">
                        <label><input type="checkbox" name="rememberUser" v-model.lazy="model.rememberUser" />Remember Me</label>
                    </div>
                </div>
                <ul class="text-danger">
                    <li v-for="error in model.errors">{{ error }}</li>
                </ul>
                <div v-if="!submitting" class="submit-row">
                    <router-link :to="{ path: '/register', query: { returnUrl }}">Register</router-link>
                    <div v-if="!forgottenPassword">
                        <button type="submit" class="btn btn-primary">Sign In</button>
                    </div>
                    <div v-if="forgottenPassword">
                        <button type="submit" class="btn btn-primary">Reset</button>
                    </div>
                </div>
            </vue-form>
            <div v-if="!submitting">
                <div class="forgotten-password-container">
                    <div v-if="!passwordReset">
                        <div v-if="!forgottenPassword">
                            <a href="#" @click.stop.prevent="forgotPassword(true)">Forgot your password?</a>
                        </div>
                        <div v-if="forgottenPassword">
                            <a href="#" @click.stop.prevent="forgotPassword(false)">Return to login</a>
                        </div>
                    </div>
                    <div v-if="passwordReset">
                        <p>Please check your email to reset your password. <a href="#" @click.stop.prevent="forgotPassword(true)">Re-try?</a></p>
                    </div>
                    <div class="external-login-providers">
                        <!--TODO: link external logins here-->
                    </div>
                </div>
            </div>
            <div v-if="submitting">
                <p class="submitting">Signing In</p>
            </div>
        </div>
    </div>
</template>

<script src="./login.ts"></script>

<style src="./user.scss" lang="scss"></style>
<style src="./login.scss" lang="scss"></style>