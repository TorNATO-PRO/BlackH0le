namespace TheJanitor

open ConfigLoader
open DSharpPlus
open DSharpPlus.CommandsNext
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

module Core =
    
    // Get Discord configuration
    let config: IConfigurationRoot = config
    let token: string = string config.["Bot-Token"]
    let discordConfig: DiscordConfiguration = DiscordConfiguration ()
    discordConfig.Token <- token
    discordConfig.TokenType <- TokenType.Bot
    discordConfig.MinimumLogLevel <- LogLevel.Warning
    discordConfig.LogTimestampFormat <- "MMM dd yyyy - hh:mm:ss tt"
    discordConfig.Intents <- DiscordIntents.All
    
    // Create a new Discord client
    let client: DiscordClient = new DiscordClient(discordConfig)
    
    // create command handler
    let commandsConfig: CommandsNextConfiguration = CommandsNextConfiguration ()
    commandsConfig.StringPrefixes <- ["/"]
    let commands: CommandsNextExtension = client.UseCommandsNext(commandsConfig)
    commands.RegisterCommands<Commands>()
