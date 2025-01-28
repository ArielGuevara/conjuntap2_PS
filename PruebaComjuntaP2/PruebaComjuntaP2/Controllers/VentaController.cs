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
    public class VentasController : ControllerBase
    {
        private readonly AppDBContext _context;

        public VentasController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
        {
            return await _context.Ventas
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    ProductoId = v.ProductoId,
                    Cantidad = v.Cantidad,
                    FechaVenta = v.FechaVenta,
                    Total = v.Total,
                    ClienteId = v.ClienteId
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Where(v => v.Id == id)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    ProductoId = v.ProductoId,
                    Cantidad = v.Cantidad,
                    FechaVenta = v.FechaVenta,
                    Total = v.Total,
                    ClienteId = v.ClienteId
                })
                .FirstOrDefaultAsync();

            if (venta == null) return NotFound();

            return venta;
        }

        [HttpPost]
        public async Task<ActionResult<VentaDto>> PostVenta([FromBody] VentaDto ventaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentDate = DateTime.Today;
            if ((ventaDto.FechaVenta?.Date ?? DateTime.UtcNow) != currentDate)
            {
                return BadRequest("La fecha de la venta debe ser la fecha del día actual.");
            }

            var existingVenta = await _context.Ventas
                .FirstOrDefaultAsync(v => v.ProductoId == ventaDto.ProductoId && v.Cantidad == ventaDto.Cantidad && v.FechaVenta == ventaDto.FechaVenta);

            if (existingVenta != null)
            {
                return Conflict("Ya existe una venta con el mismo producto, cantidad y fecha/hora.");
            }

            var venta = new Venta
            {
                ProductoId = ventaDto.ProductoId ?? 0,
                Cantidad = ventaDto.Cantidad ?? 0,
                FechaVenta = ventaDto.FechaVenta ?? DateTime.UtcNow.Date,
                Total = ventaDto.Total ?? 0,
                ClienteId = ventaDto.ClienteId ?? 0
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            ventaDto.Id = venta.Id; 

            return CreatedAtAction(nameof(GetVenta), new { id = ventaDto.Id }, ventaDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, [FromBody] VentaDto ventaDto)
        {
            if (id != ventaDto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }

            venta.ProductoId = ventaDto.ProductoId ?? 0;
            venta.Cantidad = ventaDto.Cantidad ?? 0;
            venta.FechaVenta = ventaDto.FechaVenta ?? DateTime.UtcNow.Date;
            venta.Total = ventaDto.Total ?? 0;
            venta.ClienteId = ventaDto.ClienteId ?? 0;

            _context.Entry(venta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VentaExists(id))
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

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound();
            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
