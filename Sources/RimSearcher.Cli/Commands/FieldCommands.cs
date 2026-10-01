using System;
using ConsoleAppFramework;
using RimSearcher.Cli.Infrastructure;
using RimSearcher.Cli.Queries;

namespace RimSearcher.Cli.Commands;

internal sealed class FieldCommands(FieldRepository fieldRepository, DefRepository defRepository, JsonOutput output)
{
    private const string IndexSegmentHint =
        "Hint: field paths match literally as a suffix — nested list paths need their index segment " +
        "(e.g. 'pawnGroupMakers[0].kindDef'); or filter with 'get <def> --field <path>'";

    public static void Register(ConsoleApp.ConsoleAppBuilder app, FieldRepository fieldRepository, DefRepository defRepository, JsonOutput output)
    {
        var commands = new FieldCommands(fieldRepository, defRepository, output);
        app.Add("find", commands.Find);
        app.Add("fields", commands.Fields);
        app.Add("values", commands.Values);
    }

    /// <summary>
    /// Reverse lookup by an exact field value at a literal path suffix.
    /// Paths and values are case-sensitive. Lists require an index such as comps[0].compClass;
    /// different Defs may use different indexes. No hit at comps[0] does not rule out other indexes.
    /// Use full values such as RimWorld.CompShield; partial values belong in search.
    /// Booleans are lowercase. null matches empty fields and literal null values.
    /// Examples:
    ///   rimsearcher find 'comps[0].compClass' RimWorld.CompShield --type ThingDef
    ///   rimsearcher find workerClass null
    /// </summary>
    /// <param name="fieldPath">Literal field path suffix; nested list paths need index segments.</param>
    /// <param name="value">Exact value to match; use null for null fields.</param>
    /// <param name="type">Exact Def type filter; run types to discover valid types.</param>
    /// <param name="mod">Exact mod name filter; run mods to discover mod names.</param>
    /// <param name="limit">Maximum returned matches.</param>
    private void Find([Argument] string fieldPath, [Argument] string value, string? type = null, string? mod = null, int limit = 50)
    {
        if (TypeGuard.RejectUnknown(type, defRepository))
            return;

        var results = fieldRepository.Find(fieldPath, value, type, mod, limit);
        output.Write(results);
        if (results.Count == 0)
        {
            // null 查询 0 命中 = 该路径确实无空字段
            if (value == "null")
            {
                Console.Error.WriteLine("Hint: no null-field matches for this path suffix");
            }
            else
            {
                // 建议的命令必须可执行：FTS 不接受命名空间点号，取末段（类名 token 形态）；
                // 其余 FTS 运算符字符用引号短语兜底，保证 "search <值>" 不报语法错误。
                var fuzzyValue = value;
                var lastDot = fuzzyValue.LastIndexOf('.');
                if (lastDot >= 0)
                    fuzzyValue = fuzzyValue[(lastDot + 1)..];
                if (fuzzyValue.IndexOfAny(new[] { '"', '*', '^', '(', ')', ':', '-' }) >= 0)
                    fuzzyValue = $"\"{fuzzyValue}\"";
                Console.Error.WriteLine($"Hint: no exact matches. Try fuzzy search: rimsearcher search {fuzzyValue}");
            }
            if (fieldPath.Contains('.') && !fieldPath.Contains('['))
                Console.Error.WriteLine(IndexSegmentHint);
            // 无结果非零退出（stdout 仍输出 []），脚本可用退出码区分"未找到"与"查询失败"。
            Environment.ExitCode = ExitCodes.NotFound;
        }
    }

    /// <summary>
    /// Inspect the field tree of one Def; both defName and --type are required.
    /// List indexes in a filter are literal: comps*.* includes all indexes;
    /// comps[0].* selects index 0. Quote filters to prevent shell wildcard expansion.
    /// Example: rimsearcher fields ShieldBelt --type ThingDef --filter 'comps*.*'
    /// </summary>
    /// <param name="defName">Exact Def name to inspect.</param>
    /// <param name="type">Required Def type; distinguishes names shared by different types.</param>
    /// <param name="limit">Maximum visible fields; Hint reports truncated results.</param>
    /// <param name="filter">Field path glob; only * is a wildcard and matches any sequence across path segments.</param>
    private void Fields([Argument] string defName, string type, int limit = 1000, string? filter = null)
    {
        if (TypeGuard.RejectUnknown(type, defRepository))
            return;

        var result = fieldRepository.GetFields(defName, type, limit, filter);
        output.Write(result.Values);
        if (result.IsTruncated)
            Console.Error.WriteLine($"Hint: reached limit {limit}; results may be truncated, use --limit to increase");
        if (result.Values.Count == 0)
        {
            Console.Error.WriteLine($"Hint: no fields found for '{defName}' (type '{type}') — def may not exist, or all fields are noise-filtered");
            // 无结果非零退出（stdout 仍输出 []）：与 find/get 的 NotFound 契约统一。
            Environment.ExitCode = ExitCodes.NotFound;
        }
    }

    /// <summary>
    /// Enumerate distinct values at a case-sensitive literal field path suffix.
    /// A bare field name aggregates across list depths; values compClass lists the comp-class vocabulary.
    /// Indexed paths select that literal index, not every element.
    /// Example: rimsearcher values compClass --type ThingDef
    /// </summary>
    /// <param name="fieldPath">Literal field path suffix; use a bare field name to aggregate across lists.</param>
    /// <param name="type">Exact Def type filter; run types to discover valid types.</param>
    /// <param name="limit">Maximum distinct values returned.</param>
    private void Values([Argument] string fieldPath, string? type = null, int limit = 200)
    {
        if (TypeGuard.RejectUnknown(type, defRepository))
            return;

        var values = fieldRepository.GetValues(fieldPath, type, limit);
        output.Write(values);
        if (values.Count == 0)
        {
            Console.Error.WriteLine(
                $"Hint: no values for '{fieldPath}' — the path may not exist in any def, " +
                "or --type filtered everything (try without --type)");
            if (fieldPath.Contains('.') && !fieldPath.Contains('['))
                Console.Error.WriteLine(IndexSegmentHint);
            // 无结果非零退出（stdout 仍输出 []）：与 find/get 的 NotFound 契约统一。
            Environment.ExitCode = ExitCodes.NotFound;
        }
    }
}
