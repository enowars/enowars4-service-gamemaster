"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SignalRContext = void 0;
const SignalR = require("@microsoft/signalr");
let SignalRContext = /** @class */ (() => {
    class SignalRContext {
        constructor() {
            this.handleSceneUpdate = null;
            this.lastSceneUpdate = null;
            this.sessionId = null;
            this.connection = new SignalR.HubConnectionBuilder()
                .withUrl("/hubs/session")
                .configureLogging(SignalR.LogLevel.Debug)
                .build();
            this.connection.onclose((err) => {
                console.log("SignalR connection closed: " + err);
                this.connection
                    .start()
                    .then(() => this.onConnected())
                    .catch((err) => {
                    return console.log(err);
                });
            });
            this.connection.on("test", (data) => {
                console.log("recv " + data);
            });
            this.connection.on("Chat", (msg) => {
                if (this.chat !== null) {
                    this.chat.handleChat(msg);
                }
            });
            this.connection.on("Scene", (sceneUpdate) => {
                if (this.handleSceneUpdate !== null) {
                    this.handleSceneUpdate(sceneUpdate);
                }
                else {
                    console.log("Stalling scene update: this.sceneUpdateHandler is null");
                    this.lastSceneUpdate = sceneUpdate;
                }
            });
            this.connection
                .start()
                .then(() => this.onConnected())
                .catch((err) => {
                return console.log(err);
            });
        }
        static getInstance() {
            if (!SignalRContext.instance) {
                SignalRContext.instance = new SignalRContext();
            }
            return SignalRContext.instance;
        }
        onConnected() {
            if (this.sessionId !== null) {
                this.join();
            }
        }
        setSceneUpdateHandler(handler) {
            console.log("setSceneUpdateHandler(" + handler + ")");
            this.handleSceneUpdate = handler;
            if (this.lastSceneUpdate !== null) {
                handler(this.lastSceneUpdate);
            }
        }
        dragto(dx, dy) {
            console.log("Drag(" + dx + ", " + dy + ")");
            SignalRContext.getInstance().connection.send("Drag", dx, dy);
        }
        join() {
            if (this.connection.state === SignalR.HubConnectionState.Connected) {
                console.log("Join(" + this.sessionId + ")");
                this.connection
                    .send("Join", this.sessionId)
                    .then(function () {
                    console.log("after joining...");
                });
            }
            else {
                console.log("join() aborting: HubConnection is not connected");
            }
        }
        setSessionId(sessionId) {
            if (this.sessionId !== sessionId) {
                this.sessionId = sessionId;
                this.join();
            }
        }
        ensureConnected() {
            if (this.connection.state !== SignalR.HubConnectionState.Connected
                && this.connection.state !== SignalR.HubConnectionState.Connecting
                && this.connection.state !== SignalR.HubConnectionState.Reconnecting) {
                this.connection
                    .start()
                    .then(() => this.onConnected())
                    .catch((err) => {
                    return console.log(err);
                });
            }
        }
    }
    SignalRContext.instance = null;
    return SignalRContext;
})();
exports.SignalRContext = SignalRContext;
//# sourceMappingURL=signalrhelper.js.map