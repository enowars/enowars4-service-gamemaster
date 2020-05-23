"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CombatScene = void 0;
const Phaser = require("phaser");
const SignalRhelper_1 = require("./SignalRhelper");
const sceneConfig = {
    active: false,
    visible: false,
    key: 'Game'
};
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
        console.log(this.map);
        const tiles = this.map.addTilesetImage('Desert', 'tiles');
        this.groundLayer = this.map.createDynamicLayer('Ground', tiles, 0, 0).setVisible(false);
        this.rt = this.add.renderTexture(0, 0, 800, 600);
        this.move = SignalRhelper_1.SignalRContext.getInstance().move;
        /*
        connection.on("test", (data: string) => {
            console.log("recv " + data);
        });
        connection.on("Chat", (msg: ChatMessage) => {
            this.handleChat(msg);
        });
        connection.on("Scene", (data: Scene) => {
            this.handleSceneUpdate(data);
        });  */
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
                this.units.set(id, this.add.sprite(updatedUnit.x, updatedUnit.y, 'car', 0));
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