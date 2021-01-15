/*
    O intuito desse projeto é simplificar as requisições SOAP para não existir a necessidade de ficar preso aos contratos burocráticos do WCF permitindo escalabilidade e maleabilidade das requisições
    O conceito é bem simples, e dará uma maleabilidade maior para o desenvolvedor em trabalhar as requisições e seus retornos de forma mais transparente.
    Usando metodos mais eficientes e customizaveis para coletar o SOAP/XML e o transformar em JSON para que seja mais facilmente serializado usando as bibliotecas da NewtonSoft.Json com suas annotations
    
    Essencialmente os contratos SOAP WCF não permitem a abstração e modificação de suas classes e objetos de forma amigável
    Com esse projeto será facil criar Headers e parametros para a requisição assim como o XML de retorno será transformado em um JSON.
    Desta forma haverá mais facilidade de transformar os dados em uma classe tipada ou um objeto dinamico em construção da forma que o DEV quiser!
    
    - Voce precisará criar sua classe de serialização a partir do JSON (que foi convertido do XML de retorno de determinado método WSSOAP/WSDL) 
    - Após criar as classes e fazer as annotations do NewtonSoft.Json voce instanciará um objeto da classe "SOAPRequestUtil" e executar o método "ExecutaSoapRequest()" que tem 2 sobrecargas:
            1 - T ExecutaSoapRequest<T>
            2 - dynamic ExecutaSoapRequest    
        - Esses metodos esperam o XML do request, um objeto de classe "WebProxy" e um objeto da classe "SOAPParametrosUtil"
        - O projeto tem uma classe "SOAPProxyUtil" e tem um metodo de extensão (MontaWebProxy) que retorna um "WebProxy" já parametrizado de acordo com sua necessidade.
        - O projeto tem uma classe "SOAPParametrosUtil" que tem as propriedades de parametros e um metodo de extensão (AdicionaEditaKeyValueDicionarioDeHeaders) para construção do dicionario <string,string> de headers (caso o request tenha custom headers)
    - Por fim o método irá reconhecer o o tipo da classe do objeto para que consiga serializar o JSON corretamente para a sua classe!
    
    Sugiro usar algumas ferramentas online de conversão para facilitar o desenvolvimento
    
    XML --> JSON: http://www.utilities-online.info/xmltojson/#.X_im-ebPyHu
    JSON --> C#: https://app.quicktype.io/?l=csharp

 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SOAPUtil
{
    public class SOAPRequestUtil
    {

        #region Criação de Request SOAP com WEBRequest (sem WCF)
        public T ExecutaSoapRequest<T>(string xmlRequest, bool usaProxy, WebProxy webProxy, SOAPParametrosUtil parametrosRequest)
        {
            string requestResult = string.Empty;
            try
            {
                //Proxy
                if (usaProxy)
                {
                    WebRequest.DefaultWebProxy = webProxy;
                }

                //Monta Requisição WEB com os headers e o body
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(xmlRequest);
                HttpWebRequest webRequest = CreateWebRequest(parametrosRequest);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                //Faz a Requisição
                var webResponse = webRequest.GetResponse();

                //Le o retorno Http da requisição
                StreamReader rd = new StreamReader(webResponse.GetResponseStream());
                requestResult = rd.ReadToEnd();

                //Serializa o retorno de string para XML
                XDocument doc = XDocument.Parse(requestResult);

                //Serializa o retorno de XML para JSON
                //Esse processo é executado para que o retorno possa ser serializado em uma classe tipada/customizada pelo dev com as facilidades/anotações da lib NewtonSoft.Json
                string jsonText = JsonConvert.SerializeXNode(doc);

                //Retorna o objeto serializado
                return JsonConvert.DeserializeObject<T>(jsonText);

            }
            catch (WebException wex)
            {
                string pageContent = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd().ToString();
                throw new Exception("Erro na Requisição SOAP Action: " + parametrosRequest.SOAPAction + ", Exeption: " + wex.Message + ", InnerException: " + wex.InnerException + "\n" + "HTML: " + pageContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na Requisição SOAP Action: " + parametrosRequest.SOAPAction + "; HTML: " + requestResult.Trim() + " ; EX: " + ex.Message);
            }
        }
        public dynamic ExecutaSoapRequest(string xmlRequest, bool usaProxy, WebProxy webProxy, SOAPParametrosUtil parametrosRequest)
        {

            string requestResult = string.Empty;
            try
            {
                //Proxy
                if (usaProxy)
                {
                    WebRequest.DefaultWebProxy = webProxy;
                }

                //Monta Requisição WEB com os headers e o body
                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(xmlRequest);
                HttpWebRequest webRequest = CreateWebRequest(parametrosRequest);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                //Faz a Requisição
                var webResponse = webRequest.GetResponse();

                //Le o retorno Http da requisição
                StreamReader rd = new StreamReader(webResponse.GetResponseStream());
                requestResult = rd.ReadToEnd();

                //Serializa o retorno de string para XML
                XDocument doc = XDocument.Parse(requestResult);

                //Serializa o retorno de XML para JSON
                //Esse processo é executado para que o retorno possa ser serializado em uma classe tipada/customizada pelo dev com as facilidades/anotações da lib NewtonSoft.Json
                string jsonText = JsonConvert.SerializeXNode(doc);

                //Transforma o Json em um objeto Dynamic.ExpandoObject
                var converter = new ExpandoObjectConverter();
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText, converter);

                //Retorna um objeto Dinamico
                return dyn;

            }
            catch (WebException wex)
            {
                string pageContent = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd().ToString();

                if (requestResult.Contains("Requisição inválida!"))
                {
                    throw new Exception(pageContent);
                }
                else
                {
                    throw new Exception("Erro na Requisição SOAP Action: " + parametrosRequest.SOAPAction + ", Exeption: " + wex.Message + ", InnerException: " + wex.InnerException + "\n" + "HTML: " + pageContent);
                }
            }
            catch (Exception ex)
            {
                if (requestResult.Contains("Requisição inválida!"))
                {
                    throw new Exception("Erro na Requisição SOAP Action: " + parametrosRequest.SOAPAction + "; " + requestResult.Trim());
                }
                else
                {
                    throw ex;
                }
            }
        }

        private HttpWebRequest CreateWebRequest(SOAPParametrosUtil parametrosRequest)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(parametrosRequest.postEndpoint);



            webRequest.ServerCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);
            webRequest.Timeout = parametrosRequest.timeoutRequest;
            webRequest.ContentType = parametrosRequest.contentType;
            webRequest.Method = parametrosRequest.metodoREST;
            if (!string.IsNullOrEmpty(parametrosRequest.SOAPAction))
            {
                webRequest.Headers.Add("SOAPAction", "\"" + parametrosRequest.SOAPAction + "\"");
            }
            if (parametrosRequest.usaAutenticacaoBasic)
            {
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(parametrosRequest.usaAutenticacaoBasic + ":" + parametrosRequest.autenticacaoBasicSenha));
                webRequest.Headers.Add("Authorization", "Basic " + encoded);
            }
            if (parametrosRequest.usaNetworkCredentials)
            {
                webRequest.Credentials = new NetworkCredential(parametrosRequest.networkCredentialsUsuario, parametrosRequest.networkCredentialsSenha);
            }
            if (parametrosRequest.temHeaders)
            {
                foreach (var header in parametrosRequest.dicHeaders)
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }
            }

            return webRequest;
        }

        private bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private XmlDocument CreateSoapEnvelope(string xml)
        {
            XmlDocument soapEnvelop = new XmlDocument();
            soapEnvelop.LoadXml(xml);

            return soapEnvelop;
        }

        private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        #endregion
    }
}
