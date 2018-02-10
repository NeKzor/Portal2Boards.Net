[![Build Status](https://travis-ci.org/NeKzor/Portal2Boards.Net.svg?branch=master)](https://travis-ci.org/NeKzor/Portal2Boards.Net)
[![Build Version](https://img.shields.io/badge/version-v2.2-brightgreen.svg)](https://github.com/NeKzor/Portal2Boards.Net/projects/3)
[![Release Version](https://img.shields.io/github/release/NeKzor/Portal2Boards.Net.svg)](https://github.com/NeKzor/Portal2Boards.Net/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Portal2Boards.Net.svg)](https://www.nuget.org/packages/Portal2Boards.Net)

Retrieve Portal 2 challenge mode data from the speedrunning community [board.iverb.me](https://board.iverb.me).
Client includes automatic caching system and exception event for logging purposes.

## Overview
- [C# Documentation](#c-documentation)
  - [Namespaces](#namespaces)
  - [Client](#client)
    - [Usage](#usage)
    - [Caching](#caching)
    - [Debugging](#debugging)
  - [Changelog](#changelog)
    - [Query](#query)
    - [Advanced](#advanced)
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
| Portal2Boards | Client for fetching changelog, chamber, profile, aggregated data and demo content. |
| Portal2Boards.API.Models | API models converted from raw json. |
| Portal2Boards.Extensions | Useful extension methods. |

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
// Default
var changelog = await _client.GetChangelogAsync();

// Get all wrs within 24h
changelog = await _client.GetChangelogAsync("?maxDaysAgo=1&wr=1");
```

#### Advanced

```cs
var changelog = await _client.GetChangelogAsync(q =>
{
  // Names will be escaped automatically
  q.ProfileName = "Portal Rex";
  q.WorldRecord = true;
});

// With builder
changelog = await _client.GetChangelogAsync(() =>
  new ChangelogQueryBuilder()
    .WithWorldRecord(true)
    .WithDemo(true)
    .Build()
);
```

### Chamber
```cs
// By map id (ulong)
var chamber = await _client.GetChamberAsync(id);

// With extensions
using Portal2Boards.Net.Extensions;
var map = await Portal2Map.Search("Smooth Jazz");
chamber = await _client.GetChamberAsync(map);
```

### Profile
```cs
// By name (spaces will be removed automatically)
var profile = await _client.GetProfileAsync("Portal Rex");

// By steam id (ulong)
profile = await _client.GetProfileAsync(id);
```

### Aggregated
```cs
// Overall
var aggregated = await _client.GetAggregatedAsync();

// Mode
aggregated = await _client.GetAggregatedAsync(AggregatedMode.SinglePlayer);

// Chapter
aggregated = await _client.GetAggregatedAsync(ChapterType.ThePartWhereHeKillsYou);
```

### Demo Content
```cs
// Use changelog id (ulong)
var bytes = await _client.GetDemoContentAsync(id);
```

## Examples

- [LeaderboardWebPage.cs](https://github.com/NeKzor/Portal2Boards.Net/tree/master/src/Portal2Boards.Net.Test/Examples/LeaderboardWebPage.cs) generates an example [leaderboard web page](https://nekzor.github.io/Portal2Boards.Net/sp) of all world records.
- [TwitterBot.cs](https://github.com/NeKzor/Portal2Boards.Net/tree/master/src/Portal2Boards.Net.Test/Examples/TwitterBot.cs) generates a list of latest world records for tweets.
- [NeKzBot](https://github.com/NeKzor/NeKzBot) implemented a notification system for world record updates.

## Credits
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Portal2Boards](https://github.com/iVerb1/Portal2Boards) by [iVerb](https://github.com/iVerb1)
