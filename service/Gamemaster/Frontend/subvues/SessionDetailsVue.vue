<template>
  <div>
    <h1>More Session Details for Session {{ sessionId}}:</h1>
      <SceneVue :session-id="sessionId"></SceneVue>
      <SidebarVue></SidebarVue>
  </div>
</template>

<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import SidebarVue from "./SidebarVue.vue";
    import SceneVue from "./SceneVue.vue";
    import SessionListElement from "./SessionListElement.vue";
    export default defineComponent({
        props: ['sessionId'],
        data() {
            return {
                input: {
                    access: false
                }
            }
        },
        components: {
            SidebarVue,
            SceneVue
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
                    //console.log(response);
                    if (response.status == 200) {
                    } else {
                        console.log("this should not happen...");
                    }
                }).catch(error => {
                    router.push 
                    console.log(error);
                })
            return {
            }
        }
    });
</script>