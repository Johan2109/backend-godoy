using BackendProjectAPI.Models;
using Microsoft.AspNetCore.Mvc;
using BackendProjectAPI.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BackendProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest("El usuario no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Apellidos) ||
                string.IsNullOrWhiteSpace(usuario.Cedula) ||
                string.IsNullOrWhiteSpace(usuario.Correo) ||
                string.IsNullOrWhiteSpace(usuario.Password))
                return BadRequest("Todos los campos son obligatorios: Nombre, Apellidos, Cédula, Correo y Contraseña.");

            usuario.FechaUltimoAcceso = usuario.FechaUltimoAcceso ?? DateTime.UtcNow;
            usuario.Clasificacion = "Sin clasificación";
            usuario.Puntaje = 0;

            try
            {
                var creado = await _usuarioService.CrearUsuario(usuario);
                return CreatedAtAction(nameof(ConsultarUsuario), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al crear el usuario: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> ConsultarUsuarios()
        {
            try
            {
                var usuarios = await _usuarioService.ConsultarUsuarios();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al consultar los usuarios: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> ConsultarUsuario(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("ID de usuario no válido.");

            try
            {
                var usuario = await _usuarioService.ConsultarUsuario(id);
                if (usuario == null)
                    return NotFound("Usuario no encontrado.");

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al consultar el usuario: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest("El usuario no puede ser nulo.");

            if (!ObjectId.TryParse(id, out _))
                return BadRequest("ID de usuario no válido.");

            try
            {
                var resultado = await _usuarioService.ActualizarUsuario(id, usuario);
                if (resultado == null)
                    return NotFound("Usuario no encontrado.");

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("ID de usuario no válido.");

            try
            {
                var eliminado = await _usuarioService.EliminarUsuario(id);
                if (!eliminado)
                    return NotFound("Usuario no encontrado.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar el usuario: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<Usuario>> LoginUsuario([FromBody] LoginUsuario loginUsuario)
        {
            if (loginUsuario == null ||
                string.IsNullOrWhiteSpace(loginUsuario.Correo) ||
                string.IsNullOrWhiteSpace(loginUsuario.Password))
            {
                return BadRequest("Correo y contraseña son obligatorios.");
            }

            try
            {
                var usuario = await _usuarioService.LoginUsuario(loginUsuario.Correo, loginUsuario.Password);

                if (usuario == null)
                    return Unauthorized("Correo o contraseña incorrectos.");

                usuario.Password = null;

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al iniciar sesión: {ex.Message}");
            }
        }
    }
}
