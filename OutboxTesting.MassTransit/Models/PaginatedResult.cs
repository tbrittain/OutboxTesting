namespace OutboxTesting.MassTransit.Models;

public class PaginatedRequest
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class PaginatedResult<T>
{
    public PaginatedResult(
        List<T> items,
        int pageNumber,
        int pageSize,
        int totalItems)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public PaginatedResult(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalItems)
    {
        Items = items.ToList();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}