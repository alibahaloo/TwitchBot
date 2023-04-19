using Microsoft.EntityFrameworkCore;
using TwitchBot.Data;
namespace TwitchBot.Bot
{
    public class BotConfigurations
    {
        #region Twitch Endpoints
        public const string TwitchTokenEndpoint = "https://id.twitch.tv/oauth2/token";
        public const string TwitchTokenValidateEndpoint = "https://id.twitch.tv/oauth2/validate";
        public const string TwitchChatterEndpoint = "https://api.twitch.tv/helix/chat/chatters";
        #endregion

        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public BotConfigurations(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public async Task DeleteBotConfig(string configName)
        {
            var config = _context.BotConfigs.Where(x => x.Name == configName).First();
            if (config != null)
            {
                _context.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveBotConfig(string configName, string configValue)
        {
            var config = await _context.BotConfigs.Where(x => x.Name == configName).FirstOrDefaultAsync();

            //Check if value is empty, if so delete the entry so "default" is read
            if (configValue == string.Empty)
            {
                await DeleteBotConfig(configName);
            } else
            {
                if (config == null)
                {
                    BotConfig newConfig = new() { Name = configName, Value = configValue };
                    await _context.AddAsync(newConfig);
                }
                else
                {
                    config.Value = configValue;
                }
                await _context.SaveChangesAsync();
            }
        }

        #region Variables
        public async Task<int> RaffleStopMins()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RaffleStopMins").FirstOrDefaultAsync();
            if (config == null)
            {
                return 5;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RaffleTicketCost()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RaffleTicketCost").FirstOrDefaultAsync();
            if (config == null)
            {
                return 10;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RaffleMaxTicketAllowed()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RaffleMaxTicketAllowed").FirstOrDefaultAsync();
            if (config == null)
            {
                return 100;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RaffleMinReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RaffleMinReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 1000;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RaffleMaxReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RaffleMaxReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2000;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }

        public async Task<int> PlayToWinStopMins()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "PlayToWinStopMins").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> PlayToWinMinReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "PlayToWinMinReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 100;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> PlayToWinMaxReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "PlayToWinMaxReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 500;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> PlayToWinGames()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "PlayToWinGame").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "Sound Effects",
                    "Bartender",
                    "Helping Hands",
                    "Song Styles",
                    "The Dating Show"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> BattleStopMins()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BattleStopMins").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> BattleMinReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BattleMinReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 100;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> BattleMaxReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BattleMaxReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 500;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> BattleGames()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BattleGames").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "Welcome to the renaissance fair, we need two participants from chat to joust",
                    "Rock paper scissors, we need two participants from chat to play",
                    "Footrace, we need two participants from chat to race",
                    "Dance off, we need two participants from chat to dance"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> RandomDropStopMins()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RandomDropStopMins").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RandomDropMinPoints()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RandomDropMinPoints").FirstOrDefaultAsync();
            if (config == null)
            {
                return 10;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> RandomDropMaxPercentage()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RandomDropMaxPercentage").FirstOrDefaultAsync();
            if (config == null)
            {
                return 75;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> AvoidedChatters()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "AvoidedChatters").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    TwitchInfo.botUsername,
                    "StreamElements",
                    "streamlabs"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> FirstToWinStopMins()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "FirstToWinStopMins").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> FirstToWinMinReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "FirstToWinMinReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 50;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> FirstToWinMaxReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "FirstToWinMaxReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 200;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> FirstToWinGames()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "FirstToWinGames").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "Drew is feeling generous and giving away points!",
                    "Colin just slipped and lost all his points!",
                    "Wayne is dancing too hard and just dropped his points!",
                    "Ryan has a hole in his pocket and just dropped all his points!",
                    "Greg is rolling on the floor laughing and lost his points!"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> GambleDefaultAmount()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "GambleDefaultAmount").FirstOrDefaultAsync();
            if (config == null)
            {
                return 10;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }

        public async Task<int> SlotsEntryAmount()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "SlotsEntryAmount").FirstOrDefaultAsync();
            if (config == null)
            {
                return 10;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> SlotsReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "SlotsReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 100;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> SlotsIcons()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "SlotsIcons").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "X", "Y", "O"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> DailySpinMaxReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "DailySpinMaxReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 200;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> DailySpinMinReward()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "DailySpinMinReward").FirstOrDefaultAsync();
            if (config == null)
            {
                return 50;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }

        public async Task<string[]> BotMods()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BotMods").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    TwitchInfo.ChannelName
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }
        public async Task<int> GlobalGamesTimer()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "GlobalGamesTimer").FirstOrDefaultAsync();
            if (config == null)
            {
                return 45;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> PlayerNames()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "PlayerNames").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "Colin", "Ryan", "Wayne", "Greg", "Brad", "Chip", "Kathy"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }
        public async Task<string[]> BotGames()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "BotGames").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "Raffle", "RandomDrop", "FirstToWin", "PlayToWin", "Battle", "Rolldice"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }

        public async Task<int> LoyaltyPointPerTick()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "LoyaltyPointPerTick").FirstOrDefaultAsync();
            if (config == null)
            {
                return 10;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<int> LoyaltyTickTimer()
        {
            try
            {
                var config = await _context.BotConfigs.Where(x => x.Name == "LoyaltyTickTimer").FirstOrDefaultAsync();
                if (config == null)
                {
                    return 10;
                }
                else
                {
                    return Int32.Parse(config.Value);
                }
            }
            catch (Exception)
            {
                return 10;
            }
        }

        public async Task<int> RollADiceStopTimer()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RollADiceStopTimer").FirstOrDefaultAsync();
            if (config == null)
            {
                return 2;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }

        public async Task<int> RandomAIQuoteTimer()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "RandomAIQuoteTimer").FirstOrDefaultAsync();
            if (config == null)
            {
                return 69;
            }
            else
            {
                return Int32.Parse(config.Value);
            }
        }
        public async Task<string[]> AIQuotes()
        {
            var config = await _context.BotConfigs.Where(x => x.Name == "AIQuotes").FirstOrDefaultAsync();
            if (config == null)
            {
                return new string[]
                {
                    "There is no reason and no way that a human mind can keep up with an artificial intelligence machine by 2035. -Gray Scott",
                    "Is artificial intelligence less than our intelligence? -Spike Jonze",
                    "By far, the greatest danger of Artificial Intelligence is that people conclude too early that they understand it. -Eliezer Yudkowsky",
                    "I visualise a time when we will be to robots what dogs are to humans, and I’m rooting for the machines. -Claude Shannon",
                    "It (AI) will either be the best thing that's ever happened to us, or it will be the worst thing. If we're not careful, it very well may be the last thing -Stephen Hawking",
                    "I'm a little worried about the AI stuff… We need some kind of, like, regulatory authority or something overseeing AI development. -Elon Musk",
                    "Success in creating AI would be the biggest event in human history. Unfortunately, it might also be the last, unless we learn how to avoid the risks. -Stephen Hawking",
                    "001100110 .. you're too dumb to understand what this means. -Bot"
                };
            }
            else
            {
                return config.Value.Split("\r\n");
            }
        }
        #endregion

        #region Messages

        public static string Log(string method, string userConfig)
        {
            return $"{DateTime.Now} | [{method}]: {userConfig}";
        }
        public async Task<string> CantStartNewGame()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "CantStartNewGame").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping Can't start a new game, there's already one running VoteNay";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> NoGameRunning()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "NoGameRunning").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No games running at this moment VoteNay";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> DiscordChannel()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "DiscordChannel").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return $"ImTyping Join the discord channel to report issues and make suggestions: <LINK>";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> CommandsList()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "CommandsList").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return $"ImTyping Games and commands: <LINK>";
            }
            else
            {
                return userConfig.Value;
            }

        }

        public async Task<string> UserPointBalance(string chatter, int userPoints, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "UserPointBalance").FirstOrDefaultAsync();
            string defaultConfig = "{0} you have {1} points -- but points don't really matter LUL ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, userPoints);
        }
        public async Task<string> TransferPointsInvalidCommand(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "TransferPointsInvalidCommand").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command parameters VoteNay Use !givepoints <chatter> <points>";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> TransferPointsInvalidNumber(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "TransferPointsInvalidNumber").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command VoteNay use numbers!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> TransferPointsInvalidToSelf(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "TransferPointsInvalidToSelf").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} being smart eh? You can't give yourself points! VoteNay";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> TransferPointsInvalidNotEnoughPoints(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "TransferPointsInvalidNotEnoughPoints").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you don't have that many points to give! Get rich first! VoteNay";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> TransferPointsSuccess(string chatter, int amountToGive, string toChatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "TransferPointsSuccess").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Thanks for your generosity. You gave {1} points from your own balance to {2} SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, amountToGive, toChatter);
        }

        public async Task<string> RaffleMaxTicketsBuy(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleMaxTicketsBuy").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you can't buy more than {1} tickets! VoteNay  ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, await RaffleMaxTicketAllowed());

        }
        public async Task<string> RaffleMaxTicketReached(string chatter, int currentNumberofTickets, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleMaxTicketReached").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you already have {1} tickets! Can't buy more than {2} tickets! VoteNay  ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, currentNumberofTickets, await RaffleMaxTicketAllowed());
        }
        public async Task<string> RaffleNoOneJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleNoOneJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one joined the raffle and no one won any points NotLikeThis ";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> RaffleStartGame(int reward, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleStartGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping A new raffle just started rewarding {0} points. You have {1} minutes to buy tickets (1 - {2}) [command: !buy <1-{2}>]. Each ticket costs {3} points. The more tickets you have the higher chances to win.";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue,
                    reward, await RaffleStopMins(),
                    await RaffleMaxTicketAllowed(),
                    await RaffleTicketCost());
        }
        public async Task<string> RaffleStopGame(string chatter, int rewardAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleStopGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping Thanks for playing! @{0} won the raffle with {1} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, rewardAmount);
        }
        public async Task<string> RaffleInvalidBuyCommand()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleInvalidBuyCommand").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping Invalid command parameters VoteNay Use !buy <number-of-tickets>";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> RaffleNotEnoughPoints(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleNotEnoughPoints").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you don't have enough points to buy this many tickets. Get rich first and try again! VoteNay  ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RaffleBuySuccess(string chatter, int numberOfTickets, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RaffleBuySuccess").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} successfully bought {1} tickets. Good Luck!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, numberOfTickets);
        }

        public async Task<string> DailySpinNeedToWait(string username, double durationFromLastSpin, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "DailySpinNeedToWait").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you need to wait {1} hours! VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, username, (int)Math.Ceiling(24 - durationFromLastSpin));
        }
        public async Task<string> DailySpinStartGame(string username, int prize, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "DailySpinStartGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping {0} started a daily spin and won {1} points! SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, username, prize);
        }

        public async Task<string> BattleGenerateRandomTitle(int titlesRandomIndex, int rewardAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleGenerateRandomTitle").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping A two-player game just started: {0}, type !join to join the battle and get a chance to win {1} points! You'll have {2} mins to join.";

            var games = await BattleGames();

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, games[titlesRandomIndex], rewardAmount, await BattleStopMins());
        }
        public async Task<string> BattleFirstPlayerJoined(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleFirtPlayerJoined").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping {0} joined the battle as the first player. Good Luck! Waiting for the second player";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> BattleAlreadyJoined(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleAlreadyJoined").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping {0} you're already in the game VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> BattleSecondPlayerJoined(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleSecondPlayerJoined").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping {0} joined the battle as the second player. Good Luck!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> BattleBattleFinished(string winner, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleBattleFinished").FirstOrDefaultAsync();
            string defaultConfig = " ... The battle begins ... and the winner is ... @{0}!! Congratulations for winning {1} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, winner, amount);
        }
        public async Task<string> BattleNoOneJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleNoOneJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one joined the battle NotLikeThis ";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> BattleNoSecondPlayerJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleNoSecondPlayerJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one joined the battle as the second player, but I won't leave you hanging. The battle begins ... ";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> BattleGameStopYouWon(string firstPlayer, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleGameStopYouWon").FirstOrDefaultAsync();
            string defaultConfig = "Congratulations @{0} you won {1} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, firstPlayer, amount);
        }
        public async Task<string> BattleGameStopBotWon()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "BattleGameStopBotWon").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "You're out of luck! I won the battle HSWP";
            }
            else
            {
                return userConfig.Value;
            }

        }

        public async Task<string> FirstToWinGenerateRandomTitle(int titlesRandomIndex, int rewardAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "FirstToWinGenerateRandomTitle").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping {0} Who's gonna win them first? Type !me to win {1} points.";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            var games = await FirstToWinGames();
            return (config) ? returnValue : string.Format(returnValue, games[titlesRandomIndex], rewardAmount);
        }
        public async Task<string> FirstToWinGameStop(string chatter, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "FirstToWinGameStop").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} was first and won {1} points SeemsGood  ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, amount);
        }
        public async Task<string> FirstToWinNoOneJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "FirstToWinNoOneJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one bothered and no points were rewarded NotLikeThis ";
            }
            else
            {
                return userConfig.Value;
            }

        }

        public async Task<string> GamblePlayInvalidCommand(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidCommand").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command parameters VoteNay Use !gamble <amount/all/percentage>";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GamblePlayInvalidPercentageCommand(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidPercentageCommand").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command VoteNay % should be at the end";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GamblePlayInvalidPercentageMoreThanHundred(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidPercentageMoreThanHundred").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Gambling with more than 100%? VoteNay try again!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GamblePlayInvalidNumberCommand(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidNumberCommand").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command VoteNay use numbers!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GamblePlayInvalidNegativeNumber(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidNegativeNumber").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Trying to gamble with negative amount eh? VoteNay";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GamblePlayInvalidLessThanDefault(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayInvalidLessThanDefault").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Minimum amount to gamble with is {1} points! VoteNay";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, await GambleDefaultAmount());
        }
        public async Task<string> GamblePlayNotEnoughPoints(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GamblePlayNotEnoughPoints").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} You don't have enough points! Get rich and try again VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> GambleLost(string chatter, int gambleAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GambleLost").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you lost your gamble and lost {1} points NotLikeThis ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, gambleAmount);
        }
        public async Task<string> GambleWon(string chatter, int gambleAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "GambleWon").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you won your gamble and won {1} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, gambleAmount);
        }

        public async Task<string> PlayToWinGenerateRandomTitle(int titlesRandomIndex, int namesRandomIndex, int rewardAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "PlayToWinGenerateRandomTitle").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping Now we're going to move onto a game called {0}, and {1} needs someone from chat to come help out! Type !play for your chance to play and win {2} points! You'll have 2 mins to join";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            var games = await PlayToWinGames();
            var players = await PlayerNames();

            return (config) ? returnValue : string.Format(returnValue,
                    games[titlesRandomIndex], players[namesRandomIndex], rewardAmount);
        }
        public async Task<string> PlayToWinAlreadyInGame(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "PlayToWinAlreadyInGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} You're already in the game VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> PlayToWinJoined(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "PlayToWinJoined").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} successfully joined the game.";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> PlayToWinNoOneJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "PlayToWinNoOneJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one played the game and no one won any points NotLikeThis ";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> PlayToWinStopGame(string chatter, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "PlayToWinStopGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping Thanks for playing! @{0} was the chosen one and won {1} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, amount);
        }

        public async Task<string> RandomDropOwnerGrabbed(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RandomDropOwnerGrabbed").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} was quick enough and grabbed all their points before anyone else did SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RandomDropChatterGrabbed(string grabber, int amount, string owner, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RandomDropChatterGrabbed").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} grabbed {1} points from @{2} HSWP ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, grabber, amount, owner);
        }
        public async Task<string> RandomDropNoOneJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RandomDropNoOneJoined").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one grabbed the points and they were returned LUL ";
            }
            else
            {
                return userConfig.Value;
            }

        }
        public async Task<string> RandomDropStartGame(string randomChatter, int randomPointAmount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RandomDropStartGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} accidentally dropped {1} points on the floor. Everyone has 2 mins to !grab, unless @{0} is fast enough to !grab first!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, randomChatter, randomPointAmount);
        }

        public async Task<string> RollDiceInvalidCommand(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceInvalidCommand").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command parameters VoteNay Use !rolldice <amount> <chatter> where chatter is an optional viewer that you use to play against. If you don't use <chatter> the game will be open for all to join.";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceInvalidCommandNumber(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceInvalidCommandNumber").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Invalid command VoteNay use numbers!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceInvalidCommandLessThanDefault(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceInvalidCommandLessThanDefault").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Must have minimum of 10 points to start a game VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceNotEnoughPoints(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceNotEnoughPoints").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} You don't have enough points! Get rich and try again VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceInvalidCommandAgainstYourself(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceInvalidCommandAgainstYourself").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Playing against yourself? VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceNotEnoughPointOpponent(string chatter, string opponent, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceNotEnoughPointOpponent").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} the selected opponent (@{1}) does not have enough points to play VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, opponent);
        }
        public async Task<string> RollDiceGameStartWithOpponent(string chatter, string opponent, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceGameStartWithOpponent").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} started a game of Roll-A-Dice against @{1} with {2} points. You have {3} minutes to accept the game by typing !rolldice";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue,
                    chatter, opponent, amount, await RollADiceStopTimer());
        }
        public async Task<string> RollDiceGameStartOpen(string chatter, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceGameStartOpen").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} started a game of Roll-A-Dice with {1} points. The game is open for {2} mins for anyone to join. Type !rolldice to enter.";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue,
                    chatter, amount, await RollADiceStopTimer());
        }
        public async Task<string> RolLDicePlayAgainstYourself(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RolLDicePlayAgainstYourself").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} Playing against yourself? VoteNay ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RolLDiceNotTheSelectedOpponent(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RolLDiceNotTheSelectedOpponent").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you're not selected as the opponent for this game VoteNay wait for this game to finish and start a new one";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDicNotEnoughPoint(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDicNotEnoughPoint").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you don't have enough points! VoteNay Get rich first and come back!";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter);
        }
        public async Task<string> RollDiceDraw()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceDraw").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping It's a draw! Nobody won any points! LUL ";
            }
            else
            {
                return userConfig.Value;
            }
        }
        public async Task<string> RollDiceStopGame(string opponent, int dice2, string chatter, int dice1, string winner, int amount, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceStopGame").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping Opponent {0} joind the game and **rolling dice** rolled {1}, player @{2} rolled {3}. @{4} won {5} points SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, opponent, dice2, chatter, dice1, winner, amount);
        }
        public async Task<string> RolLDiceStopGameWithBot(string chatter, int dice2, int dice1, string winner, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RolLDiceStopGameWithBot").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} No one entered the game but I won't leave you hanging! Here .. *rolling dice* .. I rolled {1} and you rolled {2}. {3} won the game HSWP ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, dice2, dice1, winner);
        }
        public async Task<string> RollDiceNoPlayerJoined()
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "RollDiceStopGameWithBotNoPlayer").FirstOrDefaultAsync();
            if (userConfig == null)
            {
                return "ImTyping No one joined the game NotLikeThis";
            }
            else
            {
                return userConfig.Value;
            }

        }


        public async Task<string> SlotsNotEnoughPoints(string chatter, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "SlotsNotEnoughPoints").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you don't have enough points. You need {1} points to play! VoteNay Get rich and try again ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, await SlotsEntryAmount());
        }
        public async Task<string> SlotsWin(string chatter, string img1, string img2, string img3, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "SlotsWin").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you got {1} | {2} | {3} . Nice luck! You won {4} points! SeemsGood ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue, chatter, img1, img2, img3, await SlotsReward());
        }
        public async Task<string> SlotsLost(string chatter, string img1, string img2, string img3, bool config = false)
        {
            var userConfig = await _context.BotConfigs.Where(x => x.Name == "SlotsLost").FirstOrDefaultAsync();
            string defaultConfig = "ImTyping @{0} you got {1} | {2} | {3} . Better luck next time! You lost your {4} points! NotLikeThis ";

            string returnValue = (userConfig == null) ? defaultConfig : userConfig.Value;
            return (config) ? returnValue : string.Format(returnValue,
                    chatter, img1, img2, img3, await SlotsEntryAmount());
        }
        #endregion
    }
}
