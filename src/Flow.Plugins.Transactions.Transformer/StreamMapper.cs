using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Flow.Application.Transactions.Contract;
using Flow.Infrastructure.IO.Contract;
using Flow.Infrastructure.Plugins.Contract;
using Flow.Plugins.Transactions.Transformer.Rules;

namespace Flow.Plugins.Transactions.Transformer;

internal sealed class Mapper : IPlugin, IFormatSpecificReader<IncomingTransaction>
{
    private readonly CultureInfo _culture;
    private readonly Dictionary<string, Func<string?, string?>> _transforms = new();
    private readonly Dictionary<string, Func<string?, bool>> _skipIf = new ();
    private readonly RowMapper _mapper;
    private readonly IDictionary<string,int> _mapping;

    public Mapper(SupportedFormat format, RuleSet ruleSet, CultureInfo culture)
    {
        if (ruleSet.Transform != null)
        {
            foreach (var transformRule in ruleSet.Transform)
            {
                var regex = new Regex(transformRule.Value.Pattern ?? string.Empty, RegexOptions.Compiled);
                string? TransformFunc(string? input) => regex.Replace(input ?? string.Empty, transformRule.Value.Replacement ?? string.Empty);
                _transforms.Add(transformRule.Key, TransformFunc);
            }
        }

        if (ruleSet.SkipIf != null)
        {
            foreach (var matchRule in ruleSet.SkipIf)
            {
                var regex = new Regex(matchRule.Value.Pattern ?? string.Empty, RegexOptions.Compiled);
                bool MatchFunc(string? input) => regex.IsMatch(input ?? string.Empty);

                _skipIf.Add(matchRule.Key, MatchFunc);
            }
        }

        _mapping = ruleSet.Mapping ?? new Dictionary<string, int>();
        _mapper = new RowMapper(culture);


        Format = format;
        _culture = culture;
    }

    public async Task<IEnumerable<IncomingTransaction>> Read(StreamReader reader, CancellationToken ct)
        => await ReadAsync(reader, ct).ToListAsync(ct);

    private async IAsyncEnumerable<IncomingTransaction> ReadAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken ct)
    {
        using var csv = new CsvReader(reader, new CsvConfiguration(_culture) { HasHeaderRecord = true, MissingFieldFound = null }, true);
        var row = new Dictionary<string, string?>();
        while (await csv.ReadAsync())
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            row.Clear();

            var shouldSkip = false;
            foreach (var map in _mapping)
            {
                var value = csv.GetField(map.Value);
                if (_transforms.TryGetValue(map.Key, out var transform))
                {
                    value = transform(value);
                }

                shouldSkip = shouldSkip || _skipIf.TryGetValue(map.Key, out var skip) && skip(value);
                
                row[map.Key] = value;
            }

            if (shouldSkip)
            {
                continue;
            }

            yield return _mapper.Map(row);
        }
    }

    public SupportedFormat Format { get; }
}
