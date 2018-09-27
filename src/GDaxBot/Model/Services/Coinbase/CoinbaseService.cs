﻿using CoinbasePro;
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

        public event CoinbaseApiEventHandler AcctionNeeded;

        public CoinbaseService(IOptions<Settings> secrets)
        {
            var authenticator = new Authenticator(secrets.Value.CoinbaseKey, secrets.Value.CoinbaseSecret, secrets.Value.CoinbasePassword);

            //create the CoinbasePro client
            _cliente = new CoinbaseProClient(authenticator);

            //Indico el maximo de muestras a almacenar (esto deberia ir al json)
            _muestras = secrets.Value.MuestrasMinuto * 1440 * secrets.Value.DiasAlmacenados;
            //Inicio la lista de productos
            List<int> productos = new List<int>();
            productos.Add(1); //BtcEur
            productos.Add(12); //BchEur
            productos.Add(4); //EthEur
            productos.Add(8); //LtcEur            
            //productos.Add(16); //EtcEur
            foreach (var product in productos)
            {
                _productos.Add(new Producto { Tipo = (ProductType)product, Umbral = secrets.Value.UmbralDisparo });
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
                    if (Math.Abs(producto.Minuto) >= producto.Umbral)
                    {
                        string Frase = $"Revisa {producto.Tipo.ToString().Substring(0, 3).ToUpper()}, ha cambiado un {producto.Minuto.ToString("0.00")}% en el ultimo minuto";
                        AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { Tipo = producto.Tipo, Cambio = producto.Minuto, Frase = Frase });
                        producto.LastMessage = DateTime.Now;
                    }
                    if (Math.Abs(producto.Hora) >= producto.Umbral)
                    {
                        string Frase = $"Revisa {producto.Tipo.ToString().Substring(0, 3).ToUpper()}, ha cambiado un {producto.Hora.ToString("0.00")}% en la ultima hora";
                        AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { Tipo = producto.Tipo, Cambio = producto.Hora, Frase = Frase });
                        producto.LastMessage = DateTime.Now;
                    }
                    if (Math.Abs(producto.MedioDia) >= producto.Umbral)
                    {
                        string Frase = $"Revisa {producto.Tipo.ToString().Substring(0, 3).ToUpper()}, ha cambiado un {producto.MedioDia.ToString("0.00")}% en las ultimas 12 horas";
                        AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { Tipo = producto.Tipo, Cambio = producto.MedioDia, Frase = Frase });
                        producto.LastMessage = DateTime.Now;
                    }
                    if (Math.Abs(producto.Dia) >= producto.Umbral)
                    {
                        string Frase = $"Revisa {producto.Tipo.ToString().Substring(0, 3).ToUpper()}, ha cambiado un {producto.Dia.ToString("0.00")}% en el ultimo dia";
                        AcctionNeeded?.Invoke(new CoinbaseApiEventArgs { Tipo = producto.Tipo, Cambio = producto.Dia, Frase = Frase });
                        producto.LastMessage = DateTime.Now;
                    }
                }
            }
            //Generacion de vistas
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"----------Fecha:{DateTime.Now.ToString("HH:mm:ss")}----------");
            int fila = 1;
            foreach (var producto in _productos)
            {
                int posicion = 0;
                if (producto.UltimosPrecios.Count > _muestras)
                    producto.UltimosPrecios.RemoveAt(_muestras - 1);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(posicion, fila);
                Console.Write($"{producto.Tipo.ToString().Substring(0, 3).ToUpper()} ->");
                string valor = producto.UltimosPrecios[0].Valor.ToString("0.00");
                Console.SetCursorPosition(7 + (7 - valor.Length), fila);
                Console.Write($"{valor} EUR");
                Console.ForegroundColor = producto.Minuto == 0 ? ConsoleColor.White : producto.Minuto > 0 ? ConsoleColor.Green : ConsoleColor.Red;
                posicion += 23;
                Console.SetCursorPosition(posicion, fila);
                string frase = "";
                if (producto.Minuto >= 0)
                    frase += "+";
                frase += producto.Minuto.ToString("0.0000");
                Console.Write($"Minuto:{frase}% ");
                Console.ForegroundColor = producto.Hora == 0 ? ConsoleColor.White : producto.Hora > 0 ? ConsoleColor.Green : ConsoleColor.Red;
                posicion += 18;
                Console.SetCursorPosition(posicion, fila);
                frase = "";
                if (producto.Hora >= 0)
                    frase += "+";
                frase += producto.Hora.ToString("0.0000");
                Console.Write($"Hora:{ frase}%");
                Console.ForegroundColor = producto.MedioDia == 0 ? ConsoleColor.White : producto.MedioDia > 0 ? ConsoleColor.Green : ConsoleColor.Red;
                posicion += 16;
                Console.SetCursorPosition(posicion, fila);
                frase = "";
                if (producto.MedioDia >= 0)
                    frase += "+";
                frase += producto.MedioDia.ToString("0.0000");
                Console.Write($"12 Horas:{ frase}%");
                Console.ForegroundColor = producto.Dia == 0 ? ConsoleColor.White : producto.Dia > 0 ? ConsoleColor.Green : ConsoleColor.Red;
                posicion += 20;
                Console.SetCursorPosition(posicion, fila);
                frase = "";
                if (producto.Dia >= 0)
                    frase += "+";
                frase += producto.Dia.ToString("0.0000");
                Console.WriteLine($"24 Horas:{ frase}%");
                fila++;
            }
        }

        public decimal GetUmbral(ProductType tipo)
        {
            return _productos.Where(x => x.Tipo == tipo).First().Umbral;
        }

        public void SetUmbral(ProductType tipo, decimal umbral)
        {
            _productos.Where(x => x.Tipo == tipo).First().Umbral = umbral;
        }
    }
}
