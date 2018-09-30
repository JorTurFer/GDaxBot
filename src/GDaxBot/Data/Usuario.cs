﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GDaxBot.Data
{
    public class Usuario
    {
        public Usuario()
        {
            Sesiones = new HashSet<Sesion>();
        }

        [Key]
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }


        //EFC
        public virtual IEnumerable<Sesion> Sesiones { get; set; }
    }
}
