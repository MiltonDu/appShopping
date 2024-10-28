using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Entidades;
using System.Net;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShoppingController(ApplicationDbContext context)
        {
            _context = context;
        }

        private IActionResult HandleError(string message, int attemptCount)
        {
            return Problem(
                detail: message,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Error",
                instance: $"Attempt {attemptCount}");
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            int attemptCount = 0;

            while (attemptCount < 3)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
                }
                catch (Exception ex)
                {
                    attemptCount++;
                    if (attemptCount >= 3)
                    {
                        return HandleError($"Failed to add product: {ex.Message}", attemptCount);
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.Include(p => p.GroupAttributes)
                                                 .ThenInclude(g => g.Attributes)
                                                 .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            int attemptCount = 0;

            while (attemptCount < 3)
            {
                try
                {
                    if (id != product.ProductId || !ModelState.IsValid)
                    {
                        return BadRequest();
                    }

                    _context.Entry(product).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    attemptCount++;
                }
                catch (Exception ex)
                {
                    attemptCount++;
                    if (attemptCount >= 3)
                    {
                        return HandleError($"Failed to update product: {ex.Message}", attemptCount);
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int attemptCount = 0;

            while (attemptCount < 3)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (Exception ex)
                {
                    attemptCount++;
                    if (attemptCount >= 3)
                    {
                        return HandleError($"Failed to delete product: {ex.Message}", attemptCount);
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPatch("{id}/increase")]
        public async Task<IActionResult> IncreaseQuantity(int id, [FromBody] int incrementBy)
        {
            int attemptCount = 0;

            while (attemptCount < 3)
            {
                try
                {
                    var product = await _context.Products.Include(p => p.GroupAttributes)
                                                         .ThenInclude(g => g.QuantityInformation)
                                                         .FirstOrDefaultAsync(p => p.ProductId == id);
                    if (product == null)
                        return NotFound();

                    // Incrementar la cantidad
                    if (product.GroupAttributes != null && product.GroupAttributes.Count > 0)
                    {
                        var groupAttribute = product.GroupAttributes.First(); // Puedes ajustar esto según tu lógica
                        groupAttribute.QuantityInformation.GroupAttributeQuantity += incrementBy;

                        await _context.SaveChangesAsync();
                        return NoContent();
                    }

                    return BadRequest("No se encontraron atributos de grupo.");
                }
                catch (Exception ex)
                {
                    attemptCount++;
                    if (attemptCount >= 3)
                    {
                        return HandleError($"Failed to increase quantity: {ex.Message}", attemptCount);
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPatch("{id}/decrease")]
        public async Task<IActionResult> DecreaseQuantity(int id, [FromBody] int decrementBy)
        {
            int attemptCount = 0;

            while (attemptCount < 3)
            {
                try
                {
                    var product = await _context.Products.Include(p => p.GroupAttributes)
                                                         .ThenInclude(g => g.QuantityInformation)
                                                         .FirstOrDefaultAsync(p => p.ProductId == id);
                    if (product == null)
                        return NotFound();

                    // Disminuir la cantidad
                    if (product.GroupAttributes != null && product.GroupAttributes.Count > 0)
                    {
                        var groupAttribute = product.GroupAttributes.First(); // Puedes ajustar esto según tu lógica
                        if (groupAttribute.QuantityInformation.GroupAttributeQuantity - decrementBy < 0)
                        {
                            return BadRequest("No se puede reducir la cantidad a un número negativo.");
                        }

                        groupAttribute.QuantityInformation.GroupAttributeQuantity -= decrementBy;

                        await _context.SaveChangesAsync();
                        return NoContent();
                    }

                    return BadRequest("No se encontraron atributos de grupo.");
                }
                catch (Exception ex)
                {
                    attemptCount++;
                    if (attemptCount >= 3)
                    {
                        return HandleError($"Failed to decrease quantity: {ex.Message}", attemptCount);
                    }
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.Include(p => p.GroupAttributes).ThenInclude(g => g.Attributes).ToListAsync();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
