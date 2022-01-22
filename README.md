# Estudos de uso de Microsoft Authentication Libraries (MSAL) em aplicações console.

## 1. Sobre

Essa é uma aplicação de estudo da utilização do sdk _Microsoft Authentication Libraries_ (MSAL) em aplicações console.

## 2. Requisitos

1. É preciso ter uma conta no Azure.
2. É preciso ter uma aplicação registrada no AzureAd configurada para uso das plataformas _Mobile and desktop aplications_. Um passo-a-passo pode ser encontrado [aqui](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app). Por utilzar o browser local é preciso a _Redirect Uri_ configurada para _http://localhost_.
3. Para o uso do _Device Code Flow_, fluxo padrão, é preciso deixar a opção _Allow public client flows_ habilitada.

## 3. Configuração

O aplicativo aceita as cofigurações através de alteração do _appsetting.json_ na raiz do diretório _src/_, através de _user secrets_ e variáveis de ambiente. Recomendamos a utilização de _user secrets_.

É preciso configurar os seguintes parâmetros para o uso:

* **TenantId**: que é o id do AzureAd da organização. Também pode assumir os valores _common_ ou _organization_ e é encontrado na área _Overview_ da aplicação configurada no AzureAD;
* **ClientId**: que o id da aplicação no AzureAd. é encontrado na área _Overview_ da aplicação configurada no AzureAD;
* **EncryptKey**: é chave de encriptação do arquivo quando a opção _UseEncriptedCache_ esta habilitada. O _UseEncriptedCache_ esta habilitado por _default_.

Os seguinte parâmetros tem configuração opcional:

* **UseConfidentialAppFlow**: Deternina se seguirá o fluxo de autenticação para aplicações _daemon_ que utilizam um _Secret_ gerado no Azure AD, o _default_ é false.
* **UseEncriptedCache**: Para evitar que aplicação solicite sempre credências para o usuário é utilizado um cache em um arquivo texto, que pode ou não estar encriptado. O parâmetro pode ser True ou False, o _default_ é True.
* **AuthenticationFlow**: Determina o tipo de fluxo de uma aplicação com cliente público. Pode ser:
    - _DeviceCodeFlow_: Utilizado em cenários onde _não_ existe browser na máquina onde o aplicativo console é executado.
    - _InteractiveFlow_: Utilizado em cenário onde existe um browser na máquina onde o aplicativo console é executado.
    - _WindowsIntegrated_: Autenticação integrada com o Windows.

Abaixo um exemplo de configuração dos _user secrets_ através de linha de comando:

``` Shell
$ dotnet user-secrets set AzureAd:TenantId 9d7978bf-cab0-46b9-b5f8-4e0524ba7eea
$ dotnet user-secrets set AzureAd:ClientId 36ac5d15-38ea-41b6-b154-eb12c02c3e89
$ dotnet user-secrets set EncryptKey z4mXxBf8TpEs7bzt
$ dotnet user-secrets set UseDeviceCodeFlow False
$ dtonet user-secrets set UseEncriptedCache False
```

## 4. Execução

No diretório raiz da aplicação digite os seguintes comandos:

``` Shell
$ cd src
$ dotnet run
```

## Referências

MICROSOFT. **Desktop app that calls web APIs: Acquire a token**, 29-set.-2021. Disponível em: <https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-desktop-acquire-token?tabs=dotnet> Acessado em: 16-jan-2022

MICROSOFT. **Token cache serialization in MSAL.NET**, 02-dez.-2021. Disponível em: <https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnetcore> Acessado em: 16-jan-2022

MICROSOFT, **Encrypting data**, 15-set.-2021. Disponível em: <https://docs.microsoft.com/en-us/dotnet/standard/security/encrypting-data> Acessado em: 16-jan-2022

MICROSOFT, **Decrypting data**, 15-set.-2021. Disponível em: <https://docs.microsoft.com/en-us/dotnet/standard/security/encrypting-data> Acessado em: 16-jan-2022
