namespace TheJanitor

open System.IO
open Microsoft.Extensions.Configuration

module ConfigLoader =

    let config =
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", false)
            .Build()
