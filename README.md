# Overview

|Namespace|Status|Description|
|---|:-:|---|
|Portal2Boards.Net|✔|Client for fetching data.|
|Portal2Boards.Net.API|✔|Advanced parameters for getting changelog.|
|Portal2Boards.Net.API.Models|✔|API classes.|
|Portal2Boards.Net.Entities|✖|API models conversion.|
|Portal2Boards.Net.Extensions|✖|Game information.|

---

# Example: Changelog

### Simple

#### 1.)
```cs
using Portal2Boards.Net;
```

#### 2.)
```cs
var client = new Portal2BoardsClient();
// Async
var changelog = await client.GetChangelogAsync("?maxDaysAgo=1&wr=1");
// Sync
changelog = client.GetChangelogAsync("?maxDaysAgo=1&wr=1").GetAwaiter().GetResult();
```

### 3.)
```cs
var dontclipthis = changelog.Data.Where(d => d.PlayerName == "Msushi" && d.PostRank == 2);
```

---
### Advanced

#### 1.)
```cs
using Portal2Boards.Net.API;
```

#### 2.)
```cs
var parameters = new ChangelogParameters()
{
    [Parameters.MaxDaysAgo] = 7,
    [Parameters.WorldRecord] = 1
};

using (var client = new Portal2BoardsClient(parameters))
{
    var changelog = await client.GetChangelogAsync();
...
```

#### 3.)
```cs
...
    foreach (var entry in changelog)
    {
        Console.WriteLine($"ID = {entry.EntryId}");
    }
}
```