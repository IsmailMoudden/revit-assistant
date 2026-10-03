using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

string BaseUrl = (Environment.GetEnvironmentVariable("BIM_BACKEND_URL") ?? "http://127.0.0.1:8000").TrimEnd('/');
const double MtoFt   = 3.28084;

var http = new HttpClient { Timeout = TimeSpan.FromSeconds(180) };
string token = Environment.GetEnvironmentVariable("BIM_BACKEND_API_KEY");
if (!string.IsNullOrWhiteSpace(token))
    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

var cases = new[]
{
    "Create a 5 meter wall from (0,0) to (5,0) at level 1",
    "Add a window at position (2, 0, 1) on the wall",
    "Add 2 doors spaced 2 meters apart at position (1, 0, 0)",
};

Console.WriteLine("=== BIM AI Test Harness ===\n");

foreach (string instruction in cases)
{
    Console.WriteLine($"► {instruction}");

    try
    {
        var content  = new StringContent(
            JsonConvert.SerializeObject(new { instruction }),
            Encoding.UTF8, "application/json");

        var resp = await http.PostAsync($"{BaseUrl}/api/v1/generate-action", content);
        var raw  = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            Console.WriteLine($"  ✗ HTTP {(int)resp.StatusCode}: {raw}\n");
            continue;
        }

        var envelope = JsonConvert.DeserializeObject<ActionResponse>(raw);

        if (envelope?.Status != "ok" || envelope.Actions == null || envelope.Actions.Count == 0)
        {
            Console.WriteLine($"  Response status: {envelope?.Status}. Raw:\n{raw}\n");
            continue;
        }

        foreach (var action in envelope.Actions)
        {
            Console.WriteLine($"  Action: {action.ActionType}");
            PrintAction(action);
        }
        Console.WriteLine();
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("  Timeout (180s). Check the backend and model provider.\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"  ✗ Connection refused — {ex.Message}\n");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ {ex.GetType().Name}: {ex.Message}\n");
    }
}

Console.WriteLine("=== Done ===");

static void PrintAction(ActionPayload a)
{
    switch (a.ActionType)
    {
        case "create_wall":
            Console.WriteLine($"  start    : ({a.Start?.X:F2}, {a.Start?.Y:F2}) m" +
                              $"  → ({ToFt(a.Start?.X):F2}, {ToFt(a.Start?.Y):F2}) ft");
            Console.WriteLine($"  end      : ({a.End?.X:F2}, {a.End?.Y:F2}) m" +
                              $"  → ({ToFt(a.End?.X):F2}, {ToFt(a.End?.Y):F2}) ft");
            Console.WriteLine($"  height   : {a.Height:F2} m → {ToFt(a.Height):F2} ft");
            Console.WriteLine($"  level    : {a.Level ?? "(fallback to lowest)"}");
            break;

        case "add_window":
        case "add_door":
            Console.WriteLine($"  position : ({a.Position?.X:F2}, {a.Position?.Y:F2}, {a.Position?.Z:F2}) m" +
                              $"  → ({ToFt(a.Position?.X):F2}, {ToFt(a.Position?.Y):F2}, {ToFt(a.Position?.Z):F2}) ft");
            Console.WriteLine($"  wall_id  : {a.WallId ?? "null (auto-select nearest)"}");
            Console.WriteLine($"  count    : {a.Count ?? 1}");
            Console.WriteLine($"  spacing  : {(a.Spacing.HasValue ? $"{a.Spacing:F2} m → {ToFt(a.Spacing):F2} ft" : "null")}");
            Console.WriteLine($"  width    : {a.Width:F2} m  height: {a.Height:F2} m");
            break;

        default:
            Console.WriteLine($"  (unrecognised action type)");
            break;
    }

    if (a.Extras is { Count: > 0 })
        Console.WriteLine($"  extras   : {string.Join(", ", a.Extras.Keys)}");
}

static double ToFt(double? m) => (m ?? 0) * MtoFt;

// ── models ────────────────────────────────────────────────────────────────────

class ActionResponse
{
    [JsonProperty("instruction")]    public string        Instruction  { get; set; }
    [JsonProperty("status")]         public string Status { get; set; }
    [JsonProperty("actions")]        public System.Collections.Generic.List<ActionPayload> Actions { get; set; }
    [JsonProperty("raw_llm_output")] public string        RawLlmOutput { get; set; }
}

class Position
{
    [JsonProperty("x")] public double X { get; set; }
    [JsonProperty("y")] public double Y { get; set; }
    [JsonProperty("z")] public double Z { get; set; }
}

class ActionPayload
{
    [JsonProperty("action")]    public string   ActionType { get; set; }

    // create_wall
    [JsonProperty("start")]     public Position Start     { get; set; }
    [JsonProperty("end")]       public Position End       { get; set; }
    [JsonProperty("height")]    public double?  Height    { get; set; }
    [JsonProperty("thickness")] public double?  Thickness { get; set; }
    [JsonProperty("level")]     public string   Level     { get; set; }

    // add_window / add_door
    [JsonProperty("wall_id")]   public string   WallId    { get; set; }
    [JsonProperty("position")]  public Position Position  { get; set; }
    [JsonProperty("width")]     public double?  Width     { get; set; }
    [JsonProperty("count")]     public int?     Count     { get; set; }
    [JsonProperty("spacing")]   public double?  Spacing   { get; set; }

    [JsonExtensionData]
    public System.Collections.Generic.Dictionary<string, JToken> Extras { get; set; }
}
