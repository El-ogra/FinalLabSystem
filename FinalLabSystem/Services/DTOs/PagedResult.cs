using System;
using System.Collections.Generic;

namespace FinalLabSystem.Services.DTOs;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore => (Page * PageSize) < TotalCount;
}
