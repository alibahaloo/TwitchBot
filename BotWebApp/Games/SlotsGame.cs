using TwitchBot.Bot;
namespace TwitchBot.Games
{


    public class SlotsGame
    {
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;

        public SlotsGame(BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        public async Task<string> PlayGame(string chatter)
        {
            string result = string.Empty;

            int userPoints = await _botFunctions.GetLoyaltyPoint(chatter);

            if (userPoints < await _botConfigurations.SlotsEntryAmount())
                return await _botConfigurations.SlotsNotEnoughPoints(chatter);

            var random = new Random();
            var icons = await _botConfigurations.SlotsIcons();
            //Generate 3 strs from the icons
            string img1, img2, img3;

            img1 = icons[random.Next(0, icons.Length)];
            img2 = icons[random.Next(0, icons.Length)];
            img3 = icons[random.Next(0, icons.Length)];

            if ((String.Compare(img1, img2) == 0 && String.Compare(img2, img3) == 0))
            {
                await _botFunctions.SetLoyaltyPoint(chatter, await _botConfigurations.SlotsReward() - await _botConfigurations.SlotsEntryAmount());
                result = await _botConfigurations.SlotsWin(chatter, img1, img2, img3);
            }
            else
            {
                await _botFunctions.SetLoyaltyPoint(chatter, await _botConfigurations.SlotsEntryAmount(), false);
                result = await _botConfigurations.SlotsLost(chatter, img1, img2, img3);
            }

            return result;
        }
    }
}
