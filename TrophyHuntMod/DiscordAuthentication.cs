using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

using TrophyHuntMod;
using System.Runtime.CompilerServices;

public class DiscordOAuthFlow
{
    private const string TokenEndpoint = "https://discord.com/api/oauth2/token";
    private const string UserEndpoint = "https://discord.com/api/users/@me";
    private HttpListener httpListener;

    string m_clientId = string.Empty;
    string m_clientSecret = string.Empty;
    string m_redirectUri = string.Empty;
    string m_code = string.Empty;
    DiscordUserResponse m_userInfo = null;

    bool VERBOSE = false;

    public DiscordUserResponse GetUserResponse() { return m_userInfo; }
    public void ClearUserResponse() { m_userInfo = null; }

    public delegate void StatusCallback();

    StatusCallback m_statusCallback = null;

    public void StartOAuthFlow(string clientId, string clientSecret, string redirectUri, StatusCallback callback)
    {
        m_clientId = clientId;
        m_clientSecret = clientSecret;  
        m_redirectUri = redirectUri;
        m_statusCallback= callback;

        if (VERBOSE) Debug.WriteLine("Starting OAuth flow...");
        StartServer(redirectUri);
        OpenDiscordAuthorization(clientId, redirectUri);
    }

    private void OpenDiscordAuthorization(string clientId, string redirectUri)
    {
        if (VERBOSE) Debug.WriteLine("Opening Discord authorization URL...");
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
        if (VERBOSE) Debug.WriteLine("Starting local HTTP server to listen for authorization code...");
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:5000/");
        httpListener.Start();

        Task.Run(() => ListenForCode());
    }

    private async Task ListenForCode()
    {
        if (VERBOSE) Debug.WriteLine("Listening for authorization code...");
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

            if (VERBOSE) Debug.WriteLine($"Authorization Code Received: {m_code}");

            // Stop the server
            StopServer();

            if (VERBOSE) Debug.WriteLine($"Exchanging code for token.");

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
        if (VERBOSE) Debug.WriteLine("Stopping local HTTP server...");
        httpListener.Stop();
        httpListener.Close();
    }

    private async Task ExchangeCodeForToken(string code)
    {
        if (VERBOSE) Debug.WriteLine("Exchanging authorization code for access token...");
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

            if (VERBOSE) Debug.WriteLine("PostAsync()");

            var response = await httpClient.PostAsync(TokenEndpoint, formData);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if (VERBOSE) Debug.WriteLine("Access token successfully received.");
                if (VERBOSE) Debug.WriteLine($"Token Response: {responseBody}");

                var tokenResponse = JsonConvert.DeserializeObject<DiscordTokenResponse>(responseBody);
                await FetchDiscordUser(tokenResponse.access_token);
            }
            else
            {
                if (VERBOSE) Debug.WriteLine($"Error exchanging code for token: {response.StatusCode}");
                string errorResponse = await response.Content.ReadAsStringAsync();
                if (VERBOSE) Debug.WriteLine($"Response: {errorResponse}");
            }
        }
    }

    private async Task FetchDiscordUser(string accessToken)
    {
        if (VERBOSE) Debug.WriteLine("Fetching Discord user information...");
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync(UserEndpoint);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if (VERBOSE) Debug.WriteLine("User information successfully received.");
                if (VERBOSE) Debug.WriteLine($"User Info Response: {responseBody}");

                m_userInfo = JsonConvert.DeserializeObject<DiscordUserResponse>(responseBody);
                if (VERBOSE) Debug.WriteLine($"User: {m_userInfo.username}#{m_userInfo.discriminator}");
                if (VERBOSE) Debug.WriteLine($"{m_userInfo.ToString()}");
            }
            else
            {
                if (VERBOSE) Debug.WriteLine($"Error fetching user info: {response.StatusCode}");
                string errorResponse = await response.Content.ReadAsStringAsync();
                if (VERBOSE) Debug.WriteLine($"Response: {errorResponse}");
            }
        }

        m_statusCallback();
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