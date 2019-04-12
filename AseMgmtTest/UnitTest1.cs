using System.Configuration;
using System.Collections.Generic;
using Microsoft.Azure.Storage.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Skokie.Cloud.AseApiAgent;
using System;

namespace AseMgmtTest
{
    [TestClass]
    public class UnitTest1
    {
        string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        string rgname = ConfigurationManager.AppSettings["rgname"];
        string asename = ConfigurationManager.AppSettings["asename"];
        string tenantId = ConfigurationManager.AppSettings["tenantId"];
        string appid = ConfigurationManager.AppSettings["appid"];
        string appKey = ConfigurationManager.AppSettings["appKey"];
        string accountName = ConfigurationManager.AppSettings["accountName"];
        string storageKey = ConfigurationManager.AppSettings["storageKey"];
        string containerName = ConfigurationManager.AppSettings["containerName"];
        string webhookurl = ConfigurationManager.AppSettings["automationwebhookurl"];

        [TestMethod]
        public void WebHookInvoke()
        {
            var wh = new WebhookNotify(webhookurl);
            var record = new AseApiRecord()
            {
                description = "App Service management",
                endpoints = new List<string>()
            };
            record.endpoints.Add("70.37.57.58/32");
            record.endpoints.Add("157.55.208.185/32");
            record.endpoints.Add("23.102.188.65/32");
            // call webhook. fire and forget.
            wh.Notify(record);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ProbeAseManagement_NoChange()
        {
            var aseName = "testase";
            var mockGetManagementIps = new Mock<IGetManagementIps>();
            var mockPersist = new Mock<IPersist>();
            var mockNotify = new Mock<INotify>();
            // setup AseApi Object
            var record = new AseApiRecord()
            {
                description= "App Service management",    
                endpoints = new List<string>()
            };
            record.endpoints.Add("70.37.57.58/32");
            record.endpoints.Add("157.55.208.185/32");
            record.endpoints.Add("23.102.188.65/32");
            mockGetManagementIps.Setup(m => m.GetManagementIps(aseName)).Returns(record);
            mockPersist.Setup(m => m.Get(aseName)).Returns(record);
            mockPersist.Setup(m => m.Save(aseName, record));
            mockNotify.Setup(m => m.Notify(record));
            var agent = new AseAgent(mockGetManagementIps.Object, mockPersist.Object, mockNotify.Object);
            var missing = agent.ProbeAseManagement(aseName);
            mockGetManagementIps.Verify(v => v.GetManagementIps(aseName));
            mockPersist.Verify(v => v.Get(aseName));
            Assert.IsTrue(missing.Count == 0);
            mockNotify.VerifyNoOtherCalls();
            mockPersist.VerifyNoOtherCalls();

        }

        [TestMethod]
        public void ProbeAseManagement_Change()
        {
            var aseName = "testase";
            var mockGetManagementIps = new Mock<IGetManagementIps>();
            var mockPersist = new Mock<IPersist>();
            var mockNotify = new Mock<INotify>();
            // setup AseApi Object
            var record = new AseApiRecord()
            {
                description = "App Service management",
                endpoints = new List<string>()
            };
            record.endpoints.Add("70.37.57.58/32");
            record.endpoints.Add("157.55.208.185/32");
            record.endpoints.Add("23.102.188.65/32");
            // setup added 2 IPs
            var record2 = new AseApiRecord()
            {
                description = "App Service management",
                endpoints = new List<string>()
            };
            record2.endpoints.Add("70.37.57.58/32");
            record2.endpoints.Add("157.55.208.185/32");
            record2.endpoints.Add("23.102.188.65/32");
            record2.endpoints.Add("191.236.154.88/32");
            record2.endpoints.Add("52.174.22.21/32");
            mockPersist.Setup(m => m.Get(aseName)).Returns(record);
            mockPersist.Setup(m => m.Save(aseName, record2));
            mockNotify.Setup(m => m.Notify(record2));            
            mockGetManagementIps.Setup(m => m.GetManagementIps(aseName)).Returns(record2);
            var agent = new AseAgent(mockGetManagementIps.Object, mockPersist.Object, mockNotify.Object);
            var missing = agent.ProbeAseManagement(aseName);
            mockGetManagementIps.Verify(v => v.GetManagementIps(aseName));
            mockPersist.Verify(v => v.Get(aseName));
            Assert.IsTrue(missing.Count == 2);            
            mockNotify.Verify(v => v.Notify(record2));
            mockPersist.Verify(v => v.Save(aseName, record2));
        }

        [TestMethod]
        public void ProbeAseManagement_FirstTime()
        {
            var aseName = "testase";
            var mockGetManagementIps = new Mock<IGetManagementIps>();
            var mockPersist = new Mock<IPersist>();
            var mockNotify = new Mock<INotify>();            
            // setup added 2 IPs
            var record2 = new AseApiRecord()
            {
                description = "App Service management",
                endpoints = new List<string>()
            };
            record2.endpoints.Add("70.37.57.58/32");
            record2.endpoints.Add("157.55.208.185/32");
            record2.endpoints.Add("23.102.188.65/32");
            record2.endpoints.Add("191.236.154.88/32");
            record2.endpoints.Add("52.174.22.21/32");
            mockPersist.Setup(m => m.Get(aseName)).Returns(null as AseApiRecord);
            mockPersist.Setup(m => m.Save(aseName, record2));
            mockNotify.Setup(m => m.Notify(record2));
            mockGetManagementIps.Setup(m => m.GetManagementIps(aseName)).Returns(record2);
            var agent = new AseAgent(mockGetManagementIps.Object, mockPersist.Object, mockNotify.Object);
            var missing = agent.ProbeAseManagement(aseName);
            mockGetManagementIps.Verify(v => v.GetManagementIps(aseName));
            mockPersist.Verify(v => v.Get(aseName));
            Assert.IsTrue(missing.Count == 5);
            mockNotify.Verify(v => v.Notify(record2));
            mockPersist.Verify(v => v.Save(aseName, record2));
        }


        [TestMethod]
        public void GetMagagementIps()
        {           
            IGetManagementIps azmgmt = new AzureManagementProvider(subscriptionId, rgname, tenantId, appid, appKey);            
            var result = azmgmt.GetManagementIps(asename);            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.endpoints.Count > 0);
            Assert.IsTrue(result.ports.Count > 0);
        }

        [TestMethod]
        public void GetAadToken()
        {
            AzureManagementProvider azmgmt = new AzureManagementProvider(subscriptionId, rgname, tenantId, appid, appKey);
            var token = azmgmt.GetAccessToken();
            Assert.IsNotNull(token);
        }

        [TestMethod]
        public void ParseJson()
        {
            AseApiRecord asmnode = ParseIps();
            Assert.IsNotNull(asmnode);
            Assert.IsTrue(asmnode.endpoints.Count > 0);
            Assert.IsTrue(asmnode.ports.Count > 0);
        }
        public AseApiRecord ParseIps()
        {
            string ips = "{\"value\":[{\"description\":\"App Service management\",\"endpoints\":[\"70.37.57.58/32\",\"157.55.208.185/32\",\"23.102.188.65/32\",\"191.236.154.88/32\",\"52.174.22.21/32\",\"13.94.149.179/32\",\"13.94.143.126/32\",\"13.94.141.115/32\",\"52.178.195.197/32\",\"52.178.190.65/32\",\"52.178.184.149/32\",\"52.178.177.147/32\",\"13.75.127.117/32\",\"40.83.125.161/32\",\"40.83.121.56/32\",\"40.83.120.64/32\",\"52.187.56.50/32\",\"52.187.63.37/32\",\"52.187.59.251/32\",\"52.187.63.19/32\",\"52.165.158.140/32\",\"52.165.152.214/32\",\"52.165.154.193/32\",\"52.165.153.122/32\",\"104.44.129.255/32\",\"104.44.134.255/32\",\"104.44.129.243/32\",\"104.44.129.141/32\",\"65.52.193.203/32\",\"70.37.89.222/32\",\"13.64.115.203/32\",\"52.225.177.153/32\",\"65.52.172.237/32\",\"23.102.135.246/32\",\"52.224.105.172/32\",\"52.151.25.45/32\",\"40.124.47.188/32\"],\"ports\":[\"454\",\"455\"]},{\"description\":\"App Service Environment VIP\",\"endpoints\":[\"52.165.238.66/32\"],\"ports\":[\"454\",\"455\",\"16001\"]},{\"description\":\"App Service Environment subnet\",\"endpoints\":[\"192.168.250.0/24\"],\"ports\":[\"All\"]}],\"nextLink\":null,\"id\":null}";
            var r = JsonConvert.DeserializeObject<AseMgmtApiResult>(ips);
            AseApiRecord asmnode = r.value.Find(x => x.description.Equals("App Service management"));           
            return asmnode;
        }

        [TestMethod]
        public void Persist()
        {
            string testaseName = "test";
            var asmnode = ParseIps();
            StorageCredentials credentials = new StorageCredentials(accountName, storageKey);
            var bpp = new BlobPersistProvider(credentials, containerName);
            bpp.Save(testaseName, asmnode);
            var record = bpp.Get(testaseName);
            Assert.AreEqual(asmnode.description, record.description);
            Assert.AreEqual(asmnode.endpoints.Count, record.endpoints.Count);
            Assert.AreEqual(asmnode.ports.Count, record.ports.Count);
        }

        [TestMethod]
        public void ReadBlobNotExisting()
        {
            string testaseName = "test" + DateTime.Now.ToString();
            var asmnode = ParseIps();
            StorageCredentials credentials = new StorageCredentials(accountName, storageKey);
            var bpp = new BlobPersistProvider(credentials, containerName);            
            var record = bpp.Get(testaseName);
            Assert.IsNull(record);            
        }
    }
}
