using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public interface IPersist
    {
        AseApiRecord Get(string aseName);
        void Save(string aseName, AseApiRecord record);
    }
}
