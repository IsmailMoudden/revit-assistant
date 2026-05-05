using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BimAiAssistant.Models
{
    // ── Request sent TO the backend ───────────────────────────────────────────

    public class BimRequest
    {
        [JsonProperty("instruction")]        public string                     Instruction       { get; set; }
        [JsonProperty("selected_level")]     public string                     SelectedLevel     { get; set; }
        [JsonProperty("answers")]            public Dictionary<string, object> Answers           { get; set; }
        [JsonProperty("history")]            public List<ConversationMessage>  History           { get; set; }
        [JsonProperty("bim_context")]        public BimContext                 BimContext        { get; set; }
        [JsonProperty("execution_results")]  public List<ExecutionResult>      ExecutionResults  { get; set; }
    }

    public class ExecutionResult
    {
        [JsonProperty("action")]          public string      Action         { get; set; }
        [JsonProperty("status")]          public string      Status         { get; set; }  // "success" | "error"
        [JsonProperty("revit_id")]        public long?       RevitId        { get; set; }  // set on success
        [JsonProperty("reason")]          public string      Reason         { get; set; }  // set on error
        [JsonProperty("original_params")] public ActionPayload OriginalParams { get; set; }
    }

    public class BimContext
    {
        [JsonProperty("existing_elements")]     public List<object> ExistingElements    { get; set; }
        [JsonProperty("levels")]                public List<string> Levels              { get; set; }
        [JsonProperty("selected_element_ids")]  public List<long>   SelectedElementIds  { get; set; }
        [JsonProperty("loaded_column_families")] public List<string> LoadedColumnFamilies { get; set; }
        [JsonProperty("loaded_beam_families")]  public List<string> LoadedBeamFamilies  { get; set; }
        [JsonProperty("loaded_wall_types")]     public List<string> LoadedWallTypes     { get; set; }
    }

    public class ConversationMessage
    {
        [JsonProperty("role")]    public string Role    { get; set; }  // "user" | "assistant"
        [JsonProperty("content")] public string Content { get; set; }
    }

    // ── Response from the backend ─────────────────────────────────────────────

    public class ActionResponse
    {
        [JsonProperty("status")]         public string                      Status       { get; set; }  // "ok" | "needs_clarification" | "error"
        [JsonProperty("actions")]        public List<ActionPayload>         Actions      { get; set; }  // status == "ok"
        [JsonProperty("questions")]      public List<ClarificationQuestion> Questions    { get; set; }  // status == "needs_clarification"
        [JsonProperty("warnings")]       public List<string>                Warnings     { get; set; }  // section substitution notices
        [JsonProperty("error")]          public ErrorDetail                 Error        { get; set; }  // status == "error"
        [JsonProperty("raw_llm_output")] public string                      RawLlmOutput { get; set; }
    }

    public class ErrorDetail
    {
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("fix")]     public string Fix     { get; set; }
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

        // create_column / create_beam — backend always resolves section before responding
        [JsonProperty("section")] public string Section { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extras { get; set; }
    }
}
