namespace KeyboardSwitch.Core.Services.AutoConfiguration;

public abstract class AutoConfigurationServiceBase : IAutoConfigurationService
{
    protected struct KeyToCharResult
    {
        private KeyToCharResult(bool isSuccess, char ch, string layoutId)
        {
            this.IsSuccess = isSuccess;
            this.Char = ch;
            this.LayoutId = layoutId;
        }

        public bool IsSuccess { get; }
        public char Char { get; }
        public string LayoutId { get; }

        public static KeyToCharResult Success(char ch, string layoutId) =>
            new(true, ch, layoutId);

        public static KeyToCharResult Failure() =>
            new(false, '\0', String.Empty);
    }

    private struct DistinctCharsState
    {
        public DistinctCharsState(
            ImmutableList<List<KeyToCharResult>> results,
            ImmutableDictionary<string, ImmutableList<char>> processedChars)
        {
            this.Results = results;
            this.DistinctChars = processedChars;
        }

        public static DistinctCharsState Initial(List<string> layoutIds) =>
            new(
                ImmutableList<List<KeyToCharResult>>.Empty,
                layoutIds.ToImmutableDictionary(layoutId => layoutId, _ => ImmutableList<char>.Empty));

        public ImmutableList<List<KeyToCharResult>> Results { get; }
        public ImmutableDictionary<string, ImmutableList<char>> DistinctChars { get; }
    }

    public Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts)
    {
        var layoutIds = layouts.Select(layout => layout.Id).ToList();

        return this.GetChars(layoutIds)
            .Where(results => results.All(result => result.IsSuccess))
            .Aggregate(DistinctCharsState.Initial(layoutIds), this.RemoveDuplicateChars)
            .Results
            .SelectMany(results => results)
            .GroupBy(result => result.LayoutId, result => result.Char)
            .ToDictionary(result => result.Key.ToString(), result => new string(result.ToArray()));
    }

    protected abstract IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds);

    private DistinctCharsState RemoveDuplicateChars(DistinctCharsState state, List<KeyToCharResult> results) =>
        results.Any(result => state.DistinctChars[result.LayoutId].Contains(result.Char))
            ? state
            : new(
                state.Results.Add(results),
                results.ToImmutableDictionary(
                    result => result.LayoutId,
                    result => state.DistinctChars[result.LayoutId].Add(result.Char)));
}
