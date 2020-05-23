<template>
    <div id="sidebar" class="sidebar">
        <ChatMessageVue v-for="msg in chatdata" :data="msg"></ChatMessageVue>
    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import ChatMessageVue from "./ChatMessageVue.vue";
    export default defineComponent({
        props: ['sessionId'],
        components: {
            ChatMessageVue
        },
        data() {
            return { chatdata: [] };
        },
        mounted() {

        }

    });

</script>