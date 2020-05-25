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

<style scoped>
    .sidebar{
        height: 100%;
        width:20%;
        z-index:1;
        overflow-x: hidden;
        padding: 60px 0px 60px 0px;
        transition: 0.5s;
    }
    @media screen and (max-height: 450px) {
        .sidebar {
            padding-top: 5px;
        }

            .sidenav a {
                font-size: 18px;
            }
    }
</style>