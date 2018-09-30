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
            AjustesProductos = new HashSet<AjustesProducto>();
        }
        [Key]
        public int IdProducto { get; set; }

        public string Nombre { get; set; }
        public string Codigo { get; set; }

        //EFC
        public virtual IEnumerable<Registro> Registros { get; set; }
        public virtual IEnumerable<AjustesProducto> AjustesProductos { get; set; }
    }
}
