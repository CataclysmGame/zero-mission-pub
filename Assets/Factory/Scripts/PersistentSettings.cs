using System.Text.RegularExpressions;
using Nethereum.Util;
using Sentry;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

public class PersistentSettings : MonoBehaviour
{
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string GetAddress();
#endif

    private static PersistentSettings _instance;

    public static PersistentSettings Instance
    {
        get
        {
#if UNITY_EDITOR
            return _instance ??= new PersistentSettings();
#endif
            return _instance;
        }
    }

    private string _userAddress = "";

    public bool UseNfts { get; set; } = true;

    public bool CanMint { get; set; } = false;

    public bool AutoFetch { get; set; } = false;

    public string UserAddress
    {
        set => _userAddress = value;
        get
        {
            var addressRet = _userAddress;
#if !UNITY_EDITOR && UNITY_WEBGL
            if (string.IsNullOrEmpty(addressRet))
            {
                SentrySdk.AddBreadcrumb("[UserAddress:get] User address is empty");
                addressRet = GetAddress();
                Debug.Log("getAddress called" + addressRet);
            }
            else if (!IsValidAddress(addressRet))
            {
                SentrySdk.AddBreadcrumb("[UserAddress:get] User address is invalid: " + _userAddress);
                addressRet = GetAddress();
                Debug.Log("getAddress called: " + addressRet);
            }
#endif
            if (string.IsNullOrEmpty(addressRet) || !IsValidAddress(addressRet))
            {
                SentrySdk.AddBreadcrumb("[UserAddress:get] User address is invalid before ret: " + _userAddress);
            }

            return addressRet;
        }
    }

    public bool WaitExternalStart { get; set; } = false;

    public void WebGlSetUserAddress(string address)
    {
        UserAddress = address;
    }

    // Exposed for WebGL
    public void WebGlSetUseNfts(int useNfts)
    {
        UseNfts = useNfts != 0;
    }

    public void WebGlSetCanMint(int canMint)
    {
        CanMint = canMint != 0;
    }

    public void WebGlSetAutoFetch(int autoFetch)
    {
        AutoFetch = autoFetch != 0;
    }

    public void WebGlSetWaitExternalStart(int externalStart)
    {
        WaitExternalStart = externalStart != 0;
    }

    private void Awake()
    {
        if (_instance != null)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
        _instance = this;
        Logger.Log("Settings instance set");
    }

    private static bool IsValidAddress(string address)
    {
        var r = new Regex("^(0x){1}[0-9a-fA-F]{40}$");
        // Doesn't match length, prefix and hex
        if (!r.IsMatch(address))
            return false;

        // It's all lowercase, so no checksum needed
        if (address == address.ToLower())
            return true;

        // Do checksum
        return AddressUtil.Current.IsChecksumAddress(address);
    }
}