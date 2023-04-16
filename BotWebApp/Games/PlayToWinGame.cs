using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    public class PlayToWinGame
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlayToWinGame> _logger;
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;

        public PlayToWinGame(IServiceProvider serviceProvider, ILogger<PlayToWinGame> logger, BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(int rewardAmount)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            PlayToWin newGame = new() { RewardAmount = rewardAmount };

            await _botDataContext.AddAsync(newGame);
            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("PlayToWin");
        }

        private async Task RemoveGame(PlayToWin runningGame)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            _botDataContext.Remove(runningGame);
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<PlayToWin?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await _botDataContext.PlayToWins.Include(x => x.Players).FirstOrDefaultAsync();
        }

        private async Task<string> GenerateRandomTitle(int rewardAmount)
        {
            var random = new Random();
            var result = await _botConfigurations.PlayerNames();

            //get a random name
            var namesRandomIndex = random.Next(0, result.Length);

            //Get a random game
            int titlesRandomIndex = random.Next(0, result.Length);

            return await _botConfigurations.PlayToWinGenerateRandomTitle(titlesRandomIndex, namesRandomIndex, rewardAmount);
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

            //Choose a random reward
            int reward = BotFunctions.RndInt(await _botConfigurations.PlayToWinMinReward(), await _botConfigurations.PlayToWinMaxReward());
            //Save the game
            await RecordGame(reward);

            //Start the Stoptimer
            stopTimer.Start();

            string result = await GenerateRandomTitle(reward);

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

        private async Task<bool> IsExistingPlayer(string chatter)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var existingUser = await _botDataContext.PlayToWinPlayers.Where(p => p.Chatter == chatter).FirstOrDefaultAsync();

            return (existingUser != null);

        }

        public async Task<string> PlayGame(string chatter)
        {
            //Get the running game
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                return await _botConfigurations.NoGameRunning();
            }

            //find if the chatter is already in the game
            if (await IsExistingPlayer(chatter))
                return await _botConfigurations.PlayToWinAlreadyInGame(chatter);

            //Add the player to the game
            PlayToWinPlayer player = new() { Chatter = chatter, PlayToWinId = runningGame.Id };

            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            await _botDataContext.AddAsync(player);
            await _botDataContext.SaveChangesAsync();

            return await _botConfigurations.PlayToWinJoined(chatter);
        }

        public async Task<string> StopGame()
        {
            string result = string.Empty;

            //Get the running game
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                //This is really a problem but we choose not to terminate the app
                _logger.LogError(BotConfigurations.Log("StopGame", await _botConfigurations.NoGameRunning()));
                return result;
            }

            if (runningGame.Players.Count == 0)
            {
                result = await _botConfigurations.PlayToWinNoOneJoined();
            }
            else
            {
                //Selecting a random winner from the list of players
                var random = new Random();
                int index = random.Next(runningGame.Players.Count);
                var winner = runningGame.Players.ElementAt(index);

                //Add the points to the user
                await _botFunctions.SetLoyaltyPoint(winner.Chatter, runningGame.RewardAmount);

                result = await _botConfigurations.PlayToWinStopGame(winner.Chatter, runningGame.RewardAmount);
            }

            _logger.LogInformation(BotConfigurations.Log("StopGame", result));
            //Remove the game
            await RemoveGame(runningGame);
            return result;
        }
    }
}
