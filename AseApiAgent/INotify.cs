using System;
using System.Collections.Generic;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{
    public interface INotify
    {
        void Notify(AseApiRecord record);
    }
}
