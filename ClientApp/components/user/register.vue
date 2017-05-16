<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Register for a New Account</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.email)">
                    <input type="email" class="form-control" name="email" placeholder="Email" v-model.trim.lazy="model.email" required />
                    <field-messages name="email" show="$touched || $submitted">
                        <div slot="required">a valid email address is required</div>
                        <div slot="email">a valid email address is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.password)">
                    <input type="password" class="form-control" name="password" placeholder="Password" v-model.trim.lazy="model.password"
                           required minlength="6" maxlength="100" pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])" />
                    <field-messages name="password" show="$touched || $submitted">
                        <div slot="required">a password is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group" v-bind:class="fieldClassName(formstate.confirmPassword)" :custom="{ customValidator: passwordMatch }">
                    <input type="password" class="form-control" name="confirmPassword" placeholder="Confirm Password" v-model.trim.lazy="model.confirmPassword" required />
                    <field-messages name="confirmPassword" show="$touched || $submitted">
                        <div slot="required">you must confirm your password</div>
                        <div slot="customValidator">your passwords must match</div>
                    </field-messages>
                </validate>
                <ul class="text-danger">
                    <li v-for="error in model.errors">{{ error }}</li>
                </ul>
                <div class="submit-row">
                    <router-link tag="button" class="btn btn-default" :to="{ path: '/login', query: { returnUrl }}">Cancel</router-link>
                    <button type="submit" class="btn btn-primary">Register</button>
                </div>
            </vue-form>
        </div>
    </div>
</template>

<script src="./register.ts"></script>

<style src="./user.scss" lang="scss"></style>