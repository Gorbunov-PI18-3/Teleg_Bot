using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace Teleg_Bot
{
    class Program
    {

        static void Main(string[] args)
        {
            var bot = new BotWorker();

            bot.Inizalize();
            bot.Start();

            Console.WriteLine("Напишите stop для прекращения работы");

            string command;
            do
            {
                command = Console.ReadLine();

            } while (command != "stop");

            bot.Stop();

        }

    }


    public class BotWorker
    {
        private ITelegramBotClient botClient;
        private BotMessageLogic logic;

        public void Inizalize()
        {
            botClient = new TelegramBotClient(BotCredentials.BotToken);
            logic = new BotMessageLogic(botClient);
        }

        public void Start()
        {
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
        }

        public void Stop()
        {
            botClient.StopReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message != null)
            {
                await logic.Response(e);
            }
        }


    }

    public class BotMessageLogic
    {
        private Messenger messanger;

        private Dictionary<long,Conversation> chatList;
        private ITelegramBotClient botClient;
        public BotMessageLogic(ITelegramBotClient botClient)
        {
            messanger = new Messenger();
            chatList = new Dictionary<long,Conversation>();
            this.botClient = botClient;
        }
        public async Task Response(MessageEventArgs e)
        {
            var Id = e.Message.Chat.Id;

            if (!chatList.ContainsKey(Id))
            {
                var newchat = new Conversation(e.Message.Chat);

                chatList.Add(Id, newchat);
            }

            var chat = chatList[Id];

            chat.AddMessage(e.Message);

            await SendTextMessage(chat);

        }
        private async Task SendTextMessage(Conversation chat)
        {
            var text = messanger.CreateTextMessage(chat);

            await botClient.SendTextMessageAsync(
            chatId: chat.GetId(), text: text);
        }

    }


    public class Messenger
    {
        public string CreateTextMessage(Conversation chat)
        {
            var text = "";

            switch (chat.GetLastMessage())
            {
                case "/saymehi":
                    text = "привет";
                    break;
                case "/askme":
                    text = "как дела";
                    break;
                default:
                    var delimiter = ",";
                    text = "История ваших сообщений: " + string.Join(delimiter, chat.GetTextMessages().ToArray());
                    break;
            }

            return text;

        }
    }


    public class Conversation
    {
        private Chat telegramChat;

        private List<Message> telegramMessages;

        public Conversation(Chat chat)
        {
            telegramChat = chat;
            telegramMessages = new List<Message>();
        }


        public void AddMessage(Message message)
        {
            telegramMessages.Add(message);
        }



        public string GetLastMessage() => telegramMessages[telegramMessages.Count - 1].Text;



        public List<string> GetTextMessages()
        {
            var textMessages = new List<string>();

            foreach (var message in telegramMessages)
            {
                if (message.Text != null)
                {
                    textMessages.Add(message.Text);
                }
            }

            return textMessages;
        }

        public long GetId() => telegramChat.Id;

        

    }

    public static class BotCredentials
    {
        public static readonly string BotToken = "1533851634:AAGo1fcjdrsF9n6D2NXoomswB-Vtzm8G3wI";

    }


    public interface IChatCommand
    {
        bool CheckMessage(string message);


    }

    public abstract class AbstractCommand : IChatCommand
    {
        public string CommandText;

        public bool CheckMessage(string message)
        {
            return CommandText == message;
        }
    }

    public class CommandParser
    {
        private List<IChatCommand> Command;

        public CommandParser()
        {
            Command = new List<IChatCommand>();
        }
    }
    interface IChatTextCommand
    {
        string ReturnText();
    }
    public class SayHiCommand : AbstractCommand,IChatTextCommand
    {
        public SayHiCommand()
        {
            CommandText = "/saymehi";
        }

        public string ReturnText()
        {
            return "привет";
        }

    }
}
