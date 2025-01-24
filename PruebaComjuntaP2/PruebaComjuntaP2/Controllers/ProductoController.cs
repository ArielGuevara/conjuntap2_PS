using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionInventario.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionInventario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDBContext _context;

        public ProductosController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            return await _context.Productos
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    CantidadStock = p.CantidadStock,
                    CategoriaId = p.CategoriaId
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    CantidadStock = p.CantidadStock,
                    CategoriaId = p.CategoriaId
                })
                .FirstOrDefaultAsync();

            if (producto == null) return NotFound();

            return producto;
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> PostProducto([FromBody] ProductoDto productoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (productoDto == null || string.IsNullOrEmpty(productoDto.Nombre) || string.IsNullOrEmpty(productoDto.Descripcion))
            {
                return BadRequest("Nombre y Descripción no pueden ser nulos o vacíos.");
            }

            if (productoDto.Precio <= 0)
            {
                return BadRequest("El precio del producto debe ser mayor a 0.");
            }

            if (productoDto.CantidadStock < 0)
            {
                return BadRequest("La cantidad en stock del producto debe ser mayor o igual a 0.");
            }

            var existingProducto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Nombre == productoDto.Nombre && p.Descripcion == productoDto.Descripcion && p.CategoriaId == productoDto.CategoriaId);

            if (existingProducto != null)
            {
                return Conflict("Ya existe un producto con ese nombre, descripción y categoría.");
            }

            var producto = new Producto
            {
                Nombre = productoDto.Nombre,
                Descripcion = productoDto.Descripcion,
                Precio = (decimal)productoDto.Precio,
                CantidadStock = (int)productoDto.CantidadStock,
                CategoriaId = (int)productoDto.CategoriaId
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            productoDto.Id = producto.Id; // Set the ID of the newly created producto

            return CreatedAtAction(nameof(GetProducto), new { id = productoDto.Id }, productoDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoDto productoDto)
        {
            if (id != productoDto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (productoDto == null || string.IsNullOrEmpty(productoDto.Nombre) || string.IsNullOrEmpty(productoDto.Descripcion))
            {
                return BadRequest("Nombre y Descripción no pueden ser nulos o vacíos.");
            }

            if (productoDto.Precio <= 0)
            {
                return BadRequest("El precio del producto debe ser mayor a 0.");
            }

            if (productoDto.CantidadStock < 0)
            {
                return BadRequest("La cantidad en stock del producto debe ser mayor o igual a 0.");
            }

            var existingProducto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id != id && p.Nombre == productoDto.Nombre && p.Descripcion == productoDto.Descripcion && p.CategoriaId == productoDto.CategoriaId);

            if (existingProducto != null)
            {
                return Conflict("Ya existe un producto con ese nombre, descripción y categoría.");
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Nombre = productoDto.Nombre;
            producto.Descripcion = productoDto.Descripcion;
            producto.Precio = (decimal)productoDto.Precio;
            producto.CantidadStock = (int)productoDto.CantidadStock;
            producto.CategoriaId = (int)productoDto.CategoriaId;

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            var ventasAsociadas = await _context.Ventas.AnyAsync(v => v.ProductoId == id);
            if (ventasAsociadas)
            {
                return Conflict("No se puede eliminar el producto porque está asociado a una venta.");
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
