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
                return await _usuarios.Find(usuario => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al consultar los usuarios", ex);
            }
        }

        public async Task<Usuario> ConsultarUsuario(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                return await _usuarios.Find(usuario => usuario.Id == objectId).FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<Usuario> ActualizarUsuario(string id, Usuario usuario)
        {
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                usuario.Id = objectId;

                await _usuarios.ReplaceOneAsync(u => u.Id == objectId, usuario);
                return usuario;
            }
            return null;
        }

        public async Task<bool> EliminarUsuario(string id)
        {
            if (ObjectId.TryParse(id, out ObjectId objectId))
            {
                var result = await _usuarios.DeleteOneAsync(usuario => usuario.Id == objectId);
                return result.DeletedCount > 0;
            }
            return false;
        }

        public async Task<Usuario> FindUsuarioByCorreoAsync(string correo)
        {
            var usuario = await _usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();

            return usuario;
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
    }
}