namespace Shared.Search;

public record SortingEntry<T>(T SortOptions, bool Ascending) where T : Enum;
