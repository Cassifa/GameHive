#pragma once
#include <thread>
#include <memory>
#include <mutex>
#include <ctime>
#include <queue>
#include <vector>
#include "safe_queue.h"
// #include "GameAI.h"

const double Confidence = 1.414;
const int LeastVisitTime = 5;
const int RolloutStep = 20;

struct MCTS_Node{
	double value;
	//访问次数
	int n;
	std::vector<Point> que;
	MCTS_Node* son[9][9];

	MCTS_Node(){
		value = 0;
		n = 0;
		memset(son, 0, sizeof son);
	}

	~MCTS_Node(){
		for (int i = 0; i < 9; i++)
			for (int j = 0; j < 9; j++)
				if (son[i][j] != nullptr)
					delete son[i][j];
	}

	double UCB(int N)const{
		if (n == 0)
			return 1e100;
		return value / n + Confidence * sqrt(log(N) / n);
	}

	Point FindMaxUCB(){
		Point ret(-1, -1);
		double mx = -2e100;
		for (auto u : que)
			if (son[u.x][u.y]->UCB(n) > mx)
				mx = son[u.x][u.y]->UCB(n), ret = u;
		return ret;
	}

	Point FindMaxRating(){
		Point ret(-1, -1);
		double mx = -2e100;
		for (auto u : que)
			if (son[u.x][u.y]->value / son[u.x][u.y]->n > mx)
				mx = son[u.x][u.y]->value / son[u.x][u.y]->n, ret = u;
		return ret;
	}
};
class GameAI_MCTS :public GameAI{
	std::thread* pMain;
	//锁
	std::mutex mv_lock;

	//消息队列
	threadsafe_queue <Message> qmsg;
	Point player_move, ai_move;
	//开始时间
	clock_t start_time;
	//需要移动 正要移动
	std::atomic_bool need_move, ready_move;

	int beginning_value;
	GameRule* board, * tboard;
	MCTS_Node* root;

	//
	void Move(Point mv);
	//反向传播
	double Rollout();
	//搜索
	double Search(MCTS_Node* cur);
	//搜索线程
	void Run();

public:
	GameAI_MCTS(Color col);
	~GameAI_MCTS();

	//设置初始状态
	void SetBeginningState();
	void SetBeginningState(const Color A[9][9]);
	//开启线程
	void Start();
	//结束线程
	void End();

	//弹入消息队列
	void SendGameMessage(Message = Message::CALC);
	//获取Ai的决定
	bool GetMove(Point& res);
	//收到玩家移动消息
	void PlayerMove(Point p);
};