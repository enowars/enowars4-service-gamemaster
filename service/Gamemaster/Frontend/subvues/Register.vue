<template>
    <div id="login">
        <h1>Register</h1>
        <form id="app"
              action="/api/account/register"
              method="post">
            <input type="text" name="username" v-model="input.username" placeholder="Username" />
            <input type="text" name="email" v-model="input.email" placeholder="Email" />
            <input type="password" name="password" v-model="input.password" placeholder="Password" />
            <button type="submit" v-on:click="register()">Register</button>
        </form>
    </div>
</template>

<script>
    export default {
        name: 'Login',
        data() {
            return {
                input: {
                    username: "",
                    email:    "",
                    password: ""
                }
            }
        },
        methods: {
            register() {
                if (this.input.username != "" && this.input.password != "") {
                    console.log("Username and password set");
                    this.$refs.form.submit();
                } else {
                    console.log("A username and password must be present");

                }
                return false;
            }
        }
    }
</script>

<style scoped>
    #login {
        width: 500px;
        border: 1px solid #CCCCCC;
        background-color: #FFFFFF;
        margin: auto;
        margin-top: 200px;
        padding: 20px;
    }
</style>