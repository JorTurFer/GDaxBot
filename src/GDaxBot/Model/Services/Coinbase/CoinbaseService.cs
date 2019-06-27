using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Data;
using GDaxBot.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDaxBot.Coinbase.Model.Services.Coinbase
{
    internal class CoinbaseService : ICoinbaseService
    {
        private readonly CoinbaseProClient _cliente;
        private DateTime LastNMotificationCheck;
        private readonly IServiceProvider _services;
        private readonly List<Producto> _productos;
        public event CoinbaseApiEventHandler AcctionNeeded;

        public CoinbaseService(IConfiguration config, IServiceProvider services)
        {
            _services = services;
            var authenticator = new Authenticator(config.GetValue<string>("Settings:CoinbaseKey"), config.GetValue<string>("Settings:CoinbaseSecret"), config.GetValue<string>("Settings:CoinbasePassword"));

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                _productos = context.Productos.ToList();
            }
        }

        public async void CheckProducts()
        {
            //Obtencion de datos
            var registros = new List<Registro>();
            foreach (var producto in _productos)
            {
                var registro = new Registro
                {
                    IdProducto = producto.IdProducto,
                    Fecha = DateTime.Now
                };
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

            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                context.Registros.AddRange(registros);
                await context.SaveChangesAsync();
            }
            if ((DateTime.Now - LastNMotificationCheck).TotalMinutes > 1)
            {
                LastNMotificationCheck = DateTime.Now;
                CheckAlerts();
            }
        }

        public async void CheckAlerts()
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                var productos = await context.Registros.OrderByDescending(x => x.Fecha).Take(4).Include(x => x.Producto).ToListAsync();
                var usuarios = await context.Usuarios.Where(x => x.LastMessage < DateTime.Now.AddMinutes(-5)).Include(x => x.AjustesProductos).Include(x => x.Sesiones).ToListAsync();

                foreach (var usuario in usuarios)
                {
                    var sb = new StringBuilder();
                    foreach (var producto in productos)
                    {
                        var valorMarcado = usuario.AjustesProductos.Where(x => x.IdProducto == producto.IdProducto)
                                                                .First().ValorMarcado;
                        //evito crashes si se marca a 0
                        if (valorMarcado == 0)
                        {
                            continue;
                        }
                        //evito notificar si ha habido error de lectura
                        if (producto.Valor < 0)
                        {
                            continue;
                        }

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
}
