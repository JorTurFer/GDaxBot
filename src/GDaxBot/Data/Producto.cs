using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GDaxBot.Data
{
    public class Producto
    {
        public Producto ()
        {
            Registros = new HashSet<Registro>();
        }
        [Key]
        public int IdProducto { get; set; }

        public string Nombre { get; set; }

        //EFC
        public IEnumerable<Registro> Registros { get; set; }
    }
}
