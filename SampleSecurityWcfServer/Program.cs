using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

// 利用 https 传输层的 WCF Server 样例
// https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-configure-a-port-with-an-ssl-certificate
// https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-retrieve-the-thumbprint-of-a-certificate
// netsh http add sslcert ipport=0.0.0.0:8080 certhash=872801bbd93e75f327b34ae76366fa05b229433b appid={c07d3793-b6f2-4afd-8819-016bada562e4}
/*
 * C:\WINDOWS\system32>netsh http add sslcert ipport=0.0.0.0:8080 certhash=872801bbd93e75f327b34ae76366fa05b229433b appid={c07d3793-b6f2-4afd-8819-016bada562e4}

成功添加 SSL 证书


C:\WINDOWS\system32>
 * https://localhost:8080/Calculator/MyCalculator
 * */
// 配置样例:
// https://stackoverflow.com/questions/15413607/wcf-ssl-connection-configurations
namespace SampleSecurityWcfServer
{
    class Program
    {
        static void Main(string[] args)
        {
            StartTransportMessage();
#if NO
            // This string uses a function to prepend the computer name at run time.
            string addressHttp = String.Format(
                "https://{0}:8089/Calculator",
                "localhost");  // System.Net.Dns.GetHostEntry("").HostName

            WSHttpBinding b = new WSHttpBinding();
            b.Security.Mode = SecurityMode.TransportWithMessageCredential;
            b.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            b.ReliableSession.Enabled = true;

            // You must create an array of URI objects to have a base address.
            Uri a = new Uri(addressHttp);
            Uri[] baseAddresses = new Uri[] { a };

            // Create the ServiceHost. The service type (Calculator) is not
            // shown here.
            ServiceHost sh = new ServiceHost(typeof(Calculator), baseAddresses);

            // Add an endpoint to the service. Insert the thumbprint of an X.509
            // certificate found on your computer.
            Type c = typeof(ICalculator);
            sh.AddServiceEndpoint(c, b, "MyCalculator");
            /*
            sh.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                StoreName.My,
                X509FindType.FindBySubjectName,
                "localhost");  // "contoso.com"
            */
            //sh.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
            //sh.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNameValidator();

            // http://localhost:8081/Calculator
            string metadata_url = String.Format(
    "http://{0}:8081/Calculator",
    "localhost");  // System.Net.Dns.GetHostEntry("").HostName

            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
            behavior.HttpGetEnabled = true;
            behavior.HttpGetUrl = new Uri(metadata_url);
            sh.Description.Behaviors.Add(behavior);


            // This next line is optional. It specifies that the client's certificate
            // does not have to be issued by a trusted authority, but can be issued
            // by a peer if it is in the Trusted People store. Do not use this setting
            // for production code. The default is PeerTrust, which specifies that
            // the certificate must originate from a trusted certificate authority.

            //sh.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
            //X509CertificateValidationMode.PeerOrChainTrust;
            try
            {
                sh.Open();

                string address = sh.Description.Endpoints[0].ListenUri.AbsoluteUri;
                Console.WriteLine("Listening @ {0}", address);
                Console.WriteLine("Press enter to close the service");
                Console.ReadLine();
                sh.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("A communication error occurred: {0}", ce.Message);
                Console.WriteLine();
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("An unforeseen error occurred: {0}", exc.Message);
                Console.ReadLine();
            }
#endif
        }

        static void StartTransportMessage()
        {
            // This string uses a function to prepend the computer name at run time.
            string addressHttp = String.Format(
                "https://{0}:8089/Calculator",
                "localhost");  // System.Net.Dns.GetHostEntry("").HostName

            WSHttpBinding b = new WSHttpBinding();
            b.Security.Mode = SecurityMode.TransportWithMessageCredential;
            b.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            b.ReliableSession.Enabled = true;

            // You must create an array of URI objects to have a base address.
            Uri a = new Uri(addressHttp);
            Uri[] baseAddresses = new Uri[] { a };

            // Create the ServiceHost. The service type (Calculator) is not
            // shown here.
            ServiceHost sh = new ServiceHost(typeof(Calculator), baseAddresses);

            // Add an endpoint to the service. Insert the thumbprint of an X.509
            // certificate found on your computer.
            Type c = typeof(ICalculator);
            sh.AddServiceEndpoint(c, b, "MyCalculator");
            /*
            sh.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                StoreName.My,
                X509FindType.FindBySubjectName,
                "localhost");  // "contoso.com"
            */

            // http://localhost:8081/Calculator
            string metadata_url = String.Format(
    "http://{0}:8081/Calculator",
    "localhost");  // System.Net.Dns.GetHostEntry("").HostName

            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
            behavior.HttpGetEnabled = true;
            behavior.HttpGetUrl = new Uri(metadata_url);
            sh.Description.Behaviors.Add(behavior);


            // This next line is optional. It specifies that the client's certificate
            // does not have to be issued by a trusted authority, but can be issued
            // by a peer if it is in the Trusted People store. Do not use this setting
            // for production code. The default is PeerTrust, which specifies that
            // the certificate must originate from a trusted certificate authority.

            //sh.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
            //X509CertificateValidationMode.PeerOrChainTrust;
            try
            {
                sh.Open();

                string address = sh.Description.Endpoints[0].ListenUri.AbsoluteUri;
                Console.WriteLine("Listening @ {0}", address);
                Console.WriteLine("Press enter to close the service");
                Console.ReadLine();
                sh.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("A communication error occurred: {0}", ce.Message);
                Console.WriteLine();
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("An unforeseen error occurred: {0}", exc.Message);
                Console.ReadLine();
            }
        }
    }

    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            return;
        }
    }

    /*
     * https://docs.microsoft.com/en-us/dotnet/framework/wcf/samples/ws-transport-security
     * Because the certificate used in this sample is a test certificate created with Makecert.exe, a security alert appears when you try to access an https: address, such as https://localhost/servicemodelsamples/service.svc, from your browser. To allow the Windows Communication Foundation (WCF) client to work with a test certificate in place, some additional code has been added to the client to suppress the security alert. This code, and the accompanying class, is not required when using production certificates.

C#

Copy
// This code is required only for test certificates like those created by Makecert.exe.  
PermissiveCertificatePolicy.Enact("CN=ServiceModelSamples-HTTPS-Server");  

     * 
     * 
     * */

    // https://github.com/dotnet/wcf/issues/2499
    // Could not establish trust relationship for the SSL/TLS secure channel with authority #2499
    /* Will this work:

BasicHttpsBinding binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
endpointAddress = new EndpointAddress(new Uri("http://myserver/MyService.svc"));
factory = new ChannelFactory<IService>(binding, endpointAddress);
factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication();
factory.Credentials.ServiceCertificate.SslCertificateAuthentication.CertificateValidationMode = X509CertificateValidationMode.None;
@alextochetto

     * 
     * 
     * 
     * 
     * */
}
