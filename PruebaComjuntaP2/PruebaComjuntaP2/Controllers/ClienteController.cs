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
    public class ClientesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public ClientesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            return await _context.Clientes
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Email = c.Email,
                    Telefono = c.Telefono
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Id == id)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Email = c.Email,
                    Telefono = c.Telefono
                })
                .FirstOrDefaultAsync();

            if (cliente == null) return NotFound();

            return cliente;
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente([FromBody] ClienteDto clienteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == clienteDto.Email);

            if (existingCliente != null)
            {
                return Conflict("Ya existe un cliente con ese correo electrónico.");
            }

            var cliente = new Cliente
            {
                Nombre = clienteDto.Nombre,
                Apellido = clienteDto.Apellido,
                Email = clienteDto.Email,
                Telefono = clienteDto.Telefono
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            clienteDto.Id = cliente.Id; // Set the ID of the newly created cliente

            return CreatedAtAction(nameof(GetCliente), new { id = clienteDto.Id }, clienteDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, [FromBody] ClienteDto clienteDto)
        {
            if (id != clienteDto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Nombre = clienteDto.Nombre;
            cliente.Apellido = clienteDto.Apellido;
            cliente.Email = clienteDto.Email;
            cliente.Telefono = clienteDto.Telefono;

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
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
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var ventasAsociadas = await _context.Ventas.AnyAsync(v => v.ClienteId == id);
            if (ventasAsociadas)
            {
                return Conflict("No se puede eliminar el cliente porque está asociado a una venta.");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
