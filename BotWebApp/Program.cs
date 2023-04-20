using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TwitchBot.Bot;
using TwitchBot.Data;
using TwitchBot.Games;

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");

bool runAsConfigure;
bool argsError;
//Check for Arguments
if (args.Length > 0 && args.Length <2)
{
    switch (args[0])
    {
        case "configure":
            runAsConfigure = true;
            argsError = false;
            break;
        default:
            runAsConfigure = false;
            argsError = true;
            break;
    }
} else if (args.Length == 0)
{
    runAsConfigure = false;
    argsError = false;
} else 
{
    runAsConfigure = false;
    argsError = true;
}

if (argsError)
{
    logger.LogCritical("Invalid number of arguments. To run the application fully with bot service, do not use any arguments. To run the application in 'Configure' mode (UI only without bot service), pass `configure` as an argument.");
    System.Environment.Exit(1);
}

//Run the latest db migrations before starting the app
using (var context = new ApplicationDbContext())
{
    //Checking database only if app is running outside of migration mode
    if (!System.Environment.CommandLine.Contains("migrations"))
    {
        //Check if there are pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            // there are pending migrations
            context.Database.Migrate();
        }

        //Getting a list of required DBsets by DbContext
        var dbSets = new List<PropertyInfo>();
        var properties = context.GetType().GetProperties();

        foreach (var property in properties)
        {
            var setType = property.PropertyType;
            var isDbSet = setType.IsGenericType && (typeof(DbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()));

            if (isDbSet)
            {
                dbSets.Add(property);
            }
        }

        //Getting the list of current tables in DB
        var sqlResult = context.Database.SqlQuery<string>($"SELECT count(*) FROM sqlite_master").ToList();
        var numberOfTables = Int32.Parse(sqlResult[0]);

        //Checking if DB has the number of required DBsets already
        //the app should already have taken care of pending migrations, so if this check fails it probably means migrations were not there to begin with.
        if (numberOfTables < dbSets.Count)
        {
            logger.LogCritical("No Database Tables Detected! Consider creating migrations first `dotnet ef migrations add <name>` -- Exiting program with errors!");
            System.Environment.Exit(1);
        }
    }
}



var builder = WebApplication.CreateBuilder(args);

//Adding DBcontext as transient, it's needed for multi-threading
builder.Services.AddTransient<ApplicationDbContext>();
//Adding necessary services
builder.Services.AddTransient<BotFunctions>();
builder.Services.AddTransient<TwitchAuth>();
builder.Services.AddTransient<BotConfigurations>();

//Checking for program arguments
if (runAsConfigure)
{
    logger.LogWarning("Running UI only without Bot service - For configuration purposes");
}
else
{
    //Adding bot service and its games
    builder.Services.AddHostedService<BotService>();
    builder.Services.AddTransient<RandomDropGame>();
    builder.Services.AddTransient<DailyspinGame>();
    builder.Services.AddTransient<PlayToWinGame>();
    builder.Services.AddTransient<FirstToWinGame>();
    builder.Services.AddTransient<RaffleGame>();
    builder.Services.AddTransient<GambleGame>();
    builder.Services.AddTransient<SlotsGame>();
    builder.Services.AddTransient<RollDiceGame>();
    builder.Services.AddTransient<BattleGame>();
    logger.LogInformation("Running application fully functional - with Bot service");
}

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
