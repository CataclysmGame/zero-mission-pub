using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Unity.Rpc;
using Sentry;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if !UNITY_EDITOR && UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Factory.Scripts.UI
{
    using AnimationFrames = List<Sprite>;

    public class SelectCharacterMenu : MonoBehaviour
    {
        private readonly Dictionary<int, string> _kSkinsIdDict = new()
        {
            { 0, "meowton-zm" },
            { 1, "hammer-zm" },
            { 2, "vyper-zm" },
            { 3, "bistury-zm" },
            { 4, "wasabi-zm" },
            { 5, "lassie-zm" }
        };

#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern bool CanMint();

        [DllImport("__Internal")]
        private static extern void Mint(int skinId);

        private void ShopButtonMint()
        {
            var currentSkinId = CurrentSkinId();
            var reversedSkinDict = _kSkinsIdDict.ToDictionary((e) => e.Value, (e) => e.Key);

            if (reversedSkinDict.TryGetValue(currentSkinId, out var skinTokenId))
            {
                Mint(skinTokenId);
            }
        }
#endif

        private enum CurrentFocus
        {
            Characters = 0,
            Skin = 1,
            Buttons = 2,
        }

        public List<Character> characters;

        public Character ceo;

        public Skin[] workerSkins;

        public StatsPanel statsPanel;

        public Image[] avatarImages;

        public GameObject[] skinSlots;

        public Image animationImage;

        public Image glowImage;

        public Image splashArtImage;

        public TMP_Text characterNameText;

        public TMP_Text characterDescriptionText;

        public List<Sprite> glowSprites;
        public List<Sprite> ceoGlowSprites;

        public Button endlessButton;
        public Button arcadeButton;

        public GameObject shopNowButton;

        public GameObject loadingPanel;

        public Image curAvatarBorder;

        public Color curAvatarBorderSelectedColor;
        public Color curAvatarBorderUnselectedColor;

        public GameObject shopObject;

        public AudioClip changeSound;

        public AudioClip backSound;

        public AudioClip selectSound;

        public AudioClip clickSound;

        public float animationInterval = 0.32f;

        private int _currentCharacterIndex;

        private int _currentSkinIndex;

        private int _currentAnimationFrame;

        private CurrentFocus _currentFocus = CurrentFocus.Characters;

        private List<List<AnimationFrames>> _animations;

        private GameControls _gameControls;
        private UIControls _uiControls;

        private bool _selected;

        private bool _ceoUnlocked;

        private bool _catacrowdOwned;

        private AudioSource _audioSource;

        // Value indicates if the skin is first-hand
        private Dictionary<string, bool> _unlockedSkins;

        private List<int> _selectedSkins;

        private Character CurrentCharacter => characters[_currentCharacterIndex];

        private Skin CurrentSkin => CurrentCharacter.skins[_currentSkinIndex];

        private void SwitchCharacter(int index, bool firstTime = false)
        {
            if (index < 0)
            {
                index = characters.Count - 1;
            }

            _currentCharacterIndex = index % characters.Count;
            var curChar = characters[_currentCharacterIndex];

            _currentSkinIndex = _selectedSkins[_currentCharacterIndex] % curChar.skins.Count;
            var curSkin = curChar.skins[_currentSkinIndex];

            var skinUnlocked = IsCurrentSkinUnlocked();
            shopObject.SetActive(!skinUnlocked);

            arcadeButton.interactable = skinUnlocked;
            endlessButton.interactable = skinUnlocked;

            for (var i = 0; i < avatarImages.Length; ++i)
            {
                var texture = characters[(i + _currentCharacterIndex) % characters.Count].avatar;
                avatarImages[i].sprite = CreateSprite(texture);
            }

            _currentAnimationFrame = 0; // Reset animation

            AnimateAtlas();

            var splashArt = curSkin.splashArt;
            splashArtImage.sprite = CreateSprite(splashArt);

            characterNameText.text =
                string.IsNullOrEmpty(curSkin.displayName) ? curChar.playerName : curSkin.displayName;

            // Use skin description if available
            var charDesc = string.IsNullOrEmpty(curSkin.description)
                ? curChar.characterDescription
                : curSkin.description;
            characterDescriptionText.text = charDesc;

            statsPanel.SetCharacter(curChar);

            // Set skins
            for (var i = 0; i < skinSlots.Length; ++i)
            {
                if (i >= curChar.skins.Count)
                {
                    skinSlots[i].SetActive(false);
                    continue;
                }

                skinSlots[i].SetActive(true);
                var skin = curChar.skins[i];
                var skinImage = skinSlots[i].transform.Find("Splash").GetComponent<Image>();
                var skinBorder = skinSlots[i].transform.Find("Border").GetComponent<Image>();

                Sprite avatarSprite;
                if (skin.avatar != null)
                {
                    avatarSprite = CreateSprite(skin.avatar);
                }
                else
                {
                    const int blockSize = 1024;
                    var splash = skin.splashArt;
                    var splashBlockX = splash.width / 2 - blockSize / 2;
                    var splashBlockY = splash.height / 2 - blockSize / 2;

                    avatarSprite = Sprite.Create(
                        splash,
                        new Rect(splashBlockX, splashBlockY, blockSize, blockSize),
                        new Vector2(0.5f, 0.5f)
                    );
                }

                skinImage.sprite = avatarSprite;
                skinBorder.gameObject.SetActive(i == _currentSkinIndex);
                skinBorder.color = _currentFocus != CurrentFocus.Characters
                    ? curAvatarBorderSelectedColor
                    : curAvatarBorderUnselectedColor;
            }

            if (!firstTime)
            {
                _audioSource.PlayOneShot(changeSound);
            }
        }

        public void SetSkin(int skinIndex)
        {
            if (skinIndex < 0) skinIndex = characters[_currentCharacterIndex].skins.Count - 1;
            if (skinIndex != _currentSkinIndex)
            {
                _selectedSkins[_currentCharacterIndex] = skinIndex % characters[_currentCharacterIndex].skins.Count;
                SwitchCharacter(_currentCharacterIndex);
            }
        }

        public void SetSkinClick(int skinIndex)
        {
            ChangeFocus(CurrentFocus.Skin);
            SetSkin(skinIndex);
        }

        private string CurrentSkinId()
        {
            var curChar = characters[_currentCharacterIndex];
            var curSkin = curChar.skins[_currentSkinIndex];

            return $"{curChar.id}-{curSkin.id}";
        }

        private bool IsCurrentSkinUnlocked()
        {
            var curChar = characters[_currentCharacterIndex];
            var curSkin = curChar.skins[_currentSkinIndex];

            if (curSkin.id == "base")
            {
                return true;
            }

            if (curSkin.id.StartsWith("worker-"))
            {
                return true;
            }

            return _unlockedSkins.ContainsKey(CurrentSkinId());
        }

        private void LoadSkinSelections()
        {
        }

        private void SaveSkinSelections()
        {
        }

        private void ShowLoading(bool loading)
        {
            loadingPanel.gameObject.SetActive(loading);
            loadingPanel.GetComponent<LoadingPanel>().SetAnimTexture(CurrentSkin.atlas);
        }

        /// <summary>
        /// Changes focus of the UI
        /// </summary>
        private void ChangeFocus(CurrentFocus focus)
        {
            if (_currentFocus == focus)
            {
                return;
            }

            var previousFocus = _currentFocus;

            if (focus == CurrentFocus.Buttons)
            {
                arcadeButton.Select();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            var skinBorder = skinSlots[_selectedSkins[_currentCharacterIndex]]
                .transform.Find("Border").GetComponent<Image>();

            skinBorder.color = focus != CurrentFocus.Characters
                ? curAvatarBorderSelectedColor
                : curAvatarBorderUnselectedColor;

            _audioSource.PlayOneShot(previousFocus == CurrentFocus.Buttons ? backSound : selectSound);

            _currentFocus = focus;
        }

        private void FocusUp()
        {
            var enumValues = (CurrentFocus[])Enum.GetValues(typeof(CurrentFocus));
            var newFocusIndex = Array.IndexOf(enumValues, _currentFocus) - 1;

            if (newFocusIndex < 0)
            {
                return;
            }

            var newFocus = enumValues[newFocusIndex % enumValues.Length];

            if (newFocus == CurrentFocus.Buttons && !IsCurrentSkinUnlocked())
            {
                return;
            }

            ChangeFocus(newFocus);
        }

        private void FocusDown()
        {
            var enumValues = (CurrentFocus[])Enum.GetValues(typeof(CurrentFocus));
            var newFocusIndex = Array.IndexOf(enumValues, _currentFocus) + 1;

            if (newFocusIndex >= enumValues.Length)
            {
                return;
            }

            var newFocus = enumValues[newFocusIndex % enumValues.Length];

            if (newFocus == CurrentFocus.Buttons && !IsCurrentSkinUnlocked())
            {
                return;
            }

            ChangeFocus(newFocus);
        }

        private Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        private List<Sprite> CreateAnimationSprites(Texture2D texture, int tileSize = 32)
        {
            var animations = new List<Sprite>();
            const int y = 2;
            const int framesCount = 4;

            for (var x = 0; x < framesCount; x++)
            {
                animations.Add(Sprite.Create(
                    texture,
                    new Rect(x * tileSize, y * tileSize, tileSize, tileSize),
                    new Vector2(0.5f, 0.5f)
                ));
            }

            return animations;
        }

        private void Awake()
        {
            _gameControls = new GameControls();
            _uiControls = new UIControls();

#if !UNITY_EDITOR && UNITY_WEBGL
            shopNowButton.GetComponent<Button>().onClick.AddListener(ShopButtonMint);

            try
            {
                if (CanMint())
                {
                    shopNowButton.SetActive(true);
                }
            }
            catch
            {
            }
#endif

            _unlockedSkins = new Dictionary<string, bool>();

#if UNITY_EDITOR
            _uiControls.UI.MiddleClick.performed += (_) =>
            {
                if (!_catacrowdOwned)
                {
                    foreach (var skinId in _kSkinsIdDict.Values)
                    {
                        _unlockedSkins.Add(skinId, true);
                    }

                    _catacrowdOwned = true;
                }
            };
#endif

            _audioSource = GetComponent<AudioSource>();

            _selectedSkins = new List<int>(characters.Count);
            for (var i = 0; i < characters.Count; i++)
            {
                _selectedSkins.Add(0);
            }

            LoadSkinSelections();

            _animations = new List<List<AnimationFrames>>(characters.Count);
            foreach (var character in characters)
            {
                var skinsAnim = new List<AnimationFrames>();
                foreach (var skin in character.skins)
                {
                    var atlas = skin.atlas;
                    skinsAnim.Add(CreateAnimationSprites(atlas));
                }

                _animations.Add(skinsAnim);
            }

            SwitchCharacter(0, true);

            InvokeRepeating(nameof(AnimateAtlas), animationInterval, animationInterval);
        }

        private void Start()
        {
            //if (PersistentSettings.Instance.UseNfts)
            //{
            FetchNFTs();
            //}
        }

        private void OnEnable()
        {
            _gameControls.Enable();
            _uiControls.Enable();

            _gameControls.Gameplay.Move.performed += (ctx) =>
            {
                var moveVec = _gameControls.Gameplay.Move.ReadValue<Vector2>();

                if (!_selected)
                {
                    if (moveVec.x > 0) Right();
                    else if (moveVec.x < 0) Left();
                }

                if (moveVec.y < 0)
                {
                    FocusDown();
                }
                else if (moveVec.y > 0)
                {
                    FocusUp();
                }
            };
        }

        private void OnDisable()
        {
            _gameControls.Disable();
            _uiControls.Disable();
        }

        private void OnDestroy()
        {
            SaveSkinSelections();

            if (PersistentMusicPlayer.Instance != null)
            {
                // Stop music
                PersistentMusicPlayer.Instance.ClearQueue();
                PersistentMusicPlayer.Instance.Stop();
            }
        }

        private void Update()
        {
            if (!_selected && _uiControls.UI.Submit.WasPressedThisFrame())
            {
                FocusDown();
            }
        }

        private void AnimateAtlas()
        {
            var charAnimations = _animations[_currentCharacterIndex][_currentSkinIndex];
            var animSprite = charAnimations[_currentAnimationFrame];

            animationImage.sprite = animSprite;
            var isCeo = characters[_currentCharacterIndex].playerName == "CEO";
            glowImage.sprite = isCeo ? ceoGlowSprites[_currentAnimationFrame] : glowSprites[_currentAnimationFrame];

            _currentAnimationFrame = (_currentAnimationFrame + 1) % charAnimations.Count;
        }

        public void Left()
        {
            if (_currentFocus == CurrentFocus.Characters)
            {
                SwitchCharacter(_currentCharacterIndex - 1);
            }
            else if (_currentFocus == CurrentFocus.Skin)
            {
                SetSkin(_currentSkinIndex - 1);
            }
        }

        public void Right(int amount = 1)
        {
            if (_currentFocus == CurrentFocus.Characters)
            {
                SwitchCharacter(_currentCharacterIndex + amount);
            }
            else if (_currentFocus == CurrentFocus.Skin)
            {
                SetSkin(_currentSkinIndex + amount);
            }
        }

        public void CharClick(int offset)
        {
            ChangeFocus(CurrentFocus.Characters);
            Right(offset);
        }

        public async void StartPlaying(bool isEndless)
        {
            _audioSource.PlayOneShot(clickSound);

            var atlasTexture = characters[_currentCharacterIndex].skins[_currentSkinIndex].atlas;
            var avatarTexture = characters[_currentCharacterIndex].avatar;
            if (CurrentSkin.id != "base")
            {
                avatarTexture = CurrentSkin.splashArt;
            }

            var sceneParameters = SceneParameters.Instance;

            sceneParameters.SetParameter(SceneParameters.PARAM_PLAYER_ATLAS, atlasTexture);
            sceneParameters.SetParameter(SceneParameters.PARAM_PLAYER_AVATAR, avatarTexture);
            sceneParameters.SetParameter(SceneParameters.PARAM_PLAYER_CHR_IDX, _currentCharacterIndex);

            var currentSkinId = CurrentSkinId();
            sceneParameters.SetParameter(SceneParameters.PARAM_PLAYER_SKIN, CurrentSkin);
            _unlockedSkins.TryGetValue(currentSkinId, out var firstHand);
            sceneParameters.SetParameter(SceneParameters.PARAM_PLAYER_SKIN_FIRST_HAND, firstHand);

            ShowLoading(true);

            if (isEndless && PersistentSettings.Instance.WaitExternalStart)
            {
#if UNITY_WEBGL
                Application.ExternalCall("window._unityEndlessStarted");
#endif
            }
            else
            {
                LoadScene(isEndless);
            }
        }

        public void StartEndless()
        {
            LoadScene(true);
        }

        private void LoadScene(bool isEndless)
        {
            var sceneName = isEndless ? "EndlessGameScene" : "GameScene";
            SceneManager.LoadSceneAsync(sceneName);
        }

        public void UnlockCEO()
        {
            if (_ceoUnlocked)
            {
                return;
            }

            ceo.skins[0].splashArt = GenerateCeoSplashArt();

            if (workerSkins is { Length: > 0 } && _catacrowdOwned)
            {
                foreach (var skin in workerSkins)
                {
                    skin.splashArt = ceo.skins[0].splashArt;
                    ceo.skins.Add(skin);
                }
            }

            ceo.avatar = GenerateCeoAvatar();
            characters.Add(ceo);

            var ceoAnimations = new List<AnimationFrames>();

            foreach (var skin in ceo.skins)
            {
                ceoAnimations.Add(CreateAnimationSprites(skin.atlas));
            }

            _animations.Add(ceoAnimations);

            _selectedSkins.Add(0);
            SwitchCharacter(characters.Count - 1);
            _ceoUnlocked = true;
        }

        // Generates a new splash art which takes random rects from all splash arts of other characters
        private Texture2D GenerateCeoSplashArt()
        {
            return RandomGlitchedImage(characters.Select((c) => Util.PickRandom(c.skins).splashArt).ToArray(), 256);
        }

        private Texture2D GenerateCeoAvatar()
        {
            return RandomGlitchedImage(characters.Select((c) => c.avatar).ToArray(), 256);
        }

        private Texture2D RandomGlitchedImage(Texture2D[] sources, int blockSize)
        {
            bool isBlockOk(Color[] block)
            {
                foreach (var color in block)
                {
                    if (color.a > 0.1f)
                    {
                        return true;
                    }
                }

                return false;
            }

            var referenceSplash = sources[0];

            var blockWidth = referenceSplash.width / blockSize;
            var blockHeight = referenceSplash.height / blockSize;
            var blocksCount = blockWidth * blockHeight;

            var blocks = new List<(int, int, int)>(blocksCount);
            while (blocks.Count < blocksCount)
            {
                var randomSrcIndex = Mathf.FloorToInt(Random.value * sources.Length);
                var srcTex = sources[randomSrcIndex];

                var randomSrcX = Mathf.FloorToInt(Random.value * blockWidth);
                var randomSrcY = Mathf.FloorToInt(Random.value * blockHeight);

                var block = srcTex.GetPixels(
                    randomSrcX * blockSize,
                    randomSrcY * blockSize,
                    blockSize,
                    blockSize
                );

                if (isBlockOk(block))
                {
                    blocks.Add((randomSrcIndex, randomSrcX * blockSize, randomSrcY * blockSize));
                }
            }

            var glitchedImage = new Texture2D(referenceSplash.width, referenceSplash.height);

            // Clear the texture
            for (var x = 0; x < glitchedImage.width; ++x)
            {
                for (var y = 0; y < glitchedImage.height; ++y)
                {
                    glitchedImage.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }

            for (var i = 0; i < blocksCount; ++i)
            {
                var srcTex = sources[blocks[i].Item1];
                var block = srcTex.GetPixels(blocks[i].Item2, blocks[i].Item3, blockSize, blockSize);
                glitchedImage.SetPixels(i % blockWidth * blockSize, i / blockWidth * blockSize, blockSize, blockSize,
                    block);
            }

            glitchedImage.Apply();

            return glitchedImage;
        }

#if UNITY_WEBGL
        public void RefreshSkins()
        {
            FetchNFTs();
        }
#endif

        private async UniTaskVoid FetchNFTs()
        {
            const string kContractAddress = "0xA864F3E91d0aa2F213E2307e2e74A77D37f3BEDa";

            const string kRpcUrl = Signore.DegliRpc;

            Logger.Log("NFT: Contract address: " + kContractAddress);
            Logger.Log("NFT: RPC URL:" + kRpcUrl);

            // var address = "0xB6e9B7F99415C722CF8dE1aF4CCd95c2DF421451";
            var address = PersistentSettings.Instance.UserAddress;

            Logger.Log("NFT: User address: " + address);

            if (string.IsNullOrEmpty(address))
            {
                Logger.LogWarn("NFT: Invalid user address, returning");
                return;
            }

            _unlockedSkins.Clear();

            ShowLoading(true);

            try
            {
                var queryReq = new QueryUnityRequest<BalanceOfFunction, BalanceOfOutputDTO>(kRpcUrl, address);

                var events = await GetAllUserTransferEvents(kRpcUrl, kContractAddress, address);

                foreach (var skinIdPair in _kSkinsIdDict)
                {
                    var tokenId = skinIdPair.Key;

                    var balanceFn = new BalanceOfFunction
                    {
                        FromAddress = address,
                        Account = address,
                        Id = tokenId
                    };

                    await queryReq.Query(balanceFn, kContractAddress);

                    var dtoResult = queryReq.Result;

                    var balance = dtoResult.ReturnValue1;

                    Logger.Log($"NFT: balanceOf({address}, {skinIdPair.Key}) = {balance}");

                    if (balance > 0)
                    {
                        // Get last "balance" events
                        // Check if one event is coming from 0
                        const string kZeroAddress = "0x0000000000000000000000000000000000000000";

                        var isFirstHand = false;

                        try
                        {
                            isFirstHand = events.Where((e) => e.Event.Id == tokenId)
                                .TakeLast((int)balance).Any((e) => e.Event.From == kZeroAddress);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Failed to check if skin is first-hand", ex.ToString());
                            SentrySdk.CaptureException(ex);
                        }

                        Logger.Log("Is first hand: " + isFirstHand);

                        _unlockedSkins.Add(skinIdPair.Value, isFirstHand);
                    }
                }

                // Check catacrowd (tokenID 6)
                {
                    var balanceFn = new BalanceOfFunction
                    {
                        FromAddress = address,
                        Account = address,
                        Id = 6
                    };

                    await queryReq.Query(balanceFn, kContractAddress);

                    var dtoResult = queryReq.Result;

                    var balance = dtoResult.ReturnValue1;

                    _catacrowdOwned = balance > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("NFT: Fetch aborted due to exception: " + ex);
                SentrySdk.CaptureException(ex);
            }
            finally
            {
                ShowLoading(false);
                Logger.Log("NFT: Done");
            }
        }

        private async UniTask<List<EventLog<TransferSingleEventDTO>>> GetAllUserTransferEvents(string rpcUrl,
            string contractAddress,
            string userAddress)
        {
            var logsReq = new EthGetLogsUnityRequest(rpcUrl);
            var singleTxEvent = new TransferSingleEventDTO().GetEventABI();
            var batchTxEvent = new TransferBatchEventDTO().GetEventABI();

            var filterSingle =
                FilterInputBuilder.GetDefaultFilterInput(contractAddress);
            filterSingle.Topics = singleTxEvent.GetTopicBuilder()
                .GetTopics<string, string, string>(null, null, userAddress);

            await logsReq.SendRequest(filterSingle);

            var singleEvents = logsReq.Result.SortLogs().DecodeAllEvents<TransferSingleEventDTO>();

            var filterBatch = FilterInputBuilder.GetDefaultFilterInput(contractAddress);
            filterBatch.Topics = batchTxEvent.GetTopicBuilder()
                .GetTopics<string, string, string>(null, null, userAddress);

            await logsReq.SendRequest(filterBatch);

            var batchEvents = logsReq.Result.SortLogs().DecodeAllEvents<TransferBatchEventDTO>();

            var events = new List<EventLog<TransferSingleEventDTO>>();
            events.AddRange(singleEvents);

            foreach (var batchEvent in batchEvents)
            {
                for (var i = 0; i < batchEvent.Event.Ids.Count; ++i)
                {
                    var id = batchEvent.Event.Ids[i];
                    var value = batchEvent.Event.Values[i];

                    var singleEvent = new TransferSingleEventDTO
                    {
                        From = batchEvent.Event.From,
                        To = batchEvent.Event.To,
                        Id = id,
                        Value = value
                    };

                    events.Add(new EventLog<TransferSingleEventDTO>(singleEvent, batchEvent.Log));
                }
            }

            int CompareEvent<T>(EventLog<T> x, EventLog<T> y)
            {
                if (x.Log.BlockNumber.Value != y.Log.BlockNumber.Value)
                    return x.Log.BlockNumber.Value.CompareTo(y.Log.BlockNumber.Value);
                return x.Log.TransactionIndex.Value != y.Log.TransactionIndex.Value
                    ? x.Log.TransactionIndex.Value.CompareTo(y.Log.TransactionIndex.Value)
                    : x.Log.LogIndex.Value.CompareTo(y.Log.LogIndex.Value);
            }

            // Sort events
            events.Sort(CompareEvent);

            return events;
        }
    }
}