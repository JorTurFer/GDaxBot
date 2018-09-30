using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Data;
using GDaxBot.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

        public CoinbaseService(IConfiguration config, GDaxBotDbContext context)
        {
            var authenticator = new Authenticator(config.GetValue<string>("Settings:CoinbaseKey"), config.GetValue<string>("Settings:CoinbaseSecret"), config.GetValue<string>("Settings:CoinbasePassword"));

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            this.context = context;
        }

        public async void CheckProducts()
        {
            //Obtencion de datos
            foreach (var producto in context.Productos)
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
                context.Add(registro);

            }
            await context.SaveChangesAsync();
        }

        public async void CheckAlerts()
        {
            var productos = await context.Registros.OrderBy(x => x.Fecha).Take(4).Include(x => x.Producto).ToListAsync();
            foreach (var usuario in context.Usuarios.Include(x=>x.AjustesProductos))
            {
                foreach(var producto in productos)
                {



                }
            }
        }
    }
}
