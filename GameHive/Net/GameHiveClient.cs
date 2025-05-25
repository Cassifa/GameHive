using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace GameHive.Net {
    public class GameHiveClient {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "http://localhost:3000";

        public GameHiveClient() {
            _httpClient = new HttpClient();
            // 只设置接受JSON响应
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 客户端登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        public async Task<AjaxResult<LoginResponse>> LoginAsync(string username, string password) {
            try {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "GameHiveClient/1.0");

                var url = $"{_baseUrl}/client/login";
                var formData = new Dictionary<string, string> {
                    { "username", username },
                    { "password", password }
                };
                var content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.PostAsync(url, content);
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"服务器响应: {jsonString}");

                try {
                    // 解析服务器返回的JSON
                    var result = JsonSerializer.Deserialize<AjaxResult<LoginResponse>>(jsonString,
                        new JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        });

                    if (result.Code == 200 && result.Data != null) {
                        // 更新用户信息
                        UserInfo.Instance.UpdateUserInfo(
                            result.Data.UserId,
                            result.Data.UserName,
                            result.Data.NickName,
                            result.Data.Token
                        );
                        Console.WriteLine($"用户信息更新成功: {result.Data.UserName}");
                    }

                    return result;
                } catch (JsonException jsonEx) {
                    Console.WriteLine($"JSON解析错误: {jsonEx.Message}");
                    return new AjaxResult<LoginResponse> {
                        Code = 500,
                        Msg = $"解析服务器响应失败: {jsonEx.Message}",
                        Data = null
                    };
                }
            } catch (Exception ex) {
                Console.WriteLine($"请求错误: {ex.Message}");
                return new AjaxResult<LoginResponse> {
                    Code = 500,
                    Msg = $"网络请求失败: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// 上传对局结果
        /// </summary>
        /// <param name="gameData">对局数据</param>
        public async Task<AjaxResult<object>> UploadGameResultAsync(Dictionary<string, object> gameData) {
            try {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(gameData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/client/upload", content);
                var jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"上传对局结果响应: {jsonString}");

                return JsonSerializer.Deserialize<AjaxResult<object>>(jsonString,
                    new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    });
            } catch (Exception ex) {
                Debug.WriteLine($"上传对局结果失败: {ex.Message}");
                return new AjaxResult<object> {
                    Code = 500,
                    Msg = $"上传失败: {ex.Message}",
                    Data = null
                };
            }
        }
    }

    public class LoginResponse {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Token { get; set; }
    }

    public class GameResult {
        public string UserId { get; set; }
        public string AlgorithmName { get; set; }
        public string GameTypeName { get; set; }
        public bool PlayerFirst { get; set; }
        public int Winner { get; set; }  // 0-先手赢，1-后手赢，2-平局
        public List<GameMove> Moves { get; set; }
    }

    public class GameMove {
        public int Id { get; set; }
        public string Role { get; set; }  // "Player" or "AI"
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class AjaxResult<T> {
        public int Code { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
    }
}