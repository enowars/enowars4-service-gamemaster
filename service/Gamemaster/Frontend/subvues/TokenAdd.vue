<template>
    <div id="login">
        <h1>Login</h1>
        <form id="login"
              action=""
              method="post">
            <input type="text" name="name" v-model="input.name" placeholder="Name" />
            <input type="password" name="description" v-model="input.description" placeholder="Description" />
            <input type="checkbox" name="isPrivate" v-model="input.isPrivate" placeholder="Description" />
            <input type="file" name="file" @bind="@Files">
            <button type="button" v-on:click="add()">Add</button>
        </form>
    </div>
</template>

<script lang="ts">
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import router from './../router';
    import { METHODS } from 'http';
    import { gmState } from "../store/gmstate";
    export default {
        name: 'Add',
        data() {
            return {
                input: {
                    name: "",
                    description: "",
                    isPrivate: true
                }
            }
        },
        methods: {
            add() {
                var bodyFormData = new FormData();
                bodyFormData.set('name', this.input.name);
                bodyFormData.set('description', this.input.description);
                bodyFormData.set('isPrivate', this.input.isPrivate);
                console.log("Adding Token...");
                const options: AxiosRequestConfig = {
                    method: 'POST',
                    data: bodyFormData,
                    headers: { 'Content-Type': 'x-www-form-urlencoded' },
                    url: '/api/account/AddToken',
                };
                axios(options).then(
                    response => {
                        console.log(response);
                        if (response.status == 200) {
                            console.log("Token Added Successfully");
                            alert("Token Added Successfully");
                            gmState.login(this.input.username)
                            router.push("/");
                        } else {
                            console.log("this should not happen...");
                        }
                    }).catch(error => {
                        console.log("Token Add failed");
                        console.log(error);
                        alert("Token Add failed");
                    })
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