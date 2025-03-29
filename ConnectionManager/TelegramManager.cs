using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Logger;

namespace ConnectionManager
{
    public class TelegramManager
    {
        public event EventHandler<(long chatID, int messageID, string? fName, string message)>? OnMessageReceive;
        public event EventHandler<string>? OnLogEvent;

        private readonly string BotToken;
        private readonly TelegramBotClient Bot;
        private readonly CancellationTokenSource cancellationTokenService;
        public bool BotConnected { get; set; } = false;
        public string BotChannel { get; set; }

        public TelegramManager(string botToken, string hostname)
        {
            BotChannel = hostname;
            BotToken = botToken;
            cancellationTokenService = new CancellationTokenSource();

            Bot = new TelegramBotClient(BotToken, cancellationToken: cancellationTokenService.Token);

            Bot.OnMessage += Bot_OnMessage;
            Bot.OnError += Bot_OnError;
            Bot.OnUpdate += Bot_OnUpdate;
        }

        public async Task ConnectAsync()
        {
            User botInfo;
            try
            {
                botInfo = await Bot.GetMe();
                BotConnected = true;
            }
            catch (ArgumentNullException ex)
            {
                BotConnected = false;
                throw new ConnectionFailedException("Telegram Connection Error", ex);
            }

            BotChannel = botInfo.Username ?? "Bot";
            OnLogEvent?.Invoke(this, "Bot Connected");
            OnLogEvent?.Invoke(this, "Bot Username: " + botInfo.Username ?? "None");
            OnLogEvent?.Invoke(this, "Bot Name: " + botInfo.FirstName + botInfo.LastName);
            OnLogEvent?.Invoke(this, "Bot Premium: " + botInfo.IsPremium.ToString());
            OnLogEvent?.Invoke(this, "Bot Join Groups: " + botInfo.CanJoinGroups.ToString());
        }

        private Task Bot_OnMessage(Message message, UpdateType type)
        {
            if (type == UpdateType.Message && message != null)
            {
                string receivedMessage = message.Text ?? string.Empty;

                // todo: Photos and Documents

                OnLogEvent?.Invoke(this, $"Incoming: {message.Chat.Id} {receivedMessage}");
                OnMessageReceive?.Invoke(this, (message.Chat.Id, message.Id, message.Chat.FirstName, receivedMessage));
            }

            return Task.CompletedTask;
        }

        private async Task Bot_OnError(Exception exception, HandleErrorSource source)
        {
            throw new BotException($"Bot Error: {exception.Message}");
        }

        private async Task Bot_OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
            {
                await Bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                await Bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
            }
        }

        public async Task SendTextMessageAsync(long chatID, string message)
        {
            await Bot.SendMessage(
                chatId: chatID,
                text: message
            );
            OnLogEvent?.Invoke(this, $"Outgoing: {chatID} {message}");
        }
        public async Task SendPhotoMessageAsync(long chatID, string message, string sendFile)
        {
            if (!string.IsNullOrEmpty(sendFile))
            {
                var photoInput = InputFile.FromFileId(sendFile);
                await Bot.SendPhoto(
                    chatId: chatID,
                    photo: photoInput,
                    caption: message
                );
            }
        }
        public async Task SendDocumentMessageAsync(long chatID, string message, string sendFile)
        {
            if (!string.IsNullOrEmpty(sendFile))
            {
                var documentInput = InputFile.FromFileId(sendFile);
                await Bot.SendDocument(
                    chatId: chatID,
                    document: documentInput,
                    caption: message
                );
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new Exception($"Bot Error: {exception.Message}");
        }

        public void Disconnect()
        {
            cancellationTokenService.Cancel();
            OnLogEvent?.Invoke(this, "Bot Disconnected");
        }
    }
}
