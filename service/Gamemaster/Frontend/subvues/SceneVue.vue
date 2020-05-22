<template>
    <div id="scene" class="scene">
        Scene Here
    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import { CombatScene } from "./../scripts/phasergame";
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
        }
    });
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
    export const game = new Phaser.Game(gameConfig);
</script>