using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    public class BattleGame
    {
        private readonly BotFunctions _botFunctions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BattleGame> _logger;
        private readonly BotConfigurations _botConfigurations;

        public BattleGame(BotFunctions botFunctions, IServiceProvider serviceProvider, ILogger<BattleGame> logger, BotConfigurations botConfiguration)
        {
            _botFunctions = botFunctions;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(int rewardAmount)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            Battle newGame = new() { FirstPlayer = "", Amount = rewardAmount };

            await _botDataContext.AddAsync(newGame);
            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("Battle");
        }
        private async Task RemoveGame(Battle runningGame)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            _botDataContext.Remove(runningGame);
            await _botDataContext.SaveChangesAsync();
        }

        private async Task RecordFirstPlayer(string player)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var runningGame = await _botDataContext.Battles.FirstOrDefaultAsync();
            if (runningGame != null) runningGame.FirstPlayer = player;
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<Battle?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await _botDataContext.Battles.FirstOrDefaultAsync();
        }
        private async Task<string> GenerateRandomTitle(int rewardAmount)
        {
            var random = new Random();
            var result = await _botConfigurations.BattleGames();
            //Get a random game
            int titlesRandomIndex = random.Next(0, result.Length);

            return await _botConfigurations.BattleGenerateRandomTitle(titlesRandomIndex, rewardAmount);
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
            int reward = BotFunctions.RndInt(await _botConfigurations.BattleMinReward(), await _botConfigurations.BattleMaxReward());
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

        public async Task<string> JoinBattle(System.Timers.Timer stopTimer, string chatter)
        {
            string result = string.Empty;
            //Get the running game
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                return await _botConfigurations.NoGameRunning();
            }

            //Chekc if it's the first player, then continue getting more, if its the second player, complelet the game
            if (runningGame.FirstPlayer == "")
            {
                await RecordFirstPlayer(chatter);
                return await _botConfigurations.BattleFirstPlayerJoined(chatter);
            }

            if (runningGame.FirstPlayer != "")
            {
                if (runningGame.FirstPlayer == chatter)
                    return await _botConfigurations.BattleAlreadyJoined(chatter);

                //Second player joining
                stopTimer.Stop();
                string winner;
                result = await _botConfigurations.BattleSecondPlayerJoined(chatter);

                int battleResult = BotFunctions.RndInt(0, 1);

                if (battleResult == 0)
                {
                    //First Player Won
                    winner = runningGame.FirstPlayer;
                }
                else
                {
                    //Second Player won
                    winner = chatter;
                }
                await _botFunctions.SetLoyaltyPoint(winner, runningGame.Amount);
                result += await _botConfigurations.BattleBattleFinished(winner, runningGame.Amount);
            }

            _logger.LogInformation(BotConfigurations.Log("JoinBattle", result));
            await RemoveGame(runningGame);
            return result;
        }

        public async Task<string> StopGame()
        {
            string result = string.Empty;
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                _logger.LogError(BotConfigurations.Log("StopGame", await _botConfigurations.NoGameRunning()));
                return result;
            }
            // We get here only if no second player joins the game in time.

            if (runningGame.FirstPlayer == "")
            {
                result = await _botConfigurations.BattleNoOneJoined();
            }
            else
            {
                //No second player joined
                result = await _botConfigurations.BattleNoSecondPlayerJoined();

                int battleResult = BotFunctions.RndInt(0, 1);

                if (battleResult == 0)
                {
                    //First Player Won
                    await _botFunctions.SetLoyaltyPoint(runningGame.FirstPlayer, runningGame.Amount);
                    result += await _botConfigurations.BattleGameStopYouWon(runningGame.FirstPlayer, runningGame.Amount);
                }
                else
                {
                    //Bot won the battle
                    result += await _botConfigurations.BattleGameStopBotWon();
                }
            }

            _logger.LogInformation(BotConfigurations.Log("StopGame", result));
            await RemoveGame(runningGame);
            return result;
        }
    }
}
