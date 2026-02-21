namespace Thunder.Md.Extensions.Config;

using System.Reflection;

public static class ThunderPaths{
    public static string Project{
        get{
            field ??= Directory.GetCurrentDirectory();
            return field;
        }
    }

    public static string Source{
        get{
            field ??= Path.Combine(Project, "src");
            return field;
        }
    }

    public static string Build{
        get{
            field ??= Path.Combine(Project, "build");
            return field;
        }
    }

    public static string ConfigPath{
        get{
            field ??= Path.Combine(Project, ThunderConstants.ConfigFileName);
            return field;
        }
    }

    public static string AssemblyPath{
        get{
            field ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            return field;
        }
    }

    public static string TemplatePath{
        get{
            field ??= Path.Combine(AssemblyPath, "templates");
            return field;
        }
    }
}