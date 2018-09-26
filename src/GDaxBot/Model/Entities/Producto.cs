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
            UltimosPrecios = new List<Muestra>();
        }
        public ProductType Tipo { get; set; }

        public decimal Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }

        public List<Muestra> UltimosPrecios { get; set; }

        public decimal Porcentaje
        {
            get
            {
                if (UltimosPrecios.Count >= 2)
                {
                    var ret = (UltimosPrecios[0].Valor - UltimosPrecios[1].Valor) / (decimal)((UltimosPrecios[0].Fecha - UltimosPrecios[1].Fecha).TotalSeconds);
                    return ret;
                }
                else
                    return 0;
            }
        }

    }
}
