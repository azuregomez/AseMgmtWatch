using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public interface IGetManagementIps
    {
        AseApiRecord GetManagementIps(string aseName);
    }
}
