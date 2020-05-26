<template>
    <div id="sidebar" class="sidebar">
        <div id="chatcontainer" class="chatcontainer">
            <div id="chatbody">
                <h2>Chat History::</h2>
                <ChatMessageVue v-for="msg in chatdata" :data="msg"></ChatMessageVue>
            </div>
            <div id="chatfooter">
                <form>
                    <input type="text" name="Message" v-model="input.msg" placeholder="Description" />
                    <button type="button" v-on:click="send()">Add</button>
                </form>
            </div>
        </div>
    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import ChatMessageVue from "./ChatMessageVue.vue";
    import { SignalRContext } from "./../scripts/signalrhelper";
    export default defineComponent({
        props: ['sessionId'],
        components: {
            ChatMessageVue
        },
        data() {
            return {
                chatdata: [],
                input: {
                    msg: ""
                }
            };
        },
        mounted() {
            console.log("SidebarVue Mounted Called");
            console.log(this.$props.sessionId);
            var bodyFormData = new FormData();
            bodyFormData.set('id', this.$props.sessionId);
            const options: AxiosRequestConfig = {
                method: 'POST',
                data: bodyFormData,
                headers: { 'Content-Type': 'x-www-form-urlencoded' },
                url: '/api/chat/Getrecent',
            };
            axios(options).then(
                response => {
                    console.log("ChatMessages");
                    this.chatdata = response.data;
                });
            return;
        },
        methods: {
            send() {
                var ctx: SignalRContext = SignalRContext.getInstance();
                ctx.sendmsg(this.input.msg);
            }
        }

    });

</script>

<style scoped>
    .sidebar{
        height: 100%;
        width: 19%;
        float: left;
        z-index: 1;
        overflow-x: hidden;
        padding: 60px 0px 60px 0px;
        transition: 0.5s;
    }
    .chatcontainer {
        background-color: coral;
        border: 1px solid blue;
        text-align: initial;
    }
    @media screen and (max-height: 450px) {
        .sidebar {
            padding-top: 5px;
        }
        .sidebar a {
            font-size: 18px;
        }
    }

</style>