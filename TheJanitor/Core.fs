namespace TheJanitor

open System.Threading.Tasks
open ConfigLoader
open Handlers
open DSharpPlus
open DSharpPlus.CommandsNext
open DSharpPlus.EventArgs
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

module Core =

    // Get Discord configuration
    let config: IConfigurationRoot = config
    let token: string = string config.["Bot-Token"]
    let discordConfig: DiscordConfiguration = DiscordConfiguration()
    discordConfig.Token <- token
    discordConfig.TokenType <- TokenType.Bot
    discordConfig.MinimumLogLevel <- LogLevel.Warning
    discordConfig.LogTimestampFormat <- "MMM dd yyyy - hh:mm:ss tt"
    discordConfig.Intents <- DiscordIntents.All

    // Create a new Discord client
    let client: DiscordClient = new DiscordClient(discordConfig)

    client.add_MessageCreated (messageLogger)
    client.add_MessageCreated (messageFuzzer)
    client.add_MessageCreated (handleDirectMessage)

    // create command handler
    let commandsConfig: CommandsNextConfiguration = CommandsNextConfiguration()
    commandsConfig.StringPrefixes <- [ "/" ]
    let commands: CommandsNextExtension = client.UseCommandsNext(commandsConfig)
    commands.RegisterCommands<Commands>()
    
