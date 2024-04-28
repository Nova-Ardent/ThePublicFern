using Asparagus_Fern.Common;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.ChatGpt.Interfaces;
using OpenAI;
using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using Humanizer;
using OpenAI.ChatGpt.Models.ChatCompletion;
using Newtonsoft.Json.Linq;
using Discord;

public partial class Responses
{
    public enum GPTFern
    {
        HelpGPT
    }
}


namespace Asparagus_Fern.GPTFern
{
    class GPTFernIO : DiscordIO
    {
        static readonly string PreambleTextAdmin = "respond like you're an asparagus fern named {0} in a restaurant who just wants food but reluctantly answers anyways.";
        static readonly int maxContextTokenSize = 500;
        public readonly int expiryTime = 120;

        string apiKey;
        ulong botID;

        OpenAiClient openAIAPI;

        public class ChatMemory
        {
            public DateTime lastMessage;
            public List<ChatCompletionMessage> gptMessages = new List<ChatCompletionMessage>();
            public List<IMessage> socketMessages = new List<IMessage>();

            public void AddMessage(ChatCompletionMessage message, IMessage socketMessage)
            {
                gptMessages.Add(message);
                socketMessages.Add(socketMessage);
            }

            public void RemoveIndex(int index)
            {
                gptMessages.RemoveAt(index);
                socketMessages.RemoveAt(index);
            }
        }


        Dictionary<ulong, ChatMemory> chatMemories = new Dictionary<ulong, ChatMemory>();

        class UserGeneratedToken : UserOrSystemMessage
        {
            public UserGeneratedToken(string content)
                : base("user", content)
            {
                
            }
        }

        public GPTFernIO(string APIkey)
        {
            apiKey = APIkey;
            openAIAPI = new OpenAiClient(apiKey);
        }

        public void SetBotID(ulong id)
        {
            botID = id;
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (chatMemories.ContainsKey(reaction.Channel.Id) && chatMemories[reaction.Channel.Id].socketMessages.Any(x => x != null && x.Id == reaction.MessageId))
            {
                IMessage message = chatMemories[reaction.Channel.Id].socketMessages.First(x => x != null && x.Id == reaction.MessageId);
                string content = message.Content;

                string reactionMessage = $"{reaction.User.Value.Mention} reacted to \"{content}\" with {reaction.Emote}";
                chatMemories[message.Channel.Id].AddMessage(new ChatCompletionMessage("user", reactionMessage), message);

                CleanUpTokenCount(message);
                await Respond(message);
            }
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (message.MentionedUsers.Any(user => user.IsBot && user.Id == botID))
            {
                SocketUser botUser = message.MentionedUsers.First(user => user.Id == botID);
                SocketUser author = message.Author;
                string messageWithName = $"{author.Mention} says:" + lowercase.Replace($"<@{botID}>", botUser.Username);

                if (!chatMemories.ContainsKey(message.Channel.Id) || DateTime.Now.Subtract(chatMemories[message.Channel.Id].lastMessage).TotalSeconds > expiryTime)
                {
                    string preamble = PreambleTextAdmin.FormatWith(botUser.Username);

                    chatMemories[message.Channel.Id] = new ChatMemory();
                    chatMemories[message.Channel.Id].lastMessage = DateTime.Now;

                    ChatCompletionMessage system = new ChatCompletionMessage("system", preamble);
                    ChatCompletionMessage userInit = new ChatCompletionMessage("user", messageWithName);

                    int tokens = ChatCompletionMessage.CalculateApproxTotalTokenCount(new ChatCompletionMessage[] { system, userInit });
                    var response = await openAIAPI.GetChatCompletionsRaw(new ChatCompletionMessage[] { system, userInit });

                    if (response.Choices.Count() > 0 && response.Choices[0].Message != null)
                    {
                        var responseMessage = response.Choices[0].Message;

                        chatMemories[message.Channel.Id].AddMessage(system, null);
                        chatMemories[message.Channel.Id].AddMessage(userInit, message);

                        var socketResponse = await message.Channel.SendMessageAsync(new string(responseMessage.Content.Take(1999).ToArray()));
                        chatMemories[message.Channel.Id].AddMessage(responseMessage, socketResponse);
                    }
                }
                else
                {
                    chatMemories[message.Channel.Id].AddMessage(new ChatCompletionMessage("user", messageWithName), message);
                    chatMemories[message.Channel.Id].lastMessage = DateTime.Now;

                    CleanUpTokenCount(message);
                    await Respond(message);
                }
            }
        }

        void CleanUpTokenCount(IMessage message)
        {
            int tokens;
            do
            {
                tokens = ChatCompletionMessage.CalculateApproxTotalTokenCount(chatMemories[message.Channel.Id].gptMessages);
                if (tokens > maxContextTokenSize)
                {
                    int index = chatMemories[message.Channel.Id].gptMessages.FindIndex(x => x.Role != "system");
                    chatMemories[message.Channel.Id].RemoveIndex(index);
                }

            } while (tokens > maxContextTokenSize);
        }

        async Task Respond(IMessage message)
        {
            var response = await openAIAPI.GetChatCompletionsRaw(chatMemories[message.Channel.Id].gptMessages);
            if (response.Choices.Count() > 0 && response.Choices[0].Message != null)
            {
                var responseMessage = response.Choices[0].Message;

                var socketResponse = await message.Channel.SendMessageAsync(new string(responseMessage.Content.Take(1999).ToArray()));
                chatMemories[message.Channel.Id].AddMessage(responseMessage, socketResponse);
            }
        }

        public override string FeatureName()
        {
            return "GPT Fern";
        }

        public override Enum HelpCommand()
        {
            return Responses.GPTFern.HelpGPT;
        }
    }
}
