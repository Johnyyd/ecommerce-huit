using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Application.Validators.Product;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Huit.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<ProductQueryParams> _queryValidator;

    public ProductsController(
        IProductService productService,
        IValidator<ProductQueryParams> queryValidator)
    {
        _productService = productService;
        _queryValidator = queryValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParams query)
    {
        var validation = await _queryValidator.ValidateAsync(query);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var products = await _productService.GetProductsAsync(query);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { error = "ProductNotFound" });

        return Ok(product);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug)
    {
        var product = await _productService.GetProductBySlugAsync(slug);
        if (product == null)
            return NotFound(new { error = "ProductNotFound" });

        return Ok(product);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _productService.GetBrandsAsync();
        return Ok(brands);
    }
}
