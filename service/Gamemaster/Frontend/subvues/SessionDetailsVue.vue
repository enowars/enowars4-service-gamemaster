<template>
    <div class=" inline">
        <h1>Session Details for Session {{ sessionId}}:</h1>
        <ChatbarVue :session-id="sessionId"></ChatbarVue>
        <RightbarVue></RightbarVue>
        <SceneVue :session-id="sessionId"></SceneVue>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import ChatbarVue from "./ChatbarVue.vue";
    import SceneVue from "./SceneVue.vue";
    import RightbarVue from "./RightbarVue.vue";
    import SessionListElement from "./SessionListElement.vue";
    export default defineComponent({
        props: ['sessionId'],
        data() {
            return {
                input: {
                    ownername: ""
                }
            }
        },
        components: {
            ChatbarVue,
            SceneVue,
            RightbarVue
        },
        mounted() {
            var bodyFormData = new FormData();
            bodyFormData.set('id', this.$props.sessionId);
            const options: AxiosRequestConfig = {
                method: 'POST',
                data: bodyFormData,
                headers: { 'Content-Type': 'x-www-form-urlencoded' },
                url: '/api/gamesession/getinfo',
            };
            axios(options).then(
                response => {
                    console.log(response);
                    if (response.status == 200) {
                        this.input.ownername = response.data.ownerName;
                    } else {
                        console.log("this should not happen...");
                    }
                }).catch(error => {
                    console.log(error);
                })
            return {
            }
        }
    });
</script>

<style scoped>
    .inline {
        display: inline-block;
        text-align: center;
        /*height: 100%;*/
        width: 100%;
        min-height: 100vh;
    }
</style>