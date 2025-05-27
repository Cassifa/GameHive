export class GameObject {
    constructor() {
        this.has_called_start = false;
        this.timedelta = 0;
        this.uuid = this.create_uuid();
    }

    create_uuid() {
        let res = "";
        for (let i = 0; i < 8; i++) {
            let x = parseInt(Math.floor(Math.random() * 10));
            res += x;
        }
        return res;
    }

    start() {  // 只会在第一帧执行一次

    }

    update() {  // 每一帧均会执行一次

    }

    on_destroy() {  // 在被销毁前执行一次

    }

    destroy() {
        this.on_destroy();

        for (let i in AC_GAME_OBJECTS) {
            if (AC_GAME_OBJECTS[i] === this) {
                AC_GAME_OBJECTS.splice(i, 1);
                break;
            }
        }
    }
}

let AC_GAME_OBJECTS = [];

export { AC_GAME_OBJECTS }; 