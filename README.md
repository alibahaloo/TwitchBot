# TwitchBot
**This file includes explanations on how the bot application works and how to set it up.**

## What is this?
A Twitch Bot with its own loyalty program and games, fully customizable through a web UI (hosted within the same app)

## Technical Architecture
This is web application that runs a Twitch Bot service in the background. The application uses [TwitchLib](https://github.com/TwitchLib/TwitchLib) library (Nuget Package) to communicate with Twitch chat and to utilize Twitch PubSub service. 

The app is written in .NET 6, the web app is done in Razor Pages while the Bot service is implemented as a background service (for the purpose of long-running services and functionalities).

The web part is used for authentication purposes; obtaining a code from Twitch with which Access and Refresh Tokens can be obtained and thus the app can communicate with Twitch API endpoints, as well as customizing the bot's chat messages and other configurations.

Uses [SQLite](https://sqlite.org/) local database.

The application takes an optional argument when starting (`configure`). If the app starts with this argument, it will not start the bot service and the web UI will be started only as the intention is for configuration only.

### Authentication with Twitch
The bot uses [API endpoints from Twitch](https://dev.twitch.tv/docs/api/reference/) to serve some of its functionalities and games; to be exact, the loyalty program which passively gives points to chatters as they watch the stream, and some games require this. This functionality requires access to [Chatters](https://dev.twitch.tv/docs/api/reference/#get-chatters) endpoint.
For that, the app needs to be authenticated with Twitch.
For this purpose, the application uses `Authorization code grant flow` to obtain a [User Access Token](https://dev.twitch.tv/docs/authentication/#user-access-tokens).

While the application is running, navigate to `/code` page. The page will check if there's an existing and valid Access and Refresh token in the database, and an appropriate message is displayed. 
If there's no valid token in the system, we need to obtain one from Twitch. A link is constructed to make things easy, however the bot needs to be registered at Twitch Developer Console and the proper Application URI needs to be added.


## High level functionalities and features
- **Loyalty program**: Reward chatters (viewers) with points. The points then can be used in various games.
- **Games**: The bot provides both games initiated by the bot itself, as well as provide functions for chatters to start games on their own.
- **Periodic reminders**: Send periodic custom messages to the chat.

## Games:
The games mentioned above are ran periodically by bot. There is a global timer that can be configured, on every tick of that timer, a random game is selected and the bot runs it. The bot keeps track of the previously ran game to avoid running the same game twice in a row.

Each game has its own stop timer which basically tells the bot the stop the game with triggered. For example, the *The Raffle* runs for 2 minutes and allows chatters to buy tickets, after 2 minutes, the timer will trigger and will stop the game.

- **Gamble**: Chatters can use their points to gamble. The chances are 50/50 to win.
- **Daily Spin**: Every chatter can run a daily spin for free and get a chance to win a random amount of points.
- **Random Drops**: Periodically, a random chatter (current viewers in the chat) will drop a portion of their points. The portion is a random amount between 10 and a maximum of 75% of their points. So if you have 100 points, the bot could randomly select an amount between 10 and 75. If you have less than 10 points, you will not be chosen.
- **Play-To-Win**: Every once in a while, the bot will start a new game and send a message to invite chatters to play the game. Once the message is fired, chatters will have 2 mins to join the game by typing !play. After this period, the bot will randomly choose a chatter and reward them with points.
- **First-To-Win**: Periodically points become available through various methods. Chatters then can grab them. Whoever is first will win the points.
- **Raffle**: Periodically a raffle game starts where chatters can purchase tickets to enter for a chance to win points. The more tickets purchased, the higher the chance to win.
- **Slots**: Chatters can play the slot machines and have a chance to win points.
- **Roll-A-Dice**: Chatters can start a game of Roll-A-Dice. Optionally they can choose the opponent they wish to play against, if not the game will start and will be open to any chatter to join. If no chatter is available to play as the opponent, the bot will. The will also initiate this game once in a while.
- **Battle**: Periodically a two-player game  starts and the bot asks chatters to join. There should be two players, and a winner is chosen randomly (50/50 chance). The chosen one will win a random amount of points.
If no one joins the battle in 2 minutes, the game will stop and no points are rewarded. If one player joins but there's no second player, the bot will act as the opponent and the battle begins.

## Prerequisite to setup and run the application:
1. **Register your app (bot) on Twitch Developer Console**: [Link](https://dev.twitch.tv/docs/authentication/register-app/) - Make sure your application URL is added. This will give you `Client ID` and `Secret ID`
2. **Get OAuth token for your bot**: The bot needs its own dedicated Twitch account. Register for an account and give permissions to it to obtain an Oauth Token. This token is used for communicating to Twitch chat. [Authorize and obtain OAuth token](https://twitchapps.com/tmi/)
3. **Find your channel's ID**: You can use [this]( https://chrome.google.com/webstore/detail/twitch-username-and-user/laonpoebfalkjijglbjbnkfndibbcoon) Google Extension to find your channel's ID.
4. **Database**: Run database migrations using `dotnet ef migrations add InitialDBCreate`
5. **Resource file**: The required information mentioned above should all be included as a resource file under the project - See details below. Edit `TwitchInfo.resx` and add the required information.

### Resource file items:
- **BotToken**: This is the token obtain from step (2)
- **botUsername**: This is the username account created for bot. In step (2)
- **ChannelID**: Your channel's ID. Obtain in step (3)
- **ChannelName**: Your channel's name.
- **client_id**: Obtained after registering your app in step (1)
- **client_secret**: Obtained after registering your app in step (1)
- **redirect_uri**: Obtained after registering your app in step (1)
- **scope**: For Chatters endpoints we'd need `moderator:read:chatters`. This is used to obtain the code and tokens from Twitch which are needed for communicating with Twitch APIs.

**NOTE**: The resource file is required for bot service to be initialized, without it the bot will not start.

## Configuration
All configurations can be done via the web page `/Config/Variables` and `/Config/Messages`.
There are default values for each configuration, however you can edit them and the bot will read the newly edited values from that point onward.

All times below are minutes.
### Loyalty tick timer and reward 
`Loyalty Point Per Tick` Defines the period after which loyalty points are rewarded.

`Loyalty Tick Timer` Defines the points given after each timer trigger.

The example above will reward 10 points after 10 minutes of watching the stream.

### Bot
`BotMods` List of mods that are allowed to start games via commands.

`BotGames` List of all available games. The bot will look at this list to randomly select a game. 
**NOTE**: If you wish to extend this bot and add new games, make sure you add the new game to this list so the bot will know to run them automatically.

### Global game timer
`Global Games Timer` Defines the period to randomly run a game.

### Games specific configurations
Most games have specific configurations that can be set individually. For example **The Raffle** game has:
  
`Stop Timer` Stop the raffle after x minutes

`Ticket Cost` Each ticket costs x points to enter the raffle

`Max Ticket Allowed` Chatters are allowed to purchase up to 100 tickets for each game

`Min Reward` Min reward that can the given game gives.

`Max Reward` Max reward that can the given game gives.

For the rest of the game's configurations, refer to Web UI.
