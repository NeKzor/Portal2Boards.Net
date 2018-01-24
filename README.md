﻿[![Build Status](https://travis-ci.org/NeKzor/Portal2Boards.Net.svg?branch=master)](https://travis-ci.org/NeKzor/Portal2Boards.Net)
[![Build Version](https://img.shields.io/badge/version-v2.0-yellow.svg)](https://github.com/NeKzor/Portal2Boards.Net/projects/2)
[![Release Status](https://img.shields.io/github/release/NeKzor/Portal2Boards.Net.svg)](https://github.com/NeKzor/Portal2Boards.Net/releases)
[![Nuget Status](https://img.shields.io/nuget/v/Portal2Boards.Net.svg)](https://www.nuget.org/packages/Portal2Boards.Net)

Retrieve Portal 2 challenge mode data from the speedrunning community [board.iverb.me](https://board.iverb.me).
Client includes automatic caching system and exception event for logging purposes.

## Overview
- [C# Documentation](#c--documentation)
  - [Namespaces](#namespaces)
  - [Client](#client)
    - [Usage](#usage)
    - [Caching](#caching)
    - [Debugging](#debugging)
  - [Changelog](#changelog)
    - [Query](#query)
    - [Parameters](#parameters)
  - [Chamber](#chamber)
  - [Profile](#profile)
  - [Aggregated](#aggregated)
  - [Demo Content](#demo-content)
- [Examples](#examples)
- [Credits](#credits)

## C# Documentation

### Namespaces

| Namespace | Description |
| --- | --- |
| Portal2Boards.Net | Client for fetching changelog, leaderboard, profile, aggregated data and demo content. |
| Portal2Boards.Net.API] | Advanced usage for getting changelog with customised parameters. |
| Portal2Boards.Net.API.Models | API models converted from raw json. |
| Portal2Boards.Net.Extensions | Useful extension methods. |

### Client

#### Usage
```cs
using Portal2Boards;

using (var client = new Portal2BoardsClient("MyApplication/1.0"))
{
    // Do stuff
}
```

#### Caching
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

#### Debugging
```cs
// Logs client-side exceptions
_client.Log += LogPortal2Boards;

Task LogPortal2Boards(object sender, LogMessage msg)
{
    // Do stuff
}
```

### Changelog

#### Query
```cs
// Default query
var changelog = await _client.GetChangelogAsync("?");

// Query example: gets all wrs within 24h
changelog = await _client.GetChangelogAsync("?maxDaysAgo=1&wr=1");
```

#### Parameters

```cs
using Portal2Boards.API;

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

### Leaderboard
```cs
// By id (uint)
var id = changelog.Data.First().MapId;
var leaderboard = await _client.GetLeaderboardAsync(id);

// With Portal2Boards.Net.Extensions
var map = await Portal2.GetMapByName("Smooth Jazz");
leaderboard = await _client.GetLeaderboardAsync(map);
```

### Profile
```cs
// By name (spaces will be removed automatically)
var profile = await _client.GetProfileAsync("Portal Rex");

// By steam id (ulong)
var id = changelog.Data.First().ProfileNumber;
profile = await _client.GetProfileAsync(id);
```

### Aggregated
```cs
// Overall
var aggregated = await _client.GetAggregatedAsync();

// Chapter
aggregated = await _client.GetAggregatedAsync(Chapter.ThePartWhereHeKillsYou);
```

### Demo Content
```cs
var id = changelog.Data.First().Id;
var bytes = await _client.GetDemoContentAsync(id);
```

## Examples

- [HtmlGenerator.cs](https://github.com/NeKzor/Portal2Boards.Net/tree/master/src/Portal2Boards.Net.Test/HtmlGenerator.cs)  generate an example leaderboard web page of all world records ([view static version](https://nekzor.github.io/Portal2Boards.Net)).
- [TwitterBot.cs](https://github.com/NeKzor/Portal2Boards.Net/tree/master/src/Portal2Boards.Net.Test/TwitterBot.cs) generates a list of latest world records for tweets.
- [NeKzBot](https://github.com/NeKzor/NeKzBot) implemented a notification system for Portal 2 world record updates.

## Credits
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Portal2Boards](https://github.com/iVerb1/Portal2Boards) by [iVerb](https://github.com/iVerb1)
