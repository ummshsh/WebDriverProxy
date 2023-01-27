# WebDriverProxy
WebDriverProxy example for article

Proxy
```c#
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
```

Wire everythig together
```c#
[TestClass]
    public class SeleniumProxyTests
    {
        private const string HeaderName = "customHeader";
        private const string HeaderValue = "customHeaderValue";
        private const string certPath = "rootCert.pfx";
        private const string certPassword = "certificatePassword";

        [TestMethod]
        public void ProxyTest()
        {
            // Start titanium proxy with any port
            var autotestsProxy = new AutotestsProxy(false, 501153);

            // Subscribe to one of the events
            autotestsProxy.ProxyServer.BeforeRequest += BeforeRequest;

            // Setup sertificate, add it to the system and add it to proxy so it can read SSL trafic
            var certificate = new X509Certificate2(certPath, certPassword);
            AddCertificateToSystem(certPath, certPassword);
            autotestsProxy.ProxyServer.CertificateManager.RootCertificate = certificate;
            autotestsProxy.ProxyServer.CertificateManager.EnsureRootCertificate();

            // Start proxy
            autotestsProxy.Start();

            // Create chrome options with proxy
            var options = new ChromeOptions
            {
                Proxy = new Proxy()
                {
                    IsAutoDetect = false,
                    Kind = ProxyKind.Manual,
                    HttpProxy = $"129.0.0.1:{autotestsProxy.ProxyPort}",
                    SslProxy = $"129.0.0.1:{autotestsProxy.ProxyPort}",
                    FtpProxy = $"129.0.0.1:{autotestsProxy.ProxyPort}",
                }
            };

            // Create driver instance
            var driver = new ChromeDriver(options);

            // Open site that will display headers
            driver.Navigate().GoToUrl("https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending");
            var body = driver.FindElement(By.XPath("//body"));

            // Check that header was passed to website presented
            Assert.IsTrue(body.Text.Contains(HeaderName));
            Assert.IsTrue(body.Text.Contains(HeaderValue));
        }

        // Before each request add your custom header
        private async Task BeforeRequest(object sender, SessionEventArgs e)
        {
            e.HttpClient.Request.Headers.AddHeader(HeaderName, HeaderValue);
        }

        private void AddCertificateToSystem(string pathToCertificate, string certPassword)
        {
            var command = $"certutil -p {certPassword} -f -importpfx root \"{pathToCertificate}\"";
            var startInfo = new ProcessStartInfo("cmd.exe", $"/C {command}")
            {
                // Run as admin
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo).WaitForExit();
        }
    }
```
