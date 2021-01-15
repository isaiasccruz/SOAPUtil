using System;
using System.Net;

namespace SOAPUtil
{
    public class SOAPProxyUtil
    {
        public WebProxy MontaWebProxy(string proxyAddress, bool usaCredenciaisDefault, bool bypassOnLocal, string[] byPassList, string username, string password)
        {
            if (usaCredenciaisDefault == true)
            {

                return new WebProxy(proxyAddress, true)
                {
                    UseDefaultCredentials = true
                };
            }
            else
            {
                var proxyURI = new Uri(proxyAddress);

                ICredentials credentials = new NetworkCredential(username, password);

                return new WebProxy(proxyURI, bypassOnLocal, byPassList, credentials);
            }

        }
    }
}
