using System;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleAppFramework;
using RimSearcher.Cli.Commands;
using RimSearcher.Cli.Infrastructure;
using RimSearcher.Cli.Queries;

Console.OutputEncoding = Encoding.UTF8;
// stderr 默认继承系统代码页（如 CP437），hint 内插的 defName 等可能含非 ASCII；
// Console.Error 无 OutputEncoding 属性，用 StreamWriter 替换并设 AutoFlush 保证即时写出。
Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

// 框架错误输出重定向到 stderr，保证"数据走 stdout、错误走 stderr"的契约。
ConsoleApp.LogError = message => Console.Error.WriteLine(message);

string databasePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "defs.db");
var connectionFactory = new DatabaseConnectionFactory(databasePath);
var output = new JsonOutput();
var app = ConsoleApp.Create();
app.UseFilter<CliExceptionFilter>();
var defRepository = new DefRepository(connectionFactory);
SearchCommands.Register(app, defRepository, output);
DefCommands.Register(app, defRepository, output);
FieldCommands.Register(app, new FieldRepository(connectionFactory), defRepository, output);
StatisticsCommands.Register(app, new StatisticsRepository(connectionFactory), output);
MaintenanceCommands.Register(app);

// 命令集合与下方 Register 调用一一对应，新增命令时需同步。
string[] knownCommands = ["search", "list", "get", "find", "fields", "values", "types", "mods", "check update"];

// 总帮助保留命令导航；参数和查询语义由框架从命令方法的文档注释生成。
if (args.Length == 0 || (args.Length == 1 && (args[0] == "-h" || args[0] == "--help")))
{
    Console.WriteLine($"""
        RimSearcher queries runtime Def snapshots exported by DataMod.
        Usage: rimsearcher <command> [arguments] [options]

        Commands:
          search        Search Def data with FTS5
          list          Browse Defs by type or mod
          get           Fetch a Def, class bridge, or JSON field
          find          Reverse lookup by exact field value
          fields        Inspect a Def's field tree
          values        Enumerate distinct field values
          types         Count Defs by type
          mods          Count Defs by mod
          check update  Check GitHub for a newer release

        Help: rimsearcher <command> --help (also -h)
        Example: rimsearcher search --help
        Version: rimsearcher --version
        Database: defs.db beside the executable, not in the shell's working directory.
        Supported DataMod export versions: {DatabaseConnectionFactory.SupportedVersions}.
        A CLI upgrade alone does not require a new export if the database is supported.
        Exit codes: 0 success; 1 error; 2 not found or needs disambiguation.
        An empty list page returns exit 0. Help needs no database, MCP, or network.
        """);
    return;
}

// 未知命令契约：框架默认输出帮助到 stdout 且 exit 0，脚本无法区分"命令不存在"与"成功"。
// 命令路径可能包含嵌套子命令；分组命令本身交给框架处理 --help。
// 选项开头（--version 等）交给框架处理。
if (args.Length > 0 && !args[0].StartsWith('-'))
{
    var commandRoot = args[0];
    var hasSubcommands = knownCommands.Any(command =>
        command.StartsWith($"{commandRoot} ", StringComparison.Ordinal));
    var commandPath = hasSubcommands && args.Length > 1 && !args[1].StartsWith('-')
        ? $"{commandRoot} {args[1]}"
        : commandRoot;
    var isKnownCommand = knownCommands.Contains(commandPath)
        || (hasSubcommands && commandPath == commandRoot);

    if (!isKnownCommand)
    {
        Console.Error.WriteLine($"Error: unknown command '{commandPath}'");
        Console.Error.WriteLine("Usage: rimsearcher <command> [options]");
        Console.Error.WriteLine("Commands: " + string.Join(", ", knownCommands));
        Environment.Exit(ExitCodes.Error);
    }
}

app.Run(args);
