using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CommunicationManager;

public class TelegramManager
{
    private readonly HashSet<long> _authenticatedUsers;
    private readonly CancellationTokenSource _cts = new();
    private readonly TelegramBotClient bot;
    private readonly User me;

    public TelegramManager(string botToken, IEnumerable<long>? authenticatedUsers = null)
    {
        _authenticatedUsers = authenticatedUsers != null
            ? new HashSet<long>(authenticatedUsers)
            : new HashSet<long>();

        bot = new TelegramBotClient(botToken, cancellationToken: _cts.Token);

        me = bot.GetMe().Result;
        bot.OnError += OnError;
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;
    }

    public void StopBot()
    {
        _cts.Cancel();
    }

    async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
        await Task.Delay(2000, _cts.Token);
    }

    async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text is not { } text)
            Console.WriteLine($"Received a message of type {msg.Type}");
        else if (text.StartsWith('/'))
        {
            var space = text.IndexOf(' ');
            if (space < 0) space = text.Length;
            var command = text[..space].ToLower();
            if (command.LastIndexOf('@') is > 0 and int at) // it's a targeted command
                if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                    command = command[..at];
                else
                    return; // command was not targeted at me
            await OnCommand(command, text[space..].TrimStart(), msg);
        }
        else
            await OnTextMessage(msg);
    }

    async Task OnTextMessage(Message msg) // received a text message that is not a command
    {
        Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
        await OnCommand("/start", "", msg); // for now we redirect to command /start
    }

    async Task OnCommand(string command, string args, Message msg)
    {
        Console.WriteLine($"Received command: {command} {args}");
        switch (command)
        {
            case "/start":
                await bot.SendMessage(msg.Chat, """
                <b><u>Bot menu</u></b>:
                /photo [url]    - send a photo <i>(optionally from an <a href="https://picsum.photos/310/200.jpg">url</a>)</i>
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /poll           - send a poll
                /reaction       - send a reaction
                """, parseMode: ParseMode.Html, linkPreviewOptions: true,
                    replyMarkup: new ReplyKeyboardRemove()); // also remove keyboard to clean-up things
                break;
            case "/photo":
                if (args.StartsWith("http"))
                    await bot.SendPhoto(msg.Chat, args, caption: "Source: " + args);
                else
                {
                    await bot.SendChatAction(msg.Chat, ChatAction.UploadPhoto);
                    await Task.Delay(2000); // simulate a long task
                    await using var fileStream = new FileStream("bot.gif", FileMode.Open, FileAccess.Read);
                    await bot.SendPhoto(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
                }
                break;
            case "/inline_buttons":
                await bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: new InlineKeyboardButton[][] {
                ["1.1", "1.2", "1.3"],
                [("WithCallbackData", "CallbackData"), ("WithUrl", "https://github.com/TelegramBots/Telegram.Bot")]
            });
                break;
            case "/keyboard":
                await bot.SendMessage(msg.Chat, "Keyboard buttons:", replyMarkup: new[] { "MENU", "INFO", "LANGUAGE" });
                break;
            case "/remove":
                await bot.SendMessage(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
                break;
            case "/poll":
                await bot.SendPoll(msg.Chat, "Question", ["Option 0", "Option 1", "Option 2"], isAnonymous: false, allowsMultipleAnswers: true);
                break;
            case "/reaction":
                await bot.SetMessageReaction(msg.Chat, msg.Id, ["❤"], false);
                break;
        }
    }

    async Task OnUpdate(Update update)
    {
        switch (update)
        {
            case { CallbackQuery: { } callbackQuery }: await OnCallbackQuery(callbackQuery); break;
            case { PollAnswer: { } pollAnswer }: await OnPollAnswer(pollAnswer); break;
            default: Console.WriteLine($"Received unhandled update {update.Type}"); break;
        }
        ;
    }

    async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        await bot.AnswerCallbackQuery(callbackQuery.Id, $"You selected {callbackQuery.Data}");
        await bot.SendMessage(callbackQuery.Message!.Chat, $"Received callback from inline button {callbackQuery.Data}");
    }

    async Task OnPollAnswer(PollAnswer pollAnswer)
    {
        if (pollAnswer.User != null)
            await bot.SendMessage(pollAnswer.User.Id, $"You voted for option(s) id [{string.Join(',', pollAnswer.OptionIds)}]");
    }

}
