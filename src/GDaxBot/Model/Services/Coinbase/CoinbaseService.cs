using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using GDaxBot.Model.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GDaxBot.Coinbase.Model.Services.Coinbase
{
    class CoinbaseService : ICoinbaseService
    {
        private readonly CoinbaseProClient _cliente;
        private List<Producto> _productos = new List<Producto>();
        private readonly int _muestras;
        public CoinbaseService(IOptions<Settings> secrets)
        {
            var authenticator = new Authenticator(secrets.Value.CoinbaseKey, secrets.Value.CoinbaseSecret, secrets.Value.CoinbasePassword);

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            //Indico el maximo de muestras a almacenar (esto deberia ir al json)
            _muestras = ;

            //Inicio la lista de productos
            List<int> productos = new List<int>();
            productos.Add(1); //BtcEur
            productos.Add(4); //EthEur
            productos.Add(8); //LtcEur
            productos.Add(12); //BchEur
            productos.Add(16); //EtcEur
            foreach (var product in productos)
            {
                _productos.Add(new Producto { Tipo = (ProductType)product });
            }
        }

        public async void CheckProducts()
        {
            foreach (var producto in _productos)
            {
                var res = await _cliente.ProductsService.GetProductTickerAsync(producto.Tipo);
                producto.UltimosPrecios.Insert(0, new Muestra { Valor = res.Price });
                if (producto.UltimosPrecios.Count > _muestras)
                    producto.UltimosPrecios.RemoveAt(_muestras - 1);
                Console.WriteLine($"{producto.Tipo}->{producto.Porcentaje}");
            }
        }

        //public 

        //async void test(IOptions<Settings> secrets)
        //{
        //    //create an authenticator with your apiKey, apiSecret and passphrase


        //    //use one of the services 
        //    var allAccounts = await coinbaseProClient.AccountsService.GetAllAccountsAsync();

        //    foreach (var a in allAccounts)
        //        Console.WriteLine($"{a.Currency}->{a.Balance}");



        //    var res2 = await coinbaseProClient.CurrenciesService.GetAllCurrenciesAsync();

        //    var ase = res2;

        //    var res3 = await coinbaseProClient.ProductsService.GetAllProductsAsync();

        //    var b = res3;

        //}
    }
}
