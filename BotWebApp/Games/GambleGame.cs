using TwitchBot.Bot;
namespace TwitchBot.Games
{
    /// <summary>
    /// User's will gamble with their points, whatever they put in, they'll get a chance to win (double)
    /// minumum to gamble with 10 points
    /// maximum ALL
    /// accepted params: !gamble 100, !gamble 25%
    /// </summary>
    public class GambleGame
    {
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;
        public GambleGame(BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        public async Task<string> PlayGame(string chatter, string message)
        {
            string result = string.Empty;

            string[] messageParts = message.Split(' ');
            if (messageParts.Length > 2) return await _botConfigurations.GamblePlayInvalidCommand(chatter);

            int gambleAmount;
            //Get user's current points
            int userPoints = await _botFunctions.GetLoyaltyPoint(chatter);

            if (messageParts.Length == 1)
            {
                gambleAmount = await _botConfigurations.GambleDefaultAmount();
            }
            else
            {
                //Check if gamble amount is in points or percentage
                if (messageParts[1].Contains("%"))
                {
                    //Check if % is at the end
                    if (!messageParts[1].EndsWith("%"))
                        return await _botConfigurations.GamblePlayInvalidPercentageCommand(chatter);

                    //Removing % from the command
                    string percentageValueStr = messageParts[1].Remove(messageParts[1].Length - 1);

                    //Try to parse it as int
                    if (!Int32.TryParse(percentageValueStr, out int percentageValue))
                        return await _botConfigurations.GamblePlayInvalidNumberCommand(chatter);

                    if (percentageValue > 100)
                        return await _botConfigurations.GamblePlayInvalidPercentageMoreThanHundred(chatter);

                    //Calculate gamble amount based on the given percentage
                    gambleAmount = (userPoints * percentageValue) / 100;
                }
                else if (messageParts[1].ToLower().Equals("all"))
                {
                    gambleAmount = userPoints;
                }
                else
                {
                    //Try to parse amount as int
                    if (!Int32.TryParse(messageParts[1], out gambleAmount))
                        return await _botConfigurations.GamblePlayInvalidNumberCommand(chatter);
                }
            }

            //check if gamble amount is negative
            if (gambleAmount < 0)
                return await _botConfigurations.GamblePlayInvalidNegativeNumber(chatter);

            if (gambleAmount < 10)
                return await _botConfigurations.GamblePlayInvalidLessThanDefault(chatter);

            //Check if user can afford it
            if (userPoints < gambleAmount)
                return await _botConfigurations.GamblePlayNotEnoughPoints(chatter);

            //if we get here, all is good, 
            int gambleResult = BotFunctions.RndInt(0, 1);

            switch (gambleResult)
            {
                case 0:
                    //Lost the game
                    await _botFunctions.SetLoyaltyPoint(chatter, gambleAmount, false);
                    result = await _botConfigurations.GambleLost(chatter, gambleAmount);
                    break;
                case 1:
                    //Won the game
                    await _botFunctions.SetLoyaltyPoint(chatter, gambleAmount * 2);
                    result = await _botConfigurations.GambleWon(chatter, gambleAmount);
                    break;
            }

            return result;
        }
    }
}
