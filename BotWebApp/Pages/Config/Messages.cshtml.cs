using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwitchBot.Bot;
namespace TwitchBot.Pages.Config
{
    public class MessagesModel : PageModel
    {
        public enum Configurations
        {
            CantStartNewGame,
            NoGameRunning,
            DiscordChannel,
            CommandsList,
            UserPointBalance,
            TransferPointsInvalidCommand,
            TransferPointsInvalidNumber,
            TransferPointsInvalidToSelf,
            TransferPointsInvalidNotEnoughPoints,
            TransferPointsSuccess,
            RaffleMaxTicketsBuy,
            RaffleMaxTicketReached,
            RaffleNoOneJoined,
            RaffleStartGame,
            RaffleStopGame,
            RaffleInvalidBuyCommand,
            RaffleNotEnoughPoints,
            RaffleBuySuccess,
            DailySpinNeedToWait,
            DailySpinStartGame,
            BattleGenerateRandomTitle,
            BattleFirstPlayerJoined,
            BattleAlreadyJoined,
            BattleSecondPlayerJoined,
            BattleBattleFinished,
            BattleNoOneJoined,
            BattleNoSecondPlayerJoined,
            BattleGameStopYouWon,
            BattleGameStopBotWon,
            FirstToWinGenerateRandomTitle,
            FirstToWinGameStop,
            FirstToWinNoOneJoined,
            GamblePlayInvalidCommand,
            GamblePlayInvalidPercentageCommand,
            GamblePlayInvalidPercentageMoreThanHundred,
            GamblePlayInvalidNumberCommand,
            GamblePlayInvalidNegativeNumber,
            GamblePlayInvalidLessThanDefault,
            GamblePlayNotEnoughPoints,
            GambleLost,
            GambleWon,
            PlayToWinGenerateRandomTitle,
            PlayToWinAlreadyInGame,
            PlayToWinJoined,
            PlayToWinNoOneJoined,
            PlayToWinStopGame,
            RandomDropOwnerGrabbed,
            RandomDropChatterGrabbed,
            RandomDropNoOneJoined,
            RandomDropStartGame,
            RollDiceInvalidCommand,
            RollDiceInvalidCommandNumber,
            RollDiceInvalidCommandLessThanDefault,
            RollDiceNotEnoughPoints,
            RollDiceInvalidCommandAgainstYourself,
            RollDiceNotEnoughPointOpponent,
            RollDiceGameStartWithOpponent,
            RollDiceGameStartOpen,
            RolLDicePlayAgainstYourself,
            RolLDiceNotTheSelectedOpponent,
            RollDicNotEnoughPoint,
            RollDiceDraw,
            RollDiceStopGame,
            RolLDiceStopGameWithBot,
            RollDiceNoPlayerJoined,
            SlotsNotEnoughPoints,
            SlotsWin,
            SlotsLost,
        }

        public bool SuccessfulSave = false;

        public readonly BotConfigurations _botConfigurations;
        public string CantStartNewGame = string.Empty;
        public string NoGameRunning = string.Empty;
        public string DiscordChannel = string.Empty;
        public string CommandsList = string.Empty;

        public string UserPointBalance = string.Empty;
        public string TransferPointsInvalidCommand = string.Empty;
        public string TransferPointsInvalidNumber = string.Empty;
        public string TransferPointsInvalidToSelf = string.Empty;
        public string TransferPointsInvalidNotEnoughPoints = string.Empty;
        public string TransferPointsSuccess = string.Empty;

        public string RaffleMaxTicketsBuy = string.Empty;
        public string RaffleMaxTicketReached = string.Empty;
        public string RaffleNoOneJoined = string.Empty;
        public string RaffleStartGame = string.Empty;
        public string RaffleStopGame = string.Empty;
        public string RaffleInvalidBuyCommand = string.Empty;
        public string RaffleNotEnoughPoints = string.Empty;
        public string RaffleBuySuccess = string.Empty;

        public string DailySpinNeedToWait = string.Empty;
        public string DailySpinStartGame = string.Empty;

        public string BattleGenerateRandomTitle = string.Empty;
        public string BattleFirstPlayerJoined = string.Empty;
        public string BattleAlreadyJoined = string.Empty;
        public string BattleSecondPlayerJoined = string.Empty;
        public string BattleBattleFinished = string.Empty;
        public string BattleNoOneJoined = string.Empty;
        public string BattleNoSecondPlayerJoined = string.Empty;
        public string BattleGameStopYouWon = string.Empty;
        public string BattleGameStopBotWon = string.Empty;

        public string FirstToWinGenerateRandomTitle = string.Empty;
        public string FirstToWinGameStop = string.Empty;
        public string FirstToWinNoOneJoined = string.Empty;

        public string GamblePlayInvalidCommand = string.Empty;
        public string GamblePlayInvalidPercentageCommand = string.Empty;
        public string GamblePlayInvalidPercentageMoreThanHundred = string.Empty;
        public string GamblePlayInvalidNumberCommand = string.Empty;
        public string GamblePlayInvalidNegativeNumber = string.Empty;
        public string GamblePlayInvalidLessThanDefault = string.Empty;
        public string GamblePlayNotEnoughPoints = string.Empty;
        public string GambleLost = string.Empty;
        public string GambleWon = string.Empty;

        public string PlayToWinGenerateRandomTitle = string.Empty;
        public string PlayToWinAlreadyInGame = string.Empty;
        public string PlayToWinJoined = string.Empty;
        public string PlayToWinNoOneJoined = string.Empty;
        public string PlayToWinStopGame = string.Empty;

        public string RandomDropOwnerGrabbed = string.Empty;
        public string RandomDropChatterGrabbed = string.Empty;
        public string RandomDropNoOneJoined = string.Empty;
        public string RandomDropStartGame = string.Empty;

        public string RollDiceInvalidCommand = string.Empty;
        public string RollDiceInvalidCommandNumber = string.Empty;
        public string RollDiceInvalidCommandLessThanDefault = string.Empty;
        public string RollDiceNotEnoughPoints = string.Empty;
        public string RollDiceInvalidCommandAgainstYourself = string.Empty;
        public string RollDiceNotEnoughPointOpponent = string.Empty;
        public string RollDiceGameStartWithOpponent = string.Empty;
        public string RollDiceGameStartOpen = string.Empty;
        public string RolLDicePlayAgainstYourself = string.Empty;
        public string RolLDiceNotTheSelectedOpponent = string.Empty;
        public string RollDicNotEnoughPoint = string.Empty;
        public string RollDiceDraw = string.Empty;
        public string RollDiceStopGame = string.Empty;
        public string RolLDiceStopGameWithBot = string.Empty;
        public string RollDiceNoPlayerJoined = string.Empty;

        public string SlotsNotEnoughPoints = string.Empty;
        public string SlotsWin = string.Empty;
        public string SlotsLost = string.Empty;
        public MessagesModel(BotConfigurations botMessages)
        {
            _botConfigurations = botMessages;
        }

        private async Task LoadData()
        {
            CantStartNewGame = await _botConfigurations.CantStartNewGame();
            NoGameRunning = await _botConfigurations.NoGameRunning();
            DiscordChannel = await _botConfigurations.DiscordChannel();
            CommandsList = await _botConfigurations.CommandsList();

            UserPointBalance = await _botConfigurations.UserPointBalance("", 0, true);
            TransferPointsInvalidCommand = await _botConfigurations.TransferPointsInvalidCommand("", true);
            TransferPointsInvalidNumber = await _botConfigurations.TransferPointsInvalidNumber("", true);
            TransferPointsInvalidToSelf = await _botConfigurations.TransferPointsInvalidToSelf("", true);
            TransferPointsInvalidNotEnoughPoints = await _botConfigurations.TransferPointsInvalidNotEnoughPoints("", true);
            TransferPointsSuccess = await _botConfigurations.TransferPointsSuccess("", 0, "", true);

            RaffleMaxTicketsBuy = await _botConfigurations.RaffleMaxTicketsBuy("", true);
            RaffleMaxTicketReached = await _botConfigurations.RaffleMaxTicketReached("", 0, true);
            RaffleNoOneJoined = await _botConfigurations.RaffleNoOneJoined();
            RaffleStartGame = await _botConfigurations.RaffleStartGame(0, true);
            RaffleStopGame = await _botConfigurations.RaffleStopGame("", 0, true);
            RaffleInvalidBuyCommand = await _botConfigurations.RaffleInvalidBuyCommand();
            RaffleNotEnoughPoints = await _botConfigurations.RaffleNotEnoughPoints("", true);
            RaffleBuySuccess = await _botConfigurations.RaffleBuySuccess("", 0, true);

            DailySpinNeedToWait = await _botConfigurations.DailySpinNeedToWait("", 0, true);
            DailySpinStartGame = await _botConfigurations.DailySpinStartGame("", 0, true);

            BattleGenerateRandomTitle = await _botConfigurations.BattleGenerateRandomTitle(0, 0, true);
            BattleFirstPlayerJoined = await _botConfigurations.BattleFirstPlayerJoined("", true);
            BattleAlreadyJoined = await _botConfigurations.BattleAlreadyJoined("", true);
            BattleSecondPlayerJoined = await _botConfigurations.BattleSecondPlayerJoined("", true);
            BattleBattleFinished = await _botConfigurations.BattleBattleFinished("", 0, true);
            BattleNoOneJoined = await _botConfigurations.BattleNoOneJoined();
            BattleNoSecondPlayerJoined = await _botConfigurations.BattleNoSecondPlayerJoined();
            BattleGameStopYouWon = await _botConfigurations.BattleGameStopYouWon("", 0, true);
            BattleGameStopBotWon = await _botConfigurations.BattleGameStopBotWon();

            FirstToWinGenerateRandomTitle = await _botConfigurations.FirstToWinGenerateRandomTitle(0, 0, true);
            FirstToWinGameStop = await _botConfigurations.FirstToWinGameStop("", 0, true);
            FirstToWinNoOneJoined = await _botConfigurations.FirstToWinNoOneJoined();

            GamblePlayInvalidCommand = await _botConfigurations.GamblePlayInvalidCommand("", true);
            GamblePlayInvalidPercentageCommand = await _botConfigurations.GamblePlayInvalidPercentageCommand("", true);
            GamblePlayInvalidPercentageMoreThanHundred = await _botConfigurations.GamblePlayInvalidPercentageMoreThanHundred("", true);
            GamblePlayInvalidNumberCommand = await _botConfigurations.GamblePlayInvalidNumberCommand("", true);
            GamblePlayInvalidNegativeNumber = await _botConfigurations.GamblePlayInvalidNegativeNumber("", true);
            GamblePlayInvalidLessThanDefault = await _botConfigurations.GamblePlayInvalidLessThanDefault("", true);
            GamblePlayNotEnoughPoints = await _botConfigurations.GamblePlayNotEnoughPoints("", true);
            GambleLost = await _botConfigurations.GambleLost("", 0, true);
            GambleWon = await _botConfigurations.GambleWon("", 0, true);

            PlayToWinGenerateRandomTitle = await _botConfigurations.PlayToWinGenerateRandomTitle(0, 0, 0, true);
            PlayToWinAlreadyInGame = await _botConfigurations.PlayToWinAlreadyInGame("", true);
            PlayToWinJoined = await _botConfigurations.PlayToWinJoined("", true);
            PlayToWinNoOneJoined = await _botConfigurations.PlayToWinNoOneJoined();
            PlayToWinStopGame = await _botConfigurations.PlayToWinStopGame("", 0, true);

            RandomDropOwnerGrabbed = await _botConfigurations.RandomDropOwnerGrabbed("", true);
            RandomDropChatterGrabbed = await _botConfigurations.RandomDropChatterGrabbed("", 0, "", true);
            RandomDropNoOneJoined = await _botConfigurations.RandomDropNoOneJoined();
            RandomDropStartGame = await _botConfigurations.RandomDropStartGame("", 0, true);

            RollDiceInvalidCommand = await _botConfigurations.RollDiceInvalidCommand("", true);
            RollDiceInvalidCommandNumber = await _botConfigurations.RollDiceInvalidCommandNumber("", true);
            RollDiceInvalidCommandLessThanDefault = await _botConfigurations.RollDiceInvalidCommandLessThanDefault("", true);
            RollDiceNotEnoughPoints = await _botConfigurations.RollDiceNotEnoughPoints("", true);
            RollDiceInvalidCommandAgainstYourself = await _botConfigurations.RollDiceInvalidCommandAgainstYourself("", true);
            RollDiceNotEnoughPointOpponent = await _botConfigurations.RollDiceNotEnoughPointOpponent("", "", true);
            RollDiceGameStartWithOpponent = await _botConfigurations.RollDiceGameStartWithOpponent("", "", 0, true);
            RollDiceGameStartOpen = await _botConfigurations.RollDiceGameStartOpen("", 0, true);
            RolLDicePlayAgainstYourself = await _botConfigurations.RolLDicePlayAgainstYourself("", true);
            RolLDiceNotTheSelectedOpponent = await _botConfigurations.RolLDiceNotTheSelectedOpponent("", true);
            RollDicNotEnoughPoint = await _botConfigurations.RollDicNotEnoughPoint("", true);
            RollDiceDraw = await _botConfigurations.RollDiceDraw();
            RollDiceStopGame = await _botConfigurations.RollDiceStopGame("", 0, "", 0, "", 0, true);
            RolLDiceStopGameWithBot = await _botConfigurations.RolLDiceStopGameWithBot("", 0, 0, "", true);
            RollDiceNoPlayerJoined = await _botConfigurations.RollDiceNoPlayerJoined();

            SlotsNotEnoughPoints = await _botConfigurations.SlotsNotEnoughPoints("", true);
            SlotsWin = await _botConfigurations.SlotsWin("", "", "", "", true);
            SlotsLost = await _botConfigurations.SlotsLost("", "", "", "", true);
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
    }
}
