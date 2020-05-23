export class Scene {
    units: { [id: string]: Unit }
}

export class ChatMessage {
    SenderName: string;
    SessionContextId: number;
    Content: string;
    date: any;
}

export class Unit {
    x: number;
    y: number;
}