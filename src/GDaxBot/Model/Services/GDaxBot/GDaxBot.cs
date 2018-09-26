using GDaxBot.Coinbase.Model.Services.Coinbase;
using GDaxBot.Coinbase.Model.Services.Telegram;
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

        public GDaxBotService(ITelegramBot telegramBot, ICoinbaseService coinbaseService)
        {
            _telegramBot = telegramBot;
            _coinbaseService = coinbaseService;
            _coinbaseService.AcctionNeeded += _coinbaseService_AcctionNeeded;
        }

        private void _coinbaseService_AcctionNeeded(Entities.CoinbaseApiEventArgs e)
        {
            _telegramBot.SendMessage(e.Frase);
        }

        public void Start()
        {
            _seguir = true;
            AutoResetEvent Trigger = new AutoResetEvent(true);
            new Thread(() => 
            {
                Thread.CurrentThread.IsBackground = true;
                while (_seguir)
                {                    
                    Thread.Sleep(15000);
                    Trigger.Set();
                }
            }).Start();

            while (_seguir)
            {
                if (Trigger.WaitOne(75000))
                    _coinbaseService.CheckProducts();
            }
        }

        public void Stop()
        {
            _seguir = false;
        }
    }
}
