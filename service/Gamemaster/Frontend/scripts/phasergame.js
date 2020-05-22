"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const signalR = require("@microsoft/signalr");
const Phaser = require("phaser");
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/session")
    .configureLogging(signalR.LogLevel.Information)
    .build();
class Scene {
}
class Unit {
}
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
        connection.on("test", (data) => {
            console.log("recv " + data);
        });
        connection.on("Scene", (data) => {
            this.handleSceneUpdate(data);
        });
        connection.start().catch(err => console.log(err));
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
            connection.send("Move", 0);
        }
        if (cursorKeys.right.isDown) {
            connection.send("Move", 1);
        }
        if (cursorKeys.down.isDown) {
            connection.send("Move", 2);
        }
        if (cursorKeys.left.isDown) {
            connection.send("Move", 3);
        }
        this.rt.draw(this.groundLayer);
    }
}
exports.CombatScene = CombatScene;
//# sourceMappingURL=phasergame.js.map