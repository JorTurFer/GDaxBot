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

        public decimal UmbralUp { get; set; }
        public decimal UmbralDown { get; set; }

        public decimal ValorMarcado { get; set; } = 0;

        public List<Muestra> UltimosPrecios { get; set; }

        decimal GetPorcentaje(int minutos)
        {
            minutos = minutos * 6;
            var valorAnterior = UltimosPrecios[UltimosPrecios.Count > minutos ? minutos : UltimosPrecios.Count - 1].Valor;
            var ret = ((UltimosPrecios[0].Valor - valorAnterior) * 100) / valorAnterior;
            return ret;
        }
        decimal GetPorcentajeMarcador()
        {
            if (ValorMarcado == 0)
                ValorMarcado = UltimosPrecios[0].Valor;
            var ret = ((UltimosPrecios[0].Valor - ValorMarcado) * 100) / ValorMarcado;
            return ret;
        }

        public decimal Marcador { get => GetPorcentajeMarcador(); }

        public decimal Hora { get => GetPorcentaje(60); }

        public decimal MedioDia { get => GetPorcentaje(60 * 12); }

        public decimal Dia { get => GetPorcentaje(60 * 24); }

        public DateTime LastMessage { get; set; }

    }
}
