using Titanium.Web.Proxy.Models;

namespace Automation.Proxy
{
    public sealed class AutotestsProxy : IDisposable
    {
        public readonly Titanium.Web.Proxy.ProxyServer ProxyServer;

        public bool IsSystemWide { get; }

        public int ProxyPort { get; }

        public AutotestsProxy(bool isSystemWide, int proxyPort)
        {
            ProxyServer = new Titanium.Web.Proxy.ProxyServer();
            ProxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;
            ProxyServer.CertificateManager.SaveFakeCertificates = true;
            IsSystemWide = isSystemWide;
            ProxyPort = proxyPort;

            var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, proxyPort, true);
            ProxyServer.AddEndPoint(explicitEndPoint);

            if (isSystemWide)
            {
                ProxyServer.SetAsSystemHttpProxy(explicitEndPoint);
                ProxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
            }
        }

        public void Start()
        {
            ProxyServer.Start();
        }

        public void Dispose()
        {
            ProxyServer.Stop();
        }
    }
}