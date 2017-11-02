<template>
    <v-layout row wrap>
        <v-flex xs10 offset-xs1>
            <h1>{{ "Hello, world!" | t }}</h1>
            <p>Welcome to your new auto-bound single-page application, built with:</p>
            <ul>
                <li><a href="https://get.asp.net/">ASP.NET Core</a> and <a href="https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx">C#</a> for cross-platform server-side code</li>
                <li><a href="https://docs.microsoft.com/en-us/ef/core/">Entity Framework Core</a> and <a href="https://www.microsoft.com/en-us/sql-server/">MS Sql Server</a> for data storage and access</li>
                <li><a href="http://identityserver.io/">IdentityServer</a> for authentication and authorization</li>
                <li><a href="https://vuejs.org/">Vue.js</a> and <a href="http://www.typescriptlang.org/">TypeScript</a> for client-side code</li>
                <li><a href="https://webpack.github.io/">Webpack</a> for building and bundling client-side resources</li>
                <li><a href="https://vuetifyjs.com/">Vuetify</a> for layout and styling</li>
                <li><a href="https://github.com/icebob/vue-form-generator">vue-form-generator</a> for automatic data display and editing</li>
            </ul>
            <p>To help you get started, we've also set up:</p>
            <ul>
                <li><strong>Client-side navigation</strong>. For example, click <em>Groups</em> then <em>Back</em> to return here.</li>
                <li><strong>Webpack dev middleware</strong>. In development mode, there's no need to run the <code>webpack</code> build tool. Your client-side resources are dynamically built on demand. Updates are available as soon as you modify any file.</li>
                <li><strong>Hot module replacement</strong>. In development mode, you don't even need to reload the page after making most changes. Within seconds of saving changes to files, your Vue app will be rebuilt and a new instance injected is into the page.</li>
                <li><strong>Efficient production builds</strong>. In production mode, development-time features are disabled, and the <code>webpack</code> build tool produces minified static CSS and JavaScript files.</li>
            </ul>
        </v-flex>
    </v-layout>
</template>

<script src="./home.ts"></script>