using System.Collections.ObjectModel;

namespace KeyboardSwitch.Core.Services.AutoConfiguration;

public abstract class AutoConfigurationServiceBase : IAutoConfigurationService
{
    protected readonly struct KeyToCharResult
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

    private readonly struct DistinctCharsState(
        ImmutableList<List<KeyToCharResult>> results,
        ImmutableDictionary<string, ImmutableList<char>> processedChars)
    {
        public ImmutableList<List<KeyToCharResult>> Results { get; } = results;
        public ImmutableDictionary<string, ImmutableList<char>> DistinctChars { get; } = processedChars;

        public static DistinctCharsState Initial(List<string> layoutIds) =>
            new([], layoutIds.ToImmutableDictionary(layoutId => layoutId, _ => ImmutableList<char>.Empty));
    }

    public IReadOnlyDictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts)
    {
        var layoutIds = layouts.Select(layout => layout.Id).ToList();

        var result = this.GetChars(layoutIds)
            .Where(results => results.All(result => result.IsSuccess))
            .Aggregate(DistinctCharsState.Initial(layoutIds), this.RemoveDuplicateChars)
            .Results
            .SelectMany(results => results)
            .GroupBy(result => result.LayoutId, result => result.Char)
            .ToDictionary(result => result.Key.ToString(), result => new string(result.ToArray()));

        return new ReadOnlyDictionary<string, string>(result);
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
