import { GameObject } from './GameObject.js';
import { GameBoardInfo } from './GameBoardInfo.js';

export class GameBoard extends GameObject {
    constructor(ctx, boardInfo, moves = []) {
        super();
        this.ctx = ctx;
        this.boardInfo = boardInfo;
        this.moves = moves; // 对局步骤数据
        this.currentMoveIndex = 0;
        this.lastMovePosition = null;
        
        // 初始化为使用简单棋子
        this.imagesLoaded = 0;
        
        // 加载棋子图片
        this.loadPieceImages();
        
        // 立即渲染一次（使用简单棋子）
        this.render();
    }

    loadPieceImages() {
        this.whitePiece = new Image();
        this.blackPiece = new Image();
        
        this.imagesLoaded = 0;
        
        this.whitePiece.onload = () => {
            this.imagesLoaded++;
            if (this.imagesLoaded === 2) {
                this.render();
            }
        };
        
        this.blackPiece.onload = () => {
            this.imagesLoaded++;
            if (this.imagesLoaded === 2) {
                this.render();
            }
        };

        this.whitePiece.onerror = () => {
            // 图片加载失败，使用简单圆形棋子
            this.imagesLoaded = -1; // 标记图片加载失败
            this.render();
        };

        this.blackPiece.onerror = () => {
            // 图片加载失败，使用简单圆形棋子
            this.imagesLoaded = -1; // 标记图片加载失败
            this.render();
        };
        
        // 使用正确的静态资源路径
        this.whitePiece.src = process.env.BASE_URL + 'static/pieces/WhitePieces.png';
        this.blackPiece.src = process.env.BASE_URL + 'static/pieces/BlackPieces.png';
    }

    start() {
        this.render();
    }

    update() {
        // 可以在这里添加动画逻辑
    }

    render() {
        this.clearBoard();
        this.drawBoard();
        this.drawCoordinates();
        this.drawPieces();

    }

    clearBoard() {
        // 清空画布并设置背景色
        this.ctx.fillStyle = '#b26227'; // 棋盘背景色
        this.ctx.fillRect(0, 0, this.boardInfo.totalSize, this.boardInfo.totalSize);
    }

    drawBoard() {
        this.ctx.strokeStyle = '#000000';
        this.ctx.lineWidth = 1;

        // 绘制横线
        this.boardInfo.rows.forEach(row => {
            this.ctx.beginPath();
            this.ctx.moveTo(this.boardInfo.bias, row);
            this.ctx.lineTo(this.boardInfo.boardLength, row);
            this.ctx.stroke();
        });

        // 绘制纵线
        this.boardInfo.columns.forEach(col => {
            this.ctx.beginPath();
            this.ctx.moveTo(col, this.boardInfo.bias);
            this.ctx.lineTo(col, this.boardInfo.boardLength);
            this.ctx.stroke();
        });
    }

    drawCoordinates() {
        const fontSize = Math.max(12, this.boardInfo.R / 3);
        this.ctx.font = `${fontSize}px Arial`;
        this.ctx.fillStyle = '#000000';
        this.ctx.textAlign = 'center';
        this.ctx.textBaseline = 'middle';

        if (this.boardInfo.isCenter) {
            // 中心落子的坐标绘制
            for (let i = 0; i < this.boardInfo.rows.length - 1; i++) {
                const row = this.boardInfo.rows[i];
                const rowText = (this.boardInfo.rows.length - i - 2).toString();
                this.ctx.fillText(
                    rowText,
                    this.boardInfo.bias / 2,
                    row + this.boardInfo.R
                );
            }

            for (let i = 0; i < this.boardInfo.columns.length - 1; i++) {
                const col = this.boardInfo.columns[i];
                const colChar = String.fromCharCode('A'.charCodeAt(0) + i);
                this.ctx.fillText(
                    colChar,
                    col + this.boardInfo.R,
                    this.boardInfo.boardLength + this.boardInfo.bias / 2
                );
            }
        } else {
            // 交点落子的坐标绘制
            for (let i = 0; i < this.boardInfo.rows.length; i++) {
                const row = this.boardInfo.rows[i];
                const rowText = (this.boardInfo.rows.length - i - 1).toString();
                this.ctx.fillText(
                    rowText,
                    this.boardInfo.bias / 2,
                    row
                );
            }

            for (let i = 0; i < this.boardInfo.columns.length; i++) {
                const col = this.boardInfo.columns[i];
                const colChar = String.fromCharCode('A'.charCodeAt(0) + i);
                this.ctx.fillText(
                    colChar,
                    col,
                    this.boardInfo.boardLength + this.boardInfo.bias / 2
                );
            }
        }
    }

    drawPieces() {
        if (this.imagesLoaded < 2) {
            // 如果图片还没加载完或加载失败，绘制简单的圆形棋子
            this.drawSimplePieces();
            return;
        }

        // 解析棋子数据并绘制
        this.parsePiecesData();
    }

    drawSimplePieces() {
        if (!this.moves) {
            return;
        }
        
        // 解析first_player_pieces和second_player_pieces
        const firstPlayerPieces = this.parsePlayerPieces(this.moves.first_player_pieces);
        const secondPlayerPieces = this.parsePlayerPieces(this.moves.second_player_pieces);
        


        // 绘制先手棋子（黑色圆形）
        firstPlayerPieces.forEach((pos, index) => {
            this.drawSimplePiece(pos.row, pos.col, 'black');
        });

        // 绘制后手棋子（白色圆形）
        secondPlayerPieces.forEach((pos, index) => {
            this.drawSimplePiece(pos.row, pos.col, 'white');
        });

        // 标记最后一步 - 黑棋和白棋都要有红点
        this.drawLastMoveMarkers(firstPlayerPieces, secondPlayerPieces);
    }

    drawSimplePiece(row, col, color) {
        const position = this.boardInfo.getScreenPosition(row, col);
        
        if (!position) {
            return;
        }

        this.ctx.fillStyle = color === 'white' ? '#ffffff' : '#000000';
        this.ctx.strokeStyle = color === 'white' ? '#000000' : '#ffffff';
        this.ctx.lineWidth = 2;
        
        this.ctx.beginPath();
        this.ctx.arc(
            position.x,
            position.y,
            this.boardInfo.showR,
            0,
            2 * Math.PI
        );
        this.ctx.fill();
        this.ctx.stroke();
    }

    parsePiecesData() {
        // 解析first_player_pieces和second_player_pieces
        const firstPlayerPieces = this.parsePlayerPieces(this.moves.first_player_pieces);
        const secondPlayerPieces = this.parsePlayerPieces(this.moves.second_player_pieces);

        // 绘制先手棋子（黑子）
        firstPlayerPieces.forEach(pos => {
            this.drawPiece(pos.row, pos.col, 'black');
        });

        // 绘制后手棋子（白子）
        secondPlayerPieces.forEach(pos => {
            this.drawPiece(pos.row, pos.col, 'white');
        });

        // 标记最后一步 - 黑棋和白棋都要有红点
        this.drawLastMoveMarkers(firstPlayerPieces, secondPlayerPieces);
    }

    parsePlayerPieces(piecesString) {
        if (!piecesString) return [];
        
        const pieces = [];
        
        // 检查是否是 (A,1) 格式
        if (piecesString.includes('(') && piecesString.includes(')')) {
            // 解析 (A,1),(B,2) 格式
            const positions = piecesString.split(',');
            let currentPos = '';
            
            positions.forEach(part => {
                currentPos += part;
                if (part.includes(')')) {
                    // 完整的坐标，如 "(A,1)"
                    const match = currentPos.match(/\(([A-Z]),(\d+)\)/);
                    if (match) {
                        const col = match[1].charCodeAt(0) - 'A'.charCodeAt(0);
                        const row = parseInt(match[2]);
                        pieces.push({ row, col });
                    }
                    currentPos = '';
                } else if (!part.includes('(')) {
                    currentPos += ',';
                }
            });
            return pieces;
        }
        
        try {
            // 尝试解析JSON数组格式
            const jsonData = JSON.parse(piecesString);
            if (Array.isArray(jsonData)) {
                jsonData.forEach(pos => {
                    if (pos.x !== undefined && pos.y !== undefined) {
                        // JSON格式：{x: 行索引, y: 列索引}
                        const row = pos.x;
                        const col = pos.y;
                        pieces.push({ row, col });

                    }
                });
                return pieces;
            }
        } catch (e) {
            // JSON解析失败，尝试其他格式
        }
        
        // 传统格式解析：A0,B1,C2
        const positions = piecesString.split(',');
        positions.forEach(pos => {
            const trimmed = pos.trim();
            if (trimmed) {
                const col = trimmed.charCodeAt(0) - 'A'.charCodeAt(0);
                const row = this.boardInfo.column - 1 - parseInt(trimmed.substring(1));
                pieces.push({ row, col });
            }
        });
        
        return pieces;
    }

    drawPiece(row, col, color) {
        const position = this.boardInfo.getScreenPosition(row, col);
        if (!position) return;

        const image = color === 'white' ? this.whitePiece : this.blackPiece;
        const size = this.boardInfo.showR * 2;
        
        this.ctx.drawImage(
            image,
            position.x - this.boardInfo.showR,
            position.y - this.boardInfo.showR,
            size,
            size
        );
    }

    drawLastMoveMarker(row, col) {
        const position = this.boardInfo.getScreenPosition(row, col);
        if (!position) return;

        // 绘制红色圆点标记最后一步
        this.ctx.fillStyle = '#ff0000';
        this.ctx.beginPath();
        this.ctx.arc(
            position.x,
            position.y,
            this.boardInfo.showR * 0.3,
            0,
            2 * Math.PI
        );
        this.ctx.fill();
    }

    drawLastMoveMarkers(firstPlayerPieces, secondPlayerPieces) {
        // 只标记整个游戏的最新一步
        if (firstPlayerPieces.length > 0 || secondPlayerPieces.length > 0) {
            let lastMove = null;
            
            // 比较两个玩家的棋子数量，确定最后一步是谁下的
            if (firstPlayerPieces.length > secondPlayerPieces.length) {
                // 先手（黑棋）最后下的
                lastMove = firstPlayerPieces[firstPlayerPieces.length - 1];
            } else if (secondPlayerPieces.length > firstPlayerPieces.length) {
                // 后手（白棋）最后下的
                lastMove = secondPlayerPieces[secondPlayerPieces.length - 1];
            } else if (firstPlayerPieces.length === secondPlayerPieces.length && firstPlayerPieces.length > 0) {
                // 棋子数量相等，说明后手（白棋）最后下的
                lastMove = secondPlayerPieces[secondPlayerPieces.length - 1];
            }
            
            if (lastMove) {
                this.drawLastMoveMarker(lastMove.row, lastMove.col);
            }
        }
    }

    // 设置新的对局数据
    setMoves(moves) {
        this.moves = moves;
        this.render();
    }
} 