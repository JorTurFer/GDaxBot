using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Model.Entities;
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
        private List<Producto> _productos = new List<Producto>();
        private readonly int _muestras;

        public event CoinbaseApiEventHandler AcctionNeeded;

        public CoinbaseService(IConfiguration config)
        {
            var authenticator = new Authenticator(config.GetValue<string>("Settings:CoinbaseKey"), config.GetValue<string>("Settings:CoinbaseSecret"), config.GetValue<string>("Settings:CoinbasePassword"));

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            //Indico el maximo de muestras a almacenar (esto deberia ir al json)
            _muestras = config.GetValue<int>("Settings:MuestrasMinuto") * 1440 * config.GetValue<int>("Settings:DiasAlmacenados");
            //Inicio la lista de productos
            List<int> productos = new List<int>();
            productos.Add(1); //BtcEur
            productos.Add(12); //BchEur
            productos.Add(4); //EthEur
            productos.Add(8); //LtcEur            
            //productos.Add(16); //EtcEur
            foreach (var product in productos)
            {
                _productos.Add(new Producto { Tipo = (ProductType)product, UmbralUp = config.GetValue<int>("Settings:UmbralDisparo"), UmbralDown = -config.GetValue<int>("Settings:UmbralDisparo") });
            }
        }

        public async void CheckProducts()
        {
            //Obtencion de datos
            foreach (var producto in _productos)
            {
                var res = await _cliente.ProductsService.GetProductTickerAsync(producto.Tipo);
                producto.UltimosPrecios.Insert(0, new Muestra { Valor = res.Price });

                if ((DateTime.Now - producto.LastMessage).TotalMinutes > 5)
                {
                    if (producto.Marcador >= producto.UmbralUp || producto.Marcador <= producto.UmbralDown)
                    {
                        string Frase = $"Revisa {producto.Tipo.ToString().Substring(0, 3).ToUpper()}, ha cambiado un {producto.Marcador.ToString("0.00")}% del valor marcado";
                        AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { Tipo = producto.Tipo, Cambio = producto.Marcador, Frase = Frase });
                        producto.LastMessage = DateTime.Now;
                    }                    
                }
            }               
        }

        public decimal GetUmbralUp(ProductType tipo)
        {
            return _productos.Where(x => x.Tipo == tipo).First().UmbralUp;
        }

        public decimal GetUmbralDown(ProductType tipo)
        {
            return _productos.Where(x => x.Tipo == tipo).First().UmbralDown;
        }

        public void SetUmbral(ProductType tipo, decimal umbral)
        {
            if (umbral > 0)
                _productos.Where(x => x.Tipo == tipo).First().UmbralUp = umbral;
            else if (umbral < 0)
                _productos.Where(x => x.Tipo == tipo).First().UmbralDown = umbral;
            else
            {
                _productos.Where(x => x.Tipo == tipo).First().UmbralUp = umbral;
                _productos.Where(x => x.Tipo == tipo).First().UmbralDown = umbral;
            }
        }

        public string GetRatio(ProductType tipo)
        {
            StringBuilder sb = new StringBuilder();
            var producto = _productos.Where(x => x.Tipo == tipo).First();
            sb.AppendLine($"===={producto.Tipo.ToString().Substring(0, 3).ToUpper()}====");
            sb.AppendLine($"Valor:{producto.UltimosPrecios[0].Valor.ToString("0.00")} EUR");
            sb.AppendLine($"Referencia: {producto.ValorMarcado.ToString("0.00")}€");
            sb.AppendLine($"Desviación: {producto.Marcador.ToString("0.0000")}%");
            sb.AppendLine($"Hora: {producto.Hora.ToString("0.0000")}%");
            sb.AppendLine($"12 Horas: {producto.MedioDia.ToString("0.0000")}%");
            sb.AppendLine($"24 Horas: {producto.Dia.ToString("0.0000")}%");
            return sb.ToString();
        }

        public string GetRatio()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var producto in _productos)
            {
                sb.AppendLine($"===={producto.Tipo.ToString().Substring(0, 3).ToUpper()}====");
                sb.AppendLine($"Valor:{producto.UltimosPrecios[0].Valor.ToString("0.00")} EUR");
                sb.AppendLine($"Referencia: {producto.ValorMarcado.ToString("0.00")}€");
                sb.AppendLine($"Desviación: {producto.Marcador.ToString("0.0000")}%");
                sb.AppendLine($"Hora: {producto.Hora.ToString("0.0000")}%");
                sb.AppendLine($"12 Horas: {producto.MedioDia.ToString("0.0000")}%");
                sb.AppendLine($"24 Horas: {producto.Dia.ToString("0.0000")}%");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public decimal SetMarcador(ProductType tipo)
        {
            var producto = _productos.Where(x => x.Tipo == tipo).First();
            producto.ValorMarcado = producto.UltimosPrecios[0].Valor;
            return producto.UltimosPrecios[0].Valor;
        }

        public IEnumerable<ProductType> GetActivosDisponibles()
        {
            foreach (var producto in _productos)
                yield return producto.Tipo;
        }
    }
}
