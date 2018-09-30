using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GDaxBot.Data;
using Microsoft.EntityFrameworkCore;

namespace GDaxBot.Model.Services.ContextServices
{
    public class ContextService : IContextService
    {
        private Mutex mutex = new Mutex();
        private GDaxBotDbContext context;
        public ContextService(GDaxBotDbContext context)
        {
            this.context = context;
        }

        public void Add<T>(T Item)
        {
            context.Add(Item);
        } 

        public IEnumerable<Producto> GetProductos()
        {
            return context.Productos;
        }

        public IEnumerable<Sesion> GetSesiones()
        {
            return context.Sesiones;
        }

        public Usuario GetUsuarioByName(string Name)
        {
            return context.Usuarios.Where(x => x.Nombre == Name).FirstOrDefault();
        }

        public Task<Usuario> GetUsuarioByNameAsync(string Name)
        {
            return context.Usuarios.Where(x => x.Nombre == Name).FirstOrDefaultAsync();
        }

        public int SaveChanges()
        {
            return context.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public Mutex GetMutex()
        {
            return mutex;
        }
    }
}
