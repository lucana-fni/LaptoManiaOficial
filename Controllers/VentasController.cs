using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LaptoManiaOficial.Contexto;
using LaptoManiaOficial.Models;
using Microsoft.AspNetCore.Authorization;
using NuGet.Packaging.Signing;

namespace LaptoManiaOficial.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly MiContext _context;

        public VentasController(MiContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var miContext = _context.Ventas.Include(v => v.Cliente).Include(v => v.Equipo).Include(v => v.Usuario);
            return View(await miContext.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Ventas == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Equipo)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
   
            // Llenar ViewData con datos para las listas desplegables en la vista
            // 1. Lista de Clientes: Clave: "ClienteId", Valor: SelectList de Clientes con "Id" como valor y "Ci" como texto
            var clientes = _context.Clientes.ToList();
            var equiposDisponibles = _context.Equipos.Where(e => e.Disponibilidad).ToList();
            var usuarios = _context.Usuarios.ToList();

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Ci");
            ViewData["EquipoId"] = new SelectList(equiposDisponibles, "Id", "Codigo");
            ViewData["UsuarioId"] = new SelectList(usuarios, "Id", "NombreCompleto");

            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venta venta)
        {
            var clientes = _context.Clientes.ToList();
            var equiposDisponibles = _context.Equipos.Where(e => e.Disponibilidad).ToList();
            var usuarios = _context.Usuarios.ToList();

            if (ModelState.IsValid)
            {
                if (!_context.Usuarios.Any(u => u.Id == venta.UsuarioId))
                {
                    // El UsuarioId no es válido
                    ModelState.AddModelError("UsuarioId", "El UsuarioId no es válido");

                    ViewData["ClienteId"] = new SelectList(clientes, "Id", "Ci", venta.ClienteId);
                    ViewData["EquipoId"] = new SelectList(equiposDisponibles, "Id", "Codigo", venta.EquipoId);
                    ViewData["UsuarioId"] = new SelectList(usuarios, "Id", "CorreoElectronico", venta.UsuarioId);

                    return View(venta);
                }

                // Actualizar la disponibilidad del equipo
                var equipo = await _context.Equipos.FindAsync(venta.EquipoId);
                if (equipo != null)
                {
                    equipo.Disponibilidad = false;
                    _context.Update(equipo);
                }

                _context.Add(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Ci", venta.ClienteId);
            ViewData["EquipoId"] = new SelectList(equiposDisponibles, "Id", "Codigo", venta.EquipoId);
            ViewData["UsuarioId"] = new SelectList(usuarios, "Id", "CorreoElectronico", venta.UsuarioId);

            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Ventas == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Ci", venta.ClienteId);
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Codigo", venta.EquipoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "CorreoElectronico", venta.UsuarioId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaVenta,UsuarioId,ClienteId,EquipoId")] Venta venta)
        {
            if (id != venta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venta);
                    if (!_context.Usuarios.Any(u => u.Id == venta.UsuarioId))
                    {
                        // El UsuarioId no es válido
                        ModelState.AddModelError("UsuarioId", "El UsuarioId no es válido");
                        // Puedes manejar este error y devolver a la vista con un mensaje de error.
                        return View(venta);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Ci", venta.ClienteId);
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Codigo", venta.EquipoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "CorreoElectronico", venta.UsuarioId);
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ventas == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Equipo)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ventas == null)
            {
                return Problem("Entity set 'MiContext.Ventas'  is null.");
            }
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                _context.Ventas.Remove(venta);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(int id)
        {
          return (_context.Ventas?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult ReporteMensual()
        {
            List<List<Venta>> ventasPorMes = new List<List<Venta>>();

            for (int mes = 1; mes <= 12; mes++)
            {
                var ventasMes = _context.Ventas
                    .Where(v => v.FechaVenta.Month == mes)
                    .Include(v => v.Equipo) // Incluir información del equipo asociado a la venta
                    .ToList();

                ventasPorMes.Add(ventasMes);
            }

            return View(ventasPorMes);
        }

    }
}
