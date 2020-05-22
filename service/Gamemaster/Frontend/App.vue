<template>
    <div class="container">
        <div id='nav'>
            <h1>Gamemaster Service</h1>
            <router-link to='/'>Home</router-link>
            <router-link to='/login'>Login</router-link>
            <router-link to='/register'>Register</router-link>
            <router-link to='/sessionList'>Sessions</router-link>
            <router-link v-if="state.username != null" to='/accountSettings'>Test {{state.username}}</router-link>
            <router-link v-if="state.username != null" to='/'><span v-on:click="logout()">Logout</span></router-link>
        </div>
        <router-view />
    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "./store/gmstate";
    import router from './router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    export default defineComponent({
        setup() {
            return {
                state: gmState.getState()
            }
        },
        methods: {
            logout() {
                console.log("Logging out...");
                const options: AxiosRequestConfig = {
                    method: 'POST',
                    headers: { 'Content-Type': 'x-www-form-urlencoded' },
                    url: '/api/account/logout',
                };
                axios(options).then(
                    response => {
                        console.log(response);
                        if (response.status == 200) {
                            console.log("Logoff Successful");
                            alert("Logoff Successful");
                            gmState.logoff();
                            router.push("/");
                        } else {
                            console.log("this should not happen...");
                        }
                    }).catch(error => {
                        console.log("Logoff failed");
                        console.log(error);
                        alert("Logoff failed");
                    })
                return false;
            }
        }
    });
</script>

<style scoped>

    #root {
        text-align: center;
        font-family: 'Lucida Sans', 'Lucida Sans Regular', 'Lucida Grande', 'Lucida Sans Unicode', Geneva, Verdana, sans-serif;
    }

    img {
        width: 200px;
    }
    h1 {
        display: inline;
    }
    #nav {
        font-size: 1.5em;
        margin-bottom: 30px;
    }

    a {
        text-decoration: none;
        margin: 30px 25px;
        color: #333;
    }

        a:hover {
            text-decoration: underline;
            color:
        }
</style>
