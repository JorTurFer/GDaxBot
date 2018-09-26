using GDaxBot.Coinbase;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace GDaxBot.Coinbase.Model.Services.Telegram
{
    class TelegramBot : ITelegramBot
    {
        private readonly TelegramBotClient _bot;
        private readonly int _userID;

        // I’ve injected <em>secrets</em> into the constructor as setup in Program.cs
        public TelegramBot(IOptions<Settings> secrets)
        {
            _userID = secrets.Value.UserID;
            _bot = new TelegramBotClient(secrets.Value.TelegramBotKey);
            _bot.OnMessage += _bot_OnMessage;
            _bot.StartReceiving();
            SendMessage("Iniciando los servicios de monitorizacion");
        }

        public async void SendMessage(string Message)
        {
            await _bot.SendTextMessageAsync(_userID, Message);
        }

        private async void _bot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text) return;
            //Si el usuario no esta dado de alta, rechaza la conexion
            if (message.From.Id != _userID)
            {                
                await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Usuario no autorizado");
                return;
            }

            switch (message.Text.Split(' ').First().ToLower())
            {
                case "vender":
                    await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Vender");
                    break;
                case "alive":
                    await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Estoy vivo");
                    break;
                default:
                    await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Envia una orden");
                    break;
            }

        }
    }
}
