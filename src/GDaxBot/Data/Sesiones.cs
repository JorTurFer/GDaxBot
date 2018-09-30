using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GDaxBot.Data
{
    public class Sesion
    {
        [Key]
        public int IdSesion { get; set; }

        public int IdUsuario { get; set; }

        public int IdTelegram { get; set; }

        //EFC
        public virtual Usuario Usuario { get; set; }
    }
}
