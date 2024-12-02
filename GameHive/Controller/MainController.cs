/*************************************************************************************
 * 文 件 名:   MainController.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:04
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.MainForm;
using GameHive.Model;
using GameHive.Model.GameManager;
namespace GameHive.Controller {
    //这个函数用于执行控制器生命周期主流程
    internal partial class Controller {
        private Form1 mainForm;
        private BoardManager boardManager;
        private View.View view;
        public Controller(Form1 mainForm) {
            //绑定页面、视图层、模型层
            this.mainForm = mainForm;
            boardManager = BoardManager.Instance(this);
            view = View.View.Instance(this, mainForm);
            Init();
        }
        private void Init() {
            //注册除运行算法外所有用户点击事件
            RegisterEvent();
            //初始化界面选择状态
            SetupInitialState();
        }

        //设定初始状态 0号游戏 0号AI 先手
        private void SetupInitialState() {
            //初始化先后手（触发一个点击先手事件）
            FirstTurnCheckedChanged(null, null);

            //初始化选择的棋盘（触发一次点击第一个菜单事件）
            MenuStrip menuStrip = mainForm.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menuStrip != null && menuStrip.Items.Count > 0) {
                // 获取第一个菜单项并触发点击事件
                ToolStripMenuItem firstMenuItem = menuStrip.Items[0] as ToolStripMenuItem;
                firstMenuItem?.PerformClick();
            }
            //初始化选择的算法
            SetDefaultAI();
        }
        private void SwitchGame(GameType gameType) {
            //切换AI并获取 boardInfo信息
            GameBoardInfo boardInfo = ModelMessageSwitchGameType(gameType);
            //用GameInfo去AI注册器更新AI列表
            RegisterAIType(boardInfo);
            //重新设置默认AI
            SetDefaultAI();
            //使用GameInfo命令View绘制地图,先后手保持不变
            view.DrowBoard(boardInfo.Column, boardInfo.IsCenter, boardManager.first);
        }

        //玩家点击可用条目触发切换算法
        private void ChoiceAlgorithm(AIAlgorithmType aIAlgorithmType) {
            //通知棋盘管理切换算法
            ModelMessageSwitchAI(aIAlgorithmType);

        }

        //设置默认出战AI（GameInfo的第一个）触发点击事件
        private void SetDefaultAI() {
            // 获取默认的 AI 类型（第一个可用的 AI 类型）
            var defaultAI = boardManager.BoardInfo.AllAIType[0];
            // 获取默认 AI 的中文名称
            var defaultAIName = defaultAI.GetChineseName();
            // 查找 ComboBox 中对应的项并设置为选中
            int defaultIndex = mainForm.AIType.Items.IndexOf(defaultAIName);
            if (defaultIndex >= 0) {
                mainForm.AIType.SelectedIndex = defaultIndex; // 触发 SelectedIndexChanged 事件
            }
        }
    }
}
