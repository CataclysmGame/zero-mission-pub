using System;
using Nethereum.ABI;
using Nethereum.Signer;
using Nethereum.Util;

public class Signore
{
    public const string DegliRpc = "https://polygon-mainnet.infura.io/v3/93c732f2dda344979ce809e10b783db5";

    private static uint fabio = 0xCF52A153;

    private string Sign(byte[] data)
    {
        uint gianfranco = 0xF245D398;

        var kb = keccak256(BitConverter.GetBytes(((ulong)fabio << 32) | gianfranco | 12));
        var ethKey = new EthECKey(kb, true);

        var msgHash = keccak256(data);

        var signer = new EthereumMessageSigner();
        return signer.Sign(msgHash, ethKey);
    }

    public string SEndless(int score, string character, string skin)
    {
        var userAddress = PersistentSettings.Instance.UserAddress;

        var abiEncode = new ABIEncode();
        var msg = abiEncode.GetABIEncodedPacked(
            new ABIValue("address", userAddress),
            new ABIValue("string", character),
            new ABIValue("string", skin),
            new ABIValue("uint256", score)
        );

        return Sign(msg);
    }

    public string SArcade(int enemiesKilled)
    {
        return SGameOver(0, enemiesKilled);
    }

    public string SGameOver(int endlessInt, int score)
    {
        var userAddress = PersistentSettings.Instance.UserAddress;
        var abiEncode = new ABIEncode();
        var msg = abiEncode.GetABIEncodedPacked(
            new ABIValue("address", userAddress),
            new ABIValue("uint256", endlessInt),
            new ABIValue("uint256", score)
        );
        return Sign(msg);
    }

    private byte[] keccak256(byte[] data)
    {
        return new Sha3Keccack().CalculateHash(data);
    }
}