<template>
    <!--<div>-->
        <!-- <h1>Session Details for Session {{$route.params.id}}</h1> -->
        <SessionDetailsDenied v-if="!input.access" />
        <SessionDetailsVue v-if="input.access" :session-id="input.id" />
    <!--</div> -->
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import { gmState } from './../store/gmstate';
    import router from './../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import SessionDetailsVue from './SessionDetailsVue.vue';
    import SessionDetailsDenied from './SessionDetailsDenied.vue';
    export default defineComponent({
        data() {
            return {
                input: {
                    access: false,
                    id: "test"
                }
            }
        },
        components: {
            SessionDetailsVue,
            SessionDetailsDenied
        },
        setup() {

        },
        mounted() {
            var bodyFormData = new FormData();
            const foo = this as any;
            bodyFormData.set('id', foo.$route.params.id);
            this.input.id = foo.$route.params.id;
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
                        this.input.access = true;
                    } else {
                        console.log("this should not happen...");
                    }
                }).catch(error => {
                    this.input.access = false;
                    console.log(error);
                })
            return {
            }
        },
        methods: {
        }
    });
</script>