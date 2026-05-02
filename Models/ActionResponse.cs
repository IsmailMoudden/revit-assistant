using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Models
{
    // ── Envelope ─────────────────────────────────────────────────────────────

    public class ActionResponse
    {
        // "ok" | "needs_clarification"
        [JsonProperty("status")]         public string             Status       { get; set; }

        // status == "ok"
        [JsonProperty("actions")]        public List<ActionPayload> Actions     { get; set; }

        // status == "needs_clarification"
        [JsonProperty("questions")]      public List<ClarificationQuestion> Questions { get; set; }

        [JsonProperty("instruction")]    public string             Instruction  { get; set; }
        [JsonProperty("raw_llm_output")] public string             RawLlmOutput { get; set; }
    }

    // ── Clarification question from backend ───────────────────────────────────

    public class ClarificationQuestion
    {
        [JsonProperty("id")]       public string Id       { get; set; }  // key used in answers dict
        [JsonProperty("question")] public string Question { get; set; }
        [JsonProperty("type")]     public string Type     { get; set; }  // "number" | "text" | "choice"
        [JsonProperty("default")]  public JToken Default  { get; set; }  // any JSON value
        [JsonProperty("choices")]  public List<string> Choices { get; set; } // for type == "choice"
    }

    // ── Shared geometry ───────────────────────────────────────────────────────

    public class Position
    {
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
        [JsonProperty("z")] public double Z { get; set; }
    }

    // ── Single action payload ─────────────────────────────────────────────────

    public class ActionPayload
    {
        [JsonProperty("action")] public string ActionType { get; set; }

        // create_wall
        [JsonProperty("start")]     public Position Start     { get; set; }
        [JsonProperty("end")]       public Position End       { get; set; }
        [JsonProperty("height")]    public double?  Height    { get; set; }
        [JsonProperty("thickness")] public double?  Thickness { get; set; }
        [JsonProperty("level")]     public string   Level     { get; set; }

        // add_window / add_door
        [JsonProperty("wall_id")]  public string   WallId   { get; set; }
        [JsonProperty("position")] public Position Position { get; set; }
        [JsonProperty("width")]    public double?  Width    { get; set; }
        [JsonProperty("count")]    public int?     Count    { get; set; }
        [JsonProperty("spacing")]  public double?  Spacing  { get; set; }

        // create_column / create_beam (reuse start/end/height/level above)
        [JsonProperty("family_name")] public string FamilyName { get; set; }
        [JsonProperty("type_name")]   public string TypeName   { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extras { get; set; }
    }

    // ── Context sent TO the backend ───────────────────────────────────────────

    public class RevitContext
    {
        [JsonProperty("selected_level")]  public string SelectedLevel  { get; set; }
        [JsonProperty("selection_count")] public int    SelectionCount { get; set; }
    }
}
