<template>
    <div id="scene" class="scene">
    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import { CombatScene } from "./../scripts/phasergame";
    import { SignalRContext } from "./../scripts/SignalRHelper";
import { join } from 'path';
    export default defineComponent({
        props: ['sessionId'],
        data() {
            return {
                input: {
                    game: null
                }
            }
        },
        components: {
        },
        mounted() {
            const gameConfig: Phaser.Types.Core.GameConfig = {
                title: 'Sample',
                type: Phaser.AUTO,
                scale: {
                    width: 800,
                    height: 600,
                },
                physics: {
                    default: 'arcade',
                    arcade: {
                        debug: true,
                    },
                },
                parent: 'scene',
                backgroundColor: '#000000',
                scene: CombatScene
            };
            var ctx: SignalRContext = SignalRContext.getInstance();
            console.log("Trying to join");
            var sid = this.sessionId;
            ctx.join(sid);
            //ctx.join(sid);
            this.input.game = new Phaser.Game(gameConfig);
            ctx.phaser = this.input.game;
        },
        beforeUnmount() {
            var ctx: SignalRContext = SignalRContext.getInstance();
        }

    });

</script>