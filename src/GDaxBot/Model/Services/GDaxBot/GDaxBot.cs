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

        public GDaxBotService(ITelegramBot telegramBot, ICoinbaseService coinbaseService)
        {
            _telegramBot = telegramBot;
            _coinbaseService = coinbaseService;
        }

        public void DoWork()
        {
            while (true)
            {
                Debug.WriteLine("Iniciando ciclo");
                _coinbaseService.CheckProducts();

                Thread.Sleep(5000);
            }
        }
    }
}
