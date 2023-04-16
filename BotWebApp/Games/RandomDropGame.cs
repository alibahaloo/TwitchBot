using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    /*
     * Idea:
     * Periodically a chatter will drop a portion of their points. There will be 2 mins for anyone to "grab". If no one grabs, the points are not deducted
     */
    public class RandomDropGame
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RandomDropGame> _logger;
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;


        public RandomDropGame(ILogger<RandomDropGame> logger, BotFunctions botFunctions, IServiceProvider serviceProvider, BotConfigurations botConfiguration)
        {
            _logger = logger;
            _botFunctions = botFunctions;
            _serviceProvider = serviceProvider;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(string chatter, int amount)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            _botDataContext.Add(new RandomDrop { Chatter = chatter, Amount = amount });

            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("RandomDrop");
        }

        private async Task RemoveGame(RandomDrop game)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _botDataContext.Remove(game);
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<RandomDrop?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await _botDataContext.RandomDrops.FirstOrDefaultAsync();
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

                if (chatter == runningGame.Chatter)
                {
                    //The inital chatter grabber their points
                    result = await _botConfigurations.RandomDropOwnerGrabbed(chatter);
                }
                else
                {
                    //remove the points from the intial user
                    await _botFunctions.SetLoyaltyPoint(runningGame.Chatter, runningGame.Amount, false);

                    //Add the points to the second user
                    await _botFunctions.SetLoyaltyPoint(chatter, runningGame.Amount);

                    result = await _botConfigurations.RandomDropChatterGrabbed(chatter, runningGame.Amount, runningGame.Chatter);
                }
            }
            else //Game stopped due to timer ending
            {
                result = await _botConfigurations.RandomDropNoOneJoined();
            }

            _logger.LogInformation(BotConfigurations.Log("StopGame", result));
            //Remove the entry from DB
            await RemoveGame(runningGame);

            return result;
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

            string randomChatter;
            int userPoints;

            while (true)
            {
                //Choose a random chatter first
                randomChatter = await _botFunctions.GetRandomChatter();

                if (randomChatter == string.Empty) return string.Empty;

                foreach (string chatter in await _botConfigurations.AvoidedChatters())
                {
                    if (randomChatter.Contains(chatter)) continue;
                }

                //Get the chatter's current points
                userPoints = await _botFunctions.GetLoyaltyPoint(randomChatter);

                //if userPoints is less than 10, then repeat the process to select another user
                if (userPoints > 10) break;
            }

            //randomly select an amount between user's points up to max%
            var randomPointAmount = BotFunctions.RndInt(10, (userPoints * await _botConfigurations.RandomDropMaxPercentage()) / 100);
            //Generate Title
            string result = await _botConfigurations.RandomDropStartGame(randomChatter, randomPointAmount);
            //Save the game in DB
            await RecordGame(randomChatter, randomPointAmount);
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
    }
}
