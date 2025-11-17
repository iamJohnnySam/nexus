using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PersonalAssistant;

public class TelegramManager : IAsyncDisposable
{
    public event EventHandler<string>? OnLogInfoEvent;
    public event EventHandler<string>? OnLogWarnEvent;
    public event EventHandler<string>? OnLogErrorEvent;

    private readonly CancellationTokenSource _cts = new();
    private Task? _receivingTask;

    public TelegramBotClient Bot { get; }

    public TelegramManager()
    {
        string json = File.ReadAllText("credentials.json");
        using var doc = JsonDocument.Parse(json);
        string token = doc.RootElement.GetProperty("telegram").GetString()
            ?? throw new KeyNotFoundException("Missing 'telegram' key");

        Bot = new TelegramBotClient(token);
    }

    /// <summary>
    /// Starts receiving updates in background.
    /// </summary>
    public async Task StartAsync()
    {
        if (_receivingTask != null)
            return;

        OnLogInfoEvent?.Invoke(this, "Starting Telegram bot…");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };

        _receivingTask = Bot.ReceiveAsync(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            _cts.Token
        );

        OnLogInfoEvent?.Invoke(this, "Telegram bot is now running.");
    }

    /// <summary>
    /// Stops the bot safely.
    /// </summary>
    public async Task StopAsync()
    {
        Console.WriteLine("Stopping Telegram bot…");
        _cts.Cancel();

        if (_receivingTask != null)
        {
            try { await _receivingTask; }
            catch (OperationCanceledException) { }
        }

        Console.WriteLine("Telegram bot stopped.");
    }

    private async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"Telegram Error: {exception}");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message is { } msg && msg.Text != null)
        {
            await OnMessage(msg, update.Type);
        }
        else if (update.CallbackQuery is { })
        {
            await OnUpdate(update);
        }
    }

    private async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text == "/start")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Left", "Left"),
                InlineKeyboardButton.WithCallbackData("Right", "Right")
            });

            await Bot.SendMessage(
                chatId: msg.Chat.Id,
                text: "Welcome! Pick one direction:",
                replyMarkup: keyboard
            );
        }
    }

    private async Task OnUpdate(Update update)
    {
        if (update.CallbackQuery is { } q)
        {
            await Bot.AnswerCallbackQuery(q.Id, $"You picked {q.Data}");
            await Bot.SendMessage(
                chatId: q.Message!.Chat.Id,
                text: $"User {q.From!.Username} clicked on {q.Data}"
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _cts.Dispose();
    }
}
