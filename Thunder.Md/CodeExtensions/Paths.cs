namespace Thunder.Md.CodeExtensions;

using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;

public static class Paths{
    private static string? _extensionPath;
    extension(ThunderPaths){
        public static string ExtensionPath{
            get{
                _extensionPath ??= Path.Combine(ThunderPaths.AssemblyPath, "extensions");
                return _extensionPath;
            }
        }
    }
}