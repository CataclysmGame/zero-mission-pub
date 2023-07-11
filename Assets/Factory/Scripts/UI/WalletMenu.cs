using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class WalletMenu : MonoBehaviour
{
#if UNITY_WEBGL
    //[DllImport("__internal")]
    //private static extern void FetchWebGlNFTs();

    //[DllImport("__internal")]
    //private static extern void MintWebGlNFT();
#endif
    struct WebGlNFTs
    {
        public string[] atlases;
        public string[] avatars;
    }

    enum WalletError
    {
        None = 0,
        QrCodeGeneration = 0x1001E,
        WalletCreation = 0x1A01F,
        WalletHandshake = 0x1B01F,
        WalletConnection = 0x1C01F,
        MetadataFetching = 0x1D01C,
        ContractError = 0x1E01C,
        NoTokens = 0x1F017,
        NFTFetching = 0x20017,
        CorruptedData = 0x21010,
        WalletDisconnection = 0x3001F,
    }

    public Image qrCodeImage;

    //public Image atlasImage;
    public Button copyWcUriButton;
    public Button arcadeButton;
    public Button endlessButton;
    public Button openWalletButton;
    public Button webGlMintButton;
    public Button skipButton;

    public Button leftButton;
    public Button rightButton;

    public Image avatar;
    public GameObject nftScrollView;
    public Image[] nftAtlasImages;
    public Text nftIndexText;
    public Text webGlMintResultText;

    public Image loadingPanel;
    public Image errorPanel;
    public Text errorDescription;
    public Image robotImage;
    public Image playerImage;
    public Sprite[] robotSprites;
    public Sprite[] playerSprites;
    public Text loadingText;

    public Text helpText;

    public float animFps = 12.0f;
    
    public bool dontUseNfts = false;
    public float localAtlasScale = 0.6f;

    public Texture2D[] localAvatars;
    public Texture2D[] localAtlases;
    public CharactersList characters;

    public CharacterInfoPanel infoPanel;

    public string localTexturesFolder = "Assets/Textures/LocalPlayers";

    private const string FactoryContractABIV0 =
        "[\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"string\",\n          \"name\": \"name_\",\n          \"type\": \"string\"\n        },\n        {\n          \"internalType\": \"string\",\n          \"name\": \"symbol_\",\n          \"type\": \"string\"\n        },\n        {\n          \"internalType\": \"string\",\n          \"name\": \"metadataURI_\",\n          \"type\": \"string\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"maxId_\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"constructor\"\n    },\n    {\n      \"anonymous\": false,\n      \"inputs\": [\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"owner\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"approved\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"Approval\",\n      \"type\": \"event\"\n    },\n    {\n      \"anonymous\": false,\n      \"inputs\": [\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"owner\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"operator\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": false,\n          \"internalType\": \"bool\",\n          \"name\": \"approved\",\n          \"type\": \"bool\"\n        }\n      ],\n      \"name\": \"ApprovalForAll\",\n      \"type\": \"event\"\n    },\n    {\n      \"anonymous\": false,\n      \"inputs\": [\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"previousOwner\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"newOwner\",\n          \"type\": \"address\"\n        }\n      ],\n      \"name\": \"OwnershipTransferred\",\n      \"type\": \"event\"\n    },\n    {\n      \"anonymous\": false,\n      \"inputs\": [\n        {\n          \"indexed\": false,\n          \"internalType\": \"string\",\n          \"name\": \"_value\",\n          \"type\": \"string\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"uint256\",\n          \"name\": \"_id\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"PermanentURI\",\n      \"type\": \"event\"\n    },\n    {\n      \"anonymous\": false,\n      \"inputs\": [\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"from\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"address\",\n          \"name\": \"to\",\n          \"type\": \"address\"\n        },\n        {\n          \"indexed\": true,\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"Transfer\",\n      \"type\": \"event\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"to\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"approve\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"owner\",\n          \"type\": \"address\"\n        }\n      ],\n      \"name\": \"balanceOf\",\n      \"outputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"contractURI\",\n      \"outputs\": [\n        {\n          \"internalType\": \"string\",\n          \"name\": \"\",\n          \"type\": \"string\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"getApproved\",\n      \"outputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"\",\n          \"type\": \"address\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"owner\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"address\",\n          \"name\": \"operator\",\n          \"type\": \"address\"\n        }\n      ],\n      \"name\": \"isApprovedForAll\",\n      \"outputs\": [\n        {\n          \"internalType\": \"bool\",\n          \"name\": \"\",\n          \"type\": \"bool\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"mint\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"name\",\n      \"outputs\": [\n        {\n          \"internalType\": \"string\",\n          \"name\": \"\",\n          \"type\": \"string\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"owner\",\n      \"outputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"\",\n          \"type\": \"address\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"ownerOf\",\n      \"outputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"\",\n          \"type\": \"address\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"renounceOwnership\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"from\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"address\",\n          \"name\": \"to\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"safeTransferFrom\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"from\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"address\",\n          \"name\": \"to\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        },\n        {\n          \"internalType\": \"bytes\",\n          \"name\": \"_data\",\n          \"type\": \"bytes\"\n        }\n      ],\n      \"name\": \"safeTransferFrom\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"operator\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"bool\",\n          \"name\": \"approved\",\n          \"type\": \"bool\"\n        }\n      ],\n      \"name\": \"setApprovalForAll\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"bytes4\",\n          \"name\": \"interfaceId\",\n          \"type\": \"bytes4\"\n        }\n      ],\n      \"name\": \"supportsInterface\",\n      \"outputs\": [\n        {\n          \"internalType\": \"bool\",\n          \"name\": \"\",\n          \"type\": \"bool\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"symbol\",\n      \"outputs\": [\n        {\n          \"internalType\": \"string\",\n          \"name\": \"\",\n          \"type\": \"string\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"index\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"tokenByIndex\",\n      \"outputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"owner\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"index\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"tokenOfOwnerByIndex\",\n      \"outputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"tokenURI\",\n      \"outputs\": [\n        {\n          \"internalType\": \"string\",\n          \"name\": \"\",\n          \"type\": \"string\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [],\n      \"name\": \"totalSupply\",\n      \"outputs\": [\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"stateMutability\": \"view\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"from\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"address\",\n          \"name\": \"to\",\n          \"type\": \"address\"\n        },\n        {\n          \"internalType\": \"uint256\",\n          \"name\": \"tokenId\",\n          \"type\": \"uint256\"\n        }\n      ],\n      \"name\": \"transferFrom\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    },\n    {\n      \"inputs\": [\n        {\n          \"internalType\": \"address\",\n          \"name\": \"newOwner\",\n          \"type\": \"address\"\n        }\n      ],\n      \"name\": \"transferOwnership\",\n      \"outputs\": [],\n      \"stateMutability\": \"nonpayable\",\n      \"type\": \"function\"\n    }\n  ]";
    //private const string FactoryContractABIV1 = "";

    private readonly Dictionary<int, string> ContractsABIByNetwork = new Dictionary<int, string>()
    {
        { EthereumNetwork.PolygonMainNet.ChainID, FactoryContractABIV0 },
        { EthereumNetwork.Mumbai.ChainID, FactoryContractABIV0 },
        //{ EthereumNetwork.Ethereum.ChainID, FactoryContractABIV1 },
    };

    private readonly Dictionary<int, string[]> ContractsAddressesByNetwork = new Dictionary<int, string[]>()
    {
        {
            EthereumNetwork.PolygonMainNet.ChainID, new[]
            {
                "0x537647677540F307959743394c4D311C63c85190",
                "0x9714e7469A0fb149e7676fD16aDC3867fCceCc1c",
            }
        },
        {
            EthereumNetwork.Mumbai.ChainID, new[]
            {
                "0xC79dc481131eF54Afd35E456109EFe1a93294c8b",
            }
        },
        // { EthereumNetwork.Ethereum.ChainID, "" },
    };

    private Web3Manager w3m = Web3Manager.Instance;
    private WalletConnect wc = null;

    private WalletError walletError = WalletError.None;

    private List<List<Sprite>> atlasSprites;
    private List<Sprite> avatarSprites;

    private int currentTokenIndex = 0;
    private int tokensCount = 0;

    private int currentAnimIndex = 0;

    private bool connectionDone = false;

    const int tileSize = 32;
    private float animTimer = 0.0f;

    private int loadingAnimIndex = 0;
    private float loadingTimer = 0;
    private float errorAnimTimer = 0;
    private int errorPanelAnimIndex = 0;

    private float loadingDotsTimer = 0;
    private int loadingDotsCount = 0;

    private UIControls uiControls;

    private void Awake()
    {
#if DONT_USE_NFTS
        dontUseNfts = true;
#else
        dontUseNfts |= !PersistentSettings.Instance.UseNfts;
#endif

        atlasSprites = new List<List<Sprite>>();
        avatarSprites = new List<Sprite>();

        uiControls = new UIControls();

        uiControls.UI.OpenSettings.performed += (_) =>
        {
            loadingPanel.gameObject.SetActive(true);
            SceneManager.LoadSceneAsync("ConnectWalletScene");
        };

        if (dontUseNfts)
        {
            foreach (var image in nftAtlasImages)
            {
                var t = image.transform;
                t.localScale = new Vector3(
                    localAtlasScale,
                    localAtlasScale,
                    t.localScale.z
                );
            }
        }
    }

    private void OnEnable()
    {
        uiControls.Enable();
    }

    private void OnDisable()
    {
        uiControls.Disable();
    }

    private void Start()
    {
        if (dontUseNfts)
        {
            HandleLocal();
            return;
        }

#if UNITY_WEBGL
        HandleWebGl();
#else
        HandleWC();
#endif
    }

    private void OnDestroy()
    {
        if (PersistentMusicPlayer.Instance != null)
        {
            // Stop music
            PersistentMusicPlayer.Instance.ClearQueue();
            PersistentMusicPlayer.Instance.Stop();
        }
    }

    private void AnimateLoading()
    {
        if ((loadingTimer += Time.deltaTime) >= (0.6f / robotSprites.Length))
        {
            loadingTimer = 0;
            robotImage.sprite = robotSprites[loadingAnimIndex];
            loadingAnimIndex = (loadingAnimIndex + 1) % robotSprites.Length;
        }

        if ((loadingDotsTimer += Time.deltaTime) >= 0.5f)
        {
            loadingDotsTimer = 0;
            loadingDotsCount = (loadingDotsCount + 1) % 4;
            loadingText.text = "LOADING " + new string('.', loadingDotsCount);
        }
    }

    private void AnimateErrorPanel()
    {
        if ((errorAnimTimer += Time.deltaTime) >= (0.6f / playerSprites.Length))
        {
            errorAnimTimer = 0;
            playerImage.sprite = playerSprites[errorPanelAnimIndex];
            errorPanelAnimIndex = (errorPanelAnimIndex + 1) % playerSprites.Length;
        }
    }

    public void OpenWallet()
    {
        Logger.Log($"WC URI: {wc.URI}");
        w3m.OpenWalletApplication(wc);
    }

    public void SkipScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void SetWalletError(WalletError error)
    {
        walletError = error;
        if (error == WalletError.NoTokens)
        {
            errorDescription.text = "YOU DON'T HAVE ANY TOKENS";
        }
        else
        {
            errorDescription.text = $"CODE: ({error:X})";
        }

        loadingPanel.gameObject.SetActive(false);
        errorPanel.gameObject.SetActive(true);
    }

    private void HandleLocal()
    {
        avatarSprites = localAvatars.Select((t) => CreateSprite(t)).ToList();
        atlasSprites = localAtlases.Select(CreateAnimationSprites).ToList();

        tokensCount = avatarSprites.Count - 1; // Removes the last one

        currentAnimIndex = 0;
        currentTokenIndex = 0;

        infoPanel.gameObject.SetActive(true);
        SetCharacter();
        UpdateNFTSprites();

        helpText.gameObject.SetActive(false);
        openWalletButton.gameObject.SetActive(false);
        webGlMintButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        qrCodeImage.gameObject.SetActive(false);
        nftScrollView.gameObject.SetActive(true);

        // Reuse copy button
        copyWcUriButton.gameObject.SetActive(false);
        //copyWcUriButton.GetComponentInChildren<Text>().text = "Play Now";

        arcadeButton.gameObject.SetActive(true);
        arcadeButton.onClick.RemoveAllListeners();
        arcadeButton.onClick.AddListener(() => StartPlaying(false));

        endlessButton.gameObject.SetActive(true);
        endlessButton.onClick.RemoveAllListeners();
        endlessButton.onClick.AddListener(() => StartPlaying(true));

        leftButton.gameObject.SetActive(tokensCount > 1);
        rightButton.gameObject.SetActive(tokensCount > 1);

        loadingPanel.gameObject.SetActive(false);

        connectionDone = true;
    }

    private async UniTask HandleWC()
    {
        try
        {
            wc = Web3Manager.Instance.CreateWallet();
            var uri = wc.URI;

            copyWcUriButton.onClick.AddListener(() => { GUIUtility.systemCopyBuffer = uri; });
        }
        catch
        {
            SetWalletError(WalletError.WalletCreation);
            return;
        }

        try
        {
            var qrCodeTex = w3m.GenerateWalletQRCodeTexture(
                wc,
                new Color(0.7764f, 0.6431f, 1.0f),
                Color.black
            );
            var qrCodeSprite = Sprite.Create(
                qrCodeTex,
                new Rect(0, 0, qrCodeTex.width, qrCodeTex.height),
                new Vector2(0.5f, 0.5f)
            );

            qrCodeImage.sprite = qrCodeSprite;
        }
        catch
        {
            SetWalletError(WalletError.QrCodeGeneration);
            return;
        }

        wc.OnSessionCreated += (s, e) =>
        {
            Logger.Log("Session created");
            //w3m.OpenWalletApplication(wc);
        };

        wc.OnSessionConnect += (s, e) =>
        {
            Logger.Log("Session connected");

            int chainID = e.ChainId;

            var network = FindNetwork(chainID);

            FetchNFTTextures(network);
        };

        wc.OnTransportConnect += (s, e) => { Logger.Log("Transport connected"); };

        try
        {
            w3m.ConnectWallet(wc);
        }
        catch
        {
            SetWalletError(WalletError.WalletHandshake);
            return;
        }
    }

    public void SetWebGlMinted(int mintedCount)
    {
        loadingPanel.gameObject.SetActive(false);
        webGlMintResultText.gameObject.SetActive(true);
        if (mintedCount > 0)
        {
            webGlMintResultText.text = "Minted " + mintedCount + " NFTs";
        }
        else
        {
            webGlMintResultText.text = "Failed to mint an NFT";
        }
    }

    public async UniTask SetWebGlNFTs(string nftsJson)
    {
        Logger.Log("Setting NFTs...");

        var nfts = JsonConvert.DeserializeObject<WebGlNFTs>(nftsJson);

        atlasSprites.Clear();
        avatarSprites.Clear();

        if (nfts.avatars.Length == 0 || nfts.atlases.Length == 0)
        {
            SetWalletError(WalletError.NoTokens);
            return;
        }

        if (nfts.avatars.Length != nfts.atlases.Length)
        {
            SetWalletError(WalletError.CorruptedData);
            return;
        }

        int totalTokens = nfts.avatars.Length;

        for (int i = 0; i < totalTokens; ++i)
        {
            var avatarUri = nfts.avatars[i];
            var atlasUri = nfts.atlases[i];

            Texture2D avatarTexture;
            Texture2D atlasTexture;

            try
            {
                var www = UnityWebRequestTexture.GetTexture(avatarUri);
                await www.SendWebRequest();
                avatarTexture = DownloadHandlerTexture.GetContent(www);

                www = UnityWebRequestTexture.GetTexture(atlasUri);
                await www.SendWebRequest();
                atlasTexture = DownloadHandlerTexture.GetContent(www);
            }
            catch
            {
                SetWalletError(WalletError.NFTFetching);
                return;
            }

            try
            {
                atlasTexture.filterMode = FilterMode.Point;

                var animation = CreateAnimationSprites(atlasTexture);

                atlasSprites.Add(animation);
                avatarSprites.Add(CreateSprite(avatarTexture));
            }
            catch
            {
                SetWalletError(WalletError.CorruptedData);
                return;
            }
        }

        try
        {
            tokensCount = totalTokens;
            currentAnimIndex = 0;
            currentTokenIndex = 0;
            UpdateNFTSprites();
        }
        catch
        {
            SetWalletError(WalletError.CorruptedData);
            return;
        }

        openWalletButton.gameObject.SetActive(false);
        webGlMintButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        qrCodeImage.gameObject.SetActive(false);
        nftScrollView.gameObject.SetActive(true);

        // Reuse copy button
        copyWcUriButton.gameObject.SetActive(false);
        //copyWcUriButton.GetComponentInChildren<Text>().text = "Play Now";

        arcadeButton.gameObject.SetActive(true);
        arcadeButton.onClick.RemoveAllListeners();
        arcadeButton.onClick.AddListener(() => StartPlaying(false));

        endlessButton.gameObject.SetActive(true);
        endlessButton.onClick.RemoveAllListeners();
        endlessButton.onClick.AddListener(() => StartPlaying(true));

        leftButton.gameObject.SetActive(tokensCount > 1);
        rightButton.gameObject.SetActive(tokensCount > 1);

        loadingPanel.gameObject.SetActive(false);

        connectionDone = true;
    }

    public void SetWebGlWalletError(string errorText)
    {
        walletError = WalletError.WalletCreation;
        errorDescription.text = errorText;
        loadingPanel.gameObject.SetActive(false);
        errorPanel.gameObject.SetActive(true);
    }

    private async UniTask HandleWebGl()
    {
        Logger.Log("Handle WebGl called");

        helpText.gameObject.SetActive(false);
        qrCodeImage.gameObject.SetActive(false);

        var openWalletTransform = openWalletButton.GetComponent<RectTransform>();
        openWalletTransform.localPosition = new Vector3(
            openWalletTransform.localPosition.x,
            232.0f,
            openWalletTransform.localPosition.z
        );

        openWalletButton.GetComponentInChildren<Text>().text = "Load NFTs";
        openWalletButton.gameObject.SetActive(true);
        openWalletButton.onClick.RemoveAllListeners();

        void FetchFn()
        {
            webGlMintResultText.gameObject.SetActive(false);
            loadingPanel.gameObject.SetActive(true);
            Application.ExternalCall("window._unityFetchNFTs");
        }

        openWalletButton.onClick.AddListener(FetchFn);

        webGlMintButton.gameObject.SetActive(PersistentSettings.Instance && PersistentSettings.Instance.CanMint);
        webGlMintButton.onClick.RemoveAllListeners();
        webGlMintButton.onClick.AddListener(() =>
        {
            webGlMintResultText.gameObject.SetActive(false);
            loadingPanel.gameObject.SetActive(true);
            Application.ExternalCall("window._unityMintNFT");
        });

        if (PersistentSettings.Instance.AutoFetch)
        {
            FetchFn();
        }
    }

    private EthereumNetwork FindNetwork(int chainID)
    {
        /*
        if (chainID != EthereumNetwork.Ethereum.ChainID)
        {
            // Ethereum is not supported
            if (EthereumNetwork.NetworksByChainID.ContainsKey(chainID))
            {
                var network = EthereumNetwork.NetworksByChainID[chainID];
                return network;
            }
        }
        */
        return Web3Manager.Instance.UseTestnet ? EthereumNetwork.Mumbai : EthereumNetwork.PolygonMainNet;
    }

    private Sprite CreateSprite(Texture2D texture, int? w = null, int? h = null)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, w ?? texture.width, h ?? texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    private List<Sprite> CreateAnimationSprites(Texture2D texture)
    {
        var animations = new List<Sprite>();
        const int y = 2;
        const int framesCount = 4;

        for (int x = 0; x < framesCount; x++)
        {
            animations.Add(Sprite.Create(
                texture,
                new Rect(x * tileSize, y * tileSize, tileSize, tileSize),
                new Vector2(0.5f, 0.5f)
            ));
        }

        return animations;
    }

    private async UniTask FetchNFTTextures(EthereumNetwork network)
    {
        Logger.Log($"Using Network: {network}");

        try
        {
            helpText.gameObject.SetActive(false);
            loadingPanel.gameObject.SetActive(true);
        }
        catch
        {
        }

        // Contracts to query
        var contracts = new List<Nethereum.Contracts.Contract>();

        // Tokens fetched from each contract
        var tokens = new List<int[]>();

        int totalTokensCount = 0;

        try
        {
            var providerUri = network.ProviderURI;

            var web3 = w3m.CreateWeb3(wc, providerUri);

            var contractAddresses = ContractsAddressesByNetwork[network.ChainID];
            var contractABI = ContractsABIByNetwork[network.ChainID];

            // Query each contract
            foreach (var contractAddr in contractAddresses)
            {
                var contract = w3m.GetContract(web3, contractAddr, contractABI);
                contracts.Add(contract);

                var contractTokens = await w3m.GetERC721EnumerableOwnerTokens(
                    wc.Accounts[0],
                    contract
                );

                Logger.Log($"Tokens of contract {contractAddr}: {contractTokens.Count}");

                tokens.Add(contractTokens.ToArray());
                totalTokensCount += contractTokens.Count;
            }
        }
        catch
        {
            SetWalletError(WalletError.ContractError);
            return;
        }

        if (totalTokensCount == 0)
        {
            SetWalletError(WalletError.NoTokens);
            return;
        }

        for (int i = 0; i < tokens.Count; ++i)
        {
            var contract = contracts[i];
            var contractTokens = tokens[i];

            foreach (var tokenID in contractTokens)
            {
                string atlasLink;
                string avatarLink;

                try
                {
                    var meta = await w3m.FetchTokenMetadata(tokenID, contract);

                    atlasLink = (string)meta["atlas"];
                    avatarLink = (string)meta["image"];
                }
                catch
                {
                    SetWalletError(WalletError.MetadataFetching);
                    return;
                }

                Texture2D atlasTexture;
                Texture2D avatarTexture;

                try
                {
                    atlasTexture = await w3m.FetchTextureFromIpfs(atlasLink);
                    avatarTexture = await w3m.FetchTextureFromIpfs(avatarLink);
                }
                catch
                {
                    SetWalletError(WalletError.NFTFetching);
                    return;
                }

                try
                {
                    atlasTexture.filterMode = FilterMode.Point;

                    var animation = CreateAnimationSprites(atlasTexture);

                    atlasSprites.Add(animation);
                    avatarSprites.Add(CreateSprite(avatarTexture));
                }
                catch
                {
                    SetWalletError(WalletError.CorruptedData);
                    return;
                }
            }
        }

        try
        {
            tokensCount = totalTokensCount;
            currentAnimIndex = 0;
            currentTokenIndex = 0;
            UpdateNFTSprites();
        }
        catch
        {
            SetWalletError(WalletError.CorruptedData);
            return;
        }

        try
        {
            await w3m.DisconnectWallet(wc);
            wc = null;
        }
        catch
        {
            SetWalletError(WalletError.WalletDisconnection);
            return;
        }

        //copyWcUriButton.gameObject.SetActive(false);
        openWalletButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        qrCodeImage.gameObject.SetActive(false);
        nftScrollView.gameObject.SetActive(true);

        // Reuse copy button
        copyWcUriButton.gameObject.SetActive(false);
        //copyWcUriButton.GetComponentInChildren<Text>().text = "Play Now";
        //copyWcUriButton.onClick.RemoveAllListeners();
        //copyWcUriButton.onClick.AddListener(() => StartPlaying());

        arcadeButton.gameObject.SetActive(true);
        arcadeButton.onClick.RemoveAllListeners();
        arcadeButton.onClick.AddListener(() => StartPlaying(false));

        endlessButton.gameObject.SetActive(true);
        endlessButton.onClick.RemoveAllListeners();
        endlessButton.onClick.AddListener(() => StartPlaying(true));

        leftButton.gameObject.SetActive(tokensCount > 1);
        rightButton.gameObject.SetActive(tokensCount > 1);

        loadingPanel.gameObject.SetActive(false);

        connectionDone = true;
    }

    private void SetAlpha(Image img)
    {
        img.color = new Color(
            img.color.r,
            img.color.g,
            img.color.b,
            1
        );
    }

    private void UpdateNFTSprites()
    {
        nftAtlasImages[1].sprite = atlasSprites[currentTokenIndex][currentAnimIndex];
        SetAlpha(nftAtlasImages[1]);

        if (tokensCount > 1)
        {
            if (currentTokenIndex == tokensCount - 1)
            {
                nftAtlasImages[2].sprite = atlasSprites[0][currentAnimIndex];
            }
            else
            {
                nftAtlasImages[2].sprite = atlasSprites[currentTokenIndex + 1][currentAnimIndex];
            }

            SetAlpha(nftAtlasImages[2]);
        }

        if (atlasSprites.Count > 2)
        {
            if (currentTokenIndex == 0)
            {
                nftAtlasImages[0].sprite = atlasSprites[tokensCount - 1][currentAnimIndex];
            }
            else
            {
                nftAtlasImages[0].sprite = atlasSprites[currentTokenIndex - 1][currentAnimIndex];
            }

            SetAlpha(nftAtlasImages[0]);
        }

        avatar.sprite = avatarSprites[currentTokenIndex];

        nftIndexText.text = $"{currentTokenIndex + 1}/{tokensCount}";
    }

    public void SelectNextNFT()
    {
        currentTokenIndex++;
        if (currentTokenIndex == tokensCount)
        {
            currentTokenIndex = 0;
        }

        SetCharacter();
        UpdateNFTSprites();
    }

    public void SelectPreviousNFT()
    {
        if (currentTokenIndex == 0)
        {
            currentTokenIndex = tokensCount - 1;
        }
        else
        {
            currentTokenIndex--;
        }

        SetCharacter();
        UpdateNFTSprites();
    }

    private void SetCharacter()
    {
        if (dontUseNfts)
        {
            var character = characters.characters[currentTokenIndex];
            infoPanel.SetCharacter(character);
        }
    }
    
    public void Retry()
    {
        if (walletError == WalletError.None)
        {
            return;
        }

        walletError = WalletError.None;
        errorPanel.gameObject.SetActive(false);
#if UNITY_WEBGL
        HandleWebGl();
#else
        HandleWC();
#endif
    }

    private void AnimateAtlases()
    {
        float animInterval = 1.0f / animFps; // 12 FPS
        if (animTimer > animInterval)
        {
            animTimer = 0;
            var frames = atlasSprites[currentTokenIndex].Count;
            currentAnimIndex++;
            if (currentAnimIndex >= frames)
            {
                currentAnimIndex = 0;
            }

            UpdateNFTSprites();
        }
        else
        {
            animTimer += Time.deltaTime;
        }
    }

    private async UniTask StartPlaying(bool isEndless)
    {
        loadingPanel.gameObject.SetActive(true);

        var atlasTexture = atlasSprites[currentTokenIndex][0].texture;
        var avatarTexture = avatarSprites[currentTokenIndex].texture;

        SceneParameters.Instance.SetParameter(SceneParameters.PARAM_PLAYER_ATLAS, atlasTexture);
        SceneParameters.Instance.SetParameter(SceneParameters.PARAM_PLAYER_AVATAR, avatarTexture);

        if (dontUseNfts)
        {
            SceneParameters.Instance.SetParameter(SceneParameters.PARAM_PLAYER_CHR_IDX, currentTokenIndex);
        }

        var sceneName = isEndless ? "EndlessGameScene" : "GameScene";
        await SceneManager.LoadSceneAsync(sceneName);
    }

    private void Update()
    {
        if (wc != null)
        {
            w3m.UpdateWalletWsQueue(wc);
        }

        if (connectionDone)
        {
            AnimateAtlases();
        }

        if (loadingPanel.isActiveAndEnabled)
        {
            AnimateLoading();
        }

        if (errorPanel.isActiveAndEnabled)
        {
            AnimateErrorPanel();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        Logger.Log("App pause: " + pause);
        if (wc != null)
        {
            w3m.OnApplicationPause(wc, pause);
        }
    }

    public void UnlockCEO()
    {
        if (tokensCount < localAtlases.Length)
        {
            Logger.Log("CEO UNLOCKED HOLA");

            tokensCount++;

            UpdateNFTSprites();
        }
    }
}
