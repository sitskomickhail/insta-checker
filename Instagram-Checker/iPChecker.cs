using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Checker
{
    public class iPChecker
    {
        public static bool EqualsRange(IEnumerable<string> connectionStrings)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var conStr in connectionStrings)
            {
                string control = conStr.Replace(" ", String.Empty);
                foreach (var ip in host.AddressList)
                {
                    if (ip.ToString() == control)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
