using System.Collections.Specialized;

namespace B2CGraphClient.Extensions
{
    public static class AppSettingsExtensions
    {
        public static IB2CGraphClientSettings ReadConfig(this NameValueCollection appSettings)
        =>
           new B2CGraphClientSettings
           {
               AuthorityUrl = appSettings["b2c:AuthorityUrl"],
               clientSecret = appSettings["b2c:ClientSecret"],
               aadGraphVersion = appSettings["b2c:GraphVersion"],
               tenant = appSettings["b2c:Tenant"],
               clientId = appSettings["b2c:ClientId"],
               aadGraphEndpoint = appSettings["b2c:GraphEndpoint"],
               aadGraphResourceId = appSettings["b2c:GraphResourceId"],
               aadGraphSuffix = appSettings["b2c:GraphSuffix"],
               aadInstance = appSettings["b2c:Instance"]
           };
    }
}