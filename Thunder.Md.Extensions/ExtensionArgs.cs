namespace Thunder.Md.Extensions;

using Microsoft.Extensions.Logging;
using Thunder.Md.Extensions.Config;

public record ExtensionArgs(ThunderConfig Config, string ExtensionPath, ILogger Logger);