namespace Portal2Boards
{
    public interface IDonator
    {
        ISteamUser Player { get; }
        decimal DonationAmount { get; }
    }
}