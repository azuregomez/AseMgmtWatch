using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public class WebhookNotify: INotify
    {
        private string _webhookUrl;
        public WebhookNotify(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }
        public void Notify(AseApiRecord record)
        {            
            RestClient restClient = new RestClient(_webhookUrl);
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(record);
            var response = restClient.Execute(request);
        }
    }
}
