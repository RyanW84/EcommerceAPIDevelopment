using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerceApp.ConsoleClient.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace ECommerceApp.ConsoleClient.Services
{

    public class ECommerceApiClient
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };

        private readonly HttpClient _httpClient;

        public ECommerceApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<PaginatedResponse<List<Product>>>> GetProductsAsync(
            ProductQuery query,
            CancellationToken cancellationToken = default)
        {
            var uri = QueryHelpers.AddQueryString("api/Product", BuildQueryDictionary(query));
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            return await ReadPaginatedResponseAsync<List<Product>>(response, cancellationToken);
        }

        public async Task<ApiResult<Product>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/Product/{id}", cancellationToken);
            return await ReadResultAsync<Product>(response, cancellationToken);
        }

        public async Task<ApiResult<Product>> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            var payload = new ApiRequest<Product>(product);
            using var response = await _httpClient.PostAsJsonAsync("api/Product", payload, SerializerOptions, cancellationToken);
            return await ReadResultAsync<Product>(response, cancellationToken);
        }

        public async Task<ApiResult<Product>> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default)
        {
            var payload = new ApiRequest<Product>(product);
            using var response = await _httpClient.PutAsJsonAsync($"api/Product/{id}", payload, SerializerOptions, cancellationToken);
            return await ReadResultAsync<Product>(response, cancellationToken);
        }

        public async Task<ApiResult<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"api/Product/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return ApiResult<bool>.CreateSuccess(true, response.StatusCode);
            }

            var error = await ReadErrorAsync(response, cancellationToken);
            return ApiResult<bool>.CreateFailure(error.Message, response.StatusCode);
        }

        public async Task<ApiResult<List<Category>>> GetCategoriesAsync(
            CategoryQuery query,
            CancellationToken cancellationToken = default)
        {
            var uri = QueryHelpers.AddQueryString("api/Categories", BuildQueryDictionary(query));
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            return await ReadResultAsync<List<Category>>(response, cancellationToken);
        }

        public async Task<ApiResult<Category>> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            var payload = new ApiRequest<Category>(category);
            using var response = await _httpClient.PostAsJsonAsync("api/Categories", payload, SerializerOptions, cancellationToken);
            return await ReadResultAsync<Category>(response, cancellationToken);
        }

        public async Task<ApiResult<Category>> UpdateCategoryAsync(int id, Category category, CancellationToken cancellationToken = default)
        {
            var payload = new ApiRequest<Category>(category);
            using var response = await _httpClient.PutAsJsonAsync($"api/Categories/{id}", payload, SerializerOptions, cancellationToken);
            return await ReadResultAsync<Category>(response, cancellationToken);
        }

        public async Task<ApiResult<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync($"api/Categories/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return ApiResult<bool>.CreateSuccess(true, response.StatusCode);
            }

            var error = await ReadErrorAsync(response, cancellationToken);
            return ApiResult<bool>.CreateFailure(error.Message, response.StatusCode);
        }

        public async Task<ApiResult<Sale>> GetSaleByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/Sales/{id}", cancellationToken);
            return await ReadResultAsync<Sale>(response, cancellationToken);
        }

        public async Task<ApiResult<PaginatedResponse<List<Sale>>>> GetSalesAsync(
            SaleQuery query,
            CancellationToken cancellationToken = default)
        {
            var uri = QueryHelpers.AddQueryString("api/Sales", BuildQueryDictionary(query));
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            return await ReadPaginatedResponseAsync<List<Sale>>(response, cancellationToken);
        }

        public async Task<ApiResult<Sale>> CreateSaleAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            var payload = new ApiRequest<Sale>(sale);
            using var response = await _httpClient.PostAsJsonAsync("api/Sales", payload, SerializerOptions, cancellationToken);
            return await ReadResultAsync<Sale>(response, cancellationToken);
        }

        private static Dictionary<string, string?> BuildQueryDictionary(object query)
        {
            return query
                .GetType()
                .GetProperties()
                .Where(prop => prop.GetValue(query) is not null)
                .ToDictionary(
                    prop => prop.Name,
                    prop => ConvertToString(prop.GetValue(query))
                );
        }

        private static string? ConvertToString(object? value) => value switch
        {
            null => null,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            _ => value.ToString()
        };

        private static async Task<ApiResult<T>> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return ApiResult<T>.CreateSuccess(default, response.StatusCode);
                }

                var data = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(SerializerOptions, cancellationToken);
                if (data is null)
                {
                    return ApiResult<T>.CreateSuccess(default, response.StatusCode);
                }

                if (data.RequestFailed)
                {
                    return ApiResult<T>.CreateFailure(data.ErrorMessage, response.StatusCode);
                }

                return ApiResult<T>.CreateSuccess(data.Data, response.StatusCode);
            }

            var error = await ReadErrorAsync(response, cancellationToken);
            return ApiResult<T>.CreateFailure(error.Message, response.StatusCode);
        }

        private static async Task<ApiResult<PaginatedResponse<T>>> ReadPaginatedResponseAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<T>>(SerializerOptions, cancellationToken);
                if (body is null)
                {
                    return ApiResult<PaginatedResponse<T>>.CreateSuccess(null, response.StatusCode);
                }

                if (body.RequestFailed)
                {
                    return ApiResult<PaginatedResponse<T>>.CreateFailure(body.ErrorMessage, response.StatusCode);
                }

                return ApiResult<PaginatedResponse<T>>.CreateSuccess(body, response.StatusCode);
            }

            var error = await ReadErrorAsync(response, cancellationToken);
            return ApiResult<PaginatedResponse<T>>.CreateFailure(error.Message, response.StatusCode);
        }

        private static async Task<ApiError> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var statusCode = response.StatusCode;
            if (response.Content.Headers.ContentLength == 0)
            {
                return new ApiError(statusCode, response.ReasonPhrase ?? "Unknown error");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return new ApiError(statusCode, response.ReasonPhrase ?? "Unknown error");
            }

            try
            {
                var problem = JsonSerializer.Deserialize<ProblemDetails>(body, SerializerOptions);
                if (problem?.Detail is { Length: > 0 })
                {
                    return new ApiError(statusCode, problem.Detail);
                }

                if (!string.IsNullOrWhiteSpace(problem?.Title))
                {
                    return new ApiError(statusCode, problem!.Title);
                }
            }
            catch
            {
                // ignored - fall back to plain message
            }

            return new ApiError(statusCode, body);
        }
    }

    public class ApiResult<T>
    {
        public bool Success { get; }
        public T? Data { get; }
        public string Message { get; }
        public HttpStatusCode StatusCode { get; }

        private ApiResult(bool success, T? data, string message, HttpStatusCode statusCode)
        {
            Success = success;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public static ApiResult<T> CreateSuccess(T? data, HttpStatusCode statusCode) => new(true, data, string.Empty, statusCode);
        public static ApiResult<T> CreateFailure(string message, HttpStatusCode statusCode) => new(false, default, message, statusCode);
    }

    public record ApiError(HttpStatusCode StatusCode, string Message);

    public record ProblemDetails(string? Title, string? Detail, int? Status, string? Instance, string? Type);
}
