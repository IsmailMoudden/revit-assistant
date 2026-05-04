using System.Collections.Generic;

namespace BimAiAssistant.Actions
{
    public static class RevitLogger
    {
        public static void Warn(List<string> warnings, string message)
        {
            warnings.Add(message);
        }
    }
}
