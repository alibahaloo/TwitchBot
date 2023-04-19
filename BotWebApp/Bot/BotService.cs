using System.Collections;
using System.Globalization;
using System.Resources;
using System.Timers;
using TwitchBot.Games;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace TwitchBot.Bot
{
    public class BotService : BackgroundService
    {
        //Loyalty Points
        private System.Timers.Timer _loyaltyPointTickTimer = new();
        //Games Timer
        private System.Timers.Timer _globalGamesStartTimer = new();
        //Raffle
        private System.Timers.Timer _raffleStopTimer = new();
        //RandomDrop
        private System.Timers.Timer _randomDropStopTimer = new();
        //PlayToWin
        private System.Timers.Timer _playToWinStopTimer = new();
        //FirstToWin
        private System.Timers.Timer _firstToWinStopTimer = new();
        //Roll-A-Dice
        private System.Timers.Timer _rollDiceStopTimer = new();
        //Two-Player Battle
        private System.Timers.Timer _battleStopTimer = new();
        //AI Random Quote
        private System.Timers.Timer _randomAIQuoteTimer = new();

        private readonly ILogger<BotService> _logger;
        private readonly BotConfigurations _botConfigurations;
        private readonly BotFunctions _botFunctions;
        private readonly RandomDropGame _randomDropGame;
        private readonly DailyspinGame _dailyspinGame;
        private readonly PlayToWinGame _playToWinGame;
        private readonly FirstToWinGame _firstToWinGame;
        private readonly RaffleGame _raffleGame;
        private readonly GambleGame _gambleGame;
        private readonly SlotsGame _slotsGame;
        private readonly RollDiceGame _rollDiceGame;
        private readonly BattleGame _battleGame;

        //Used for communicating with Twitch chat
        private readonly TwitchClient client;
        //Used for listening to Twitch chat events
        private readonly TwitchPubSub PubSub;

        public BotService(ILogger<BotService> logger,
                   BotFunctions botFunctions,
                   RandomDropGame randomDropGame,
                   DailyspinGame dailyspinGame,
                   PlayToWinGame playToWinGame,
                   FirstToWinGame firstToWinGame,
                   RaffleGame raffleGame,
                   GambleGame gambleGame,
                   SlotsGame slotsGame,
                   RollDiceGame rollDiceGame,
                   BattleGame battleGame,
                   BotConfigurations botConfigurations)
        {
            _logger = logger;
            _botFunctions = botFunctions;
            _randomDropGame = randomDropGame;
            _dailyspinGame = dailyspinGame;
            _playToWinGame = playToWinGame;
            _firstToWinGame = firstToWinGame;
            _raffleGame = raffleGame;
            _gambleGame = gambleGame;
            _slotsGame = slotsGame;
            _rollDiceGame = rollDiceGame;
            _battleGame = battleGame;
            _botConfigurations = botConfigurations;

            //Check for required resources
            CheckResourceFile();

            //Setting up Twitch Client
            ConnectionCredentials credentials = new(TwitchInfo.botUsername, TwitchInfo.BotToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, TwitchInfo.ChannelName);

            client.OnLog += Client_OnLog;
            client.OnError += Client_OnError;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnConnected += Client_OnConnected;

            //Set up Twitch PubSub
            PubSub = new TwitchPubSub();
            PubSub.OnListenResponse += OnListenResponse;
            PubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            PubSub.OnPubSubServiceClosed += OnPubSubServiceClosed;
            PubSub.OnPubSubServiceError += OnPubSubServiceError;

            PubSub.OnFollow += PubSub_OnFollow;
            PubSub.ListenToFollows(TwitchInfo.ChannelID);

            PubSub.OnViewCount += PubSub_OnViewCount;
            PubSub.ListenToVideoPlayback(TwitchInfo.ChannelID);
        }
        private void CheckResourceFile()
        {
            bool error = false;

            ResourceManager MyResourceClass = new(typeof(TwitchInfo));
            ResourceSet? resourceSet = MyResourceClass.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            if (resourceSet != null)
            {
                //Looping through all entries to make sure they're available
                foreach (DictionaryEntry entry in resourceSet)
                {
                    string? resourceKey = entry.Key.ToString();
                    object? resource = entry.Value;

                    if (((resource != null) && (resource.ToString() == string.Empty)) || (resource == null))
                    {
                        _logger.LogCritical(BotConfigurations.Log("CheckResourceFile", $"Missing Resource => {resourceKey}"));
                        error = true;
                    }

                }
            }
            else
            {
                error = true;
                _logger.LogCritical(BotConfigurations.Log("CheckResourceFile", $"Missing Resource File: TwitchInfo.resx"));
            }

            if (error)
            {
                //Exit the program if resources are not available
                _logger.LogCritical(BotConfigurations.Log("CheckResourceFile", "Exiting program with errors!"));
                System.Environment.Exit(1);
            }
        }

        #region Timers and Elapsed Events
        private async Task InitTimers()
        {
            _loyaltyPointTickTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.LoyaltyTickTimer()).TotalMilliseconds);
            _globalGamesStartTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.GlobalGamesTimer()).TotalMilliseconds);
            _raffleStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.RaffleStopMins()).TotalMilliseconds);
            _randomDropStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.RandomDropStopMins()).TotalMilliseconds);
            _playToWinStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.PlayToWinStopMins()).TotalMilliseconds);
            _firstToWinStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.FirstToWinStopMins()).TotalMilliseconds);
            _rollDiceStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.RollADiceStopTimer()).TotalMilliseconds);
            _battleStopTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.BattleStopMins()).TotalMilliseconds);
            _randomAIQuoteTimer =
                new(TimeSpan.FromMinutes(await _botConfigurations.RandomAIQuoteTimer()).TotalMilliseconds);

            #region Timers
            _loyaltyPointTickTimer.Elapsed += LoyaltyPointTickTimer_Elapsed;
            _loyaltyPointTickTimer.AutoReset = true;
            _loyaltyPointTickTimer.Enabled = true;

            _randomAIQuoteTimer.Elapsed += RandomAIQuoteTimer_Elapsed;
            _randomAIQuoteTimer.AutoReset = true;
            _randomAIQuoteTimer.Enabled = true;

            _globalGamesStartTimer.Elapsed += GlobalGamesStartTimer_Elapsed;
            _globalGamesStartTimer.AutoReset = true;
            _globalGamesStartTimer.Enabled = true;

            _firstToWinStopTimer.Elapsed += FirstToWinStopTimer_Elapsed;
            _firstToWinStopTimer.AutoReset = false;
            _firstToWinStopTimer.Enabled = false;

            _playToWinStopTimer.Elapsed += PlayToWinStopTimer_Elapsed;
            _playToWinStopTimer.AutoReset = false;
            _playToWinStopTimer.Enabled = false;

            _raffleStopTimer.Elapsed += RaffleStopTimer_Elapsed;
            _raffleStopTimer.AutoReset = false;
            _raffleStopTimer.Enabled = false;

            _randomDropStopTimer.Elapsed += RandomDropStopTimer_Elapsed;
            _randomDropStopTimer.AutoReset = false;
            _randomDropStopTimer.Enabled = false;

            _rollDiceStopTimer.Elapsed += RollDiceStopTimer_Elapsed;
            _rollDiceStopTimer.AutoReset = false;
            _rollDiceStopTimer.Enabled = false;

            _battleStopTimer.Elapsed += BattleStopTimer_Elapsed;
            _battleStopTimer.AutoReset = false;
            _battleStopTimer.Enabled = false;
            #endregion
        }
        private async void RandomAIQuoteTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            client.SendMessage(TwitchInfo.ChannelName, await _botFunctions.GetRandomAIQuoteAsync());
        }
        private async void LoyaltyPointTickTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            await _botFunctions.RecordLoyaltyPoints();
        }
        private async void GlobalGamesStartTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            string randomGame = await _botFunctions.GetRandomGame();
            string result = string.Empty;

            switch (randomGame)
            {
                case "Raffle":
                    result = await _raffleGame.StartGame(_raffleStopTimer);
                    break;
                case "RandomDrop":
                    result = await _randomDropGame.StartGame(_randomDropStopTimer);
                    break;
                case "FirstToWin":
                    result = await _firstToWinGame.StartGame(_firstToWinStopTimer);
                    break;
                case "PlayToWin":
                    result = await _playToWinGame.StartGame(_playToWinStopTimer);
                    break;
                case "Battle":
                    result = await _battleGame.StartGame(_battleStopTimer);
                    break;
                case "Rolldice":
                    int randomValue = BotFunctions.RndInt(10, 100);
                    result = await _rollDiceGame.StartGame(_rollDiceStopTimer, TwitchInfo.botUsername, $"!rolldice {randomValue}");
                    break;
            }

            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);

        }
        private async void BattleStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _battleGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private async void RollDiceStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _rollDiceGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private async void FirstToWinStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _firstToWinGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private async void PlayToWinStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _playToWinGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private async void RandomDropStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _randomDropGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private async void RaffleStopTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var result = await _raffleGame.StopGame();
            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        #endregion

        #region Client Functions
        private async void Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            string result = string.Empty;

            string command = e.Command.CommandText.ToLower().Trim();
            string message = e.Command.ChatMessage.Message.ToLower().Trim();
            string user = e.Command.ChatMessage.DisplayName.ToLower();

            //Check if the user is a mod
            var mods = await _botConfigurations.BotMods();
            bool isModUser = mods.Contains(user);

            if (isModUser)
            {
                switch (command)
                {
                    case "test":
                        result = "Hi Boss";
                        break;
                    case "add_points":
                        result = await _botFunctions.AddPointsToUser(command);
                        break;
                    case "start_raffle":
                        result = await _raffleGame.StartGame(_raffleStopTimer, user);
                        break;
                    case "start_randomdrop":
                        result = await _randomDropGame.StartGame(_randomDropStopTimer, user);
                        break;
                    case "start_playtowin":
                        result = await _playToWinGame.StartGame(_playToWinStopTimer, user);
                        break;
                    case "start_firsttowin":
                        result = await _firstToWinGame.StartGame(_firstToWinStopTimer, user);
                        break;
                    case "start_battle":
                        result = await _battleGame.StartGame(_battleStopTimer, user);
                        break;
                }
            }

            //Commands that everyone can use (including the owner, mods)
            switch (command)
            {
                case "bal":
                case "points":
                    result = await _botFunctions.GetUserPointBalance(user);
                    break;
                case "givepoints":
                    result = await _botFunctions.TransferLoyaltyPoints(user, message);
                    break;
                case "discord":
                    result = await _botConfigurations.DiscordChannel();
                    break;
                case "commands":
                    result = await _botConfigurations.CommandsList();
                    break;
                case "dailyspin":
                    result = await _dailyspinGame.StartGame(user);
                    break;
                case "grab":
                    result = await _randomDropGame.StopGame(user, _randomDropStopTimer);
                    break;
                case "play":
                    result = await _playToWinGame.PlayGame(user);
                    break;
                case "me":
                    result = await _firstToWinGame.StopGame(user, _firstToWinStopTimer);
                    break;
                case "buy":
                    result = await _raffleGame.BuyTicket(user, message);
                    break;
                case "gamble":
                    result = await _gambleGame.PlayGame(user, message);
                    break;
                case "slots":
                    result = await _slotsGame.PlayGame(user);
                    break;
                case "rolldice":
                    result = await _rollDiceGame.StartGame(_rollDiceStopTimer, user, message);
                    break;
                case "join":
                    result = await _battleGame.JoinBattle(_battleStopTimer, user);
                    break;
            }

            if (result != string.Empty) client.SendMessage(TwitchInfo.ChannelName, result);
        }
        private void Client_OnError(object? sender, OnErrorEventArgs e)
        {
            _logger.LogError(BotConfigurations.Log("Client_OnError", e.Exception.Message));
        }
        private void Client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            _logger.LogInformation(BotConfigurations.Log("Client_OnLog", $"{e.BotUsername} : {e.Data}"));
        }
        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            _logger.LogInformation(BotConfigurations.Log("Client_OnConnected", $"Connected to {e.AutoJoinChannel}"));
        }
        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation(BotConfigurations.Log("Client_OnJoinedChannel", $"Joined Channel {TwitchInfo.ChannelName}"));
        }
        #endregion

        #region PubSub Functions
        private void OnPubSubServiceError(object? sender, OnPubSubServiceErrorArgs e)
        {
            _logger.LogError(BotConfigurations.Log("OnPubSubServiceError", $"{e.Exception.Message}"));
        }
        private void OnPubSubServiceClosed(object? sender, EventArgs e)
        {
            _logger.LogInformation(BotConfigurations.Log("OnPubSubServiceClosed", "Connection closed to pubsub server."));
        }
        private void OnPubSubServiceConnected(object? sender, EventArgs e)
        {
            PubSub.SendTopics(TwitchInfo.BotToken);
            _logger.LogInformation(BotConfigurations.Log("OnPubSubServiceConnected", "Connected to pubsub server"));
        }
        private void OnListenResponse(object? sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
            {
                _logger.LogError(BotConfigurations.Log("OnListenResponse", $"Failed to listen! Response{e.Response}"));
            }
        }
        private void PubSub_OnFollow(object? sender, OnFollowArgs e)
        {
            client.SendMessage(TwitchInfo.ChannelName, $"Thank you for following @{e.Username} -- Hope you enjoy the stream <3");
            _logger.LogInformation(BotConfigurations.Log("PubSub_OnFollow", $"{e.Username} is now following."));
        }
        private void PubSub_OnViewCount(object? sender, OnViewCountArgs e)
        {
            _logger.LogInformation(BotConfigurations.Log("PubSub_OnViewCount", $"Current viewers: {e.Viewers}"));
        }
        #endregion
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot Hosted Service is starting.");
            //Initialize Timed Events
            await InitTimers();
            //Connect to Twitch
            client.Connect();
            PubSub.Connect();

            client.SendMessage(TwitchInfo.ChannelName, "Bot service has started -- Hi Chat");
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot Hosted Service is stopping.");
            client.SendMessage(TwitchInfo.ChannelName, "Bot service is shutting down -- Bye Bye");

            return Task.CompletedTask;
        }
    }
}