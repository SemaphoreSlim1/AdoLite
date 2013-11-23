using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSpecific
{
    public class ConnectionStringSetting
    {
        public String ConnectionString { get; set; }
        public String Name { get; set; }
        public String ProviderName { get; set; }

        public ConnectionStringSetting() { }

        public ConnectionStringSetting(System.Configuration.ConnectionStringSettings settings)
        {
            if(settings == null)
            { return; }

            this.ConnectionString = settings.ConnectionString;
            this.Name = settings.Name;
            this.ProviderName = settings.ProviderName;
        }
    }
}
