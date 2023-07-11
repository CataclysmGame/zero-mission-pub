using Nethereum.Web3;
using System;
using UnityEngine;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.NEthereum;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using Nethereum.Contracts;
using Cysharp.Threading.Tasks;
using QRCoder;
using System.Linq;

public class Web3Manager
{
    private static readonly string ERC71_ABI = "[ { \"inputs\": [ { \"internalType\": \"string\", \"name\": \"name_\", \"type\": \"string\" }, { \"internalType\": \"string\", \"name\": \"symbol_\", \"type\": \"string\" } ], \"stateMutability\": \"nonpayable\", \"type\": \"constructor\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"approved\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"Approval\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\" }, { \"indexed\": false, \"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\" } ], \"name\": \"ApprovalForAll\", \"type\": \"event\" }, { \"anonymous\": false, \"inputs\": [ { \"indexed\": true, \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\" }, { \"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"Transfer\", \"type\": \"event\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"approve\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\" } ], \"name\": \"balanceOf\", \"outputs\": [ { \"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"getApproved\", \"outputs\": [ { \"internalType\": \"address\", \"name\": \"\", \"type\": \"address\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\" } ], \"name\": \"isApprovedForAll\", \"outputs\": [ { \"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [], \"name\": \"name\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"ownerOf\", \"outputs\": [ { \"internalType\": \"address\", \"name\": \"\", \"type\": \"address\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" }, { \"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\" } ], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\" }, { \"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\" } ], \"name\": \"setApprovalForAll\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"bytes4\", \"name\": \"interfaceId\", \"type\": \"bytes4\" } ], \"name\": \"supportsInterface\", \"outputs\": [ { \"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [], \"name\": \"symbol\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"tokenURI\", \"outputs\": [ { \"internalType\": \"string\", \"name\": \"\", \"type\": \"string\" } ], \"stateMutability\": \"view\", \"type\": \"function\" }, { \"inputs\": [ { \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\" }, { \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\" }, { \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\" } ], \"name\": \"transferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\" } ]";

    public struct WalletConnectionResult
    {
        public string Account { get; private set; }
        public Web3 Web3 { get; private set; }

        public WalletConnectionResult(string account, Web3 web3)
        {
            Account = account;
            Web3 = web3;
        }
    }

    private static Web3Manager sInstance = null;

    public static Web3Manager Instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = new Web3Manager();
            }
            return sInstance;
        }
    }

    public string IpfsGatewayUrl { get; set; } = "https://nftfactory.mypinata.cloud/ipfs/";

    public bool UseTestnet { get; set; } = false;

    public WalletConnect CreateWallet()
    {
        var meta = new ClientMeta()
        {
            Description = "NFTFactory WalletConnect Test",
            Icons = new[] { "https://www.nft-factory.club/favicon.ico" },
            Name = "WalletConnect Test",
            URL = "nft-factory.club",
        };

        return new WalletConnect(meta);
    }

    public async void ConnectWallet(WalletConnect wc)
    {
        Logger.Log("Connecting to wallet...");
        await wc.Connect();
        Logger.Log("Connected to wallet");
    }

    public Web3 CreateWeb3(WalletConnect wc, string providerUri)
    {
        // Return first account
        var account = wc.Accounts[0];

        var provider = wc.CreateProvider(new Uri(providerUri));

        return new Web3(provider);
    }

    public async UniTask DisconnectWallet(WalletConnect wc)
    {
        await wc.Disconnect();
        Logger.Log("Disconnected from wallet");
    }

    public Contract GetContract(Web3 web3, string address, string abi)
    {
        return web3.Eth.GetContract(abi, address);
    }

    public Contract GetERC721Contract(Web3 web3, string address)
    {
        return web3.Eth.GetContract(ERC71_ABI, address);
    }

    public async UniTask<int> GetERC721Balance(string account, Contract contract)
    {
        return await contract.GetFunction("balanceOf").CallAsync<int>(account);
    }

    public async UniTask<string> GetERC721OwnerOf(int token, Contract contract)
    {
        return await contract.GetFunction("ownerOf").CallAsync<string>(token);
    }

    public async UniTask<string> GetERC721TokenUri(int token, Contract contract)
    {
        return await contract.GetFunction("tokenURI").CallAsync<string>(token);
    }

    /**
     * Gets all the tokens of a given account using the ERC721Enumerable extension
     */
    public async UniTask<List<int>> GetERC721EnumerableOwnerTokens(string account, Contract contract)
    {
        var balance = await GetERC721Balance(account, contract);
        var tokens = new List<int>(balance);
        for (int i = 0; i < balance; i++)
        {
            var token = await contract.GetFunction("tokenOfOwnerByIndex").CallAsync<int>(account, i);
            tokens.Add(token);
        }
        return tokens;
    }

    /**
     * Gets all the token of a given account by iterating ownership history
     */
    public async UniTask<List<int>> GetERC721OwnerTokensByHistory(string account, Contract contract)
    {
        var tokens = new List<int>();
        var address = Web3.ToChecksumAddress(account);
        var history = await contract.GetFunction("getOwnershipHistory").CallAsync<int[]>();
        foreach (int token in history)
        {
            var owner = await GetERC721OwnerOf(token, contract);
            if (owner == address)
            {
                tokens.Add(token);
            }
        }
        return tokens;
    }

    public async UniTask<Dictionary<string, object>> FetchTokenMetadata(int token, Contract contract)
    {
        var metadataUri = await GetERC721TokenUri(token, contract);
        var metadataBytes = await FetchDataFromIpfs(metadataUri);
        if (metadataBytes == null || metadataBytes.Length == 0)
        {
            return null;
        }

        var metadataText = System.Text.Encoding.UTF8.GetString(metadataBytes);

        return JsonConvert.DeserializeObject<Dictionary<string, object>>(metadataText);
    }

    public async UniTask<Texture2D> FetchTokenTexture(string textureUrlMetadataField, int token, Contract contract)
    {
        var metadata = await FetchTokenMetadata(token, contract);
        if (metadata == null)
        {
            return null;
        }

        var textureIpfsUri = (string)metadata[textureUrlMetadataField];
        var texture = await FetchTextureFromIpfs(textureIpfsUri);

        return texture;
    }

    public async UniTask<byte[]> FetchDataFromIpfs(string ipfsAddr)
    {
        var www = UnityWebRequest.Get(BuildIpfsGwAddress(ipfsAddr));
        await www.SendWebRequest();
        return www.downloadHandler.data;
    }

    public async UniTask<Texture2D> FetchTextureFromIpfs(string ipfsAddr)
    {
        var www = UnityWebRequestTexture.GetTexture(BuildIpfsGwAddress(ipfsAddr));
        await www.SendWebRequest();
        return DownloadHandlerTexture.GetContent(www);
    }

    private string BuildIpfsGwAddress(string ipfsAddr)
    {
        var addrWithoutProtocol = ipfsAddr.Replace("ipfs://", "");
        return IpfsGatewayUrl + addrWithoutProtocol;
    }

    public void UpdateWalletWsQueue(WalletConnect wc)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (wc.Transport is UnityTransport)
        {
            (wc.Transport as UnityTransport).DispatchSocketMessageQueue();
        }
#endif
    }

    public async UniTask OnApplicationPause(WalletConnect wc, bool pauseStatus)
    {
        if (wc.Transport is UnityTransport)
        {
            await (wc.Transport as UnityTransport).OnApplicationPause(pauseStatus);
        }
    }

    public QRCodeData GenerateQRCodeData(string uri)
    {
        var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.M);
        return data;
    }

    public QRCodeData GenerateWalletQRCodeData(WalletConnect wc)
    {
        return GenerateQRCodeData(wc.URI);
    }

    public Texture2D GenerateQRCodeTexture(QRCodeData data, int pixelsPerModule, Color darkColor, Color lightColor)
    {
        int size = data.ModuleMatrix.Count * pixelsPerModule;
        var newTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        for (int x = 0; x < size; x += pixelsPerModule)
        {
            for (int y = 0; y < size; y += pixelsPerModule)
            {
                var module = data.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];

                newTexture.SetPixels(
                        x,
                        y,
                        pixelsPerModule,
                        pixelsPerModule,
                        Enumerable.Repeat(module ? darkColor : lightColor, pixelsPerModule * pixelsPerModule).ToArray()
                );
            }
        }
        newTexture.Apply();
        return newTexture;
    }

    public Texture2D GenerateWalletQRCodeTexture(
        WalletConnect wc,
        Color lightColor,
        Color darkColor,
        int pixelsPerModule = 10)
    {
        return GenerateQRCodeTexture(
            GenerateWalletQRCodeData(wc),
            pixelsPerModule,
            darkColor,
            lightColor
        );
    }

    public Texture2D GenerateWalletQRCodeTexture(
        WalletConnect wc,
        int pixelsPerModule = 10)
    {
        return GenerateWalletQRCodeTexture(
            wc,
            Color.black,
            Color.white,
            pixelsPerModule
        );
    }

    public void OpenWalletApplication(WalletConnect wc)
    {
#if UNITY_ANDROID
        /*
        var walletURI = wc.URI;

        var unityPlayerClazz = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayerClazz.GetStatic<AndroidJavaObject>("currentActivity");
        //var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        var intentClazz = new AndroidJavaClass("android.content.Intent");
        var intent = new AndroidJavaObject("android.content.Intent");

        var uriClazz = new AndroidJavaClass("android.net.Uri");
        var uriData = uriClazz.CallStatic<AndroidJavaObject>("parse", walletURI);
        //var intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", "io.metamask");

        intent.Call<AndroidJavaObject>("setAction", "android.intent.action.VIEW");
        intent.Call<AndroidJavaObject>("setData", uriData);

        currentActivity.Call("startActivity", intent);
        */
        Application.OpenURL(wc.URI);
#else
        Application.OpenURL(wc.URI);
#endif
    }
}
