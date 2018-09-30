using GDaxBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GDaxBot.Model.Services.ContextServices
{
    public interface IContextService
    {
        IEnumerable<Sesion> GetSesiones();
        IEnumerable<Producto> GetProductos();

        Usuario GetUsuarioByName(string Name);
        Task<Usuario> GetUsuarioByNameAsync(string Name);

        int SaveChanges();
        Task<int> SaveChangesAsync();

        void Add<T>(T Item);

        Mutex GetMutex();

    }
}
