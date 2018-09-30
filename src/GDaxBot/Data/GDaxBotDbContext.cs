using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Data
{
    public class GDaxBotDbContext : DbContext
    {
        //Constructor con parametros para la configuracion
        public GDaxBotDbContext(DbContextOptions options)
        : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }
    }
}
