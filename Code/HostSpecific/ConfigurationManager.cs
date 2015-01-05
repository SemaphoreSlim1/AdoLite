using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSpecific
{
    public class ConfigurationManager
    {
        /// <summary>
        /// Gets and sets whether or not the host specific settings have been initialized
        /// </summary>
        protected static Boolean IsInitialized { get; set; }

        /// <summary>
        /// Gets and sets the host-specific connection strings
        /// </summary>
        public static Dictionary<String, ConnectionStringSetting> ConnectionStrings { get; set; }

        /// <summary>
        /// Gets and sets the host-specific settings
        /// </summary>
        public static Dictionary<String, String> Settings { get; set; }

        /// <summary>
        /// Gets (and protected sets) the hosting environment that this application is currently executing in
        /// </summary>
        public static SupportedEnvironment HostingEnvironment { get; protected set; }

        #region Synonymous Environments

        private static IEnumerable<PrioritizedEnviornment> _SynonymousEnvironments;

        /// <summary>
        /// Gets the environments that are synonymous to the current hosting environment. IE, dev is synonymous to localhost, etc.
        /// </summary>
        private static IEnumerable<PrioritizedEnviornment> SynonymousEnvironments
        {
            get
            {
                if(_SynonymousEnvironments != null)
                { return _SynonymousEnvironments; }

                switch (HostingEnvironment)
                {
                    case SupportedEnvironment.DeveloperMachine:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { 
                            new PrioritizedEnviornment(1, SupportedEnvironment.DeveloperMachine),
                            new PrioritizedEnviornment(2, SupportedEnvironment.Development)
                        };
                        break;
                    case SupportedEnvironment.Development:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { new PrioritizedEnviornment(1, SupportedEnvironment.Development) };
                        break;
                    case SupportedEnvironment.Test:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { new PrioritizedEnviornment(2, SupportedEnvironment.Test)};
                        break;
                    case SupportedEnvironment.QA:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { 
                            new PrioritizedEnviornment(1,SupportedEnvironment.QA),
                            new PrioritizedEnviornment(2, SupportedEnvironment.Production)
                        };
                        break;
                    case SupportedEnvironment.Production:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { new PrioritizedEnviornment(1, SupportedEnvironment.Production)};
                        break;
                    default:
                        _SynonymousEnvironments = new PrioritizedEnviornment[] { new PrioritizedEnviornment(1, HostingEnvironment)};
                        break;
                }

                return _SynonymousEnvironments;
            }
        }

        #endregion

        public static void Init(SupportedEnvironment runningEnvironment)
        { 
            if(HostSpecific.ConfigurationManager.IsInitialized)
            { return;  }

            HostingEnvironment = runningEnvironment;

            //set up the connection strings
            var enumerableConnections = System.Configuration.ConfigurationManager.ConnectionStrings.Cast<System.Configuration.ConnectionStringSettings>();
            var connectionStringNames = enumerableConnections.Select(cn => cn.Name);
            var desiredConnectionStringKeys = KeysForHostedEnvironment(connectionStringNames);

            ConnectionStrings = new Dictionary<String, ConnectionStringSetting>();
            foreach (var key in desiredConnectionStringKeys)
            {
                var connection = new ConnectionStringSetting(System.Configuration.ConfigurationManager.ConnectionStrings[key]);
                var localizedKey = LocalizeKey(key);

                ConnectionStrings[localizedKey] = connection;
            }

            //do the same thing with the settings
            var enumerableKeys = System.Configuration.ConfigurationManager.AppSettings.AllKeys;
            var desiredSettingsKeys = KeysForHostedEnvironment(enumerableKeys);

            //and copy them over, normalized
            Settings = new Dictionary<String, String>();
            foreach (var key in desiredSettingsKeys)
            {
                var value = System.Configuration.ConfigurationManager.AppSettings[key];
                var localizedKey = LocalizeKey(key);
                Settings[localizedKey] = value;
            }

            IsInitialized = true;
            
        }

        /// <summary>
        /// Makes a host-specific key into a regular key by stripping off host information
        /// </summary>
        /// <param name="key">The key to localize</param>
        /// <returns>The localized key</returns>
        private static String LocalizeKey(String key)
        {
            var localizedKey = key;
            foreach(var prioritizedEnvironment in HostSpecific.ConfigurationManager.SynonymousEnvironments)
            { localizedKey = localizedKey.Replace(prioritizedEnvironment.Environment.ToString() + ".", String.Empty); }

            return localizedKey;
        }

        private static IEnumerable<String> KeysForHostedEnvironment(IEnumerable<String> keys)
        {
            var localizedTargetKeys = new HashSet<String>();
            var targetKeys = new HashSet<String>();


            foreach(var key in keys.Where(k => k.Contains(".") == false)) //first grab the keys that aren't tied to an environment
            {
                localizedTargetKeys.Add(key);
                targetKeys.Add(key);
            }

            //then get the keys that are tied to an environment, starting with priority 1, and working on down

            //for example, lets say the hosting environment is the developer's machine. It's synonymous environments are developer machine and dev.
            //therefore, we'll first look for keys that start with DeveloperMachine.*, then for keys that start with Development.*
            //because we're adding these values to a hashset, we don't have to worry about overwriting developer machine values with dev 
            //as hashset only adds values to the set if they haven't been added yet.

            //the end result is such that if there's no developer machine value, but there IS a development value, then it'll pick up (and use)
            //the value associated with the dev environment. But if we later come in and add a value specific to the developer machine,
            //then it'll use that value the next time the values are parsed.

            foreach (var prioritizedEnvironment in HostSpecific.ConfigurationManager.SynonymousEnvironments.OrderBy(env => env.Priority))
            {
                var environmentKeys = keys.Where(k => k.StartsWith(prioritizedEnvironment.Environment.ToString() + "."));
                foreach (var environmentKey in environmentKeys)
                { 
                    //get a localized version of the key
                    var localizedKey = LocalizeKey(environmentKey);

                    //once localized, we can determine if we've already added it
                    //if we haven't, then add it to the target collection
                    if(localizedTargetKeys.Add(localizedKey))
                    { targetKeys.Add(environmentKey); }
                }
            }

            return targetKeys;
        }
    }
}
