using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TwitchBot.Data;

namespace TwitchBot.Bot
{
    public class BotFunctions
    {
        private readonly ILogger<BotFunctions> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TwitchAuth _twitchAuth;
        private readonly BotConfigurations _botConfigurations;
        public BotFunctions(ILogger<BotFunctions> logger, IServiceProvider serviceProvider, TwitchAuth twitchAuth, BotConfigurations botConfiguration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _twitchAuth = twitchAuth;
            _botConfigurations = botConfiguration;
        }

        //Used to generate random number between two ints
        public static int RndInt(int min, int max)
        {
            int value;

            Random rnd = new();

            value = rnd.Next(min, max + 1);

            return value;
        }
        private async Task<string> GetLastGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var lastGame = await _botDataContext.LastGames.FirstOrDefaultAsync();

            if (lastGame != null)
            {
                return lastGame.Game;
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task RecordLastGame(string game)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var lastGame = _botDataContext.LastGames.FirstOrDefault();

            if (lastGame != null)
            {
                //Update the row
                lastGame.Game = game;
            }
            else
            {
                //Create one if none exists
                _botDataContext.Add(new LastGame { Game = game });
            }

            await _botDataContext.SaveChangesAsync();
        }
        public async Task<string> GetRandomGame()
        {
            //Check what the last game was
            string lastGame = await GetLastGame();
            var random = new Random();
            //randomly select a game that is NOT the last game
            while (true)
            {
                //Get a random game from the list
                var result = await _botConfigurations.BotGames();
                int randomIndex = random.Next(0, result.Length);
                string randomGame = result[randomIndex];
                if (randomGame == lastGame)
                {
                    continue;
                }
                else
                {
                    return randomGame;
                }
            }
        }
        public async Task<string> GetRandomAIQuoteAsync()
        {
            var result = await _botConfigurations.AIQuotes();
            int randomIndex = RndInt(0, result.Length);
            return result[randomIndex];
        }
        public async Task<string> AddPointsToUser(string message)
        {
            string[] messageParts = message.Split(' ');

            if (messageParts.Length != 3) return "ImTyping Invalid command parameters.";

            var amount = int.Parse(messageParts[2]);

            if (amount < 10 || amount > 10000) return $"ImTyping Amount should be between 10 and 1000.";

            var userName = messageParts[1];

            await SetLoyaltyPoint(userName, amount);
            return $"ImTyping Added {amount} points to {userName}";
        }
        public async Task<string> GetRandomChatter()
        {
            var currentChatters = await GetCurrentChatters();

            if (currentChatters == null) return string.Empty;

            var random = new Random();
            int index = random.Next(currentChatters.Count);
            var selectedChatter = currentChatters[index];

            return selectedChatter;
        }
        public async Task<List<string>?> GetCurrentChatters()
        {
            string accessToken = await _twitchAuth.GetAccessToken();

            if (accessToken == string.Empty) return null;

            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("Client-Id", TwitchInfo.client_id);

            UriBuilder builder = new(BotConfigurations.TwitchChatterEndpoint)
            {
                Query = $"broadcaster_id={TwitchInfo.ChannelID}&moderator_id={TwitchInfo.ChannelID}"
            };

            var response = await httpClient.GetAsync(builder.Uri);

            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var body = JsonConvert.DeserializeObject<TwitchChattersDTO>(responseContent.ToString());

                if (body != null)
                {
                    List<string> result = new();
                    foreach (var item in body.Data)
                    {
                        result.Add(item.user_login);
                    }

                    return result;
                }
                else
                {
                    _logger.LogError(BotConfigurations.Log("GetCurrentChatters", "response body is null"));
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(BotConfigurations.Log("GetCurrentChatters", e.Message));
                return null;
            }
            finally { httpClient.Dispose(); }
        }
        public async Task RecordLoyaltyPoints()
        {
            var listOfCurrentChatters = await GetCurrentChatters();

            if (listOfCurrentChatters == null)
            {
                _logger.LogWarning(BotConfigurations.Log("RecordLoyaltyPoints", "No loyalty points recorded -- current chatters empty"));
                return;
            }

            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var chatter in listOfCurrentChatters)
            {
                //Check if the user exists, if yes, update, if not, add
                var currentRecord = await _botDataContext.LoyaltyPoints.Where(x => x.Chatter == chatter).FirstOrDefaultAsync();

                if (currentRecord == null)
                {
                    LoyaltyPoint loyaltyPoint = new() { Chatter = chatter, Amount = await _botConfigurations.LoyaltyPointPerTick() };
                    await _botDataContext.AddAsync(loyaltyPoint);
                }
                else
                {
                    currentRecord.Amount += await _botConfigurations.LoyaltyPointPerTick();
                }
            }

            await _botDataContext.SaveChangesAsync();
            string listOfChatters = string.Join(",", listOfCurrentChatters);
            _logger.LogInformation(BotConfigurations.Log("RecordLoyaltyPoints", $" Loyalty Points recorded for {listOfCurrentChatters.Count} chatters: {listOfChatters}"));
        }
        public async Task<string> GetUserPointBalance(string chatter)
        {
            int userPoints = await GetLoyaltyPoint(chatter);

            return await _botConfigurations.UserPointBalance(chatter, userPoints);
        }
        public async Task<int> GetLoyaltyPoint(string chatter)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var currentRecord = await _botDataContext.LoyaltyPoints.Where(x => x.Chatter == chatter).ToListAsync();
            return currentRecord.Count == 0 ? 0 : currentRecord.First().Amount;
        }
        public async Task SetLoyaltyPoint(string chatter, int amount, bool addMode = true)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var currentRecord = await _botDataContext.LoyaltyPoints.Where(x => x.Chatter == chatter).FirstOrDefaultAsync();
            if (currentRecord == null)
            {
                LoyaltyPoint loyaltyPoint = new() { Chatter = chatter, Amount = amount };
                await _botDataContext.AddAsync(loyaltyPoint);
            }
            else
            {
                if (addMode)
                {
                    currentRecord.Amount += amount;
                }
                else
                {
                    currentRecord.Amount -= amount;
                }
            }

            await _botDataContext.SaveChangesAsync();
        }
        public async Task<string> TransferLoyaltyPoints(string chatter, string message)
        {

            string[] messageParts = message.Split(' ');
            if (messageParts.Length > 3) return await _botConfigurations.TransferPointsInvalidCommand(chatter);

            if (!int.TryParse(messageParts[2], out int amountToGive))
                return await _botConfigurations.TransferPointsInvalidNumber(chatter);

            string toChatter = messageParts[1];

            //if toChatter has @ in the beginning, lose it
            toChatter = toChatter.TrimStart('@');

            //Checking if the user is sending to themselves
            if (chatter.Equals(toChatter))
                return await _botConfigurations.TransferPointsInvalidToSelf(chatter);

            int fromChatterPoints = await GetLoyaltyPoint(chatter);

            if (amountToGive > fromChatterPoints)
                return await _botConfigurations.TransferPointsInvalidNotEnoughPoints(chatter);

            await SetLoyaltyPoint(chatter, amountToGive, false);
            await SetLoyaltyPoint(toChatter, amountToGive);

            return await _botConfigurations.TransferPointsSuccess(chatter, amountToGive, toChatter);
        }
    }
}
