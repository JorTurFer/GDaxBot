﻿using GDaxBot.Coinbase.Model.Services.Coinbase;
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
                        var umbral = _coinbaseService.GetUmbral(e.Tipo);
                        string message = $"El umbral de notificación para {e.Tipo.ToString().Substring(0, 3).ToUpper()} es ±{umbral}%";
                        _telegramBot.SendMessage(message);
                        break;
                    }
                case TelegramCommands.UmbralSet:
                    {
                        _coinbaseService.SetUmbral(e.Tipo,e.Valor);
                        string message = $"El nuevo umbral de notificación para {e.Tipo.ToString().Substring(0, 3).ToUpper()} es ±{e.Valor}%";
                        _telegramBot.SendMessage(message);
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
            Console.WindowWidth = 100;
            Console.WindowHeight = 6;
            _seguir = true;
            AutoResetEvent Trigger = new AutoResetEvent(true);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (_seguir)
                {
                    Thread.Sleep((60 / _muestrasMinuto) * 1000);
                    Trigger.Set();
                }
            }).Start();

            while (_seguir)
            {
                if (Trigger.WaitOne((60 / _muestrasMinuto) * 1500))
                    _coinbaseService.CheckProducts();
            }
        }

        public void Stop()
        {
            _seguir = false;
        }
    }
}
