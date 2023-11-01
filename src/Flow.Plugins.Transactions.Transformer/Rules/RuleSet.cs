namespace Flow.Plugins.Transactions.Transformer.Rules;

internal sealed class RuleSet
{
    public IDictionary<int, TransformRule>? Transform { get; set; }

    public IDictionary<int, MatchRule>? SkipIf { get; set; }

    public IDictionary<int, string>? Mapping { get; set; }
}
