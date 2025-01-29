using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApiWithRedis.Data;
using ProductApiWithRedis.Models;
using ProductApiWithRedis.Services;

namespace ProductApiWithRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RedisCacheService _cacheService;
        public ProductsController(AppDbContext context, RedisCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        /*
         * This method is responsible for returning all the products stored in the database.
         * It first tries to retrieve the products from the cache (Redis),
         * and if it doesn't find it, it queries the database,
         * stores the result in the cache, and returns the products.
        */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var cachedProducts = await _cacheService.GetCacheAsync<IEnumerable<Product>>("all_products");
            if(cachedProducts != null)
                return Ok(cachedProducts);

            var products = await _context.Products.ToListAsync();
            await _cacheService.SetCacheAsync("all_products", products);
            return Ok(products);
        }

        /*
         * This method returns a single product, identified by id.
         * As in the previous method, it first tries to fetch the product from the cache and,
         * if it doesn't find it, it queries the database,
         * stores the result in the cache and returns the product.
        */
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var cacheKey = $"product_{id}";
            var cachedProduct = await _cacheService.GetCacheAsync<Product>(cacheKey);
            if (cachedProduct != null)
                return Ok(cachedProduct);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            await _cacheService.SetCacheAsync(cacheKey, product);
            return Ok(product);
        }

        /*
         * This method is used to create a new product.
         * It adds the product to the database, and then removes the data stored in the product cache
         * to ensure that the cache reflects the changes.
        */
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveCacheAsync("all_products");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        /*
         * This method is responsible for updating an existing product.
         * It receives the id and the new product object, updates the database,
         * and clears the cache related to that product and the product list.
        */
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _cacheService.RemoveCacheAsync("all_products");
            await _cacheService.RemoveCacheAsync($"product_{id}");

            return NoContent();
        }

        /*
         * This method deletes a product from the database.
         * After deletion, it removes the product from the cache,
         * as well as the full list of products, to ensure that the cached data is up to date.
        */
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null) 
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveCacheAsync("all_products");
            await _cacheService.RemoveCacheAsync($"product_{id}");

            return NoContent();
        }
    }
}
