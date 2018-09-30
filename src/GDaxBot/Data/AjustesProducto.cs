using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GDaxBot.Data
{
    public class AjustesProducto
    {
        [Key]
        public int IdAjuste { get; set; }

        public int IdUsuario { get; set; }

        public int IdProducto { get; set; }

        public decimal ValorMarcado { get; set; }

        public decimal UmbralInferior { get; set; }

        public decimal UmbralSuperior { get; set; }

        //EFC
        public virtual Usuario Usuario { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
