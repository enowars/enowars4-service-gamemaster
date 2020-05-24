import * as signalR from "@microsoft/signalr";
import * as Phaser from 'phaser';
import { Scene, Unit } from "./types";
import { SignalRContext } from "./signalrhelper";

const sceneConfig: Phaser.Types.Scenes.SettingsConfig = {
    active: false,
    visible: false,
    key: 'Game'
};

export class Token extends Phaser.GameObjects.Sprite {
    constructor(scene: Phaser.Scene, x: number, y: number) {
        console.log("Token Created");
        super(scene, x, y, 'car', 0);
        this.setInteractive();
        scene.input.setDraggable(this);
        scene.input.on('dragend', function (pointer: any, gameObject: Token) {
            console.log(`dragend (${gameObject.x} | ${gameObject.y})`);
            SignalRContext.getInstance().dragto(gameObject.x, gameObject.y);
        });
        scene.input.on('drag', (pointer: any, gameObject: Token, dragX: number, dragY: number) => {
            gameObject.x = dragX
            gameObject.y = dragY
        });
    }
}


export class CombatScene extends Phaser.Scene {
    private square: Phaser.GameObjects.Rectangle & { body: Phaser.Physics.Arcade.Body };
    private map: Phaser.Tilemaps.Tilemap;
    private groundLayer: Phaser.Tilemaps.DynamicTilemapLayer;
    private unitsLayer: Phaser.Tilemaps.DynamicTilemapLayer;
    private rt: Phaser.GameObjects.RenderTexture;
    private units: Map<string, Token> = new Map();
    private sessionid: number;
    public move: (dir: number) => void;
    constructor() {
        super(sceneConfig);
    }

    public preload() {
        this.load.image('tiles', '/assets/tilemaps/tiles/tmw_desert_spacing.png');
        this.load.tilemapTiledJSON('map', '/assets/tilemaps/maps/desert.json');
        this.load.image('car', '/assets/sprites/car.png');
    }

    public create() {
        
        this.map = this.make.tilemap({ key: 'map' });
        const tiles = this.map.addTilesetImage('Desert', 'tiles');
        this.groundLayer = this.map.createDynamicLayer('Ground', tiles, 0, 0).setVisible(false);
        this.rt = this.add.renderTexture(0, 0, 800, 600);
        var ctx: SignalRContext = SignalRContext.getInstance();
        this.move = ctx.move;
        this.rt.draw(this.groundLayer);
    }


    public handleSceneUpdate(sceneUpdate: Scene) {
        for (const id in sceneUpdate.units) {
            const updatedUnit = sceneUpdate.units[id];
            if (this.units.has(id)) {
                const sprite = this.units.get(id);
                sprite.x = updatedUnit.x;
                sprite.y = updatedUnit.y;
            } else {
                var t = new Token(this, updatedUnit.x, updatedUnit.y);
                this.add.existing(t);
                this.units.set(id, t);
                //this.units.set(id, this.add.sprite(updatedUnit.x, updatedUnit.y, 'car', 0));
            }
        }
        this.units.forEach((sprite: Phaser.GameObjects.Sprite, id: string) => {
            if (sceneUpdate.units[id] === undefined) {
                this.units.get(id).destroy();
                this.units.delete(id);
            }
        });
    }

    public update() {
        const cursorKeys = this.input.keyboard.createCursorKeys();
        if (cursorKeys.up.isDown) {
            this.move(0);
        }
        if (cursorKeys.right.isDown) {
            this.move(1);
        }
        if (cursorKeys.down.isDown) {
            this.move(2);
        }
        if (cursorKeys.left.isDown) {
            this.move(3);
        }
        this.rt.draw(this.groundLayer);
    }
}

