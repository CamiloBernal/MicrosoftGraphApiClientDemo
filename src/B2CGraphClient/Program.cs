using B2CGraphClient.Extensions;
using Newtonsoft.Json;
using System;
using System.Configuration;

namespace B2CGraphClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var userName = args[0];
                var clientSettings = ConfigurationManager.AppSettings.ReadConfig();
                var client = new B2CGraphClient(clientSettings);
                CreateUser(client, userName);
            }
            else
            {
                Console.Error.WriteLine("User as not passed");
            }
            Console.ReadKey();
        }

        private static void CreateUser(B2CGraphClient client, string userName)
        {
            var json = @"{
    'accountEnabled': true,
    'signInNames': [
        {
            'type': 'emailAddress',
            'value': '{USER_NAME}'
        }
    ],
    'creationType': 'LocalAccount',
    'displayName': 'Camilo Bernal',
    'mailNickname': 'Camilo',
    'passwordProfile': {
        'password': 'P@ssword!',
        'forceChangePasswordNextLogin': false
    },
    'passwordPolicies': 'DisablePasswordExpiration'
}";

            json = json.Replace("{USER_NAME}", userName);

            var createdUser = client.CreateUserAsync(json).Result;
            var formatted = JsonConvert.DeserializeObject(createdUser);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(JsonConvert.SerializeObject(formatted, Formatting.Indented));
        }
    }
}