using CoinbasePro.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Model.Entities
{
    public class Producto
    {
        public Producto()
        {
            UltimosPrecios = new List<decimal>();
        }
        public ProductType Tipo { get; set; }

        public decimal Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }

        public List<decimal> UltimosPrecios { get; set; }
    }
}
