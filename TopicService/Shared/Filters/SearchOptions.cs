namespace Shared.Filters;

public abstract record SearchOptions<T>(string? SearchTerm, List<FilterEntry<T>>? Filters) where T : Enum;
