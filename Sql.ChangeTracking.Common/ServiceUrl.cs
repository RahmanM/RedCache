using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.ChangeTracking.Common
{
    public static class ServiceUrl
    {
        public static string SqlTrackingWcfServiceAddress { get; set; } = "net.tcp://localhost:9002/Sql.ChangeTracking.Wcf/wcf/SqlChangeTrackingWcfService";
    }
}
