using GDaxBot.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Coinbase.Model.Services.Telegram
{
    public interface ITelegramBot
    {
        void SendMessage(long idTelegram,string Message);
    }
}
