using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GDaxBot.Data
{
    public class Registro
    {
        [Key]
        public int IdRegistro { get; set; }

        public int IdProducto { get; set; }

        public decimal Valor { get; set; }

        public DateTime Fecha { get; set; }

        //EFC
        public virtual Producto Producto { get; set; }
    }
}
