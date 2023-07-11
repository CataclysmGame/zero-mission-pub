using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;

public class WalletConnect : WalletConnectSession
{
    static WalletConnect()
    {
        TransportFactory.Instance.RegisterDefaultTransport((eventDelegator) => new UnityTransport(eventDelegator));
    }

    public WalletConnect(
        ClientMeta clientMeta,
        string bridgeUrl = null,
        ITransport transport = null,
        ICipher cipher = null,
        int chainId = 1,
        EventDelegator eventDelegator = null) : base(clientMeta, bridgeUrl, transport, cipher, chainId, eventDelegator)
    { }
}
