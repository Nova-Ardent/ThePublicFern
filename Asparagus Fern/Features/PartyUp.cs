using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Text.Json;
using System.Linq;


namespace Asparagus_Fern.Features
{
    class PartyUp
    {
        class Party
        {
            IList<string> membersSorted = new List<string>();
            IList<string> members = new List<string>();
            Random rng = new Random();

            public Party(string name)
            {
                members.Add(name);
                membersSorted.Add(name);
                (membersSorted as List<string>).Sort();
            }

            public void Shuffle()
            {
                int n = members.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    string value = members[k];
                    members[k] = members[n];
                    members[n] = value;
                }
            }

            public Embed GetPartyEmbed()
            {
                string partyMembers = membersSorted.Select(x => x + "\n").Aggregate((a, b) => a + b);

                var embed = new EmbedBuilder() { Description = partyMembers, Title = "Current party" }.Build();
                return embed;
            }

            public void Add(string member)
            {
                members.Add(member);
                membersSorted.Add(member);
                (membersSorted as List<string>).Sort();
            }

            public int Size()
            {
                return members.Count();
            }

            public Embed[] GetTeamsEmbeds()
            {
                Embed[] embeds = new Embed[2];

                IList<string> team1 = members.Take(Size() / 2).ToList();
                IList<string> team2 = members.Skip(Size() / 2).ToList();

                embeds[0] = new EmbedBuilder() { Description = team1.Select(x => x + "\n").Aggregate((a, b) => a + b), Color = Color.Orange, Title = "Orange team" }.Build();
                embeds[1] = new EmbedBuilder() { Description = team2.Select(x => x + "\n").Aggregate((a, b) => a + b), Color = Color.Blue, Title = "Blue team" }.Build();

                return embeds;
            }

            public void Clear()
            {
                membersSorted.Clear();
                members.Clear();
            }
        }

        private DiscordSocketClient client;
        private Dictionary<ulong, Party> parties = new Dictionary<ulong, Party>();

        public PartyUp(DiscordSocketClient client)
        {
            this.client = client;
            client.MessageReceived += Message;
        }

        private async Task Message(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (message.Content.StartsWith("party up"))
            {
                if (!parties.ContainsKey(message.Author.Id))
                {
                    await message.Channel.SendMessageAsync("creating party");
                    parties[message.Author.Id] = new Party(message.Author.Username + message.Author.Discriminator);
                }

                string[] content = message.Content.Split();
                if (content.Count() == 3 && content[2].Equals("party"))
                {
                    await message.Channel.SendMessageAsync(embed: parties[message.Author.Id].GetPartyEmbed());
                }
                if (content.Count() > 3 && content[2].Equals("with"))
                {
                    foreach (var member in message.MentionedUsers)
                    {
                        parties[message.Author.Id].Add(member.Username + member.Discriminator);
                        await message.Channel.SendMessageAsync($"added user {member.Username}{member.Discriminator}");
                    }
                }
                if (content.Count() == 4 && content[2].Equals("create") && content[3].Equals("random"))
                {
                    int size = parties[message.Author.Id].Size();

                    parties[message.Author.Id].Shuffle();
                    Embed[] embeds = parties[message.Author.Id].GetTeamsEmbeds();
                    message.Channel.SendMessageAsync(embed: embeds[0]);
                    await message.Channel.SendMessageAsync(embed: embeds[1]);
                }
                if (content.Count() == 3 && content[2].Equals("reset"))
                {
                    await message.Channel.SendMessageAsync("reseting your setup");
                    parties[message.Author.Id].Clear();
                    parties[message.Author.Id] = new Party(message.Author.Username + message.Author.Discriminator);
                }
            }
        }
    }
}
