"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CombatScene = exports.Token = void 0;
const Phaser = require("phaser");
const signalrhelper_1 = require("./signalrhelper");
const sceneConfig = {
    active: false,
    visible: false,
    key: 'Game'
};
class Token extends Phaser.GameObjects.Sprite {
    constructor(scene, x, y) {
        console.log("Token Created");
        super(scene, x, y, 'car', 0);
        this.setInteractive();
        scene.input.setDraggable(this);
        scene.input.on('dragend', function (_pointer, gameObject) {
            signalrhelper_1.SignalRContext.getInstance().dragto(gameObject.x, gameObject.y);
        });
        scene.input.on('drag', (_pointer, gameObject, dragX, dragY) => {
            gameObject.x = dragX;
            gameObject.y = dragY;
        });
    }
}
exports.Token = Token;
class CombatScene extends Phaser.Scene {
    constructor() {
        super(sceneConfig);
        this.units = new Map();
    }
    preload() {
        console.log("CombatScene.preload");
        this.load.image('tiles', '/assets/tilemaps/tiles/tmw_desert_spacing.png');
        this.load.tilemapTiledJSON('map', '/assets/tilemaps/maps/desert.json');
        this.load.image('car', '/assets/sprites/car.png');
    }
    create() {
        console.log("CombatScene.create");
        this.map = this.make.tilemap({ key: 'map' });
        const tiles = this.map.addTilesetImage('Desert', 'tiles');
        this.groundLayer = this.map.createDynamicLayer('Ground', tiles, 0, 0).setVisible(false);
        this.rt = this.add.renderTexture(0, 0, 800, 600);
        this.rt.draw(this.groundLayer);
        signalrhelper_1.SignalRContext.getInstance().setSceneUpdateHandler((s) => this.handleSceneUpdate(s));
    }
    handleSceneUpdate(sceneUpdate) {
        console.log("handleSceneUpdate(" + JSON.stringify(sceneUpdate) + ")");
        for (const id in sceneUpdate.units) {
            const updatedUnit = sceneUpdate.units[id];
            const sprite = this.units.get(id);
            if (sprite !== undefined) {
                console.log("Updating sprite " + id);
                sprite.x = updatedUnit.x;
                sprite.y = updatedUnit.y;
            }
            else {
                console.log("Adding sprite " + id);
                const t = new Token(this, updatedUnit.x, updatedUnit.y);
                this.add.existing(t);
                this.units.set(id, t);
                //this.units.set(id, this.add.sprite(updatedUnit.x, updatedUnit.y, 'car', 0));
            }
        }
        this.units.forEach((_sprite, id) => {
            if (sceneUpdate.units[id] === undefined) {
                const sprite = this.units.get(id);
                if (sprite !== undefined) {
                    sprite.destroy();
                }
                this.units.delete(id);
            }
        });
    }
}
exports.CombatScene = CombatScene;
//# sourceMappingURL=phasergame.js.map