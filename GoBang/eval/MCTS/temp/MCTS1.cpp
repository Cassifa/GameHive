// #include "MCTS1.hpp"

// GameAI_MCTS::GameAI_MCTS(Color col) : GameAI(col){//
// 	start_time = 0;
// 	pMain = nullptr;
// 	need_move = false;
// 	ready_move = false;

// 	root = nullptr;
// 	board = nullptr;
// 	tboard = nullptr;
// }

// GameAI_MCTS::~GameAI_MCTS()
// {
// 	this->End();
// 	if (pMain != nullptr)
// 		delete pMain;
// }

// void GameAI_MCTS::Move(Point mv){//在mv落子
// 	board->setPiece(mv.x, mv.y, board->moveColor());
// 	MCTS_Node* tmp = root->son[mv.x][mv.y];
// 	if (tmp == nullptr)
// 		tmp = new MCTS_Node();
// 	root->son[mv.x][mv.y] = nullptr;
// 	delete root;
// 	root = tmp;
// }
// //
// double GameAI_MCTS::Rollout(){
// 	GameRule tmp = *tboard;
// 	std::vector<Point> op[2];
// 	unsigned int cnt[2], c;

// 	for (int i = 0; i < 9; i++)
// 		for (int j = 0; j < 9; j++){
// 			if (tmp.isLegal(i, j, Color::BLACK))
// 				op[0].push_back(Point(i, j));
// 			if (tmp.isLegal(i, j, Color::WHITE))
// 				op[1].push_back(Point(i, j));
// 		}
// 	cnt[0] = (unsigned int)op[0].size();
// 	cnt[1] = (unsigned int)op[1].size();

// 	if (cnt[0] && cnt[1]){
// 		for (int i = 0; i < RolloutStep; i++){
// 			c = tmp.moveColor() == Color::WHITE;
// 			int id = rand() % op[c].size();
// 			while (cnt[c] && (op[c][id].x == -1 || !tmp.isLegal(op[c][id].x, op[c][id].y, tmp.moveColor()))){
// 				cnt[c]--;
// 				op[c][id].x = -1;
// 				id = rand() % op[c].size();
// 			}
// 			if (cnt[c] == 0)
// 				break;
// 			tmp.setPiece(op[c][id].x, op[c][id].y, tmp.moveColor());
// 			op[c][id].x = -1;
// 			cnt[c]--;
// 		}
// 	}
// 	else{
// 		c = tmp.moveColor() == Color::WHITE;
// 		if (cnt[c] == 0)
// 			return ai_color == tmp.moveColor() ? -1e100 : 1e100;
// 		if (cnt[c ^ 1] == 0)
// 			return ai_color == tmp.moveColor() ? 1e100 : -1e100;
// 	}
// 	int ret = tmp.Evaluate(ai_color) - beginning_value;
// 	return ret;
// }
// //选择
// double GameAI_MCTS::Search(MCTS_Node* cur){
// 	//叶节点
// 	if (cur->que.empty()){
// 		//小于最小模拟次数则模拟并反向传播
// 		if (cur->n < LeastVisitTime){
// 			double up = Rollout();
// 			if (tboard->moveColor() == ai_color)
// 				up = -up;
// 			cur->n++;
// 			if (up > 0)
// 				cur->value += up;
			
// 			return up;
// 		}
// 		//否则拓展
// 		else{
// 			//将合法操作加入
// 			for (int i = 0; i < 9; i++)
// 				for (int j = 0; j < 9; j++)
// 					if (tboard->isLegal(i, j, tboard->moveColor())){
// 						cur->son[i][j] = new MCTS_Node();
// 						cur->que.push_back(Point(i, j));
// 					}
// 			//没有合法操作,游戏已经结束
// 			if (cur->que.empty())
// 				return tboard->moveColor() == ai_color ? -1e100 : 1e100;
// 		}
// 	}
// 	Point t = cur->FindMaxUCB();
// 	tboard->setPiece(t.x, t.y, tboard->moveColor());
// 	double up = -Search(cur->son[t.x][t.y]);
// 	cur->n++;
	
// 	if (up > 0)
// 		cur->value += up;
		
// 	//cur->value += up;
// 	return up;
// }

// void GameAI_MCTS::Run(){
// 	bool bQuit = false;
// 	while (!bQuit){
// 		//处理消息队列
// 		for (;;){
// 			Message message;
// 			if (!this->qmsg.try_pop(message))
// 				break;

// 			if (message == Message::END){
// 				bQuit = true;
// 				break;
// 			}
// 			else if (message == Message::MOVE){
// 				Move(player_move);
// 				delete root;
// 				root = new MCTS_Node();
// 			}
// 			else if (message == Message::CALC){
// 				start_time = clock();
// 				std::lock_guard<std::mutex> lg(this->mv_lock);
// 				need_move = true;
// 				ready_move = false;
// 			}
// 		}
// 		if (bQuit)
// 			break;
// 		if (need_move){
// 			int search_times = 0;
// 			beginning_value = tboard->Evaluate(ai_color);
// 			while /*(search_times<=1000)*/(clock() - start_time <= 950){
// 				memcpy(tboard, board, sizeof(GameRule));
// 				Search(root);
// 				search_times++;
// 			}
// 			ai_move = root->FindMaxRating();
// 			Move(ai_move);

// 			std::lock_guard <std::mutex> lg(this->mv_lock);
// 			need_move = false;
// 			ready_move = true;
// 		}
// 		else
// 			std::this_thread::yield();
// 	}
// }

// void GameAI_MCTS::SetBeginningState(){
// 	if (root != nullptr)
// 		delete root;
// 	root = new MCTS_Node();
// 	if (board != nullptr)
// 		delete board;
// 	board = new GameRule();
// 	if (tboard != nullptr)
// 		delete tboard;
// 	tboard = new GameRule();
// }
// void GameAI_MCTS::SetBeginningState(const Color A[9][9]){
// 	SetBeginningState();
// 	for (int i = 0; i < 9; i++)
// 		for (int j = 0; j < 9; j++)
// 			board->A[i][j] = A[i][j];
// 	board->Restucture();
// }

// void GameAI_MCTS::Start(){
// 	srand((unsigned int)time(0));
// 	pMain = new std::thread(&GameAI_MCTS::Run, this);
// }

// void GameAI_MCTS::End(){
// 	this->SendGameMessage(Message::END);
// 	pMain->join();
// }

// void GameAI_MCTS::SendGameMessage(Message msg){
// 	qmsg.push(msg);
// }

// //获取决定
// bool GameAI_MCTS::GetMove(Point& res){
// 	if (!ready_move)
// 		return false;
// 	std::lock_guard<std::mutex> lg(this->mv_lock);
// 	res = ai_move;
// 	ready_move = false;
// 	return true;
// }

// //玩家移动
// void GameAI_MCTS::PlayerMove(Point p){
// 	this->SendGameMessage(Message::MOVE);
// 	player_move = p;
// }