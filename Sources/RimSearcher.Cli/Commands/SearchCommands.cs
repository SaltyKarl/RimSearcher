using System;
using System.Linq;
using ConsoleAppFramework;
using RimSearcher.Cli.Infrastructure;
using RimSearcher.Cli.Queries;
using RimSearcher.Cli.Search;

namespace RimSearcher.Cli.Commands;

internal sealed class SearchCommands(DefRepository repository, JsonOutput output)
{
    public static void Register(ConsoleApp.ConsoleAppBuilder app, DefRepository repository, JsonOutput output)
    {
        var commands = new SearchCommands(repository, output);
        app.Add("search", commands.Search);
        app.Add("list", commands.List);
    }

    /// <summary>
    /// Search runtime Def data using FTS5.
    /// FTS matches whole tokens; camelCase is not split. Use term* for a token prefix,
    /// OR / NOT for expressions, or double quotes inside the query for a phrase.
    /// A single bare word also matches defName/label substrings (at least 3 characters,
    /// or any CJK term; excludes AND/OR/NOT). Token hits come first; substring hits fill remaining slots.
    /// CJK queries use automatic bigrams. Rank is negative; more negative is more relevant.
    /// Rank applies only to token hits; substring hits have no rank.
    /// Examples:
    ///   rimsearcher search raid --type ThingDef
    ///   rimsearcher search 'shield*' --count
    ///   rimsearcher search '"shield belt"'
    /// </summary>
    /// <param name="keyword">Keyword or FTS5 expression. Quote expressions as a single shell argument.</param>
    /// <param name="type">Exact Def type filter; run types to discover valid types.</param>
    /// <param name="mod">Exact mod name filter; run mods to discover mod names.</param>
    /// <param name="limit">Maximum returned rows; ignored with --count.</param>
    /// <param name="count">Return total matching count instead of rows; not limited by --limit.</param>
    /// <param name="nameOnly">Search only defName; excludes labels and other data.</param>
    private void Search([Argument] string keyword, string? type = null, string? mod = null, int limit = 20, bool count = false, bool nameOnly = false)
    {
        if (TypeGuard.RejectUnknown(type, repository))
            return;

        if (count)
        {
            var countResult = repository.CountSearchResults(keyword, type, mod, nameOnly);
            output.Write(new { count = countResult });
            if (countResult == 0)
            {
                MaybePrefixWildcardHint(keyword);
                Environment.ExitCode = ExitCodes.NotFound;
            }
            return;
        }
        var results = repository.Search(keyword, type, mod, limit, nameOnly);
        output.Write(results);
        if (results.Count == 0)
        {
            MaybePrefixWildcardHint(keyword);
            // 无结果非零退出（stdout 仍输出 []）：与 find/get 的 NotFound 契约统一。
            Environment.ExitCode = ExitCodes.NotFound;
        }
    }

    /// <summary>
    /// Browse Defs by type or mod with pagination.
    /// An empty page is normal pagination and returns exit 0.
    /// Example: rimsearcher list --type ThingDef --limit 20 --offset 20 --total
    /// </summary>
    /// <param name="type">Exact Def type filter; run types to discover valid types.</param>
    /// <param name="mod">Exact mod name filter; run mods to discover mod names.</param>
    /// <param name="limit">Maximum rows in the page.</param>
    /// <param name="offset">Number of rows to skip.</param>
    /// <param name="total">Return total matching count alongside the page results.</param>
    private void List(string? type = null, string? mod = null, int limit = 20, int offset = 0, bool total = false)
    {
        if (TypeGuard.RejectUnknown(type, repository))
            return;

        var results = repository.List(type, mod, limit, offset);
        if (total)
            output.Write(new { total = repository.CountListed(type, mod), results });
        else
            output.Write(results);
    }

    /// <summary>
    /// 0 命中时的方向指引：单裸词已做过子串补充仍 0 命中，即名字与文本中确实不存在该词，
    /// 提示拼写检查与浏览路径；复合 FTS 查询未做子串补充，而是提示前缀通配机制。
    /// </summary>
    private static void MaybePrefixWildcardHint(string keyword)
    {
        if (keyword.Contains('*'))
        {
            Console.Error.WriteLine(
                $"Hint: 0 hits for '{keyword}' even with a prefix wildcard — the keyword may not be a def name; " +
                $"browse with 'list --type <T>' (run 'types' to list valid types)");
            return;
        }

        if (SearchSubstring.IsBareWord(keyword))
        {
            Console.Error.WriteLine(
                $"Hint: no defs match '{keyword}' in names or text — check spelling, or browse with 'list --type <T>' " +
                $"(run 'types' to list valid types)");
            return;
        }

        if (!keyword.Any(char.IsAsciiLetter))
            return;
        Console.Error.WriteLine(
            $"Hint: 0 hits for '{keyword}'. FTS expressions match whole tokens — try a prefix wildcard on a term " +
            $"(e.g. 'term*'), simpler terms, or browse with 'list --type <T>' (run 'types' to list valid types)");
    }
}
