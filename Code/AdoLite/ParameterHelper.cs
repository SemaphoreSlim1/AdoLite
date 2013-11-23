using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite
{
    public class ParameterHelper
    {
        /// <summary>
        /// Generates a parameter name that isn't already contained in the dictionary
        /// </summary>
        /// <param name="cmdParams">The existing parameter collection</param>
        public static String GenerateParameterName(IDictionary<String, Object> cmdParams)
        {
            var seed = cmdParams.Keys.Count;
            var newName = String.Empty;
            do
            {
                seed++;
                newName = "p" + seed.ToString();
            } while (cmdParams.ContainsKey(newName));

            return newName;
        }


        /// <summary>
        /// Generates a set of parameter names that are not already contained in the parameter dictionary
        /// </summary>
        /// <param name="cmdParams">The existing parameter collection</param>
        /// <param name="values">The values to inject into the dictionary</param>
        /// <returns></returns>
        public static IDictionary<String, Object> GenerateParameterNames(IDictionary<String, Object> cmdParams, IEnumerable<Object> values)
        {
            var seed = cmdParams.Keys.Count;
            var newParameters = new Dictionary<String, Object>();
            var newName = String.Empty;

            for (int i = 0; i < values.Count(); i++)
            {
                do
                {
                    seed++;
                    newName = "p" + seed.ToString();
                } while (cmdParams.ContainsKey(newName) || newParameters.ContainsKey(newName));

                newParameters[newName] = values.ElementAt(i);
            }

            return newParameters;
        }
    }
}
