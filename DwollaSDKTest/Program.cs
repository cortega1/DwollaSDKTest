using Dwolla.Client;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using DwollaSDKTest.ObjectRequests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DwollaSDKTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DwollaManager manager = new DwollaManager();
            await manager.CreateCustomerWithApi();
            Console.ReadLine();
        }
    }

    public class DwollaManager
    {
        private readonly string Client = ConfigurationManager.AppSettings["client"].ToString();
        private readonly string Key = ConfigurationManager.AppSettings["key"].ToString();
        private readonly string Grants = ConfigurationManager.AppSettings["token_grants"].ToString();
        private readonly Headers _headers = new Headers();

        public DwollaManager()
        {
        }

        public string GetTokenWebRequest()
        {
            WebClient wc = new WebClient();
            var data = new NameValueCollection();
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            data.Add("client_id", Client);
            data.Add("client_secret", Key);
            data.Add("grant_type", Grants);

            byte[] result = wc.UploadValues("https://sandbox.dwolla.com/oauth/v2/token", "POST", data);

            var token = JsonConvert.DeserializeObject<Token>(Encoding.UTF8.GetString(result));

            return token.access_token;
        }

        public async Task ListCustomers()
        {
            var client = DwollaClient.Create(isSandbox: true);

            var token = await client.PostAuthAsync<AppTokenRequest, TokenResponse>(
                new Uri("https://sandbox.dwolla.com/oauth/v2/token"),
                new AppTokenRequest { Key = Client, Secret = Key });

            var headers = new Headers { { "Authorization", $"Bearer {token.Content.Token}" } };
            var rootRes = (await client.GetAsync<RootResponse>(new Uri(client.ApiBaseAddress), headers)).Content;

            var customers = await client.GetAsync<GetCustomersResponse>(rootRes.Links["customers"].Href, headers);
        }

        public async Task CreateCustomerWithApi()
        {
            WebClient wc = new WebClient();
            var tokenData = new NameValueCollection();
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

            tokenData.Add("client_id", Client);
            tokenData.Add("client_secret", Key);
            tokenData.Add("grant_type", Grants);

            byte[] result = wc.UploadValues("https://sandbox.dwolla.com/oauth/v2/token", "POST", tokenData);

            var token = JsonConvert.DeserializeObject<Token>(Encoding.UTF8.GetString(result));

            var request = (HttpWebRequest)WebRequest.Create("https://api-sandbox.dwolla.com/customers");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.dwolla.v1.hal+json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

            var dict = new Dictionary<string, string>
            {
                { "firstName", "Zapopan" },
                { "lastName", "Jalisquillo" },
                { "email", "zjalisquillo@testtest.com.mx" },
                { "type", "personal" },
                { "address1", "Default Location 122" },
                { "city", "Existing City" },
                { "state", "WA" },
                { "postalCode", "12345" },
                { "dateOfBirth", "2000-02-02" },
                { "ssn", "1234" }
            };
            var body = JsonConvert.SerializeObject(dict);
            var buffer = Encoding.UTF8.GetBytes(body);

            using (var contentRequest = new ByteArrayContent(buffer))
            {
                contentRequest.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.dwolla.v1.hal+json");
                var response = client.PostAsync("https://api-sandbox.dwolla.com/customers", contentRequest).Result;

                Uri newCustomer = response.Headers.Location;
                response = client.GetAsync(newCustomer).Result;
                var customer = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(customer);
            }
        }

        public async Task CreateCustomerWithSdk()
        {
            var client = DwollaClient.Create(isSandbox: true);

            var tokenRes = await client.PostAuthAsync<AppTokenRequest, TokenResponse>(
                new Uri($"{client.AuthBaseAddress}/token"),
                new AppTokenRequest { Key = Client, Secret = Key });

            var headers = new Headers();
            headers.Add("Authorization", $"Bearer {tokenRes.Content.Token}");
            var rootRes = (await client.GetAsync<RootResponse>(new Uri(client.ApiBaseAddress), headers)).Content;
            var customers = await client.PostAsync(rootRes.Links["customers"].Href, new CreateCustomerRequest
            {
                FirstName = "Ricardino",
                LastName = "Flores",
                Email = "rnoflores@testtest.com.mx",
                Type = "personal",
                Address1 = "Default Location 777",
                City = "Existing City",
                State = "WA",
                PostalCode = "12345",
                DateOfBirth = DateTime.Today.AddYears(-20),
                Ssn = "1234"
            }, headers);
        }
    }

    public class Customer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
    }
}
