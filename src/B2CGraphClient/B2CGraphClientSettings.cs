namespace B2CGraphClient
{
    public class B2CGraphClientSettings : IB2CGraphClientSettings
    {
        public string aadGraphEndpoint { get; set; }
        public string aadGraphResourceId { get; set; }
        public string aadGraphSuffix { get; set; }
        public string aadGraphVersion { get; set; }
        public string aadInstance { get; set; }
        public string AuthorityUrl { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string tenant { get; set; }
    }
}