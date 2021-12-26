using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Model = Portal2Boards.API.ChamberEntryModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{Id,nq}")]
    public class ChamberEntry : IEntity<ulong>, IChamberEntry
    {
        public ulong Id { get; private set; }
        public uint ChangelogId { get; private set; }
        public DateTime? Date { get; private set; }
        public ISteamUser Player { get; private set; }
        public uint? PlayerRank { get; private set; }
        public uint? ScoreRank { get; private set; }
        public uint? Score { get; private set; }
        public bool DemoExists { get; private set; }
        public string YouTubeId { get; private set; }
        public bool IsSubmission { get; private set; }
        public string Comment { get; private set; }

        public string DemoUrl
            => $"https://board.portal2.sr/getDemo?id={ChangelogId}";
        public string VideoUrl
            => $"https://youtu.be/{YouTubeId}";

        public bool CommentExists
            => !(string.IsNullOrEmpty(Comment));
        public bool VideoExists
            => !(string.IsNullOrEmpty(YouTubeId));

        internal Portal2BoardsClient Client { get; private set; }

        public async Task<IProfile> GetProfileAsync(bool ignoreCache = false)
            => await Client.GetProfileAsync(Id, ignoreCache).ConfigureAwait(false);
        public async Task<byte[]> GetDemoContentAsync(bool ignoreCache = false)
            => await Client.GetDemoContentAsync(ChangelogId, ignoreCache).ConfigureAwait(false);

        internal static ChamberEntry Create(Portal2BoardsClient client, ulong id, Model model)
        {
            return new ChamberEntry()
            {
                Id = id,
                ChangelogId = model.Score.ChangelogId,
                Date = (!string.IsNullOrEmpty(model.Score.Date))
                    ? DateTime.Parse(model.Score.Date)
                    : default,
                Player = SteamUser.Create(client, id, model.User.BoardName, model.User.Avatar),
                PlayerRank = model.Score.PlayerRank,
                ScoreRank = model.Score.ScoreRank,
                Score = model.Score.Score,
                DemoExists = model.Score.HasDemo == "1",
                YouTubeId = model.Score.YouTubeId,
                IsSubmission = model.Score.Submission == "1",
                Comment = WebUtility.HtmlDecode(model.Score.Note),
                Client = client
            };
        }
    }
}
