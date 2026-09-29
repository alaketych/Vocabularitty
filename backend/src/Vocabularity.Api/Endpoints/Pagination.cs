using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Api.Endpoints;

public sealed record PageResponse<T>(
    [property: JsonProperty("pageNumber")] int PageNumber,
    [property: JsonProperty("pageSize")] int PageSize,
    [property: JsonProperty("data")] IReadOnlyList<T> Data);

public sealed record Pagination(int PageNumber, int PageSize)
{
    public static Pagination Read(HttpRequest request)
    {
        int ReadNumber(string name, int fallback) => !request.Query.ContainsKey(name) ? fallback :
            request.Query[name].Count == 1 && int.TryParse(request.Query[name], out var value) && value > 0
                ? value : throw new ApiException(400, $"{name} must be a positive integer.");
        var page = ReadNumber("pageNumber", 1);
        var size = ReadNumber("pageSize", 12);
        if (size > 100 || (long)(page - 1) * size > int.MaxValue)
            throw new ApiException(400, "pageSize must be between 1 and 100 and pageNumber must be within range.");
        return new(page, size);
    }
}
