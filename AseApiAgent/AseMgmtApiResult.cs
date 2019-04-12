using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public class AseApiRecord
    {
        public string description { get; set; }
        public List<string> endpoints { get; set; }
        public List<string> ports { get; set; }
    }
    public class AseMgmtApiResult
    {
        public List<AseApiRecord> value { get; set; }
    }
}
