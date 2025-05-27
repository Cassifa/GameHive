import { AC_GAME_OBJECTS } from './GameObject.js';

export class GameEngine {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.isRunning = false;
        this.lastTime = 0;
    }

    start() {
        this.isRunning = true;
        this.lastTime = Date.now();
        this.gameLoop();
    }

    stop() {
        this.isRunning = false;
    }

    gameLoop() {
        if (!this.isRunning) return;

        const currentTime = Date.now();
        const timedelta = currentTime - this.lastTime;
        this.lastTime = currentTime;

        this.update(timedelta);
        this.render();

        requestAnimationFrame(() => this.gameLoop());
    }

    update(timedelta) {
        for (let obj of AC_GAME_OBJECTS) {
            if (!obj.has_called_start) {
                obj.has_called_start = true;
                obj.start();
            } else {
                obj.timedelta = timedelta;
                obj.update();
            }
        }
    }

    render() {
        // 清空画布
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        // 渲染所有游戏对象
        for (let obj of AC_GAME_OBJECTS) {
            if (obj.render) {
                obj.render();
            }
        }
    }

    addObject(obj) {
        AC_GAME_OBJECTS.push(obj);
    }

    removeObject(obj) {
        const index = AC_GAME_OBJECTS.indexOf(obj);
        if (index > -1) {
            AC_GAME_OBJECTS.splice(index, 1);
        }
    }

    clearObjects() {
        AC_GAME_OBJECTS.length = 0;
    }
} 