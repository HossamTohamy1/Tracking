using Application.ViewModel;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Commands.UploadImage
{
    public class UploadProductImageCommandHandler
        : IRequestHandler<UploadProductImageCommand, ResponseViewModel<string>>
    {
        private readonly IGeneralRepository<Product> _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadProductImageCommandHandler> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UploadProductImageCommandHandler(
            IGeneralRepository<Product> repository,
            IWebHostEnvironment environment,
            ILogger<UploadProductImageCommandHandler> logger)
        {
            _repository = repository;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ResponseViewModel<string>> Handle(
            UploadProductImageCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Uploading image for product {ProductId}", request.ProductId);

            var product = await _repository.GetByIdAsync(request.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found for image upload.", request.ProductId);
                throw new BusinessLogicException(
                    $"Product with id '{request.ProductId}' was not found.",
                    "Products",
                    AppErrorCode.NotFound);
            }

            // Validate extension
            var ext = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ValidationException(
                    "Invalid image format.",
                    new Dictionary<string, string[]>
                    {
                        { "Image", new[] { $"Allowed formats: {string.Join(", ", AllowedExtensions)}" } }
                    });

            // Validate size
            if (request.Image.Length > MaxFileSizeBytes)
                throw new ValidationException(
                    "Image too large.",
                    new Dictionary<string, string[]>
                    {
                        { "Image", new[] { "Maximum allowed size is 5 MB." } }
                    });

            // Save to wwwroot/products/
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await request.Image.CopyToAsync(stream, cancellationToken);

            var relativeUrl = $"/products/{fileName}";

            // Delete old image from disk if exists
            if (!string.IsNullOrWhiteSpace(product.MainImageUrl))
            {
                var oldPath = Path.Combine(_environment.WebRootPath,
                    product.MainImageUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            product.MainImageUrl = relativeUrl;
            product.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdatePartialAsync(product,
                nameof(Product.MainImageUrl),
                nameof(Product.UpdatedAt));

            _logger.LogInformation("Image uploaded for product {ProductId} → {Url}",
                request.ProductId, relativeUrl);

            return ResponseViewModel<string>.Success(relativeUrl, "Image uploaded successfully.");
        }
    }
}