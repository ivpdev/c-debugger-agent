using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DebugAgentPrototype.Models;

namespace DebugAgentPrototype.Services;

public static class ToolGetSourceCode
{
    public static ToolConfig GetConfig()
    {
        return new ToolConfig("get_source_code", "Get the source code of a file", new { type = "object", properties = new { } });
    }

    public static string CallAsync(AppState state, LldbService lldbService, CancellationToken ct)
    {
        return SourceCodeService.GetInspectedFileContent();
    }
}

