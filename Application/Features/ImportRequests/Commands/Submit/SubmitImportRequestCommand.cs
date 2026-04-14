using Application.DTOs.ImportRequests;
using Application.ViewModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums.Enums_Models;
using Domain.Exceptions.Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Application.Features.ImportRequests.Commands.Submit
{
    public record SubmitImportRequestCommand(SubmitImportRequestDto Dto) : IRequest<ResponseViewModel<ImportRequestDto>>;

    public class SubmitImportRequestCommandHandler
        : IRequestHandler<SubmitImportRequestCommand, ResponseViewModel<ImportRequestDto>>
    {
        private readonly IGeneralRepository<ImportRequest> _requestRepo;
        private readonly IGeneralRepository<Product> _productRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubmitImportRequestCommandHandler> _logger;

        public SubmitImportRequestCommandHandler(
            IGeneralRepository<ImportRequest> requestRepo,
            IGeneralRepository<Product> productRepo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SubmitImportRequestCommandHandler> logger)
        {
            _requestRepo = requestRepo;
            _productRepo = productRepo;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseViewModel<ImportRequestDto>> Handle(
            SubmitImportRequestCommand request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(
                _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _logger.LogInformation(
                "User {UserId} is submitting a {ShipmentType} import request for Product {ProductId}",
                userId, request.Dto.ShipmentType, request.Dto.ProductId); // ← عدّلنا الـ log

            var product = await _productRepo.GetByIdAsync(request.Dto.ProductId);
            if (product is null)
                throw new NotFoundException("Product not found.", "ImportRequests");

            if (request.Dto.Quantity < product.MinOrderQuantity)
                throw new Domain.Exceptions.ValidationException(
                    "Quantity is below the minimum order quantity.",
                    new Dictionary<string, string[]>
                    {
                        ["Quantity"] = [$"Minimum order quantity is {product.MinOrderQuantity}."]
                    },
                    "ImportRequests");

            if (product.StockQuantity < request.Dto.Quantity)
                throw new Domain.Exceptions.BusinessLogicException(
                    "Requested quantity exceeds available stock.",
                    "ImportRequests");

            var importRequest = new ImportRequest
            {
                UserId = userId,
                ProductId = request.Dto.ProductId,
                Quantity = request.Dto.Quantity,
                TotalWeightKg = request.Dto.Quantity * product.WeightPerUnitKg,
                TotalVolumeCbm = request.Dto.Quantity * product.VolumePerUnitCbm,
                ShipmentType = request.Dto.ShipmentType, // ← السطر الأساسي اللي كان غلط
                Status = RequestStatus.Pending,
                ShippingAddress = request.Dto.ShippingAddress,
                SpecialInstructions = request.Dto.SpecialInstructions,
                RequestedDeliveryDate = request.Dto.RequestedDeliveryDate
            };

            await _requestRepo.AddAsync(importRequest);

            _logger.LogInformation(
                "Import request {RequestId} ({ShipmentType}) created successfully by User {UserId}",
                importRequest.Id, importRequest.ShipmentType, userId);

            var result = await _requestRepo
                .GetAll()
                .Where(r => r.Id == importRequest.Id)
                .ProjectTo<ImportRequestDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return ResponseViewModel<ImportRequestDto>.Success(result!, "Import request submitted successfully.");
        }
    }
}