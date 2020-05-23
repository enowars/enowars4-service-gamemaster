"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SignalRContext = void 0;
const signalR = require("@microsoft/signalr");
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/session")
    .configureLogging(signalR.LogLevel.Debug)
    .build();
class SignalRContext {
    create() { }
    setup() {
        connection.on("test", (data) => {
            console.log("recv " + data);
        });
        connection.on("Chat", (msg) => {
            this.proxyhandleChat(msg);
        });
        connection.on("Scene", (sceneUpdate) => {
            this.proxyhandleSceneUpdate(sceneUpdate);
        });
        connection.start().catch(err => console.log(err));
    }
    static getInstance() {
        if (!SignalRContext.instance) {
            SignalRContext.instance = new SignalRContext();
            SignalRContext.instance.setup();
        }
        return SignalRContext.instance;
    }
    proxyhandleChat(msg) {
        if (!(this.chat == null)) {
            this.chat.handleChat(msg);
        }
    }
    proxyhandleSceneUpdate(sceneUpdate) {
        console.log("proxyhandleSceneUpdate");
        console.log(sceneUpdate);
        console.log(this);
        if (!(this.phaser == null)) {
            var phasercontext = this.phaser.scene.getScene('CombatScene');
            phasercontext.handleSceneUpdate(sceneUpdate);
        }
    }
    async move(dir) {
        connection.send("Move", dir);
    }
    async join(sid) {
        console.log("Joining...");
        console.log(connection);
        var r = connection.send("Join", sid);
        console.log(r);
        await r;
        console.log(r);
        console.log("after joining...");
    }
}
exports.SignalRContext = SignalRContext;
//# sourceMappingURL=SignalRhelper.js.map