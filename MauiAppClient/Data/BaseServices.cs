using MauiAppClient.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiAppClient.Data
{
    public class BaseServices
    {
        public BaseServices()
        {
            GetUserSession();
        }

        public string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "https://l65nsmvx-7006.uks1.devtunnels.ms" : "https://localhost:7006";
        public UserSession UserSession { get; set; } = new();
        public async void GetUserSession()
        {
            string getUserSessionFromStorage = await SecureStorage.Default.GetAsync("UserSession");
            if (!string.IsNullOrEmpty(getUserSessionFromStorage))
                UserSession = JsonSerializer.Deserialize<UserSession>(getUserSessionFromStorage);
        }

        public HttpClient AddHeaderToHttp()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserSession.Token);
            return httpClient;
        }
    }
}
