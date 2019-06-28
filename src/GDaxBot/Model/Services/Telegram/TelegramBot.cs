using GDaxBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GDaxBot.Coinbase.Model.Services.Telegram
{
    internal class TelegramBot : ITelegramBot
    {
        private readonly TelegramBotClient _bot;
        private readonly IServiceProvider _services;
        ILogger<TelegramBot> _logger;
        private readonly string botPassword;
        // I’ve injected <em>secrets</em> into the constructor as setup in Program.cs
        public TelegramBot(IConfiguration config, IServiceProvider services, ILogger<TelegramBot> logger)
        {
            _services = services;
            _logger = logger;
            botPassword = config.GetValue<string>("Settings:TelegramBotPassword");
            _bot = new TelegramBotClient(config.GetValue<string>("Settings:TelegramBotKey"));
            _bot.OnMessage += _bot_OnMessage;
            _bot.OnMessageEdited += _bot_OnMessageEdited;
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                foreach (var sesion in context.Sesiones.Include(x => x.Usuario))
                {
                    SendMessage(sesion.IdTelegram, $"{sesion.Usuario.Nombre} , acabamos de reiniciar los servicios");
                }
            }
            _bot.StartReceiving();
        }

        private void _bot_OnMessageEdited(object sender, MessageEventArgs e)
        {
            ProcessMessage(e);
        }

        private void _bot_OnMessage(object sender, MessageEventArgs e)
        {
            ProcessMessage(e);
        }
        public async void SendMessage(long ChatID, string Message)
        {
            await _bot.SendTextMessageAsync(ChatID, Message);
        }

        private async void ProcessMessage(MessageEventArgs e)
        {
            try
            {
                using (var context = _services.GetService<GDaxBotDbContext>())
                {
                    var logged = true;
                    var message = e.Message;

                    if (message == null || message.Type != MessageType.Text)
                    {
                        return;
                    }

                    var entrada = message.Text.ToLower().Split(' ');

                    //Lanzo el comando registrar en caso de ser el que se envia
                    if (entrada.First() == "-user")
                    {
                        if (entrada[2] == botPassword)
                        {
                            var session = new Sesion();
                            var usuario = await context.Usuarios.Where(x => x.Nombre.ToLower() == entrada[1].ToLower()).FirstOrDefaultAsync();
                            if (usuario == null) //Si no existe, creo el usuario y los ajustes
                            {
                                usuario = new Usuario
                                {
                                    Nombre = entrada[1]
                                };
                                context.Add(usuario);
                                foreach (var producto in context.Productos)
                                {
                                    var ajuste = new AjustesProducto
                                    {
                                        IdProducto = producto.IdProducto,
                                        IdUsuario = usuario.IdUsuario,
                                        UmbralInferior = -5,
                                        UmbralSuperior = 5,
                                        ValorMarcado = context.Registros.Where(x => x.IdProducto == producto.IdProducto)
                                                                            .OrderByDescending(x => x.Fecha)
                                                                            .First().Valor
                                    };
                                    context.Add(ajuste);
                                }
                            }
                            session.IdTelegram = message.Chat.Id;
                            session.IdUsuario = usuario.IdUsuario;
                            context.Add(session);
                            await context.SaveChangesAsync();
                            await _bot.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Se ha añadido la sesion a tu cuenta");
                            entrada[0] = "-help";
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Contraseña incorrecta");
                            return;
                        }
                    }

                    if (!context.Sesiones.Any(x => x.IdTelegram == message.Chat.Id))
                    {
                        logged = false;
                        entrada[0] = "";
                    }

                    StringBuilder sb;
                    switch (entrada.First())
                    {

                        case "-help":
                            sb = new StringBuilder();
                            sb.AppendLine("Lista de Comandos:");
                            sb.AppendLine("\t\tUmbral get/set All/\"Activo\"");
                            sb.AppendLine("\t\tRatio All/\"Activo\"");
                            sb.AppendLine("\t\tMarcador \"Activo\"");
                            sb.AppendLine("\t\tActivos");

                            await _bot.SendTextMessageAsync(
                                message.Chat.Id,
                                sb.ToString());
                            break;
                        case "umbral":
                            UmbralCommand(entrada, message);
                            break;
                        case "ratio":
                            RatioCommand(entrada, message);
                            break;
                        case "marcador":
                            MarcadorCommand(entrada, message);
                            break;
                        case "activos":
                            sb = new StringBuilder();
                            foreach (var producto in context.Productos)
                            {
                                sb.AppendLine(producto.Nombre);
                            }

                            await _bot.SendTextMessageAsync(
                                message.Chat.Id,
                                sb.ToString());
                            break;
                        default:
                            if (!logged)
                            {
                                await _bot.SendTextMessageAsync(
                                        message.Chat.Id,
                                        "Introduce la contraseña y tu usuario mediante el comando -user \"Usuario\" ContraseñaDelBot");
                                return;
                            }
                            else
                            {
                                ReplyKeyboardMarkup ReplyKeyboard = new[]
                                {
                                    new[] { "Ratio", "Umbral" },
                                    new[] { "Marcador", "Activos" },
                            };

                                await _bot.SendTextMessageAsync(
                                    e.Message.Chat.Id,
                                    "Envia una orden, si tienes dudas, envia \"-help\" para pedir ayuda",
                                    replyMarkup: ReplyKeyboard);

                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gestionar mensaje de telegram");
                ReplyKeyboardMarkup ReplyKeyboard = new[]
                {
                        new[] { "Ratio", "Umbral" },
                        new[] { "Marcador", "Activos" },
                };

                await _bot.SendTextMessageAsync(
                                e.Message.Chat.Id,
                                "Envia una orden, si tienes dudas, envia \"-help\" para pedir ayuda",
                                replyMarkup: ReplyKeyboard);
            }
        }

        private bool AvailableActive(string activo)
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                return context.Productos.Any(x => x.Nombre == activo);
            }
        }

        private async void UmbralCommand(string[] entrada, Message message)
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                if (entrada.Length == 1 || entrada[1] == "-help")
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Lista de Subcomandos \"Umbral\":");
                    sb.AppendLine("\tGet \"Activo\"/\"All\"·->Obtiene el valor de activación de la alerta para el activo indicado");
                    sb.AppendLine("\tSet \"Activo\"/\"All\" \"porcentaje\"->Modifica el valor de activación de la alerta para el activo indicado");
                    await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    sb.ToString());
                    return;
                }
                if (entrada[1] == "get")
                {
                    try
                    {
                        var sb = new StringBuilder();
                        if (entrada[2] == "all")
                        {
                            foreach (var ajustes in context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id))
                                                                            .Include(x => x.Producto))
                            {
                                sb.AppendLine($"{ajustes.Producto.Nombre} -> {ajustes.UmbralInferior.ToString("0.00")}% y {ajustes.UmbralSuperior.ToString("0.00")}%");
                            }
                        }
                        else if (AvailableActive(entrada[2]))
                        {
                            var ajustes = await context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id)
                                                                    && x.Producto.Nombre.ToLower() == entrada[2]).Include(x => x.Producto).FirstAsync();
                            sb.AppendLine($"Los umbrales de notificacion de {ajustes.Producto.Nombre} son {ajustes.UmbralInferior.ToString("0.00")}% y {ajustes.UmbralSuperior.ToString("0.00")}%");
                        }
                        else
                        {
                            sb.AppendLine($"{entrada[2]} no es un activo valido");
                        }
                        await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               sb.ToString());
                    }
                    catch
                    {
                        await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
                    }
                }
                else if (entrada[1] == "set")
                {
                    try
                    {
                        if (Decimal.TryParse(entrada[3], out var valor))
                        {
                            var sb = new StringBuilder();
                            if (entrada[2] == "all")
                            {
                                foreach (var ajustes in context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id))
                                                                                .Include(x => x.Producto))
                                {
                                    if (valor > 0)
                                    {
                                        ajustes.UmbralSuperior = valor;
                                    }
                                    else
                                    {
                                        ajustes.UmbralInferior = valor;
                                    }

                                    sb.AppendLine($"{ajustes.Producto.Nombre} -> {ajustes.UmbralInferior.ToString("0.00")}% y {ajustes.UmbralSuperior.ToString("0.00")}%");

                                }
                                await context.SaveChangesAsync();
                            }
                            else if (AvailableActive(entrada[2]))
                            {
                                var ajustes = await context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id)
                                                                        && x.Producto.Nombre.ToLower() == entrada[2]).Include(x => x.Producto).FirstAsync();
                                if (valor > 0)
                                {
                                    ajustes.UmbralSuperior = valor;
                                }
                                else
                                {
                                    ajustes.UmbralInferior = valor;
                                }

                                await context.SaveChangesAsync();
                                sb.AppendLine($"{ajustes.Producto.Nombre} -> {ajustes.UmbralInferior.ToString("0.00")}% y {ajustes.UmbralSuperior.ToString("0.00")}%");
                            }
                            else
                            {
                                sb.AppendLine($"{entrada[2]} no es un activo valido");
                            }
                            await _bot.SendTextMessageAsync(
                                   message.Chat.Id,
                                   sb.ToString());
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }
                    catch
                    {
                        await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
                    }
                }
                else
                {
                    await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
                }
            }
        }

        private async void RatioCommand(string[] entrada, Message message)
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                var sb = new StringBuilder();
                if (entrada.Length == 1 || entrada[1] == "-help")
                {
                    sb.AppendLine("Lista de Subcomandos \"Ratio\":");
                    sb.AppendLine("\tRatio \"Activo\"/\"All\"->Obtiene el valor de los activos o del activo seleccionado");
                }
                else if (entrada[1] == "all")
                {
                    var user = await context.Usuarios.Where(x => x.Sesiones.Any(y => y.IdTelegram == message.Chat.Id))
                                                      .Include(x => x.AjustesProductos)
                                                      .FirstAsync();
                    foreach (var producto in context.Productos)
                    {
                        sb.AppendLine(GetRatio(producto, user));
                    }
                }
                else if (AvailableActive(entrada[1]))
                {
                    var user = await context.Usuarios.Where(x => x.Sesiones.Any(y => y.IdTelegram == message.Chat.Id))
                                                      .Include(x => x.AjustesProductos)
                                                      .FirstAsync();
                    var producto = await context.Productos.Where(x => x.Nombre.ToLower() == entrada[1])
                                                            .FirstAsync();
                    sb.AppendLine(GetRatio(producto, user));
                }
                else
                {
                    sb.AppendLine($"{entrada[1]} no es un activo valido");
                }
                await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               sb.ToString());
            }
        }

        private async void MarcadorCommand(string[] entrada, Message message)
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                if (entrada.Length == 1 || entrada[1] == "-help")
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Lista de Subcomandos \"Marcador\":");
                    sb.AppendLine("\tMarcador \"Activo\"/\"All\"->Ajusta el valor sobre el que operan los porcentajes personalizados al valor actual de la divisa");
                    await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    sb.ToString());
                    return;
                }
                try
                {
                    var sb = new StringBuilder();
                    if (entrada[1] == "all")
                    {
                        foreach (var ajustes in context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id))
                                                                        .Include(x => x.Producto))
                        {
                            ajustes.ValorMarcado = context.Registros.Where(x => x.IdProducto == ajustes.IdProducto).OrderByDescending(x => x.Fecha).First().Valor;
                            sb.AppendLine($"El nuevo valor de referencia para {ajustes.Producto.Nombre} es {ajustes.ValorMarcado.ToString("0.00")}€");
                        }
                        await context.SaveChangesAsync();
                    }
                    else if (AvailableActive(entrada[1]))
                    {
                        var ajustes = await context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id)
                                                                   && x.Producto.Nombre.ToLower() == entrada[1]).Include(x => x.Producto).FirstAsync();
                        ajustes.ValorMarcado = context.Registros.Where(x => x.IdProducto == ajustes.IdProducto).OrderByDescending(x => x.Fecha).First().Valor;

                        await context.SaveChangesAsync();
                        sb.AppendLine($"El nuevo valor de referencia para {ajustes.Producto.Nombre} es {ajustes.ValorMarcado.ToString("0.00")}€");
                    }
                    else
                    {
                        sb.AppendLine($"{entrada[1]} no es un activo valido");
                    }
                    await _bot.SendTextMessageAsync(
                               message.Chat.Id,
                               sb.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar marcador");
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"Marcador -help\" para pedir ayuda");
                }
            }
        }

        private string GetRatio(Producto producto, Usuario user)
        {
            using (var context = _services.GetService<GDaxBotDbContext>())
            {
                //Obtengo los datos a mostrar
                var ultimoRegistro = context.Registros.Where(x => x.IdProducto == producto.IdProducto)
                                                    .OrderByDescending(x => x.Fecha)
                                                    .First();
                var valorMarcado = user.AjustesProductos.Where(x => x.IdProducto == producto.IdProducto)
                                                        .First().ValorMarcado;
                var desviacion = ((ultimoRegistro.Valor - valorMarcado) * 100) / valorMarcado;

                var registrosHora = context.Registros.Where(x => x.IdProducto == producto.IdProducto && x.Fecha >= ultimoRegistro.Fecha.AddMinutes(-60.2) && x.Fecha <= ultimoRegistro.Fecha.AddMinutes(-59.8));
                var valorHora = registrosHora.Count() > 0 ? registrosHora.Average(x => x.Valor) : -1;
                var desviacionHora = GetPorcentaje(ultimoRegistro.Valor, valorHora);

                var registros12Horas = context.Registros.Where(x => x.IdProducto == producto.IdProducto && x.Fecha >= ultimoRegistro.Fecha.AddMinutes(-720.2) && x.Fecha <= ultimoRegistro.Fecha.AddMinutes(-719.8));
                var valor12Horas = registros12Horas.Count() > 0 ? registros12Horas.Average(x => x.Valor) : -1;
                var desviacion12Horas = GetPorcentaje(ultimoRegistro.Valor, valor12Horas);

                var registros24Horas = context.Registros.Where(x => x.IdProducto == producto.IdProducto && x.Fecha >= ultimoRegistro.Fecha.AddMinutes(-1440.2) && x.Fecha <= ultimoRegistro.Fecha.AddMinutes(-1439.8));
                var valor24Horas = registros24Horas.Count() > 0 ? registros24Horas.Average(x => x.Valor) : -1;
                var desviacion24Horas = GetPorcentaje(ultimoRegistro.Valor, valor24Horas);

                var sb = new StringBuilder();
                sb.AppendLine($"===={producto.Nombre.ToString()}====");
                sb.AppendLine($"Valor:{ultimoRegistro.Valor.ToString("0.00")} EUR");
                sb.AppendLine($"Referencia: {valorMarcado.ToString("0.00")}€");
                sb.AppendLine($"Desviación: {desviacion.ToString("0.0000")}%");

                if (valorHora == -1)
                {
                    sb.AppendLine($"Hora: Sin datos disponibles");
                }
                else
                {
                    sb.AppendLine($"Hora: {desviacionHora.ToString("0.0000")}%");
                }

                if (valor12Horas == -1)
                {
                    sb.AppendLine($"12 Horas: Sin datos disponibles");
                }
                else
                {
                    sb.AppendLine($"12 Horas: {desviacion12Horas.ToString("0.0000")}%");
                }

                if (valor24Horas == -1)
                {
                    sb.AppendLine($"24 Horas: Sin datos disponibles");
                }
                else
                {
                    sb.AppendLine($"24 Horas: {desviacion24Horas.ToString("0.0000")}%");
                }

                return sb.ToString();
            }
        }

        private decimal GetPorcentaje(decimal valorActual, decimal valorRegistro)
        {
            return ((valorActual - valorRegistro) * 100) / valorRegistro;
        }
    }
}
