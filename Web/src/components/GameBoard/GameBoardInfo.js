export class GameBoardInfo {
    constructor(column, isCenter) {
        this.column = column;
        this.isCenter = isCenter;
        this.totalSize = 600; // 棋盘总大小
        this.calculateMapping();
    }

    calculateMapping() {
        this.chessCenter = [];
        this.columns = [];
        this.rows = [];

        if (this.isCenter) {
            // 中心落子逻辑
            const gridCount = this.column;
            this.R = this.totalSize / (gridCount * 2 + 2);
            this.bias = this.R;
            const cellSize = this.R * 2;
            this.boardLength = this.totalSize - this.R;

            for (let i = 0; i < this.column; i++) {
                const rowCenters = [];
                for (let j = 0; j < this.column; j++) {
                    const x = j * cellSize + cellSize / 2 + this.R;
                    const y = i * cellSize + cellSize / 2 + this.R;
                    rowCenters.push({ x, y });
                    
                    if (i === 0) {
                        this.columns.push(j * cellSize + this.R);
                    }
                }
                this.chessCenter.push(rowCenters);
                this.rows.push(i * cellSize + this.R);
            }
            this.rows.push(this.column * cellSize + this.R);
            this.columns.push(this.column * cellSize + this.R);
        } else {
            // 交点落子逻辑
            const gridCount = this.column - 1;
            this.R = this.totalSize / (gridCount * 2 + 4);
            this.bias = 2 * this.R;
            const cellSize = this.R * 2;
            this.boardLength = this.totalSize - 2 * this.R;

            for (let i = 0; i < this.column; i++) {
                const rowCenters = [];
                for (let j = 0; j < this.column; j++) {
                    const x = j * cellSize + this.R * 2;
                    const y = i * cellSize + this.R * 2;
                    rowCenters.push({ x, y });
                    
                    if (i === 0) {
                        this.columns.push(j * cellSize + this.R * 2);
                    }
                }
                this.chessCenter.push(rowCenters);
                this.rows.push(i * cellSize + this.R * 2);
            }
        }

        this.inputR = this.R * 3 / 5;
        this.showR = this.R * 5 / 6;
    }

    // 根据屏幕坐标获取棋盘坐标
    getChessPosition(screenX, screenY) {
        for (let i = 0; i < this.chessCenter.length; i++) {
            for (let j = 0; j < this.chessCenter[i].length; j++) {
                const center = this.chessCenter[i][j];
                const distance = Math.sqrt(
                    Math.pow(screenX - center.x, 2) + 
                    Math.pow(screenY - center.y, 2)
                );
                
                if (distance <= this.inputR) {
                    return { row: i, col: j, x: center.x, y: center.y };
                }
            }
        }
        return null;
    }

    // 根据棋盘坐标获取屏幕坐标
    getScreenPosition(row, col) {
        if (row >= 0 && row < this.chessCenter.length && 
            col >= 0 && col < this.chessCenter[row].length) {
            return this.chessCenter[row][col];
        }
        return null;
    }
} 