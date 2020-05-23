import * as signalR from "@microsoft/signalr";
import { Scene, ChatMessage } from "./types";
import { CombatScene } from "./phasergame";
import { ChatHandler } from "./chatHandler";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/session")
    .configureLogging(signalR.LogLevel.Debug)
    .build();

export class SignalRContext{
    private static instance: SignalRContext;
    private sessionid: number;
    public phaser: Phaser.Game;
    public chat: ChatHandler;


    private create() { }
    public setup() {
        connection.on("test", (data: string) => {
            console.log("recv " + data);
        });
        connection.on("Chat", (msg: ChatMessage) => {
            this.proxyhandleChat(msg);
        });
        connection.on("Scene", (sceneUpdate: Scene) => {
            this.proxyhandleSceneUpdate(sceneUpdate);
        });
        connection.start().catch(err => console.log(err));
    }

    static getInstance(): SignalRContext {
        if (!SignalRContext.instance) {
            SignalRContext.instance = new SignalRContext();
            SignalRContext.instance.setup();
        }
        return SignalRContext.instance;
    }


    private proxyhandleChat(msg: ChatMessage) {
        if (!(this.chat == null)) {
            this.chat.handleChat(msg);
        }
    }
    private proxyhandleSceneUpdate(sceneUpdate: Scene) {
        console.log("proxyhandleSceneUpdate")
        console.log(sceneUpdate);
        console.log(this);
        if (!(this.phaser == null)) {
            var phasercontext = this.phaser.scene.getScene('CombatScene') as CombatScene;
            phasercontext.handleSceneUpdate(sceneUpdate);
        }
    }

    public move(dir:number) {
        connection.send("Move", dir);
    }
    public join(sid: number) {
        console.log("Joining...");
        console.log(connection);
        connection.connectionState.then(function () {
            connection.send("Join", sid).then(function () { console.log("after joining..."); });
        });
    }
}

