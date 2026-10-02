using ConsoleAppFramework;
using RimSearcher.Cli.Infrastructure;
using RimSearcher.Cli.Queries;

namespace RimSearcher.Cli.Commands;

internal sealed class StatisticsCommands(StatisticsRepository repository, JsonOutput output)
{
    public static void Register(ConsoleApp.ConsoleAppBuilder app, StatisticsRepository repository, JsonOutput output)
    {
        var commands = new StatisticsCommands(repository, output);
        app.Add("types", commands.Types);
        app.Add("mods", commands.Mods);
    }

    /// <summary>
    /// Discover all Def types with their Def counts.
    /// Use the returned type names with --type on query commands.
    /// Example: rimsearcher types
    /// </summary>
    private void Types() => output.Write(repository.GetTypes());

    /// <summary>
    /// Discover mods represented in the Def snapshot with their package IDs and Def counts.
    /// Use the returned mod names with --mod on query commands.
    /// Example: rimsearcher mods
    /// </summary>
    private void Mods() => output.Write(repository.GetMods());
}
