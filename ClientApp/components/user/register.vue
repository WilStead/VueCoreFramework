<template>
    <div class="user-container">
        <div class="user-form">
            <div class="logo-container"></div>
            <h4>Register for a New Account</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <div v-if="getModelError('*')" class="text-danger">
                    <span>{{ getModelError('*') }}</span>
                </div>
                <validate :custom="{customValidator: emailModelErrorValidator}" auto-label class="form-group" v-bind:class="fieldClassName(formstate.email)">
                    <input type="email" class="form-control" name="email" placeholder="Email" required v-model.trim.lazy="model.email" />
                    <field-messages name="email" show="$invalid && $dirty">
                        <div slot="required">a valid email address is required</div>
                        <div slot="email">a valid email address is required</div>
                        <div slot="emailModelErrorValidator">{{ getModelError('Email') }}</div>
                    </field-messages>
                </validate>
                <validate :custom="{customValidator: passwordModelErrorValidator}" auto-label class="form-group" v-bind:class="fieldClassName(formstate.password)">
                    <input type="password" class="form-control" name="password" placeholder="Password" required minlength="6" maxlength="100" v-model.trim.lazy="model.password" />
                    <field-messages name="password" show="$invalid && $dirty">
                        <div slot="required">a password is required</div>
                        <div slot="passwordModelErrorValidator">{{ getModelError('Password') }}</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.confirmPassword)" :custom="{ customValidator: passwordMatch }">
                    <input type="password" class="form-control" name="confirmPassword" placeholder="Confirm Password" required v-model.trim.lazy="model.confirmPassword" />
                    <field-messages name="confirmPassword" show="$invalid && $dirty">
                        <div slot="required">you must confirm your password</div>
                        <div slot="passwordMatch">your passwords must match</div>
                    </field-messages>
                </validate>
            </vue-form>
            <router-link tag="button" class="btn btn-default" to="/login">Cancel</router-link>
            <button class="btn btn-primary">Register</button>
        </div>
    </div>
</template>

<script src="./register.ts"></script>

<style src="./user.scss" lang="scss"></style>