using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
using GDaxBot.Data;
using GDaxBot.Model.Entities;
using Microsoft.Extensions.Configuration;
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

        public GDaxBotService(ITelegramBot telegramBot, ICoinbaseService coinbaseService, IConfiguration config)
        {
            _muestrasMinuto = config.GetValue<int>("Settings:MuestrasMinuto"); 
            _telegramBot = telegramBot;
            _coinbaseService = coinbaseService;
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
