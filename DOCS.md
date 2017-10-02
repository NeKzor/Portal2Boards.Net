# C# Documentation

- [Client](#client)
  - [Usage](#usage)
  - [HttpClient](#httpclient)
  - [Caching](#caching)
  - [Debugging](#debugging)
- [Changelog](#changelog)
  - [Query](#query)
  - [Parameters](#parameters)
- [Leaderboard](#leaderboard)
- [Profile](#profile)
- [Aggregated](#aggregated)
- [Demo Content](#demo-content)
- [Terms & Conversion](#terms--conversion)
  - [Example](#example)
- [Examples](#examples)

# Client

## Usage
```cs
using Portal2Boards.Net;

var client = new Portal2BoardsClient();
// Do stuff
client.Dispose();

// With using keyword
using (var client = new Portal2BoardsClient())
{
    // Do stuff
}

// Recommended
static Portal2BoardsClient _client = new Portal2BoardsClient();
```

## HttpClient
```cs
using System.Net.Https;

// Constructor can take your own HttpClient object
// Add your own user agent etc.
var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("TestApplication/1.0");
_client = new Portal2BoardsClient(http);
```

## Caching
```cs
// Everything will be handled automatically
// This is default
_client.AutoCache = true;

// Reset to default time (5 minutes)
_client.CacheResetTime = 0;

// Stop caching
_client.AutoCache = false;

// Reset timer manually
await _client.ResetCacheTimer();

// Clear cache manually
await _client.ClearCache();
```

## Debugging
```cs
// Logs client-side exceptions
_client.OnException += Log_onException;

Task Log_onException(object sender, LogMessage msg)
{
    // Send stuff to your logger etc.
}
```

# Changelog

## Query
```cs
// Default query
var changelog = await _client.GetChangelogAsync("?");

// Query example: gets all wrs within 24h
changelog = await _client.GetChangelogAsync("?maxDaysAgo=1&wr=1");
```

## Parameters

```cs
using Portal2Boards.Net.API;

static ChangelogParameters _parameters = new ChangelogParameters()
{
    [Parameters.MaxDaysAgo]= 7,
    [Parameters.WorldRecord] = 1,
    // Everything else will be null by default
    
    // Strings will be escaped automatically
    [Parameters.PlayerName] = "Name With Spaces",

/*
    [Parameters.MapId] = null,
    [Parameters.ChapterId] = null,
    [Parameters.SteamId] = null,
    [Parameters.Type] = null,
    [Parameters.SinglePlayer] = null,
    [Parameters.Cooperative] = null,
    [Parameters.Banned] = null,
    [Parameters.Demo] = null,
    [Parameters.YouTube] = null,
    [Parameters.Submission] = null,
    [Parameters.HasDate] = null,
    [Parameters.EntryId] = null
*/
};

// Set parameters of client
_client.Parameters = _parameters;

// String query will be generated automatically
var changelog = await client.GetChangelogAsync();
```

# Leaderboard
```cs
// By id (uint)
var id = changelog.Data.First().MapId;
var leaderboard = await _client.GetLeaderboardAsync(id);

// With Portal2Boards.Net.Extensions
var map = await Portal2.GetMapByName("Smooth Jazz");
leaderboard = await _client.GetLeaderboardAsync(map);
```

# Profile
```cs
// By name (spaces will be removed automatically)
var profile = await _client.GetProfileAsync("Portal Rex");

// By steam id (ulong)
var id = changelog.Data.First().ProfileNumber;
profile = await _client.GetProfileAsync(id);
```

# Aggregated
```cs
// Overall
var aggregated = await _client.GetAggregatedAsync();

// Chapter
aggregated = await _client.GetAggregatedAsync(Chapter.ThePartWhereHeKillsYou);
```

# Demo Content
```cs
var id = changelog.Data.First().Id;
var bytes = await _client.GetDemoContentAsync(id);
```

# Terms & Conversion

Portal2Boards API is very simple but has various naming conventions of same objects which can be
tedious to work with. To make things easier: Most API data models can be converted to its own
"twin" type.

|API Data|IEntity|
|---|---|
|AggregatedData|-|
|BoardData|ChamberData|
|ChangelogData|EntryData|
|ProfileData|UserData|

## Example
```cs
using Portal2Boards.Net.Entities;

var profile = await client.GetProfileAsync("iVerb");

// .Data can be casted to IEntity
var user = (UserData)profile.Data;

// Or with Portal2Boards.Net.Extensions
user = profile.Data.Convert();
```

# [Examples](Portal2Boards.Net.Test)

- [HtmlGenerator.cs](Portal2Boards.Net.Test/HtmlGenerator.cs)  will generate an example leaderboard web page of all world records ([view static version](https://nekzor.github.io/Portal2Boards.Net)).
- [TwitterBot.cs](Portal2Boards.Net.Test/TwitterBot.cs) will generate a list of the latest world records in special tweet format. Algorithms were copied from [NeKzBot project](https://github.com/NeKzor/NeKzBot).