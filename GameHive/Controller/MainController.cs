/*************************************************************************************
 * 文 件 名:   MainController.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:04
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.MainForm;
using GameHive.Model.GameManager;
namespace GameHive.Controller {
    //这个函数用于执行控制器生命周期主流程
    internal partial class Controller {
        private Form1 mainForm;
        private BoardManager boardManager;
        public Controller(Form1 mainForm) {
            //绑定页面、视图层、模型层
            this.mainForm = mainForm;
            boardManager = BoardManager.Instance(this);
            //TODO绑定视图层
            Init();
        }
        private void Init() {
            //注册除运行算法外所有用户点击事件
            RegisterEvent();
            //初始化界面状态
            SetupInitialState();
        }
        private void SetupInitialState() {
            //初始化选择的棋盘（触发一次点击第一个事件）
            //初始化选择的算法（触发一次点击第一个AI事件）
            //初始化先后手（触发一个点击先手事件）
        }

        //选择
        private void ChoiceAlgorithm(AIAlgorithmType aIAlgorithmType) {
            //通知棋盘管理切换算法

        }
        //设置默认出战AI（GameInfo的第一个）
        private void SetDefaultAI() {

        }
    }
}
