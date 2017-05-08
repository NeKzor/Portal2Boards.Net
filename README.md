# Usage

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
var parameters = new BoardParameters()
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