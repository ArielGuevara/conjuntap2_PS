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
    public class CategoriasController : ControllerBase
    {
        private readonly AppDBContext _context;

        public CategoriasController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            return await _context.Categorias
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.Id == id)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion
                })
                .FirstOrDefaultAsync();

            if (categoria == null) return NotFound();

            return categoria;
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> PostCategoria([FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || string.IsNullOrEmpty(categoriaDto.Nombre) || string.IsNullOrEmpty(categoriaDto.Descripcion))
            {
                return BadRequest("Nombre y Descripción no pueden ser nulos o vacíos.");
            }

            var existingCategoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nombre == categoriaDto.Nombre && c.Descripcion == categoriaDto.Descripcion);

            if (existingCategoria != null)
            {
                return Conflict("Ya existe una categoría con ese nombre y descripción.");
            }

            var categoria = new Categoria
            {
                Nombre = categoriaDto.Nombre,
                Descripcion = categoriaDto.Descripcion
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            categoriaDto.Id = categoria.Id;

            return CreatedAtAction(nameof(GetCategoria), new { id = categoriaDto.Id }, categoriaDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, [FromBody] CategoriaDto categoriaDto)
        {
            if (id != categoriaDto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || string.IsNullOrEmpty(categoriaDto.Nombre) || string.IsNullOrEmpty(categoriaDto.Descripcion))
            {
                return BadRequest("Nombre y Descripción no pueden ser nulos o vacíos.");
            }

            var existingCategoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id != id && c.Nombre == categoriaDto.Nombre && c.Descripcion == categoriaDto.Descripcion);

            if (existingCategoria != null)
            {
                return Conflict("Ya existe una categoría con ese nombre y descripción.");
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Nombre = categoriaDto.Nombre;
            categoria.Descripcion = categoriaDto.Descripcion;

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(id))
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
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            var productosAsociados = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (productosAsociados)
            {
                return Conflict("No se puede eliminar la categoría porque tiene productos asociados.");
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
