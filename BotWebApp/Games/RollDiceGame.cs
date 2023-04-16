using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    /// <summary>
    /// A user puts an amount forward and rolls a dice, has the opton to choose a player, or leave it open
    /// another user accepts and rolls a dice
    /// the person with the highest number will win (amount x 2) and the other will lose
    /// If no one accepts in 2 mins, the games stops 
    /// </summary>
    public class RollDiceGame
    {
        private readonly BotFunctions _botFunctions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RollDiceGame> _logger;
        private readonly BotConfigurations _botConfigurations;


        public RollDiceGame(BotFunctions botFunctions, IServiceProvider serviceProvider, ILogger<RollDiceGame> logger, BotConfigurations botConfiguration)
        {
            _botFunctions = botFunctions;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(string chatter, int amount, string opponent = "")
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            RollDice newGame = new() { Chatter = chatter, Amount = amount, Opponent = opponent };

            await _botDataContext.AddAsync(newGame);
            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("Rolldice");
        }

        private async Task RemoveGame(RollDice runningGame)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            _botDataContext.Remove(runningGame);
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<RollDice?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await _botDataContext.RollDices.FirstOrDefaultAsync();
        }

        public async Task<string> StartGame(System.Timers.Timer stopTimer, string chatter, string message)
        {
            //check if there's a running game, if so, people are trying to join
            var runningGame = await GetRunningGame();
            if (runningGame != null)
                return await PlayGame(stopTimer, chatter);

            //Reacing to !roll
            string result;

            string opponent;

            //Command would be like !rolldice 100 <user>
            string[] messageParts = message.Split(' ');
            if ((messageParts.Length > 3) || (messageParts.Length < 2))
                return await _botConfigurations.RollDiceInvalidCommand(chatter);

            if (!Int32.TryParse(messageParts[1], out int amount))
                return await _botConfigurations.RollDiceInvalidCommandNumber(chatter);

            if (amount < 10)
                return await _botConfigurations.RollDiceInvalidCommandLessThanDefault(chatter);

            if (messageParts.Length == 3) opponent = messageParts[2].TrimStart('@'); else opponent = "";

            //Check if user can afford the given points
            int userPoints = await _botFunctions.GetLoyaltyPoint(chatter);
            if (amount > userPoints)
                return await _botConfigurations.RollDiceNotEnoughPoints(chatter);

            if (opponent != "")
            {
                //check if an opponent name was given
                if (opponent == chatter)
                    return await _botConfigurations.RollDiceInvalidCommandAgainstYourself(chatter);

                //check if the opponent can afford the game
                int opponentPoints = await _botFunctions.GetLoyaltyPoint(opponent);
                if (amount > opponentPoints)
                    return await _botConfigurations.RollDiceNotEnoughPointOpponent(chatter, opponent);

                //Recording the game
                await RecordGame(chatter, amount, opponent);

                result = await _botConfigurations.RollDiceGameStartWithOpponent(chatter, opponent, amount);
            }
            else
            {
                //Keep the game open
                await RecordGame(chatter, amount);

                result = await _botConfigurations.RollDiceGameStartOpen(chatter, amount);
            }

            //Start the timer
            stopTimer.Start();

            return result;
        }

        private async Task<string> PlayGame(System.Timers.Timer stopTimer, string chatter)
        {
            string result;

            //reacting to roledice
            var runningGame = await GetRunningGame();
            if (runningGame == null)
                return await _botConfigurations.NoGameRunning();

            if ((runningGame.Chatter == chatter) && (runningGame.Opponent != ""))
                return await _botConfigurations.RolLDicePlayAgainstYourself(chatter);

            if ((runningGame.Opponent != chatter) && (runningGame.Opponent != ""))
                return await _botConfigurations.RolLDiceNotTheSelectedOpponent(chatter);

            //The opponent || random player entered the game
            int userPoints = await _botFunctions.GetLoyaltyPoint(chatter);

            if (userPoints < runningGame.Amount)
                return await _botConfigurations.RollDicNotEnoughPoint(chatter);

            stopTimer.Stop();

            //roll a dice
            int dice1 = BotFunctions.RndInt(1, 6);
            int dice2 = BotFunctions.RndInt(1, 6);
            string winner, loser;

            if (dice1 > dice2)
            {
                winner = runningGame.Chatter;
                loser = chatter;
            }
            else if (dice1 < dice2)
            {
                winner = chatter;
                loser = runningGame.Chatter;
            }
            else
            {
                await RemoveGame(runningGame);
                return await _botConfigurations.RollDiceDraw();
            }

            await _botFunctions.SetLoyaltyPoint(winner, runningGame.Amount);
            await _botFunctions.SetLoyaltyPoint(loser, runningGame.Amount, false);
            await RemoveGame(runningGame);

            result = await _botConfigurations.RollDiceStopGame(chatter, dice2, runningGame.Chatter, dice1, winner, runningGame.Amount);

            return result;

        }

        public async Task<string> StopGame()
        {
            string result = string.Empty;
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                _logger.LogError($"{DateTime.Now} [{this.GetType().Name + ":" + MethodBase.GetCurrentMethod()?.Name}]:  runningGame == null");
                return result;
            }

            //We get here if no one joined the game, cuz if they did, they would go through PlayGame()

            //If Bot started the game and yet no one started, no need to roll
            if (runningGame.Chatter == TwitchInfo.botUsername)
            {
                return await _botConfigurations.RollDiceNoPlayerJoined();
            }

            //roll a dice on behalf of Bot
            int dice1 = BotFunctions.RndInt(1, 6);
            int dice2 = BotFunctions.RndInt(1, 6);
            string winner;

            if (dice1 > dice2)
            {
                winner = "You";
                await _botFunctions.SetLoyaltyPoint(winner, runningGame.Amount);
            }
            else if (dice1 < dice2)
            {
                winner = "I";
                await _botFunctions.SetLoyaltyPoint(winner, runningGame.Amount, false);
            }
            else
            {
                await RemoveGame(runningGame);
                return await _botConfigurations.RollDiceDraw();
            }

            result = await _botConfigurations.RolLDiceStopGameWithBot(runningGame.Chatter, dice2, dice1, winner);
            await RemoveGame(runningGame);

            return result;
        }
    }
}
