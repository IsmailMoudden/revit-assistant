using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Models
{
    // ── Request sent TO the backend ───────────────────────────────────────────

    public class BimRequest
    {
        [JsonProperty("instruction")]    public string                     Instruction   { get; set; }
        [JsonProperty("selected_level")] public string                     SelectedLevel { get; set; }
        [JsonProperty("answers")]        public Dictionary<string, object> Answers       { get; set; }  // null on first call
        [JsonProperty("history")]        public List<ConversationMessage>  History       { get; set; }
    }

    public class ConversationMessage
    {
        [JsonProperty("role")]    public string Role    { get; set; }  // "user" | "assistant"
        [JsonProperty("content")] public string Content { get; set; }
    }

    // ── Response from the backend ─────────────────────────────────────────────

    public class ActionResponse
    {
        [JsonProperty("status")]         public string                     Status       { get; set; }  // "ok" | "needs_clarification"
        [JsonProperty("actions")]        public List<ActionPayload>        Actions      { get; set; }  // status == "ok"
        [JsonProperty("questions")]      public List<ClarificationQuestion> Questions   { get; set; }  // status == "needs_clarification"
        [JsonProperty("raw_llm_output")] public string                     RawLlmOutput { get; set; }
    }

    // ── Clarification question ────────────────────────────────────────────────

    public class ClarificationQuestion
    {
        [JsonProperty("id")]       public string      Id       { get; set; }
        [JsonProperty("question")] public string      Question { get; set; }
        [JsonProperty("type")]     public string      Type     { get; set; }  // "number" | "text" | "choice"
        [JsonProperty("default")]  public JToken      Default  { get; set; }
        [JsonProperty("choices")]  public List<string> Choices { get; set; }
    }

    // ── Shared geometry ───────────────────────────────────────────────────────

    public class Position
    {
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
        [JsonProperty("z")] public double Z { get; set; }
    }

    // ── Action payload ────────────────────────────────────────────────────────

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

        // create_column / create_beam
        // "section" = structural profile name e.g. "HEA200", "IPE300"
        [JsonProperty("section")]     public string Section     { get; set; }
        // legacy fields kept for compatibility
        [JsonProperty("family_name")] public string FamilyName  { get; set; }
        [JsonProperty("type_name")]   public string TypeName    { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extras { get; set; }
    }
}
