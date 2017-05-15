<template>
    <div class="register-form-container">
        <div class="register-form">
            <div class="logo-container"></div>
            <h4>Register for a New Account</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.username)">
                    <input type="text" class="form-control" name="username" placeholder="User Name" required v-model.trim.lazy="model.username" />
                    <field-messages name="username" show="$invalid && $dirty">
                        <div slot="required">a username is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.email)">
                    <input type="email" class="form-control" name="email" placeholder="Email" required v-model.trim.lazy="model.email" />
                    <field-messages name="email" show="$invalid && $dirty">
                        <div slot="required">a valid email address is required</div>
                        <div slot="email">a valid email address is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.password)">
                    <input type="password" class="form-control" name="password" placeholder="Password" required v-model.trim.lazy="model.password" />
                    <field-messages name="password" show="$invalid && $dirty">
                        <div slot="required">a password is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.passwordConfirm)" :custom="{ customValidator: passwordMatch }">
                    <input type="password" class="form-control" name="passwordConfirm" placeholder="Confirm Password" required v-model.trim.lazy="model.passwordConfirm" />
                    <field-messages name="passwordConfirm" show="$invalid && $dirty">
                        <div slot="required">you must confirm your password</div>
                        <div slot="passwordMatch">your passwords must match</div>
                    </field-messages>
                </validate>
                <div class="checkbox">
                    <label><input type="checkbox" name="rememberUser" />Remember Me</label>
                </div>
            </vue-form>
            <router-link tag="button" class="btn btn-default" to="/signin">Cancel</router-link>
            <button class="btn btn-primary">Register</button>
        </div>
    </div>
</template>

<script src="./register.ts"></script>

<style src="./register.scss" lang="scss"></style>