﻿using Crypto.Shim;

namespace ChainLib.Wallets
{
    public interface IWalletAddressStorageFormat
    {
        KeyPair Import(Wallet wallet, string input);
        string Export(Wallet wallet, byte[] address);
    }
}