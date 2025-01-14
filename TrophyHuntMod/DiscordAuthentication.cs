﻿/*

using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Threading;

using UnityEngine;
using UnityEngine.Networking;
using Microsoft.Win32.SafeHandles;
using System.Threading.Tasks;

namespace TrophyHuntMod
{
    public class DiscordAuthentication : MonoBehaviour
    {
//        private const string TokenEndpoint = "https://discord.com/api/oauth2/token";
//        private HttpListener listener;
//        private Thread listenerThread;

        enum AuthState
        {
            NotStarted,
            RetrievingCode,
            RetrievingToken,
            RetrievingUser,
            Done
        }

        AuthState m_authState = AuthState.NotStarted;

        public string clientId = "1328474573334642728";
        public string clientSecret = "_xNynKcfZMu7sjkzvMWe9mfmN4xFDYsV";
        public string redirectUri = "http://localhost:5000/callback";
        public string accessToken = "";
        public string authCode = "";

        /*
                public void Authenticate()
                {
                    m_authState = AuthState.NotStarted;

                    RequestUserAuth();

                    StartAuthThread();
                }
                public void RequestUserAuth()
                {
                    string scope = "identify";

                    string authUrl = $"https://discord.com/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";

                    // Open the browser
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = authUrl,
                        UseShellExecute = true
                    });
                }

                public void StartAuthThread()
                {
                    Debug.LogWarning($"StartAuthThread()");

                    m_authState = AuthState.RetrievingCode;

                    listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:5000/");
                    listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                    listener.Start();

                    Debug.LogWarning($"StartAuthThread() Start Thread");

                    listenerThread = new Thread(ListenerThread);
                    listenerThread.Start();
                }

                private void ListenerThread()
                {
        //            Debug.LogWarning($"ListenerThread: {m_authState.ToString()}");

                    while (m_authState != AuthState.Done)
                    {
                        switch (m_authState)
                        {
                            case AuthState.NotStarted:
                                break;
                            case AuthState.RetrievingCode:
                                IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
                                result.AsyncWaitHandle.WaitOne();
                                break;
                            case AuthState.RetrievingToken:
                                RequestToken();
                                break;
                            case AuthState.RetrievingUser:
                                RequestUserData(accessToken);
                                break;
                        }
                    }
                }

                private void ListenerCallback(IAsyncResult result)
                {
                    Debug.LogWarning($"ListenerCallback()");
                    var context = listener.EndGetContext(result);

                    authCode = context.Request.QueryString["code"];

                    Debug.Log($"Code: '{authCode}'");

                    m_authState = AuthState.RetrievingToken;
                }

                public void RequestToken()
                {
                    Debug.LogWarning($"RequestToken()");
                    // Form parameters
                    WWWForm form = new WWWForm();

                    form.AddField("client_id", clientId);
                    form.AddField("client_secret", clientSecret);
                    form.AddField("grant_type", "authorization_code");
                    form.AddField("code", authCode);
                    form.AddField("redirect_uri", redirectUri);

                    // Create the UnityWebRequest
                    using (UnityWebRequest webRequest = UnityWebRequest.Post(TokenEndpoint, form))
                    {
                        // Send the request and wait for the response
                        webRequest.SendWebRequest();

                        while (!webRequest.isDone)
                        {
                            // Wait for request
                            // TODO: timeout?
                        }

                        //System.Threading.Thread.Sleep(3000); // Example delay for testing

                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            string jsonResponse = webRequest.downloadHandler.text;

                            UnityEngine.Debug.Log("Token Response: " + jsonResponse);

                            var response = JsonUtility.FromJson<DiscordTokenResponse>(jsonResponse);
                            UnityEngine.Debug.Log("Access Token: " + response.access_token);

                            accessToken = response.access_token;

                            m_authState = AuthState.RetrievingUser;

                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Error requesting token: " + webRequest.error);
                            UnityEngine.Debug.LogError("Response: " + webRequest.downloadHandler.text);
                        }
                    }
                }

                public void RequestUserData(string accessToken)
                {
                    Debug.LogWarning($"RequestUserData()");

                    HttpClient client = new HttpClient();

                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                    var response = client.GetAsync("https://discord.com/api/users/@me");
                    while (!response.IsCompleted)
                    {

                    }

                    var responseString = response.Result.ToString();

                    System.Text.Json.JsonDocument json = System.Text.Json.JsonDocument.Parse(responseString);
                    string username = json.RootElement.GetProperty("username").GetString();
                    string discriminator = json.RootElement.GetProperty("discriminator").GetString();

                    UnityEngine.Debug.LogError($"RequestUserData() username: {username} discriminator: {discriminator} ");
                }


                [System.Serializable]
                public class DiscordUserResponse
                {
                    public string id;
                    public string username;
                    public string discriminator;
                    public string avatar;
                }


        * /


        public void Start()
        {
            Debug.LogWarning($"Start()");
            // Start the local server
            DiscordAuthServer authServer = new DiscordAuthServer();
            authServer.StartServer(clientId, clientSecret, redirectUri);

            // Open the browser for authentication
            OpenDiscordAuthorization(clientId, redirectUri);
        }

        public void OpenDiscordAuthorization(string clientId, string redirectUri)
        {
            Debug.LogWarning($"OpenDiscordAuthorization()");
            string scope = "identify";
            string authUrl = $"https://discord.com/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";

            // Open the user's default web browser
            Application.OpenURL(authUrl);
        }

        public class DiscordAuthServer : MonoBehaviour
        {
            private HttpListener httpListener;

            string m_clientId;
            string m_clientSecret;
            string m_redirectUri;

            public void StartServer(string clientId, string clientSecret, string redirectUri)
            {
                Debug.LogWarning($"StartServer()");
                m_clientId = clientId;
                m_clientSecret = clientSecret;
                m_redirectUri = redirectUri;

                httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://localhost:5000/");
                httpListener.Start();

                Task.Run(() => ListenForCode());
            }

            private async Task ListenForCode()
            {
                Debug.LogWarning($"ListenForCode()");
                while (httpListener.IsListening)
                {
                    var context = await httpListener.GetContextAsync();
                    string code = context.Request.QueryString["code"];

                    // Send a response back to the browser
                    string responseString = "<html><body>Authentication successful! You can close this window.</body></html>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();

                    Debug.Log($"Authorization Code Received: {code}");

                    // Stop the listener once we have the code
                    StopServer();

                    // Continue with token exchange
                    StartCoroutine(ExchangeCodeForToken(code));
                }
            }

            public void StopServer()
            {
                Debug.LogWarning($"StopServer()");
                httpListener.Stop();
                httpListener.Close();
            }

            private IEnumerator ExchangeCodeForToken(string code)
            {
                Debug.LogWarning($"ExchangeCodeForToken()");
                string tokenEndpoint = "https://discord.com/api/oauth2/token";

                WWWForm form = new WWWForm();
                form.AddField("client_id", m_clientId);
                form.AddField("client_secret", m_clientSecret);
                form.AddField("grant_type", "authorization_code");
                form.AddField("code", code);
                form.AddField("redirect_uri", m_redirectUri);

                using (UnityWebRequest webRequest = UnityWebRequest.Post(tokenEndpoint, form))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Token Response: " + webRequest.downloadHandler.text);

                        var tokenResponse = JsonUtility.FromJson<DiscordTokenResponse>(webRequest.downloadHandler.text);
                        StartCoroutine(FetchDiscordUser(tokenResponse.access_token));
                    }
                    else
                    {
                        Debug.LogError("Error exchanging code for token: " + webRequest.error);
                    }
                }
            }
            private IEnumerator FetchDiscordUser(string accessToken)
            {
                Debug.LogWarning($"ExchangeCodeForToken()");
                string userEndpoint = "https://discord.com/api/users/@me";

                using (UnityWebRequest webRequest = UnityWebRequest.Get(userEndpoint))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");

                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("User Info Response: " + webRequest.downloadHandler.text);

                        var userInfo = JsonUtility.FromJson<DiscordUserResponse>(webRequest.downloadHandler.text);
                        Debug.Log($"User: {userInfo.username}#{userInfo.discriminator}");

                    }
                    else
                    {
                        Debug.LogError("Error fetching user info: " + webRequest.error);
                    }
                }
            }
        }

        [System.Serializable]
        public class DiscordTokenResponse
        {
            public string access_token;
            public string token_type;
            public int expires_in;
            public string refresh_token;
            public string scope;
        }

        [System.Serializable]
        public class DiscordUserResponse
        {
            public string id;
            public string username;
            public string discriminator;
            public string avatar;
        }


    }

 }
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

public class DiscordOAuthFlow
{
    private const string TokenEndpoint = "https://discord.com/api/oauth2/token";
    private const string UserEndpoint = "https://discord.com/api/users/@me";
    private HttpListener httpListener;

    string m_clientId = string.Empty;
    string m_clientSecret = string.Empty;
    string m_redirectUri = string.Empty;
    string m_code = string.Empty;
    DiscordUserResponse m_userInfo;

    public DiscordUserResponse GetUserResponse() { return m_userInfo; }

    public void StartOAuthFlow(string clientId, string clientSecret, string redirectUri)
    {
        m_clientId = clientId;
        m_clientSecret = clientSecret;  
        m_redirectUri = redirectUri;

        Debug.WriteLine("Starting OAuth flow...");
        StartServer(redirectUri);
        OpenDiscordAuthorization(clientId, redirectUri);
    }

    private void OpenDiscordAuthorization(string clientId, string redirectUri)
    {
        Debug.WriteLine("Opening Discord authorization URL...");
        string scope = "identify";
        string authUrl = $"https://discord.com/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}";

        // Opens the authorization URL in the default web browser
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });
    }

    private void StartServer(string redirectUri)
    {
        Debug.WriteLine("Starting local HTTP server to listen for authorization code...");
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:5000/");
        httpListener.Start();

        Task.Run(() => ListenForCode());
    }

    private async Task ListenForCode()
    {
        Debug.WriteLine("Listening for authorization code...");
        while (httpListener.IsListening)
        {
            var context = await httpListener.GetContextAsync();
            m_code = context.Request.QueryString["code"];

            // Send a response back to the browser
            string responseString = //"<html><body>Authentication successful! You can close this window.</body></html>";
            "<html>\r\n\r\n<body style=\"background-color:#202020;\\\">\r\n    <center>\r\n        <figure class=\"image image-style-align-left\\\"><img style=\"aspect-ratio:256/256;\" src=\"https://gcdn.thunderstore.io/live/repository/icons/oathorse-TrophyHuntMod-0.8.8.png.256x256_q95_crop.jpg\" width=\"256\\\" height=\"256\\\"></figure>\r\n        <p>&nbsp;</p>\r\n        <p><span style=\"color:#f0e080;font-size:22px;\"><strong>Congratulations! You've connected Discord to the TrophyHuntMod and have enabled online reporting!</p></strong></span>\r\n        \r\n        <p><span style=\"color:#e0e0e0;font-size:20px;\"><strong>Data reported by the mod can now be used in official Trophy Hunt Tournaments.</strong></span></p>\r\n        <p>&nbsp;</p>\r\n        <p><span style=\"color:#e0e0e0;font-size:22px;\"><strong>Only your Discord id and username are used, and not for anything but Trophy Hunt event leaderboards.</strong></span></p>\r\n        <p><span style=\"color:#e04040;font-size:20px;\"><strong>They will not be shared with anyone else.</strong></span></p>\r\n        <p>&nbsp;</p>\r\n        <p>&nbsp;</p>\r\n        <p><span style=\"color:#e0e0e0;font-size:24px;\\\"><strong>You can now close this window.</strong></span></p>\r\n    </center>\r\n</body>\r\n\r\n</html>";
            
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
 
            Debug.WriteLine($"Authorization Code Received: {m_code}");

            // Stop the server
            StopServer();

            Debug.WriteLine($"Exchanging code for token.");

            try
            {


                // Exchange the code for a token
                await ExchangeCodeForToken(m_code);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    private void StopServer()
    {
        Debug.WriteLine("Stopping local HTTP server...");
        httpListener.Stop();
        httpListener.Close();
    }

    private async Task ExchangeCodeForToken(string code)
    {
        Debug.WriteLine("Exchanging authorization code for access token...");
        using (HttpClient httpClient = new HttpClient())
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", m_clientId),
                new KeyValuePair<string, string>("client_secret", m_clientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", m_redirectUri)
            });

            Debug.WriteLine("PostAsync()");

            var response = await httpClient.PostAsync(TokenEndpoint, formData);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Access token successfully received.");
                Debug.WriteLine($"Token Response: {responseBody}");

                var tokenResponse = JsonConvert.DeserializeObject<DiscordTokenResponse>(responseBody);
                await FetchDiscordUser(tokenResponse.access_token);
            }
            else
            {
                Debug.WriteLine($"Error exchanging code for token: {response.StatusCode}");
                string errorResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response: {errorResponse}");
            }
        }
    }

    private async Task FetchDiscordUser(string accessToken)
    {
        Debug.WriteLine("Fetching Discord user information...");
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync(UserEndpoint);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("User information successfully received.");
                Debug.WriteLine($"User Info Response: {responseBody}");

                m_userInfo = JsonConvert.DeserializeObject<DiscordUserResponse>(responseBody);
                Debug.WriteLine($"User: {m_userInfo.username}#{m_userInfo.discriminator}");
            }
            else
            {
                Debug.WriteLine($"Error fetching user info: {response.StatusCode}");
                string errorResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response: {errorResponse}");
            }
        }
    }
}

// Helper Classes
public class DiscordTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string refresh_token { get; set; }
    public string scope { get; set; }
}

public class DiscordUserResponse
{
    public string id { get; set; }
    public string username { get; set; }
    public string discriminator { get; set; }
    public string avatar { get; set; }
    public string global_name { get; set; }
}