using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DebugAgentPrototype.Models;

public interface IToolConfig
{
    string Name { get; }
    string? Description { get; }
    object Parameters { get; }
}

