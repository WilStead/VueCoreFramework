<template>
    <div class="login-container">
        <div class="login-form">
            <div class="logo-container"></div>
            <h4>Sign In</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <validate :custom="{customValidator: modelErrorValidator}" class="text-danger">
                    <field-messages show="$submitted">
                        <div slot="modelErrorValidator">{{ getModelError('*') }}</div>
                    </field-messages>
                </validate>
                <validate :custom="{customValidator: emailModelErrorValidator}" auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.email)">
                    <input type="email" class="form-control" name="email" placeholder="Email" required v-model.trim.lazy="model.email" />
                    <field-messages name="email" show="$invalid && $dirty">
                        <div slot="required">a valid email is required</div>
                        <div slot="email">a valid email is required</div>
                        <div slot="emailModelErrorValidator">{{ getModelError('Email') }}</div>
                    </field-messages>
                </validate>
                <div v-if="!forgottenPassword && !passwordReset">
                    <validate :custom="{customValidator: passwordModelErrorValidator}" auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.password)">
                        <input type="password" class="form-control" name="password" placeholder="Password" required v-model.trim.lazy="model.password" />
                        <field-messages name="password" show="$invalid && $dirty">
                            <div slot="required">a password is required</div>
                            <div slot="passwordModelErrorValidator">{{ getModelError('Password') }}</div>
                        </field-messages>
                    </validate>
                    <div class="checkbox">
                        <label><input type="checkbox" name="rememberUser" />Remember Me</label>
                    </div>
                </div>
            </vue-form>
            <div v-if="!submitting">
                <div>
                    <router-link :to="{ path: '/register', query: { returnUrl: $props.returnUrl }}">Register</router-link>
                    <div v-if="!forgottenPassword">
                        <button type="submit" class="btn btn-primary">Sign In</button>
                    </div>
                    <div v-if="forgottenPassword">
                        <button type="submit" class="btn btn-primary">Reset</button>
                    </div>
                </div>
                <div>
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
                <p class="submitting">Signing In...</p>
            </div>
        </div>
    </div>
</template>

<script src="./login.ts"></script>

<style src="./login.scss" lang="scss"></style>