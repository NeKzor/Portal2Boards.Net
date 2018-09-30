using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Model = Portal2Boards.API.DonatorModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{Id,nq}")]
    public class Donator : IEntity<ulong>, IDonator
    {
        public ulong Id => (Player as IEntity<ulong>).Id;
        public ISteamUser Player { get; private set; }
        public decimal DonationAmount { get; private set; }

        public string Url
            => $"https://steamcommunity.com/profiles/{Id}";

        internal Portal2BoardsClient Client { get; private set; }

        public async Task<IProfile> GetProfileAsync(bool ignoreCache = false)
            => await Client.GetProfileAsync(Id, ignoreCache).ConfigureAwait(false);

        internal static Donator Create(Portal2BoardsClient client, Model model)
        {
            return new Donator()
            {
                Player = SteamUser.Create(client, model.ProfileNumber, model.PlayerName, model.Avatar),
                DonationAmount = model.DonationAmount
            };
        }
    }
}
