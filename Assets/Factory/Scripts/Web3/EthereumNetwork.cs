using System.Collections.Generic;
using System.Linq;

public class EthereumNetwork
{
    public static EthereumNetwork Ethereum = new EthereumNetwork(
        "Ethereum",
        1,
        "https://eth-mainnet.alchemyapi.io/v2/9165B5y_1pSCD_pvcHDHtzRbe-Mbp4p6",
        "ETH"
    );

    public static EthereumNetwork PolygonMainNet = new EthereumNetwork(
        "Polygon MainNet",
        137,
        "https://polygon-mainnet.g.alchemy.com/v2/_ardLoXNZT6XMltv6IGsk5pRcCcrqYUH",
        "MATIC"
    );

    public static EthereumNetwork Mumbai = new EthereumNetwork(
        "Mumbai",
        80001,
        "https://polygon-mumbai.g.alchemy.com/v2/7-JdW5r4plvD14_IqlX4WKDjQE_2rMLi",
        "MATIC"
    );

    public static EthereumNetwork[] AllNetworks = new[] { Ethereum, PolygonMainNet, Mumbai, };

    public static Dictionary<int, EthereumNetwork> NetworksByChainID = AllNetworks
        .ToDictionary((n) => n.ChainID);

    public string Name { get; set; }
    public int ChainID { get; set; }
    public string ProviderURI {get;set;}

    public string Currency { get; set; }

    EthereumNetwork(string name, int chainID, string providerURI, string currency)
    {
        Name = name;
        ChainID = chainID;
        ProviderURI = providerURI;
        Currency = currency;
    }

    public override string ToString()
    {
        return $"EthereumNetwork {{ {ChainID} - {Name} - {Currency} }}";
    }
}
