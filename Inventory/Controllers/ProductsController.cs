using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Repository;
using Inventory.Request;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(ProductRequest productRequest)
        {
            
            try
            {
                var product = new Product(productRequest.Name, productRequest.Category, productRequest.Amount, productRequest.Description, productRequest.BaseDiscountInPercentage);


                if (_context.Products.Any(p => p.Name == product.Name))
                {
                    return Conflict("A product with the same name already exists.");
                }
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {               
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, ProductRequest productRequest)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                else if(_context.Products.Any(p => p.Name == productRequest.Name && p.Id != id))
                {
                    return Conflict("A product with the same name already exists.");
                }
                else
                {
                    existingProduct.Name = productRequest.Name;
                    existingProduct.Category = productRequest.Category;
                    existingProduct.Amount = productRequest.Amount;
                    existingProduct.Description = productRequest.Description;
                    existingProduct.BaseDiscountInPercentage = productRequest.BaseDiscountInPercentage;
                    await _context.SaveChangesAsync();

                    return Ok(existingProduct);

                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }




        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}
