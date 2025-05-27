using GameHive.MainForm;
using GameHive.Net;

namespace GameHive {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // 自动登录
            await AutoLogin();
            
            Application.Run(new DoubleBufferedForm());
        }
        
        /// <summary>
        /// 自动登录功能
        /// </summary>
        private static async Task AutoLogin() {
            try {
                Console.WriteLine("[AutoLogin] 开始自动登录...");
                var client = new GameHiveClient();
                var result = await client.LoginAsync("bb", "1");
                
                if (result.Code == 200) {
                    Console.WriteLine("[AutoLogin] 自动登录成功");
                } else {
                    Console.WriteLine($"[AutoLogin] 自动登录失败: {result.Msg}");
                }
            } catch (Exception ex) {
                Console.WriteLine($"[AutoLogin] 自动登录异常: {ex.Message}");
            }
        }
    }
}
