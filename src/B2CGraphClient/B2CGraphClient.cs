using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace B2CGraphClient
{
    public class B2CGraphClient
    {
        private readonly AuthenticationContext _authContext;
        private readonly ClientCredential _credential;
        private readonly IB2CGraphClientSettings _settings;

        public B2CGraphClient(IB2CGraphClientSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            // The client_id, client_secret, and tenant are pulled in from the App.config file
            _settings = settings;

            // The AuthenticationContext is ADAL's primary class, in which you indicate the direcotry
            // to use.
            _authContext = new AuthenticationContext(_settings.AuthorityUrl + _settings.tenant);

            // The ClientCredential is where you pass in your client_id and client_secret, which are
            // provided to Azure AD in order to receive an access_token using the app's identity.
            _credential = new ClientCredential(_settings.clientId, _settings.clientSecret);
        }

        public async Task<string> CreateUserAsync(string json, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphPostRequestAsync("/users", json, cancellationToken).ConfigureAwait(false);

        public async Task<string> DeleteUserAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphDeleteRequestAsync("/users/" + objectId, cancellationToken).ConfigureAwait(false);

        public async Task<string> GetAllUsersAsync(string query, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphGetRequestAsync("/users", query, cancellationToken).ConfigureAwait(false);

        public async Task<string> GetApplicationsAsync(string query, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphGetRequestAsync("/applications", query, cancellationToken).ConfigureAwait(false);

        public async Task<string> GetExtensionsAsync(string appObjectId, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphGetRequestAsync("/applications/" + appObjectId + "/extensionProperties", null, cancellationToken).ConfigureAwait(false);

        public async Task<string> GetUserByObjectIdAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphGetRequestAsync("/users/" + objectId, null, cancellationToken).ConfigureAwait(false);

        public async Task<string> RegisterExtensionAsync(string objectId, string body, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphPostRequestAsync("/applications/" + objectId + "/extensionProperties", body, cancellationToken).ConfigureAwait(false);

        public async Task<string> SendGraphGetRequestAsync(string api, string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            // First, use ADAL to acquire a token using the app's identity (the credential) The first
            // parameter is the resource we want an access_token for; in this case, the Graph API.
            var result = await _authContext.AcquireTokenAsync(_settings.aadGraphEndpoint, _credential).ConfigureAwait(false);

            // For B2C user managment, be sure to use the 1.6 Graph API version.
            var http = new HttpClient();
            var url = _settings.aadGraphEndpoint + _settings.tenant + api + "?" + _settings.aadGraphVersion;
            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GET " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("");

            // Append the access token for the Graph API to the Authorization header of the request,
            // using the Bearer scheme.
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> UnregisterExtensionAsync(string appObjectId, string extensionObjectId, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphDeleteRequestAsync("/applications/" + appObjectId + "/extensionProperties/" + extensionObjectId, cancellationToken).ConfigureAwait(false);

        public async Task<string> UpdateUserAsync(string objectId, string json, CancellationToken cancellationToken = default(CancellationToken)) => await SendGraphPatchRequest("/users/" + objectId, json, cancellationToken).ConfigureAwait(false);

        private async Task<string> SendGraphDeleteRequestAsync(string api, CancellationToken cancellationToken = default(CancellationToken))
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            var result = await _authContext.AcquireTokenAsync(_settings.aadGraphResourceId, _credential).ConfigureAwait(false);
            var http = new HttpClient();
            var url = _settings.aadGraphEndpoint + _settings.tenant + api + "?" + _settings.aadGraphVersion;
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DELETE " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<string> SendGraphPatchRequest(string api, string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            var result = await _authContext.AcquireTokenAsync(_settings.aadGraphResourceId, _credential).ConfigureAwait(false);
            var http = new HttpClient();
            var url = _settings.aadGraphEndpoint + _settings.tenant + api + "?" + _settings.aadGraphVersion;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("PATCH " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("Content-Type: application/json");
            Console.WriteLine("");
            Console.WriteLine(json);
            Console.WriteLine("");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<string> SendGraphPostRequestAsync(string api, string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            var result = await _authContext.AcquireTokenAsync(_settings.aadGraphResourceId, _credential).ConfigureAwait(false);
            var http = new HttpClient();
            var url = _settings.aadGraphEndpoint + _settings.tenant + api + "?" + _settings.aadGraphVersion;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("POST " + url);
            Console.WriteLine("Authorization: Bearer " + result.AccessToken.Substring(0, 80) + "...");
            Console.WriteLine("Content-Type: application/json");
            Console.WriteLine("");
            Console.WriteLine(json);
            Console.WriteLine("");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine((int)response.StatusCode + ": " + response.ReasonPhrase);
            Console.WriteLine("");

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}