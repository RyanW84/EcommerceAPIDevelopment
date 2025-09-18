using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ApiResponseDto<Category>> CreateCategoryAsync(
        ApiRequestDto<Category> request,
        CancellationToken cancellationToken = default
    )
    {
        var payload = request?.Payload;
        if (payload is null || string.IsNullOrWhiteSpace(payload.Name))
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Category name is required.",
            };
        }

        var name = payload.Name.Trim();
        if (await _categoryRepository.CategoryExistsAsync(name, cancellationToken))
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = $"Category with name '{name}' already exists.",
            };
        }

        var category = new Category
        {
            Name = name,
            Description = payload.Description?.Trim() ?? string.Empty,
        };

        var addResult = await _categoryRepository.AddAsync(category, cancellationToken);

        if (addResult.RequestFailed)
        {
            return addResult;
        }

        return new ApiResponseDto<Category>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.Created,
            ErrorMessage = string.Empty,
            Data = addResult.Data
        };
    }

    public async Task<ApiResponseDto<Category>> GetCategoryAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var repoResult = await _categoryRepository.GetByIdAsync(id, cancellationToken);

        if (repoResult.RequestFailed || repoResult.Data is null)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Category with id {id} not found.",
            };
        }

        return new ApiResponseDto<Category>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = repoResult.Data
        };
    }

    public async Task<ApiResponseDto<List<Category>>> GetAllCategoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var repoResult = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);

        if (repoResult.RequestFailed)
        {
            return new ApiResponseDto<List<Category>>
            {
                RequestFailed = true,
                ResponseCode = repoResult.ResponseCode,
                ErrorMessage = repoResult.ErrorMessage,
                Data = []
            };
        }

        return new ApiResponseDto<List<Category>>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = repoResult.Data ?? []
        };
    }

    public async Task<ApiResponseDto<Category>> UpdateCategoryAsync(
        int id,
        ApiRequestDto<Category> request,
        CancellationToken cancellationToken = default
    )
    {
        var payload = request?.Payload;
        if (payload is null)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Request payload is required.",
            };
        }

        var repoResult = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        var existing = repoResult?.Data;
        if (existing is null)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Category with id {id} not found.",
            };
        }

        var updatedCategory = new Category
        {
            CategoryId = existing.CategoryId,
            Name = string.IsNullOrWhiteSpace(payload.Name) ? existing.Name : payload.Name.Trim(),
            Description = payload.Description is null ? existing.Description : payload.Description.Trim(),
            Products = existing.Products,
            Sales = existing.Sales
        };

        // If name changed, ensure uniqueness (allow same category)
        if (!string.Equals(updatedCategory.Name, existing.Name, StringComparison.OrdinalIgnoreCase))
        {
            var byName = await _categoryRepository.GetByNameAsync(updatedCategory.Name, cancellationToken);
            if (byName.RequestFailed == false && byName.Data is not null && byName.Data.CategoryId != existing.CategoryId)
            {
                return new ApiResponseDto<Category>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.Conflict,
                    ErrorMessage = $"Category with name '{updatedCategory.Name}' already exists.",
                };
            }
        }

        var updateResult = await _categoryRepository.UpdateAsync(updatedCategory, cancellationToken);
        if (updateResult.RequestFailed)
        {
            return updateResult;
        }

        return new ApiResponseDto<Category>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = updateResult.Data
        };
    }

    public async Task<ApiResponseDto<bool>> DeleteCategoryAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var repoResult = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        var existing = repoResult?.Data;
        if (existing is null)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Category with id {id} not found.",
                Data = false,
            };
        }

        var delResult = await _categoryRepository.DeleteAsync(existing, cancellationToken);
        if (delResult.RequestFailed)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = delResult.ResponseCode,
                ErrorMessage = delResult.ErrorMessage,
                Data = false
            };
        }

        return new ApiResponseDto<bool>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = true,
        };
    }

    public async Task<ApiResponseDto<Category>> GetCategoryByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Name is required.",
            };
        }

        var repoResult = await _categoryRepository.GetByNameAsync(name.Trim(), cancellationToken);
        if (repoResult.RequestFailed || repoResult.Data is null)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                ErrorMessage = $"Category '{name}' not found.",
            };
        }

        return new ApiResponseDto<Category>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = repoResult.Data,
        };
    }
}
