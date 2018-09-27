using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Model.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace GDaxBot.Model.Services.GDaxBot
{
    public class GDaxBotService : IGDaxBotService
    {
        private readonly ITelegramBot _telegramBot;
        private readonly ICoinbaseService _coinbaseService;

        private bool _seguir = true;
        private readonly int _muestrasMinuto;

        private AutoResetEvent _eventoCierre = new AutoResetEvent(false);

        public GDaxBotService(ITelegramBot telegramBot, ICoinbaseService coinbaseService, IOptions<Settings> secrets)
        {
            _muestrasMinuto = secrets.Value.MuestrasMinuto;
            _telegramBot = telegramBot;
            _telegramBot.AcctionNeeded += _telegramBot_AcctionNeeded;
            _coinbaseService = coinbaseService;
            _coinbaseService.AcctionNeeded += _coinbaseService_AcctionNeeded;
        }

        private void _telegramBot_AcctionNeeded(TelegramBotEventArgs e)
        {
            switch (e.Comando)
            {
                case TelegramCommands.UmbralGet:
                    {
                        var umbralUp = _coinbaseService.GetUmbralUp(e.Tipo);
                        var umbralDown = _coinbaseService.GetUmbralDown(e.Tipo);
                        string message = $"Los umbrales de notificación para {e.Tipo.ToString().Substring(0, 3).ToUpper()} son +{umbralUp}% y {umbralDown}%";
                        _telegramBot.SendMessage(message);
                        break;
                    }
                case TelegramCommands.UmbralSet:
                    {
                        _coinbaseService.SetUmbral(e.Tipo, e.Valor);

                        string message = $"El nuevo umbral de notificación para {e.Tipo.ToString().Substring(0, 3).ToUpper()} es {e.Valor}%";
                        _telegramBot.SendMessage(message);
                    }
                    break;
                case TelegramCommands.RatioAll:
                    {
                        string message = _coinbaseService.GetRatio();
                        _telegramBot.SendMessage(message);
                    }
                    break;
                case TelegramCommands.RatioTipo:
                    {
                        string message = _coinbaseService.GetRatio(e.Tipo);
                        _telegramBot.SendMessage(message);
                    }
                    break;
                case TelegramCommands.MarcadorSetTipo:
                    {
                        var valor = _coinbaseService.SetMarcador(e.Tipo);
                        string message = $"El nuevo valor de referencia para {e.Tipo.ToString().Substring(0, 3).ToUpper()} es {valor.ToString("0.00")}€";
                        _telegramBot.SendMessage(message);
                    }
                    break;
                case TelegramCommands.ActivosDisponibles:
                    {
                        var activos = _coinbaseService.GetActivosDisponibles();
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("===Activos Disponibles===");
                        foreach (var activo in activos)
                            sb.AppendLine(activo.ToString().Substring(0, 3).ToUpper());
                        _telegramBot.SendMessage(sb.ToString());
                    }
                    break;
            }
        }

        private void _coinbaseService_AcctionNeeded(CoinbaseApiEventArgs e)
        {
            _telegramBot.SendMessage(e.Frase);
        }

        public void Start()
        {
            _seguir = true;
            AutoResetEvent Trigger = new AutoResetEvent(true);
            //Disparador de triggers de ciclo
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (_seguir)
                {
                    //Hago la espera con el evento para poder salir
                    _eventoCierre.WaitOne((60 / _muestrasMinuto) * 1000);
                    Trigger.Set();
                }
            }).Start();

            //Ciclo
            while (_seguir)
            {
                if (Trigger.WaitOne((60 / _muestrasMinuto) * 1500))
                    if (_seguir) //Añado este if para no ejecutar el check si estamos saliendo
                        _coinbaseService.CheckProducts();
            }
        }

        public void Stop()
        {
            _seguir = false;
            _eventoCierre.Set();
        }
    }
}
