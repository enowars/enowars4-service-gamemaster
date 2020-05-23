import * as signalR from "@microsoft/signalr";
import { ChatMessage } from "./types";



export class ChatHandler {
    public move: (msg: string) => void;
    constructor() {
        
    }
    public handleChat(msg: ChatMessage) {
        console.log(msg);
    }
}

