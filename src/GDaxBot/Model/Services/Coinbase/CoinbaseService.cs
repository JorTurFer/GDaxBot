using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Data;
using GDaxBot.Model.Entities;
using GDaxBot.Model.Services.ContextServices;
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
        IContextService contextService;
        private readonly int _muestras;

        public event CoinbaseApiEventHandler AcctionNeeded;

        public CoinbaseService(IConfiguration config, IContextService contextService)
        {
            this.contextService = contextService;

            var authenticator = new Authenticator(config.GetValue<string>("Settings:CoinbaseKey"), config.GetValue<string>("Settings:CoinbaseSecret"), config.GetValue<string>("Settings:CoinbasePassword"));

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            //Indico el maximo de muestras a almacenar (esto deberia ir al json)
            _muestras = config.GetValue<int>("Settings:MuestrasMinuto") * 1440 * config.GetValue<int>("Settings:DiasAlmacenados");
        }

        public async void CheckProducts()
        {
            //Obtencion de datos
            contextService.GetMutex().WaitOne();
            foreach (var producto in contextService.GetProductos())
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
                contextService.Add(registro);
               
            }
            await contextService.SaveChangesAsync();
            contextService.GetMutex().ReleaseMutex();
        }

        public decimal GetUmbralUp(ProductType tipo)
        {
            return 0;
            //return _productos.Where(x => x.Tipo == tipo).First().UmbralUp;
        }

        public decimal GetUmbralDown(ProductType tipo)
        {
            return 0;
            //return _productos.Where(x => x.Tipo == tipo).First().UmbralDown;
        }

        public void SetUmbral(ProductType tipo, decimal umbral)
        {
            //if (umbral > 0)
            //    _productos.Where(x => x.Tipo == tipo).First().UmbralUp = umbral;
            //else if (umbral < 0)
            //    _productos.Where(x => x.Tipo == tipo).First().UmbralDown = umbral;
            //else
            //{
            //    _productos.Where(x => x.Tipo == tipo).First().UmbralUp = umbral;
            //    _productos.Where(x => x.Tipo == tipo).First().UmbralDown = umbral;
            //}
        }

        public string GetRatio(ProductType tipo)
        {
            StringBuilder sb = new StringBuilder();
            //var producto = _productos.Where(x => x.Tipo == tipo).First();
            //sb.AppendLine($"===={producto.Tipo.ToString().Substring(0, 3).ToUpper()}====");
            //sb.AppendLine($"Valor:{producto.UltimosPrecios[0].Valor.ToString("0.00")} EUR");
            //sb.AppendLine($"Referencia: {producto.ValorMarcado.ToString("0.00")}€");
            //sb.AppendLine($"Desviación: {producto.Marcador.ToString("0.0000")}%");
            //sb.AppendLine($"Hora: {producto.Hora.ToString("0.0000")}%");
            //sb.AppendLine($"12 Horas: {producto.MedioDia.ToString("0.0000")}%");
            //sb.AppendLine($"24 Horas: {producto.Dia.ToString("0.0000")}%");
            return sb.ToString();
        }

        public string GetRatio()
        {
            StringBuilder sb = new StringBuilder();
            //foreach (var producto in _productos)
            //{
            //    sb.AppendLine($"===={producto.Tipo.ToString().Substring(0, 3).ToUpper()}====");
            //    sb.AppendLine($"Valor:{producto.UltimosPrecios[0].Valor.ToString("0.00")} EUR");
            //    sb.AppendLine($"Referencia: {producto.ValorMarcado.ToString("0.00")}€");
            //    sb.AppendLine($"Desviación: {producto.Marcador.ToString("0.0000")}%");
            //    sb.AppendLine($"Hora: {producto.Hora.ToString("0.0000")}%");
            //    sb.AppendLine($"12 Horas: {producto.MedioDia.ToString("0.0000")}%");
            //    sb.AppendLine($"24 Horas: {producto.Dia.ToString("0.0000")}%");
            //    sb.AppendLine();
            //}
            return sb.ToString();
        }

        public decimal SetMarcador(ProductType tipo)
        {
            //var producto = _productos.Where(x => x.Tipo == tipo).First();
            //producto.ValorMarcado = producto.UltimosPrecios[0].Valor;
            //return producto.UltimosPrecios[0].Valor;
            return 0;
        }

        public IEnumerable<ProductType> GetActivosDisponibles()
        {
            foreach (var producto in contextService.GetProductos())
                yield return (ProductType)producto.IdProducto;
        }
    }
}
