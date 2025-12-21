using HotelListing.Api.Common.Models.Paging;
using Microsoft.EntityFrameworkCore;
namespace HotelListing.Api.Common.Models.Extentions;

public static class QueryableExtentions
{
    public static async Task<PagedResult<T>> ToPageResultAsync<T>
        (this IQueryable<T> source, PaginationParameters paginationParameters) 
    {
        var totalCount = await source.CountAsync();
        var items = await source
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);

        var metaData = new PaginationMetaData
        {
            TotalCount = totalCount,
            PageSize = paginationParameters.PageSize,
            CurrentPage = paginationParameters.PageNumber,
            TotalPages = totalPages,
            HasNext = paginationParameters.PageNumber < totalPages,
            HasPrevious = paginationParameters.PageNumber > 1
        };

        return new PagedResult<T>
        {
            Data = items,
            MetaData = metaData
        };
    }

}
