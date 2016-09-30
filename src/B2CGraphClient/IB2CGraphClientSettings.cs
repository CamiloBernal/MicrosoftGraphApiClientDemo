namespace B2CGraphClient
{
    public interface IB2CGraphClientSettings
    {
        string aadGraphEndpoint { get; }
        string aadGraphResourceId { get; }
        string aadGraphSuffix { get; }
        string aadGraphVersion { get; }
        string aadInstance { get; }
        string AuthorityUrl { get; }

        string clientId { get; }

        string clientSecret { get; }

        string tenant { get; }
    }
}