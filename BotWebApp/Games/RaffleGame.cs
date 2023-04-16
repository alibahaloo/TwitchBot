using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    /// <summary>
    /// A game is preidoically started, lasts for 5 mins
    /// People can buy tickets 1 - 100
    /// Draw a ticket and reward points
    /// </summary>
    public class RaffleGame
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RaffleGame> _logger;
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;

        public RaffleGame(ILogger<RaffleGame> logger, IServiceProvider serviceProvider, BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        private async Task RecordGame(int rewardAmount)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            Raffle newGame = new() { RewardAmount = rewardAmount };

            await _botDataContext.AddAsync(newGame);
            await _botDataContext.SaveChangesAsync();

            await _botFunctions.RecordLastGame("Raffle");
        }

        private async Task RemoveGame(Raffle runningGame)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            _botDataContext.Remove(runningGame);
            await _botDataContext.SaveChangesAsync();
        }

        public async Task<Raffle?> GetRunningGame()
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await _botDataContext.Raffles.Include(x => x.Tickets).FirstOrDefaultAsync();
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
            int reward = BotFunctions.RndInt(await _botConfigurations.RaffleMinReward(), await _botConfigurations.RaffleMaxReward());
            //Generate Title
            string result = await _botConfigurations.RaffleStartGame(reward);
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

            if (runningGame.Tickets.Count == 0)
            {
                result = await _botConfigurations.RaffleNoOneJoined();
            }
            else
            {
                //Selecting a random winner from the list of players
                var random = new Random();
                int index = random.Next(runningGame.Tickets.Count);
                var winner = runningGame.Tickets.ElementAt(index);

                //Add the points to the user
                await _botFunctions.SetLoyaltyPoint(winner.Chatter, runningGame.RewardAmount);

                result = await _botConfigurations.RaffleStopGame(winner.Chatter, runningGame.RewardAmount);
            }

            _logger.LogInformation(BotConfigurations.Log("StopGame", result));
            //Remove the game
            await RemoveGame(runningGame);
            return result;
        }

        public async Task<string> BuyTicket(string chatter, string message)
        {
            string[] messageParts = message.Split(' ');
            int numberOfTickets;
            if (messageParts.Length > 2) return await _botConfigurations.RaffleInvalidBuyCommand();

            //Get the running game
            var runningGame = await GetRunningGame();

            if (runningGame == null)
            {
                return await _botConfigurations.NoGameRunning();
            }

            if (messageParts.Length == 1)
            {
                numberOfTickets = 1;
            }
            else
            {
                //check if commandParts[1] actually parsable
                if (!Int32.TryParse(messageParts[1], out numberOfTickets))
                    return await _botConfigurations.RaffleInvalidBuyCommand();

                if (numberOfTickets > await _botConfigurations.RaffleMaxTicketAllowed()) return await _botConfigurations.RaffleMaxTicketsBuy(chatter);
            }

            int currentNumberofTickets = runningGame.Tickets.Where(x => x.Chatter == chatter).Count();

            //Check if user has already bought the max allowed tickets
            if (currentNumberofTickets + numberOfTickets >= await _botConfigurations.RaffleMaxTicketAllowed() + 1)
            {
                return await _botConfigurations.RaffleMaxTicketReached(chatter, currentNumberofTickets);
            }

            int currentUserPoints = await _botFunctions.GetLoyaltyPoint(chatter);
            int raffleCost = await _botConfigurations.RaffleTicketCost();

            //check if user can afford buy the number of tickets
            if (currentUserPoints <= raffleCost * numberOfTickets)
            {
                return await _botConfigurations.RaffleNotEnoughPoints(chatter);
            }

            //Substract points from user
            await _botFunctions.SetLoyaltyPoint(chatter, numberOfTickets * raffleCost);

            //Add the player to the game
            RaffleTicket ticket = new() { Chatter = chatter, RaffleId = runningGame.Id };
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            await _botDataContext.AddAsync(ticket);
            await _botDataContext.SaveChangesAsync();

            return await _botConfigurations.RaffleBuySuccess(chatter, numberOfTickets);
        }
    }
}
