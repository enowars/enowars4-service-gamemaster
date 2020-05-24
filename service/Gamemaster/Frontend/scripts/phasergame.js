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
        scene.input.on('dragend', function (pointer, gameObject) {
            console.log(`dragend (${gameObject.x} | ${gameObject.y})`);
            signalrhelper_1.SignalRContext.getInstance().dragto(gameObject.x, gameObject.y);
        });
        scene.input.on('drag', (pointer, gameObject, dragX, dragY) => {
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
        this.load.image('tiles', '/assets/tilemaps/tiles/tmw_desert_spacing.png');
        this.load.tilemapTiledJSON('map', '/assets/tilemaps/maps/desert.json');
        this.load.image('car', '/assets/sprites/car.png');
    }
    create() {
        this.map = this.make.tilemap({ key: 'map' });
        const tiles = this.map.addTilesetImage('Desert', 'tiles');
        this.groundLayer = this.map.createDynamicLayer('Ground', tiles, 0, 0).setVisible(false);
        this.rt = this.add.renderTexture(0, 0, 800, 600);
        var ctx = signalrhelper_1.SignalRContext.getInstance();
        this.move = ctx.move;
        this.rt.draw(this.groundLayer);
    }
    handleSceneUpdate(sceneUpdate) {
        for (const id in sceneUpdate.units) {
            const updatedUnit = sceneUpdate.units[id];
            if (this.units.has(id)) {
                const sprite = this.units.get(id);
                sprite.x = updatedUnit.x;
                sprite.y = updatedUnit.y;
            }
            else {
                var t = new Token(this, updatedUnit.x, updatedUnit.y);
                this.add.existing(t);
                this.units.set(id, t);
                //this.units.set(id, this.add.sprite(updatedUnit.x, updatedUnit.y, 'car', 0));
            }
        }
        this.units.forEach((sprite, id) => {
            if (sceneUpdate.units[id] === undefined) {
                this.units.get(id).destroy();
                this.units.delete(id);
            }
        });
    }
    update() {
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
exports.CombatScene = CombatScene;
//# sourceMappingURL=phasergame.js.map