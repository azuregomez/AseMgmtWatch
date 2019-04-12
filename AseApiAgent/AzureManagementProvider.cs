using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public class AzureManagementProvider : IGetManagementIps
    {
        private string _subscriptionId;
        private string _resourceGroup;
        // AAD token params
        private string _tenantId;
        private string _clientId;
        private string _clientSecret;

        /// <summary>
        /// Constructor for class that invokes ASE Management API
        /// </summary>
        /// <param name="subscriptionId">Subscription Id where ASE resides</param>
        /// <param name="resourceGroup">Resource Group of the ASE</param>
        /// <param name="tenantId">AAD tenent ID</param>
        /// <param name="clientId">AAD App Registration Client ID</param>
        /// <param name="clientSecret">AAD App Registration Key (Password)</param>
        public AzureManagementProvider(string subscriptionId, string resourceGroup, string tenantId, string clientId, string clientSecret)
        {            
            _subscriptionId = subscriptionId;
            _resourceGroup = resourceGroup;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }
        public AseApiRecord GetManagementIps(string aseName)
        {
             var managementApiUrl = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Web/hostingEnvironments/{2}/inboundnetworkdependenciesendpoints?api-version=2016-09-01", _subscriptionId, _resourceGroup, aseName);
            // Authenticate with Azure AD
            var token = GetAccessToken();
            RestClient restClient = new RestClient(managementApiUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = restClient.Execute(request);
            var r = JsonConvert.DeserializeObject<AseMgmtApiResult>(response.Content);
            AseApiRecord asmNode = r.value.Find(x => x.description.Equals("App Service management"));
            return asmNode;
        }

        public string GetAccessToken()
        {
            string authContextURL = "https://login.windows.net/" + _tenantId;
            var authenticationContext = new AuthenticationContext(authContextURL);
            var credential = new ClientCredential(_clientId, _clientSecret);
            System.Threading.Tasks.Task<AuthenticationResult> result = authenticationContext.AcquireTokenAsync(resource: "https://management.azure.com/", clientCredential: credential);
            result.Wait();
            string token = result.Result.AccessToken;
            return token;
        }
    }
}
