using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateStock;
using Application.Features.Products.Commands.UploadImage;
using Application.Features.Products.Queries.GetAllProducts;
using Application.Features.Products.Queries.GetMyProducts;
using Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APi_Presentation.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("my")]
        [Authorize(Roles = "ImportOffice")]
        public async Task<IActionResult> GetMine([FromQuery] GetMyProductsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(result);
        }

     
        [HttpPost]
        [Authorize(Roles = "ImportOffice")]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                result);
        }


        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ImportOffice")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

    
        [HttpPatch("{id:guid}/stock")]
        [Authorize(Roles = "ImportOffice")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockCommand command)
        {
            command.ProductId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id:guid}/image")]
        [Authorize(Roles = "ImportOffice")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile image)
        {
            var result = await _mediator.Send(new UploadProductImageCommand
            {
                ProductId = id,
                Image = image
            });
            return Ok(result);
        }

    
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ImportOffice,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteProductCommand(id));
            return Ok(result);
        }
    }
}