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
        private readonly IMongoCollection<Usuario> _usuarios;

        public UsuariosController(IUsuarioService usuarioService, IMongoClient mongoClient)
        {
            _usuarioService = usuarioService;
            var database = mongoClient.GetDatabase("BackendProjectDB");
            _usuarios = database.GetCollection<Usuario>("Usuarios");
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest("El usuario no puede ser nulo.");

            if (string.IsNullOrEmpty(usuario.Nombre) || string.IsNullOrEmpty(usuario.Apellidos) ||
                string.IsNullOrEmpty(usuario.Cedula) || string.IsNullOrEmpty(usuario.Correo) ||
                string.IsNullOrEmpty(usuario.Password))
                return BadRequest("Nombre, Apellidos, Cédula, Correo Electrónico y Contraseña son campos obligatorios.");

            if (string.IsNullOrEmpty(usuario.Clasificacion))
            {
                usuario.Clasificacion = "Sin clasificación";
            }

            usuario.FechaUltimoAcceso = DateTime.UtcNow;

            var creado = await _usuarioService.CrearUsuario(usuario);
            return CreatedAtAction(nameof(ConsultarUsuario), new { id = creado.Id }, creado);
        }

        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> ConsultarUsuarios()
        {
            var usuarios = await _usuarioService.ConsultarUsuarios();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> ConsultarUsuario(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return BadRequest("ID de usuario no válido.");
                }

                var usuario = await _usuarioService.ConsultarUsuario(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado.");
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest("El usuario no puede ser nulo.");

            var resultado = await _usuarioService.ActualizarUsuario(id, usuario);
            if (resultado == null)
                return NotFound("Usuario no encontrado.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var eliminado = await _usuarioService.EliminarUsuario(id);
            if (!eliminado)
                return NotFound("Usuario no encontrado.");

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<Usuario>> LoginUsuario([FromBody] LoginUsuario loginUsuario)
        {
            var usuario = await _usuarioService.FindUsuarioByCorreoAsync(loginUsuario.Correo);

            if (usuario == null || usuario.Password != loginUsuario.Password)
                return Unauthorized("Correo o contraseña incorrectos.");

            var fechaActual = DateTime.UtcNow;
            usuario.FechaUltimoAcceso = fechaActual;

            if (usuario.FechaUltimoAcceso > fechaActual.AddHours(-12))
            {
                usuario.Clasificacion = "Hechicero";
            }
            else if (usuario.FechaUltimoAcceso > fechaActual.AddHours(-48))
            {
                usuario.Clasificacion = "Luchador";
            }
            else if (usuario.FechaUltimoAcceso > fechaActual.AddDays(-7))
            {
                usuario.Clasificacion = "Explorador";
            }
            else
            {
                usuario.Clasificacion = "Olvidado";
            }

            await _usuarioService.ActualizarUsuario(usuario.Id.ToString(), usuario);

            return Ok(usuario);
        }
    }
}