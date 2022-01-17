namespace TheJanitor

open ConfigLoader
open DSharpPlus
open Microsoft.Extensions.Configuration

module Core =
    
    // Get Discord configuration
    let config: IConfigurationRoot = config
    let token: string = string config.["Bot-Token"]
    let discordConfig: DiscordConfiguration = DiscordConfiguration ()
    discordConfig.Token <- token
    discordConfig.TokenType <- TokenType.Bot

    // Create a new Discord client
    let client = new DiscordClient(discordConfig)

