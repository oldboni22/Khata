using Shared.Filters.Topic;

namespace Shared.Filters;

public record FilterEntry<T>(T SortOptions, bool Ascending) where T : Enum;
