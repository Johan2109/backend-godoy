using BackendProjectAPI.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendProjectAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IMongoCollection<Usuario> _usuarios;

        public UsuarioService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("BackendProjectDB");
            _usuarios = database.GetCollection<Usuario>("Usuarios");
        }

        public async Task<Usuario> CrearUsuario(Usuario usuario)
        {
            try
            {
                usuario.Puntaje = CalcularPuntaje(usuario);
                usuario.Clasificacion = ClasificarUsuario(usuario.FechaUltimoAcceso ?? DateTime.UtcNow);

                await _usuarios.InsertOneAsync(usuario);
                return usuario;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario", ex);
            }
        }

        public async Task<List<Usuario>> ConsultarUsuarios()
        {
            try
            {
                return await _usuarios.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al consultar los usuarios", ex);
            }
        }

        public async Task<Usuario> ConsultarUsuario(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return null;

            return await _usuarios.Find(usuario => usuario.Id == objectId).FirstOrDefaultAsync();
        }

        public async Task<Usuario> ActualizarUsuario(string id, Usuario usuario)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return null;

            usuario.Id = objectId;

            return await ActualizarClasificacionYPuntaje(usuario);
        }

        public async Task<bool> EliminarUsuario(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return false;

            var result = await _usuarios.DeleteOneAsync(usuario => usuario.Id == objectId);
            return result.DeletedCount > 0;
        }

        public async Task<Usuario> FindUsuarioByCorreoAsync(string correo)
        {
            return await _usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();
        }

        public async Task<Usuario> LoginUsuario(string correo, string password)
        {
            var usuario = await _usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();

            if (usuario == null)
            {
                return null;
            }

            if (usuario.Password != password)
            {
                return null;
            }

            usuario.FechaUltimoAcceso = DateTime.UtcNow;

            if (usuario.FechaUltimoAcceso > DateTime.UtcNow.AddHours(-12))
            {
                usuario.Clasificacion = "Hechicero";
            }
            else if (usuario.FechaUltimoAcceso > DateTime.UtcNow.AddHours(-48))
            {
                usuario.Clasificacion = "Luchador";
            }
            else if (usuario.FechaUltimoAcceso > DateTime.UtcNow.AddDays(-7))
            {
                usuario.Clasificacion = "Explorador";
            }
            else
            {
                usuario.Clasificacion = "Olvidado";
            }

            await _usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);

            return usuario;
        }

        public async Task<Usuario> ActualizarClasificacionYPuntaje(Usuario usuario)
        {
            usuario.FechaUltimoAcceso ??= DateTime.UtcNow;
            usuario.Clasificacion = ClasificarUsuario(usuario.FechaUltimoAcceso.Value);
            usuario.Puntaje = CalcularPuntaje(usuario);

            await _usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);

            return usuario;
        }

        private string ClasificarUsuario(DateTime fechaUltimoAcceso)
        {
            var horasDesdeUltimoAcceso = (DateTime.UtcNow - fechaUltimoAcceso).TotalHours;

            return horasDesdeUltimoAcceso switch
            {
                <= 12 => "Hechicero",
                <= 48 => "Luchador",
                <= 168 => "Explorador",
                _ => "Olvidado"
            };
        }

        private int CalcularPuntaje(Usuario usuario)
        {
            int puntaje = 0;

            int longitudNombre = usuario.Nombre.Length;
            puntaje += longitudNombre > 10 ? 20 : (longitudNombre >= 5 ? 10 : 0);

            string dominio = usuario.Correo.Split('@').Length > 1 ? usuario.Correo.Split('@')[1] : "";
            puntaje += dominio switch
            {
                "gmail.com" => 40,
                "hotmail.com" => 20,
                _ => 10
            };

            return puntaje;
        }
    }
}
