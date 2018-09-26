using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Model.Entities
{
    public class Muestra
    {
        public Muestra()
        {
            Fecha = DateTime.Now;
        }

        public DateTime Fecha { get; }
        public decimal Valor { get; set; }
    }
}
