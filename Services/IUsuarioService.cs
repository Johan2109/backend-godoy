using BackendProjectAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendProjectAPI.Services
{
    public interface IUsuarioService
    {
        Task<Usuario> CrearUsuario(Usuario usuario);
        Task<List<Usuario>> ConsultarUsuarios();
        Task<Usuario> ConsultarUsuario(string id);
        Task<Usuario> FindUsuarioByCorreoAsync(string correo);
        Task<Usuario> ActualizarUsuario(string id, Usuario usuario);
        Task<bool> EliminarUsuario(string id);
        Task<Usuario> LoginUsuario(string correo, string password);
    }
}