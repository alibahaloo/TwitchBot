using Microsoft.EntityFrameworkCore;
using TwitchBot.Bot;
using TwitchBot.Data;

namespace TwitchBot.Games
{
    public class DailyspinGame
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BotFunctions _botFunctions;
        private readonly BotConfigurations _botConfigurations;

        public DailyspinGame(IServiceProvider serviceProvider, BotFunctions botFunctions, BotConfigurations botConfiguration)
        {
            _serviceProvider = serviceProvider;
            _botFunctions = botFunctions;
            _botConfigurations = botConfiguration;
        }

        //Take a user, randomly select a number, then return points
        public async Task<string> StartGame(string username)
        {
            //Find the last time user has started the game
            var durationFromLastSpin = await GetDurationFromLastSpin(username);

            if (durationFromLastSpin >= 24)
            {
                //All good, allow the game
                //Get a random prize
                int prize = BotFunctions.RndInt(await _botConfigurations.DailySpinMinReward(), await _botConfigurations.DailySpinMaxReward());

                //Add the prize to user
                await _botFunctions.SetLoyaltyPoint(username, prize);

                //Record the last spin value
                RecordLastSpin(username);

                //Return success message
                return await _botConfigurations.DailySpinStartGame(username, prize);
            }
            else
            {
                //Not allowed
                return await _botConfigurations.DailySpinNeedToWait(username, durationFromLastSpin);
            }
        }

        private async Task<double> GetDurationFromLastSpin(string username)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var dailyspin = await _botDataContext.Dailyspins.Where(ds => ds.Username == username).FirstOrDefaultAsync();

            if (dailyspin == null) return 24;

            if (dailyspin.Lastspin == null) return 24;

            DateTime dsDT = DateTime.Parse(dailyspin.Lastspin);
            return (DateTime.Now - dsDT).TotalHours;
        }

        private async void RecordLastSpin(string username)
        {
            var _botDataContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var dailyspin = await _botDataContext.Dailyspins.Where(ds => ds.Username == username).FirstOrDefaultAsync();

            if (dailyspin != null)
            {
                dailyspin.Lastspin = DateTime.Now.ToString();
            }
            else
            {
                //No record found, so create one
                _botDataContext.Add(new Dailyspin { Username = username, Lastspin = DateTime.Now.ToString() });
            }

            await _botDataContext.SaveChangesAsync();
        }

    }
}
