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
    private readonly IDictionary<int, Func<string?, string?>> _transforms = new Dictionary<int, Func<string?, string?>>();
    private readonly IDictionary<int, Func<string?, bool>> _skipIf = new Dictionary<int, Func<string?, bool>>();
    private readonly RowMapper _mapper;
    private readonly int[] _columnIds;

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

        var mapping = ruleSet.Mapping ?? new Dictionary<int, string>();
        _mapper = new RowMapper(mapping, culture);

        _columnIds = _transforms.Keys.Union(_skipIf.Keys).Union(mapping.Keys).Distinct().ToArray();

        Format = format;
        _culture = culture;
    }

    public async Task<IEnumerable<IncomingTransaction>> Read(StreamReader reader, CancellationToken ct)
        => await ReadAsync(reader, ct).ToListAsync(ct);

    private async IAsyncEnumerable<IncomingTransaction> ReadAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken ct)
    {
        using var csv = new CsvReader(reader, new CsvConfiguration(_culture) { HasHeaderRecord = true }, true);
        var row = new Dictionary<int, string?>();
        while (await csv.ReadAsync())
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            row.Clear();

            foreach (var id in _columnIds)
            {
                row[id] = csv.GetField(id);
            }

            foreach (var transform in _transforms)
            {
                row[transform.Key] = transform.Value(row[transform.Key]);
            }

            if (_skipIf.Any(r => r.Value(row[r.Key])))
            {
                continue;
            }

            yield return _mapper.Map(row);
        }
    }

    public SupportedFormat Format { get; }
}
