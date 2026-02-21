namespace Thunder.Md;

using System.Reflection;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Thunder.Md.CodeExtensions;
using Thunder.Md.Extensions;
using Thunder.Md.Extensions.Config;
using Thunder.Md.InternalExtensions;
using Thunder.Md.Logging;
using Thunder.Md.Readers;
using Thunder.Md.Writers;

class Program{
    public static bool Error{ get; set; } = false;
    public static ExtensionLoader? ExtensionLoader{ get; private set; }

    private static ILoggerFactory _loggerFactory = LoggerFactory.Create(logging => {
        logging.Configure(options => {
                   options.ActivityTrackingOptions
                       =
                       ActivityTrackingOptions
                           .Tags |
                       ActivityTrackingOptions
                           .Baggage;
               })
               .AddProvider(new CustomProvider());
    });

    static void Main(string[] args){
        QuestPDF.Settings.License = LicenseType.Community;

        string[] optionArgs = args.Where(x => x.StartsWith('-')).ToArray();
        args = args.Where(x => !optionArgs.Contains(x)).ToArray();

        LogLevel minLogLevel = LogLevel.Information;
        if(optionArgs.Contains("-l") || optionArgs.Contains("--log")){
            minLogLevel = LogLevel.Trace;
        }

        _loggerFactory = LoggerFactory.Create(logging => {
            logging.SetMinimumLevel(minLogLevel);
            logging.Configure(options => {
                options.ActivityTrackingOptions = ActivityTrackingOptions.Tags | ActivityTrackingOptions.Baggage;
            }).AddProvider(new CustomProvider());
        });

        if(optionArgs.Contains("-v") || optionArgs.Contains("--version")){
            string? versionString = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            Console.WriteLine($"Version: {versionString}");
            return;
        }

        ExtensionLoader = new ExtensionLoader(CreateLogger<ExtensionLoader>(), ThunderPaths.ExtensionPath);
        ExtensionLoader.AddExtension<InternalExtensionWrapper>();
        ExtensionLoader.LoadExtensions();

        if(optionArgs.Contains("-h") || optionArgs.Contains("-?") || optionArgs.Contains("--help")){
            if(args.Length != 1){
                PrintHelp(args[1], ExtensionLoader);
                return;
            }

            PrintHelp(null, ExtensionLoader);
            return;
        }

        if(args.Length < 1){
            PrintHelp(null, ExtensionLoader);
            return;
        }

        string task = args[0].ToLower();

        ConfigLoader configLoader = new(CreateLogger<ConfigLoader>());
        configLoader.TryRead(out ThunderProjectConfig? projectConfig);

        var config = new ThunderConfig(new ThunderGlobalConfig(), projectConfig);

        if(ExtensionLoader.TryGetCommand(task, out ICommand? command)){
            ExtensionArgs extensionConfig = ExtensionLoader.GetExtensionConfig(command, config);
            command.Execute(args[1..], extensionConfig);
            return;
        }


        PrintHelp(null, ExtensionLoader);
    }


    private static void PrintHelp(string? arg, ExtensionLoader loader){
        Console.WriteLine("Invalid arguments");

        if(arg is not null && loader.TryGetCommand(arg, out ICommand? command)){
            Console.WriteLine(command.Help);
            return;
        }

        Console.WriteLine("Syntax: protex [task]");
        Console.WriteLine("Tasks:");

        foreach(var cmd in loader.GetCommandNames()){
            Console.WriteLine($"- {cmd}");
        }

        Console.WriteLine();
    }

    public static ILogger CreateLogger<T>(){
        return CreateLogger(typeof(T));
    }

    public static ILogger CreateLogger(Type type){
        return _loggerFactory.CreateLogger(type.Name);
    }
}