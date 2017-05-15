<template>
    <div class="sign-in-container">
        <div class="sign-in-form">
            <div class="logo-container"></div>
            <h4>Sign In</h4>
            <vue-form :state="formstate" v-model="formstate" @submit.prevent="onSubmit">
                <validate auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.username)">
                    <input type="text" class="form-control" name="username" placeholder="User Name" required v-model.trim.lazy="model.username" />
                    <field-messages name="username" show="$invalid && $dirty">
                        <div slot="required">a username is required</div>
                    </field-messages>
                </validate>
                <validate auto-label class="form-group required-field" v-bind:class="fieldClassName(formstate.password)">
                    <input type="password" class="form-control" name="password" placeholder="Password" required v-model.trim.lazy="model.password" />
                    <field-messages name="password" show="$invalid && $dirty">
                        <div slot="required">a password is required</div>
                    </field-messages>
                </validate>
                <div class="checkbox">
                    <label><input type="checkbox" name="rememberUser" />Remember Me</label>
                </div>
            </vue-form>
            <div v-if="!submitting">
                <router-link to="/register">Register</router-link>
                <button type="submit" class="btn btn-primary">Sign In</button>
            </div>
            <div v-if="submitting">
                <p class="submitting">Signing In...</p>
            </div>
        </div>
    </div>
</template>

<script src="./signin.ts"></script>

<style src="./signin.scss" lang="scss"></style>