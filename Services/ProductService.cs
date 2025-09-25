using System.Net;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services
{
    public class ProductService(IProductRepository productRepository) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;

        public async Task<ApiResponseDto<Product>> CreateProductAsync(ApiRequestDto<Product> request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request.Payload == null)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.BadRequest,
                        ErrorMessage = "Product data is required"
                    };
                }

                var result = await _productRepository.AddAsync(request.Payload, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.Created,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to create product: {ex.Message}"
                };
            }
        }

        public async Task<PaginatedResponseDto<List<Product>>> GetProductsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.GetAllProductsAsync(page, pageSize, cancellationToken);
                if (result.RequestFailed)
                {
                    return new PaginatedResponseDto<List<Product>>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage,
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                return new PaginatedResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };
            }
            catch (Exception ex)
            {
                return new PaginatedResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to retrieve products: {ex.Message}",
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        public async Task<ApiResponseDto<Product?>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<Product?>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<Product?>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product?>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to retrieve product: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<Product>>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.GetProductsByCategoryIdAsync(categoryId, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<List<Product>>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to retrieve products: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<Product>> UpdateProductAsync(int id, ApiRequestDto<Product> request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request.Payload == null)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.BadRequest,
                        ErrorMessage = "Product data is required"
                    };
                }

                // First check if product exists
                var existingResult = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (existingResult.RequestFailed || existingResult.Data == null)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        ErrorMessage = "Product not found"
                    };
                }

                // Update the product
                var productToUpdate = request.Payload;
                productToUpdate.ProductId = id; // Ensure ProductId is set

                // Preserve the existing price to prevent price updates
                productToUpdate.Price = existingResult.Data!.Price;

                var result = await _productRepository.UpdateAsync(productToUpdate, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to update product: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.DeleteAsync(id, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<bool>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<bool>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to delete product: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<List<Product>>> GetDeletedProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.GetDeletedProductsAsync(cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<List<Product>>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to retrieve deleted products: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponseDto<bool>> RestoreProductAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productRepository.RestoreAsync(id, cancellationToken);
                if (result.RequestFailed)
                {
                    return new ApiResponseDto<bool>
                    {
                        RequestFailed = true,
                        ResponseCode = result.ResponseCode,
                        ErrorMessage = result.ErrorMessage
                    };
                }

                return new ApiResponseDto<bool>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to restore product: {ex.Message}"
                };
            }
        }
    }
}
