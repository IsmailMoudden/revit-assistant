using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Models
{
    // ── Envelope ─────────────────────────────────────────────────────────────

    public class ActionResponse
    {
        [JsonProperty("instruction")]    public string             Instruction  { get; set; }
        [JsonProperty("actions")]        public List<ActionPayload> Actions     { get; set; }
        [JsonProperty("raw_llm_output")] public string             RawLlmOutput { get; set; }
    }

    // ── Shared geometry ───────────────────────────────────────────────────────

    public class Position
    {
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
        [JsonProperty("z")] public double Z { get; set; }
    }

    // ── Single action payload ─────────────────────────────────────────────────
    // One flat class covers all action types.
    // Fields unused by a given action type are null and ignored by the handler.

    public class ActionPayload
    {
        [JsonProperty("action")] public string ActionType { get; set; }

        // ── create_wall ───────────────────────────────────────────────────────
        [JsonProperty("start")]     public Position Start     { get; set; }
        [JsonProperty("end")]       public Position End       { get; set; }
        [JsonProperty("height")]    public double?  Height    { get; set; }
        [JsonProperty("thickness")] public double?  Thickness { get; set; }
        [JsonProperty("level")]     public string   Level     { get; set; }

        // ── add_window / add_door ─────────────────────────────────────────────
        [JsonProperty("wall_id")]  public string   WallId   { get; set; }
        [JsonProperty("position")] public Position Position { get; set; }
        [JsonProperty("width")]    public double?  Width    { get; set; }
        [JsonProperty("count")]    public int?     Count    { get; set; }
        [JsonProperty("spacing")]  public double?  Spacing  { get; set; }

        // ── create_column ─────────────────────────────────────────────────────
        // position   → base point (x, y, z) in meters
        // height     → column height in meters       (reuses Height field)
        // level      → host level name               (reuses Level field)
        // family_name / type_name → structural column family

        // ── create_beam ───────────────────────────────────────────────────────
        // start / end → beam endpoints in meters     (reuses Start / End fields)
        // level       → host level name              (reuses Level field)
        // family_name / type_name → structural framing family

        // ── shared: family resolution (column + beam + window + door) ─────────
        [JsonProperty("family_name")] public string FamilyName { get; set; }
        [JsonProperty("type_name")]   public string TypeName   { get; set; }

        // Absorbs unknown future fields without breaking deserialization
        [JsonExtensionData]
        public Dictionary<string, JToken> Extras { get; set; }
    }

    // ── Context sent TO the backend ───────────────────────────────────────────

    public class RevitContext
    {
        [JsonProperty("selected_level")]   public string SelectedLevel   { get; set; }
        [JsonProperty("selection_count")]  public int    SelectionCount  { get; set; }
    }
}
