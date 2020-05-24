"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SignalRContext = void 0;
const SignalR = require("@microsoft/signalr");
let SignalRContext = /** @class */ (() => {
    class SignalRContext {
        constructor() {
            this.connection = new SignalR.HubConnectionBuilder()
                .withUrl("/hubs/session")
                .configureLogging(SignalR.LogLevel.Debug)
                .build();
            this.connection.on("test", (data) => {
                console.log("recv " + data);
            });
            this.connection.on("Chat", (msg) => {
                this.proxyhandleChat(msg);
            });
            this.connection.on("Scene", (sceneUpdate) => {
                this.proxyhandleSceneUpdate(sceneUpdate);
            });
            this.connection.start().then(() => {
                this.join(this.joinid);
            }).catch((err) => {
                return console.log(err);
            });
        }
        reconnect() {
            this.connection = new SignalR.HubConnectionBuilder()
                .withUrl("/hubs/session")
                .configureLogging(SignalR.LogLevel.Debug)
                .build();
            this.connection.on("test", (data) => {
                console.log("recv " + data);
            });
            this.connection.on("Chat", (msg) => {
                this.proxyhandleChat(msg);
            });
            this.connection.on("Scene", (sceneUpdate) => {
                this.proxyhandleSceneUpdate(sceneUpdate);
            });
            this.connection.start().then(() => {
                this.join(this.joinid);
            }).catch((err) => {
                return console.log(err);
            });
        }
        static getInstance() {
            if (!SignalRContext.instance) {
                SignalRContext.instance = new SignalRContext();
            }
            return SignalRContext.instance;
        }
        proxyhandleChat(msg) {
            if (!(this.chat == null)) {
                this.chat.handleChat(msg);
            }
        }
        proxyhandleSceneUpdate(sceneUpdate) {
            if (!(this.phaser == null)) {
                var scene = this.phaser.scene.scenes[0];
                scene.handleSceneUpdate(sceneUpdate);
            }
        }
        move(dir) {
            SignalRContext.getInstance().connection.send("Move", dir);
        }
        dragto(dx, dy) {
            console.log("sendDrag");
            console.log(dx);
            console.log(dy);
            SignalRContext.getInstance().connection.send("Drag", dx, dy);
        }
        join(sid) {
            console.log("Method: Joining...");
            console.log(this.joinid);
            this.connection.send("Join", sid).then(function () { console.log("after joining..."); });
        }
        tryjoin(sid) {
            console.log("Try Joining...");
            console.log(sid);
            this.joinid = sid;
            this.connection.send("Join", sid).then(function () { console.log("after joining..."); });
        }
    }
    SignalRContext.instance = null;
    return SignalRContext;
})();
exports.SignalRContext = SignalRContext;
//# sourceMappingURL=signalrhelper.js.map