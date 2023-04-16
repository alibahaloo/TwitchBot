using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    /// <summary>
    /// drew drops points, the first one to say !me will win the points
    /// </summary>
    public class FirstToWinGame
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FirstToWinGame> _logger;
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;

        public FirstToWinGame(ILogger<FirstToWinGame> logger, IServiceProvider serviceProvider, BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(int amount)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _botDataContext.Add(new FirstToWin { Amount = amount });
            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("FirstToWin");
        }

        private async Task RemoveGame(FirstToWin game)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _botDataContext.Remove(game);
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<FirstToWin?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            return await _botDataContext.FirstToWins.FirstOrDefaultAsync();
        }

        private async Task<string> GenerateRandomTitle(int rewardAmount)
        {
            var random = new Random();
            var result = await _botConfigurations.FirstToWinGames();
            //Get a random game
            int titlesRandomIndex = random.Next(0, result.Length);

            return await _botConfigurations.FirstToWinGenerateRandomTitle(titlesRandomIndex, rewardAmount);
        }

        public async Task<string> StartGame(System.Timers.Timer stopTimer, string fromUser = "")
        {
            if (await GetRunningGame() != null)
            {
                //game already running, can't start
                if (fromUser != "")
                {
                    _logger.LogInformation(BotConfigurations.Log("StartGame", await _botConfigurations.CantStartNewGame()));
                    return await _botConfigurations.CantStartNewGame();
                }
                else
                {
                    _logger.LogError(BotConfigurations.Log("StartGame", await _botConfigurations.CantStartNewGame()));
                    return string.Empty;
                }
            }
            //Choose a random reward and a title for the game
            int reward = BotFunctions.RndInt(await _botConfigurations.FirstToWinMinReward(), await _botConfigurations.FirstToWinMaxReward());
            string result = await GenerateRandomTitle(reward);

            //Save the game
            await RecordGame(reward);

            //Start the Stoptimer
            stopTimer.Start();

            if (fromUser != "")
            {
                _logger.LogInformation(BotConfigurations.Log("StartGame", $"command by user: {fromUser} : {result}"));
            }
            else
            {
                _logger.LogInformation(BotConfigurations.Log("StartGame", $"command by timer : {result}"));
            }
            return result;
        }

        public async Task<string> StopGame(string chatter = "", System.Timers.Timer? stopTimer = null)
        {
            stopTimer?.Stop();

            string result = "";
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                if (chatter != "")
                {
                    return await _botConfigurations.NoGameRunning();
                }
                else
                {
                    _logger.LogError(BotConfigurations.Log("StopGame", await _botConfigurations.NoGameRunning()));
                }

                return result;
            }

            //Game has stopped by a chatter grabbing
            if (chatter != "")
            {

                await _botFunctions.SetLoyaltyPoint(chatter, runningGame.Amount);

                result = await _botConfigurations.FirstToWinGameStop(chatter, runningGame.Amount);
            }
            else //Game stopped due to timer ending
            {
                result = await _botConfigurations.FirstToWinNoOneJoined();
            }
            _logger.LogInformation(BotConfigurations.Log("StopGame", result));
            //Remove the entry from DB
            await RemoveGame(runningGame);

            return result;
        }
    }
}
