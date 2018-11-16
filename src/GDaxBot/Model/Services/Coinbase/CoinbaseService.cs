using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Data;
using GDaxBot.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDaxBot.Coinbase.Model.Services.Coinbase
{
    class CoinbaseService : ICoinbaseService
    {
        private readonly CoinbaseProClient _cliente;
        GDaxBotDbContext context;
        DateTime LastNMotificationCheck;
        List<Producto> _productos;
        public event CoinbaseApiEventHandler AcctionNeeded;

        public CoinbaseService(IConfiguration config, GDaxBotDbContext context)
        {
            var authenticator = new Authenticator(config.GetValue<string>("Settings:CoinbaseKey"), config.GetValue<string>("Settings:CoinbaseSecret"), config.GetValue<string>("Settings:CoinbasePassword"));

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            this.context = context;
            _productos = context.Productos.ToList();
        }

        public async void CheckProducts()
        {
            //Obtencion de datos
            List<Registro> registros = new List<Registro>();
            foreach (var producto in _productos)
            {
                var registro = new Registro();
                registro.IdProducto = producto.IdProducto;
                registro.Fecha = DateTime.Now;
                try
                {
                    var res = await _cliente.ProductsService.GetProductTickerAsync((ProductType)producto.IdProducto);
                    registro.Valor = res.Price;
                }
                catch //En caso de error registro menos 1
                {
                    registro.Valor = -1;
                }
                registros.Add(registro);
            }

            context.WorkInProgress.WaitOne();
            context.Registros.AddRange(registros);
            await context.SaveChangesAsync();
            context.WorkInProgress.Set();
            if ((DateTime.Now - LastNMotificationCheck).TotalMinutes > 1)
            {
                LastNMotificationCheck = DateTime.Now;
                CheckAlerts();
            }
        }

        public async void CheckAlerts()
        {
            context.WorkInProgress.WaitOne();
            var productos = await context.Registros.OrderByDescending(x => x.Fecha).Take(4).Include(x => x.Producto).ToListAsync();
            var usuarios = await context.Usuarios.Where(x => x.LastMessage < DateTime.Now.AddMinutes(-5)).Include(x => x.AjustesProductos).Include(x => x.Sesiones).ToListAsync();
            context.WorkInProgress.Set();
            foreach (var usuario in usuarios)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var producto in productos)
                {
                    var valorMarcado = usuario.AjustesProductos.Where(x => x.IdProducto == producto.IdProducto)
                                                            .First().ValorMarcado;
                    //evito crashes si se marca a 0
                    if (valorMarcado == 0)
                        continue;
                    //evito notificar si ha habido error de lectura
                    if (producto.Valor < 0)
                        continue;
                    var desviacion = ((producto.Valor - valorMarcado) * 100) / valorMarcado;
                    var ajustes = usuario.AjustesProductos.Where(x => x.IdProducto == producto.IdProducto).First();

                    if (desviacion <= ajustes.UmbralInferior || desviacion >= ajustes.UmbralSuperior)
                    {
                        sb.AppendLine($"Revisa {producto.Producto.Nombre}, ha cambiado un {desviacion.ToString("0.00")}%, valor total {producto.Valor.ToString("0.00")}€, valor marcado {ajustes.ValorMarcado.ToString("0.00")}€");
                    }
                }
                if (sb.Length > 0)
                {
                    AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { UsuarioNotifiacion = usuario, Mensaje = sb.ToString() });
                    usuario.LastMessage = DateTime.Now;
                }
            }
        }
    }
}
