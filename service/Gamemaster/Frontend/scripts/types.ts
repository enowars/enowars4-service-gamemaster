export class Scene {
    units: { [id: string]: Unit } | null = null;
}

export class ChatMessage {
    id = 0;
    SenderName = "SenderName";
    SessionContextId = 0;
    Content = "Content";
    date = "date";
}

export class Unit {
    x = 0;
    y = 0;
}
