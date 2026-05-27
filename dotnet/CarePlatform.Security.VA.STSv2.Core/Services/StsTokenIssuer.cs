using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CarePlatform.Security.VA.STSv2.Certificates;
using CarePlatform.Security.VA.STSv2.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarePlatform.Security.VA.STSv2.Services
{
    /// <summary>
    /// Issues a SAML token from the VA SSOi STS by sending a WS-Trust SOAP request
    /// signed with a client certificate. The OBO access token is sent inside
    /// <c>BinarySecurityToken</c>.
    /// </summary>
    public class StsTokenIssuer
    {
        private readonly SsoConfigOptions _sso;
        private readonly CertificateUtil _certificateUtil;
        private readonly ILogger<StsTokenIssuer> _logger;

        public StsTokenIssuer(
            IOptions<SsoConfigOptions> ssoOptions,
            CertificateUtil certificateUtil,
            ILogger<StsTokenIssuer> logger)
        {
            _sso = ssoOptions.Value;
            _certificateUtil = certificateUtil;
            _logger = logger;
        }

        public string BuildSoapRequest(string oboAccessToken, string appUrlOverride = null)
        {
            var appUrl = appUrlOverride ?? _sso.AppUrl;

            var soapRequest = new StringBuilder();
            soapRequest.Append("<?xml version=\"1.0\"?>");
            soapRequest.Append("<soap:Envelope ");
            soapRequest.Append("xmlns:ns1=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512\" ");
            soapRequest.Append("xmlns:wss=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" ");
            soapRequest.Append("xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
            soapRequest.Append("xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" ");
            soapRequest.Append("xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" ");
            soapRequest.Append("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" ");
            soapRequest.Append("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            soapRequest.Append("<soap:Body>");
            soapRequest.Append("<ns1:RequestSecurityToken>");
            soapRequest.Append("<ns1:OnBehalfOf>");
            soapRequest.Append("<ns1:Base>");
            soapRequest.Append("<wss:BinarySecurityToken ");
            soapRequest.Append("EncodingType=\"http://ibm.com/2004/01/itfim/base64encode\" ");
            soapRequest.Append("ValueType=\"urn:ietf:params:oauth:token-type:jwt\">");
            soapRequest.Append(oboAccessToken);
            soapRequest.Append("</wss:BinarySecurityToken>");
            soapRequest.Append("</ns1:Base>");
            soapRequest.Append("</ns1:OnBehalfOf>");
            soapRequest.Append("<wsp:AppliesTo>");
            soapRequest.Append("<wsa:EndpointReference>");
            soapRequest.AppendFormat("<wsa:Address>{0}</wsa:Address>", appUrl);
            soapRequest.Append("</wsa:EndpointReference>");
            soapRequest.Append("</wsp:AppliesTo>");
            soapRequest.Append("<ns1:Issuer>");
            soapRequest.AppendFormat("<wsa:Address>{0}</wsa:Address>", _sso.StsIssuerAddress);
            soapRequest.Append("</ns1:Issuer>");
            soapRequest.Append("<ns1:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</ns1:RequestType>");
            soapRequest.Append("<ns1:TokenType>urn:oasis:names:tc:SAML:2.0:assertion</ns1:TokenType>");
            soapRequest.Append("</ns1:RequestSecurityToken>");
            soapRequest.Append("</soap:Body>");
            soapRequest.Append("</soap:Envelope>");

            return soapRequest.ToString();
        }

        public async Task<string> SendAsync(string soapRequest)
        {
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            using var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                SslProtocols = SslProtocols.Tls12,
            };

            _logger.LogDebug("STS: Looking for cert - Issuer: {Issuer} - Subject: {Subject}",
                _sso.IssuerName, _sso.CertificateName);

            var cert = _certificateUtil.GetCertificate(
                StoreName.My, StoreLocation.LocalMachine,
                _sso.CertificateName, "Client Authentication", _sso.IssuerName);
            handler.ClientCertificates.Add(cert);

            _logger.LogDebug("STS: Cert subject {Subject}, thumbprint {Thumb}",
                cert.SubjectName.Name, cert.Thumbprint);

            using var client = new HttpClient(handler);

            _logger.LogDebug("STS: POST {Endpoint}", _sso.SsoEndpoint);

            var response = await client.PostAsync(_sso.SsoEndpoint, content).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public static string ExtractSamlAssertion(string soapResponseXml)
        {
            var doc = XDocument.Parse(soapResponseXml);
            XNamespace ns = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            var elementName = ns + "RequestedSecurityToken";

            foreach (var element in doc.Descendants(elementName))
            {
                using var reader = element.CreateReader();
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }

            return null;
        }

        public string SsoEndpoint => _sso.SsoEndpoint;
        public string CertificateName => _sso.CertificateName;
    }
}
