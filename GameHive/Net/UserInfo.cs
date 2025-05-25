using System.Diagnostics;

namespace GameHive.Net {
    public class UserInfo {
        private static UserInfo _instance;
        private static readonly object _lock = new object();

        public long UserId { get; private set; }
        public string UserName { get; private set; }
        public string NickName { get; private set; }
        public bool IsLoggedIn { get; private set; }

        // 添加登录状态改变事件
        public event EventHandler LoginStatusChanged;

        private UserInfo() {
            Clear();
            Debug.WriteLine("[UserInfo] 已创建单例实例");
        }

        public static UserInfo Instance {
            get {
                if (_instance == null) {
                    lock (_lock) {
                        if (_instance == null) {
                            _instance = new UserInfo();
                        }
                    }
                }
                return _instance;
            }
        }

        public void UpdateUserInfo(long userId, string userName, string nickName) {
            Debug.WriteLine($"[UserInfo.UpdateUserInfo] 更新前: UserId={UserId}, UserName={UserName}, NickName={NickName}, IsLoggedIn={IsLoggedIn}");

            UserId = userId;
            UserName = userName;
            NickName = nickName;
            IsLoggedIn = true;

            // 触发登录状态改变事件
            LoginStatusChanged?.Invoke(this, EventArgs.Empty);

            Debug.WriteLine($"[UserInfo.UpdateUserInfo] 更新后: UserId={UserId}, UserName={UserName}, NickName={NickName}, IsLoggedIn={IsLoggedIn}");
        }

        public void Clear() {
            Debug.WriteLine("[UserInfo.Clear] 清除用户信息");
            UserId = 0;
            UserName = null;
            NickName = null;
            IsLoggedIn = false;

            // 触发登录状态改变事件
            LoginStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}