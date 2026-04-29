using System.Text.Json;
using System.Text;
 
// Load config
const string CONFIG_FILE = "config.json";
 
if (!File.Exists(CONFIG_FILE))
{
    Console.WriteLine("config.json not found! Creating default config...");
    var defaultConfig = new AppConfig();
    File.WriteAllText(CONFIG_FILE, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine("Edit config.json with your settings and restart.");
    return;
}
 
var config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(CONFIG_FILE)) ?? new AppConfig();
 
Console.WriteLine($"[{DateTime.Now}] FreeGamesMonitor started.");
Console.WriteLine($"ASF URL: {config.AsfUrl} | Bot: {config.BotName} | Interval: {config.CheckIntervalHours}h");
 
while (true)
{
    await CheckAndRedeem(config);
    await Task.Delay(TimeSpan.FromHours(config.CheckIntervalHours));
}
 
static async Task CheckAndRedeem(AppConfig config)
{
    Console.WriteLine($"[{DateTime.Now}] Checking for free games...");
 
    var seen = LoadSeen(config.SeenFile);
    var games = await GetFreeGames();
    var newGames = games.Where(g => !seen.Contains(g.Key)).ToList();
 
    if (!newGames.Any())
    {
        Console.WriteLine("No new free games found.");
        return;
    }
 
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authentication", config.AsfPassword);
 
    foreach (var kv in newGames)
    {
        var appId = kv.Key;
        var name = kv.Value;
 
        Console.WriteLine($"Redeeming: {name} ({appId})");
 
        var payload = JsonSerializer.Serialize(new
        {
            Command = $"addlicense {config.BotName} app/{appId}"
        });
 
        var response = await http.PostAsync(
            $"{config.AsfUrl}/Api/Command",
            new StringContent(payload, Encoding.UTF8, "application/json")
        );
 
        Console.WriteLine($"Result: {response.StatusCode}");
        seen.Add(appId);
    }
 
    SaveSeen(config.SeenFile, seen);
}
 
static async Task<Dictionary<string, string>> GetFreeGames()
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
 
    var url = "https://store.steampowered.com/search/results?specials=1&maxprice=free&json=1&count=50";
 
    try
    {
        var json = await http.GetStringAsync(url);
        var doc = JsonDocument.Parse(json);
        var games = new Dictionary<string, string>();
 
        if (!doc.RootElement.TryGetProperty("items", out var items))
        {
            Console.WriteLine("Steam response missing 'items' (likely blocked or changed API)");
            return games;
        }
 
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idProp)) continue;
            if (!item.TryGetProperty("name", out var nameProp)) continue;
 
            games[idProp.GetInt32().ToString()] = nameProp.GetString() ?? "Unknown";
        }
 
        return games;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching games: {ex.Message}");
        return new Dictionary<string, string>();
    }
}
 
static HashSet<string> LoadSeen(string path)
{
    if (!File.Exists(path)) return new HashSet<string>();
    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
}
 
static void SaveSeen(string path, HashSet<string> seen)
{
    File.WriteAllText(path, JsonSerializer.Serialize(seen));
}
 
class AppConfig
{
    public string AsfUrl { get; set; } = "http://localhost:1242";
    public string AsfPassword { get; set; } = "YOUR_IPC_PASSWORD";
    public string BotName { get; set; } = "YOUR_BOT_NAME";
    public string SeenFile { get; set; } = "seen_games.json";
    public int CheckIntervalHours { get; set; } = 24;
}