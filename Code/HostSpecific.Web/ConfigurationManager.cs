using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HostSpecific.Web
{
    public class ConfigurationManager : HostSpecific.ConfigurationManager
    {
        /// <summary>
        /// Initializes the host-specific settings based on the environment variables that have been declared on the server
        /// </summary>
        /// <param name="req">The request which triggers the initialization</param>       
        public static void Init(HttpRequest req)
        { 
            if(HostSpecific.ConfigurationManager.IsInitialized)
            { return; }

            //first, determine where we're being hosted - dev, test, qa or prod
            var serverRole = Environment.GetEnvironmentVariable("SERVER_ROLE");

            if(String.IsNullOrWhiteSpace(serverRole) == false)
            { serverRole = serverRole.ToUpper(); }
            else if(String.IsNullOrWhiteSpace(serverRole) && req.ServerVariables["SERVER_NAME"].ToUpper() == "LOCALHOST")
            { serverRole = "DEVELOPER_MACHINE"; } //localhost doesn't define server role
            else
            { serverRole = String.Empty; }

            SupportedEnvironment runningEnvironment;

            switch(serverRole)
            {
                case "DEVELOPER_MACHINE":
                    runningEnvironment = SupportedEnvironment.DeveloperMachine;
                    break;
                case "DEV":
                    runningEnvironment = SupportedEnvironment.Development;
                    break;
                case "TEST":
                    runningEnvironment = SupportedEnvironment.Test;
                    break;
                case "STAGE": 
                    runningEnvironment = SupportedEnvironment.QA;
                    break;
                default:
                    runningEnvironment = SupportedEnvironment.Production;
                    break;
            }

            Init(runningEnvironment);
        }
    }
}
