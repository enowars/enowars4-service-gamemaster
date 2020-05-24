import * as SignalR from "@microsoft/signalr";
import { Scene, ChatMessage } from "./types";
import { CombatScene } from "./phasergame";
import { ChatHandler } from "./chatHandler";



export class SignalRContext{
    private static instance: SignalRContext = null;
    private sessionid: number;
    public phaser: Phaser.Game;
    public chat: ChatHandler;
    public joinid: number;
    public connection: SignalR.HubConnection;

    private constructor() {
        this.connection = new SignalR.HubConnectionBuilder()
            .withUrl("/hubs/session")
            .configureLogging(SignalR.LogLevel.Debug)
            .build();
        this.connection.on("test", (data: string) => {
            console.log("recv " + data);
        });
        this.connection.on("Chat", (msg: ChatMessage) => {
            this.proxyhandleChat(msg);
        });
        this.connection.on("Scene", (sceneUpdate: Scene) => {
            this.proxyhandleSceneUpdate(sceneUpdate);
        });
        this.connection.start().then(() => {
            this.join(this.joinid);
        }).catch((err:any) => {
            return console.log(err);
        });
    }

    public reconnect() {
        this.connection = new SignalR.HubConnectionBuilder()
            .withUrl("/hubs/session")
            .configureLogging(SignalR.LogLevel.Debug)
            .build();
        this.connection.on("test", (data: string) => {
            console.log("recv " + data);
        });
        this.connection.on("Chat", (msg: ChatMessage) => {
            this.proxyhandleChat(msg);
        });
        this.connection.on("Scene", (sceneUpdate: Scene) => {
            this.proxyhandleSceneUpdate(sceneUpdate);
        });
        this.connection.start().then(() => {
            this.join(this.joinid);
        }).catch((err: any) => {
            return console.log(err);
        });
    }

    static getInstance(): SignalRContext {
        if (!SignalRContext.instance) {
            SignalRContext.instance = new SignalRContext();
        }
        return SignalRContext.instance;
    }


    private proxyhandleChat(msg: ChatMessage) {
        if (!(this.chat == null)) {
            this.chat.handleChat(msg);
        }
    }
    private proxyhandleSceneUpdate(sceneUpdate: Scene) {
        if (!(this.phaser == null)) {
            var scene = this.phaser.scene.scenes[0];
            (scene as CombatScene).handleSceneUpdate(sceneUpdate);
        }
    }

    public move(dir: number) {
        SignalRContext.getInstance().connection.send("Move", dir);
    }
    public dragto(dx: number, dy: number) {
        console.log("sendDrag");
        console.log(dx);
        console.log(dy);
        SignalRContext.getInstance().connection.send("Drag", dx, dy);
    }
    private join(sid: number) {
        console.log("Method: Joining...");
        console.log(this.joinid);
        this.connection.send("Join", sid).then(function () { console.log("after joining..."); });
    }
    public tryjoin(sid: number) {
        console.log("Try Joining...");
        console.log(sid);
        this.joinid = sid;
        this.connection.send("Join", sid).then(function () { console.log("after joining..."); });
    }
}