using Application.ViewModel;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Products.Commands.UploadImage
{
    public class UploadProductImageCommand : IRequest<ResponseViewModel<string>>
    {
        public Guid ProductId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}