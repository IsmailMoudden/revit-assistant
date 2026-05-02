using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Models
{
    public class ActionResponse
    {
        [JsonProperty("instruction")]    public string        Instruction  { get; set; }
        [JsonProperty("action")]         public ActionPayload Action       { get; set; }
        [JsonProperty("raw_llm_output")] public string        RawLlmOutput { get; set; }
    }

    // Nested position object used by add_window, add_door, and start/end in create_wall
    public class Position
    {
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
        [JsonProperty("z")] public double Z { get; set; }
    }

    public class ActionPayload
    {
        [JsonProperty("action")] public string ActionType { get; set; }

        // --- create_wall ---
        [JsonProperty("start")]     public Position Start     { get; set; }
        [JsonProperty("end")]       public Position End       { get; set; }
        [JsonProperty("height")]    public double?  Height    { get; set; }
        [JsonProperty("thickness")] public double?  Thickness { get; set; }
        [JsonProperty("level")]     public string   Level     { get; set; }

        // --- add_window / add_door ---
        [JsonProperty("wall_id")]   public string   WallId    { get; set; }   // backend sends string or null
        [JsonProperty("position")]  public Position Position  { get; set; }
        [JsonProperty("width")]     public double?  Width     { get; set; }
        [JsonProperty("count")]     public int?     Count     { get; set; }
        [JsonProperty("spacing")]   public double?  Spacing   { get; set; }

        // Absorb any future backend fields without breaking deserialization
        [JsonExtensionData]
        public System.Collections.Generic.Dictionary<string, JToken> Extras { get; set; }
    }
}
