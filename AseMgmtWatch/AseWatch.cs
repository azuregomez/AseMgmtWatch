using System;
using System.Configuration;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Skokie.Cloud.AseApiAgent;

namespace Skokie.Cloud.AseMgmtWatch
{
    /// <summary>
    /// ASEWatch function
    /// Probes ASE Management IPs
    /// If IPs change, invoked an Azure Automation Webhook
    /// Azure Automation can update UDRs, NSGs
    /// </summary>
    public static class AseWatch
    {
        [FunctionName("AseWatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // get config values
            string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
            string rgName = ConfigurationManager.AppSettings["rgname"];
            string aseName = ConfigurationManager.AppSettings["asename"];
            string tenantId = ConfigurationManager.AppSettings["tenantId"];
            string appid = ConfigurationManager.AppSettings["appid"];
            string appKey = ConfigurationManager.AppSettings["appKey"];
            string accountName = ConfigurationManager.AppSettings["accountName"];
            string storageKey = ConfigurationManager.AppSettings["storageKey"];
            string containerName = ConfigurationManager.AppSettings["containerName"];
            string automationWebhookUrl = ConfigurationManager.AppSettings["automationwebhookurl"];
            // Az management API provider
            var azmgmt = new AzureManagementProvider(subscriptionId, rgName, tenantId, appid, appKey);            
            // Blob Storage paersistence
            var bpp = new BlobPersistProvider(new StorageCredentials(accountName, storageKey), containerName);
            // Azure Automation webhook notifier
            var webhook = new WebhookNotify(automationWebhookUrl);
            // probe ASE Management
            var agent = new AseAgent(azmgmt,bpp, webhook);
            var result = agent.ProbeAseManagement(aseName);
            // if there are changes log it
            if (result.Count > 0)
            {
                foreach(string newip in result)
                {
                    log.Info("New ASE Mangement IP " + newip + DateTime.Now.ToString());
                }
            }
            else
            {
                log.Info($"No change ASE Mangement IPs at {DateTime.Now}");
            }
        }
    }
}
