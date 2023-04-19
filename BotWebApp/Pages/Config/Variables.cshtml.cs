using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwitchBot.Bot;

namespace TwitchBot.Pages.Config
{
    public class VariablesModel : PageModel
    {
        public enum Configurations
        {
            RaffleStopMins,
            RaffleTicketCost,
            RaffleMaxTicketAllowed,
            RaffleMinReward,
            RaffleMaxReward,
            PlayToWinStopMins,
            PlayToWinMinReward,
            PlayToWinMaxReward,
            PlayToWinGames,
            BattleStopMins,
            BattleMinReward,
            BattleMaxReward,
            BattleGames,
            RandomDropStopMins,
            RandomDropMinPoints,
            RandomDropMaxPercentage,
            AvoidedChatters,
            FirstToWinStopMins,
            FirstToWinMinReward,
            FirstToWinMaxReward,
            FirstToWinGames,
            GambleDefaultAmount,
            SlotsEntryAmount,
            SlotsReward,
            SlotsIcons,
            DailySpinMaxReward,
            DailySpinMinReward,
            BotMods,
            GlobalGamesTimer,
            PlayerNames,
            BotGames,
            LoyaltyPointPerTick,
            LoyaltyTickTimer,
            RollADiceStopTimer,
            RandomAIQuoteTimer,
            AIQuotes,
        }


        private readonly ILogger<VariablesModel> _logger;

        public bool SuccessfulSave = false;

        private readonly BotConfigurations _botConfigurations;
        public int RaffleStopMins = 0;
        public int RaffleTicketCost = 0;
        public int RaffleMaxTicketAllowed = 0;
        public int RaffleMinReward = 0;
        public int RaffleMaxReward = 0;

        public int PlayToWinStopMins = 0;
        public int PlayToWinMinReward = 0;
        public int PlayToWinMaxReward = 0;
        public string PlayToWinGames = string.Empty;

        public int BattleStopMins = 0;
        public int BattleMinReward = 0;
        public int BattleMaxReward = 0;
        public string BattleGames = string.Empty;

        public int RandomDropStopMins = 0;
        public int RandomDropMinPoints = 0;
        public int RandomDropMaxPercentage = 0;
        public string AvoidedChatters = string.Empty;

        public int FirstToWinStopMins = 0;
        public int FirstToWinMinReward = 0;
        public int FirstToWinMaxReward = 0;
        public string FirstToWinGames = string.Empty;

        public int GambleDefaultAmount = 0;

        public int SlotsEntryAmount = 0;
        public int SlotsReward = 0;
        public string SlotsIcons = string.Empty;

        public int DailySpinMaxReward = 0;
        public int DailySpinMinReward = 0;

        public string BotMods = string.Empty;
        public int GlobalGamesTimer = 0;
        public string PlayerNames = string.Empty;
        public string BotGames = string.Empty;

        public int LoyaltyPointPerTick = 0;
        public int LoyaltyTickTimer = 0;

        public int RollADiceStopTimer = 0;

        public int RandomAIQuoteTimer = 0;
        public string AIQuotes = string.Empty;
        public VariablesModel(BotConfigurations botConfigurations, ILogger<VariablesModel> logger)
        {
            _botConfigurations = botConfigurations;
            _logger = logger;
        }
        private async Task LoadData()
        {
            RaffleStopMins = await _botConfigurations.RaffleStopMins();
            RaffleTicketCost = await _botConfigurations.RaffleTicketCost();
            RaffleMaxTicketAllowed = await _botConfigurations.RaffleMaxTicketAllowed();
            RaffleMinReward = await _botConfigurations.RaffleMinReward();
            RaffleMaxReward = await _botConfigurations.RaffleMaxReward();

            PlayToWinStopMins = await _botConfigurations.PlayToWinStopMins();
            PlayToWinMinReward = await _botConfigurations.PlayToWinMinReward();
            PlayToWinMaxReward = await _botConfigurations.PlayToWinMaxReward();
            PlayToWinGames = String.Join("\r\n", await _botConfigurations.PlayToWinGames());

            BattleStopMins = await _botConfigurations.BattleStopMins();
            BattleMinReward = await _botConfigurations.BattleMinReward();
            BattleMaxReward = await _botConfigurations.BattleMaxReward();
            BattleGames = String.Join("\r\n", await _botConfigurations.BattleGames());

            RandomDropStopMins = await _botConfigurations.RandomDropStopMins();
            RandomDropMinPoints = await _botConfigurations.RandomDropMinPoints();
            RandomDropMaxPercentage = await _botConfigurations.RandomDropMaxPercentage();
            AvoidedChatters = String.Join("\r\n", await _botConfigurations.AvoidedChatters());

            FirstToWinStopMins = await _botConfigurations.FirstToWinStopMins();
            FirstToWinMinReward = await _botConfigurations.FirstToWinMinReward();
            FirstToWinMaxReward = await _botConfigurations.FirstToWinMaxReward();
            FirstToWinGames = String.Join("\r\n", await _botConfigurations.FirstToWinGames());

            GambleDefaultAmount = await _botConfigurations.GambleDefaultAmount();

            SlotsEntryAmount = await _botConfigurations.SlotsEntryAmount();
            SlotsReward = await _botConfigurations.SlotsReward();
            SlotsIcons = String.Join("\r\n", await _botConfigurations.SlotsIcons());

            DailySpinMaxReward = await _botConfigurations.DailySpinMaxReward();
            DailySpinMinReward = await _botConfigurations.DailySpinMinReward();

            BotMods = String.Join("\r\n", await _botConfigurations.BotMods());
            GlobalGamesTimer = await _botConfigurations.GlobalGamesTimer();
            PlayerNames = String.Join("\r\n", await _botConfigurations.PlayerNames());
            BotGames = String.Join("\r\n", await _botConfigurations.BotGames());

            LoyaltyPointPerTick = await _botConfigurations.LoyaltyPointPerTick();
            LoyaltyTickTimer = await _botConfigurations.LoyaltyTickTimer();

            RollADiceStopTimer = await _botConfigurations.RollADiceStopTimer();

            RandomAIQuoteTimer = await _botConfigurations.RandomAIQuoteTimer();
            AIQuotes = String.Join("\r\n", await _botConfigurations.AIQuotes());
        }
        public async Task<IActionResult> OnPostSave()
        {
            foreach (var key in Enum.GetNames(typeof(Configurations)))
            {
                var _value = Request.Form[key].ToString().Trim();
                await _botConfigurations.SaveBotConfig(key, _value);
            }

            SuccessfulSave = true;

            await LoadData();
            return Page();
        }
        public async Task<IActionResult> OnPostReset()
        {
            foreach (var key in Enum.GetNames(typeof(Configurations)))
            {
                await _botConfigurations.DeleteBotConfig(key);
            }
            SuccessfulSave = true;
            await LoadData();
            return Page();
        }
        public async Task OnGet()
        {
            await LoadData();
        }

        public void OnPostShutdown()
        {
            _logger.LogWarning("Shutting down application via UI");
            System.Environment.Exit(1);
        }
    }
}
