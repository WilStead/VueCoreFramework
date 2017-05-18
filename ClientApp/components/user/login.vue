<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Sign In</h4>
            <ul class="text-danger">
                <li v-for="error in model.errors">{{ error }}</li>
            </ul>
            <vue-form-generator :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            <div class="submit-row">
                <button v-if="this.forgottenPassword" class="btn btn-primary" @click.stop.prevent="resetPassword">Reset</button>
                <button v-if="!this.forgottenPassword" class="btn btn-primary" @click.stop.prevent="onSubmit">Sign In</button>
            </div>
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