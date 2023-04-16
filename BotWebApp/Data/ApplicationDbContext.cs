using Microsoft.EntityFrameworkCore;

namespace TwitchBot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Dailyspin> Dailyspins { get; set; }
        public DbSet<RandomDrop> RandomDrops { get; set; }
        public DbSet<PlayToWin> PlayToWins { get; set; }
        public DbSet<PlayToWinPlayer> PlayToWinPlayers { get; set; }
        public DbSet<FirstToWin> FirstToWins { get; set; }
        public DbSet<Raffle> Raffles { get; set; }
        public DbSet<RaffleTicket> RaffleTickets { get; set; }
        public DbSet<LastGame> LastGames { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<RollDice> RollDices { get; set; }
        public DbSet<Battle> Battles { get; set; }
        public DbSet<TwitchCode> TwitchCodes { get; set; }
        public DbSet<BotConfig> BotConfigs { get; set; }
        public string DbPath { get; }
        public ApplicationDbContext()
        {
            //C:\Users\USER\AppData
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "TwitchBot.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public class TwitchCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class LastGame
    {
        public int Id { get; set; }
        public string Game { get; set; } = null!;
    }
    public class Dailyspin
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Lastspin { get; set; } = null!;
    }
    public class RandomDrop
    {
        public int Id { get; set; }
        public string Chatter { get; set; } = null!;
        public int Amount { get; set; }
    }
    public class FirstToWin
    {
        public int Id { get; set; }
        public int Amount { get; set; }
    }
    public class PlayToWin
    {
        public int Id { get; set; }
        public int RewardAmount { get; set; }
        public ICollection<PlayToWinPlayer> Players { get; set; } = null!;
    }
    public class PlayToWinPlayer
    {
        public int Id { get; set; }
        public string Chatter { get; set; } = null!;
        public int PlayToWinId { get; set; }
        public PlayToWin PlayToWin { get; set; } = null!;
    }
    public class Raffle
    {
        public int Id { get; set; }
        public int RewardAmount { get; set; }
        public ICollection<RaffleTicket> Tickets { get; set; } = null!;
    }
    public class RaffleTicket
    {
        public int Id { get; set; }
        public string Chatter { get; set; } = null!;
        public int RaffleId { get; set; }
        public Raffle Raffle { get; set; } = null!;
    }
    public class LoyaltyPoint
    {
        public int Id { get; set; }
        public string Chatter { get; set; } = null!;
        public int Amount { get; set; }
    }
    public class RollDice
    {
        public int Id { get; set; }
        public string Chatter { get; set; } = null!;
        public string Opponent { get; set; } = null!;
        public int Amount { get; set; }
    }
    public class Battle
    {
        public int Id { get; set; }
        public string FirstPlayer { get; set; } = null!;
        public int Amount { get; set; }
    }
    public class BotConfig
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
