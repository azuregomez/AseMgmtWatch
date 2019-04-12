using RestSharp;
using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skokie.Cloud.AseApiAgent
{
    public class AseAgent
    {

        private IGetManagementIps _azmgmt;
        private IPersist _blobstg;
        private INotify _webhook;

        /// <summary>
        /// for testing purposes
        /// </summary>
        /// <param name="targeturl"></param>
        public AseAgent(IGetManagementIps igetManagementIps, IPersist ipersist, INotify inotify)
        {
            _azmgmt = igetManagementIps;
            _blobstg = ipersist;
            _webhook = inotify;
        }
               
        public List<string> ProbeAseManagement(string aseName)
        {
            // get new ips
            var newIps =_azmgmt.GetManagementIps(aseName);
            // get old ips
            var oldIps = _blobstg.Get(aseName);
            // determine missing ips
            List<String> missingIps = new List<String>();
            foreach(string ip in newIps.endpoints)
            {
                if (oldIps==null || !oldIps.endpoints.Contains(ip))
                {
                    missingIps.Add(ip);
                }
            }
            if (missingIps.Count > 0)
            {
                _webhook.Notify(newIps);
                _blobstg.Save(aseName, newIps);
            }
            return missingIps;            
        }
    }
}
