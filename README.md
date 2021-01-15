# SOAPUtil

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
