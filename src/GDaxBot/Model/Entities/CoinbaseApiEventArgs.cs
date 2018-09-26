using CoinbasePro.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Model.Entities
{
    public class CoinbaseApiEventArgs : EventArgs
    {
        public ProductType Tipo { get; set; } 
        public decimal Cambio { get; set; }
        public string Frase { get; set; } //HACK: Cambiar esto por una unidad de tiempo
    }
    public delegate void CoinbaseApiEventHandler(CoinbaseApiEventArgs e);
}
