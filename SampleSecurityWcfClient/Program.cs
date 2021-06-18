using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using SampleSecurityWcfClient.ServiceReference1;

// https://stackoverflow.com/questions/18149819/messagesecurityexception-the-http-request-was-forbidden-with-client-authenticat
namespace SampleSecurityWcfClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RequestTransportMessage();
        }

        static void RequestTransportMessage()
        {
            // http://localhost:8081/Calculator

            /*
            CalculatorClient client = new CalculatorClient("WSHttpBinding_ICalculator");

            client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.PeerOrChainTrust;
            client.ClientCredentials.ServiceCertificate.Authentication.CustomCertificateValidator =
new MyValidator();
            */

            // Create the binding.  
            var myBinding = new WSHttpBinding();
            myBinding.Security.Mode = SecurityMode.TransportWithMessageCredential;  // .Transport;
            myBinding.Security.Transport.ClientCredentialType =
               HttpClientCredentialType.Certificate;
            myBinding.ReliableSession.Enabled = true;

            // Create the endpoint address. Note that the machine name
            // must match the subject or DNS field of the X.509 certificate  
            // used to authenticate the service.
            var ea = new
               EndpointAddress("https://localhost:8089/Calculator/MyCalculator");

            // Create the client. The code for the calculator
            // client is not shown here. See the sample applications  
            // for examples of the calculator code.  
            var client =
               new CalculatorClient(myBinding, ea);

            /*
            // The client must specify a certificate trusted by the server.  
            client.ClientCredentials.ClientCertificate.SetCertificate(
                StoreLocation.CurrentUser,  // 这里和服务器端不同
                StoreName.My,
                X509FindType.FindByThumbprint,  // .FindBySubjectName,
                "872801bbd93e75f327b34ae76366fa05b229433b");  // "contoso.com"
            */

            var result = client.Add(1, 2);
            Console.Write($"result={result}");
        }
    }

    public class MyValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
            return;
        }
    }
}
