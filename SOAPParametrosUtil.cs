using System.Collections.Generic;

namespace SOAPUtil
{
    public class SOAPParametrosUtil
    {
        public string postEndpoint { get; set; }
        public string SOAPAction { get; set; }
        public string metodoREST { get; set; }
        public int timeoutRequest { get; set; }
        public bool usaAutenticacaoBasic { get; set; }
        public string autenticacaoBasicUsuario { get; set; }
        public string autenticacaoBasicSenha { get; set; }
        public bool usaNetworkCredentials { get; set; }
        public string networkCredentialsUsuario { get; set; }
        public string networkCredentialsSenha { get; set; }
        public string networkCredentialsDominio { get; set; }        
        public string contentType { get; set; }
        public bool temHeaders { get; set; }
        public IDictionary<string, string> dicHeaders { get; set; }

        #region Dicionario de Headers
        public IDictionary<string, string> AdicionaEditaKeyValueDicionarioDeHeaders(IDictionary<string, string> _dicionario, string _chave, string _valor)
        {
            if (_dicionario.ContainsKey(_chave))
            {
                _dicionario.Remove(_chave);
            }
            _dicionario.Add(_chave, _valor);
            return _dicionario;
        }
        #endregion
    }
}
