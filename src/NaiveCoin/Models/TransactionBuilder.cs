﻿using NaiveCoin.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NaiveCoin.Models
{
    public class TransactionBuilder
    {
        private string _secretKey;
        private TransactionType _type;
        private long _feeAmount;
        private byte[] _changeAddress;
        private byte[] _outputAddress;
        private long? _totalAmount;
        private IEnumerable<TransactionOutput> _utxo;

        public TransactionBuilder()
        {
            _type = TransactionType.Regular;
        }

        public TransactionBuilder From(IEnumerable<TransactionOutput> utxo)
        {
            _utxo = utxo;
            return this;
        }

        public TransactionBuilder To(byte[] address, long amount)
        {
            _outputAddress = address;
            _totalAmount = amount;
            return this;
        }

        public TransactionBuilder Change(byte[] changeAddress)
        {
            _changeAddress = changeAddress;
            return this;
        }

        public TransactionBuilder Fee(long amount)
        {
            _feeAmount = amount;
            return this;
        }

        public TransactionBuilder Sign(string secretKey)
        {
            _secretKey = secretKey;
            return this;
        }

        public TransactionBuilder Type(TransactionType type)
        {
            _type = type;
            return this;
        }

        public Transaction Build()
        {
            // Check required information
            if (_utxo == null)
                throw new ArgumentException($"It's necessary to provide a list of unspent output transactions.");
            if (_outputAddress == null)
                throw new ArgumentException($"It's necessary to provide the destination address.");
            if (_totalAmount == null)
                throw new ArgumentException($"It's necessary to provide the transaction value.");

            // Calculates the change amount
            var changeAmount = _utxo.Sum(x => x.Amount) - _totalAmount - _feeAmount;

            // For each transaction input, calculates the hash of the input and signs the data
            var inputs = _utxo.Select(utxo =>
            {
                utxo.Signature = CryptoEdDsaUtil.SignHash(CryptoEdDsaUtil.GenerateKeyPairFromSecret(_secretKey), CryptoUtil.Hash(new
                {
                    Transaction = utxo.TransactionId,
                    utxo.Index,
                    utxo.Address
                }));
                return utxo;
            }).Select(x => new TransactionInput
            {
                Address = x.Address,
                Amount = x.Amount,
                Index = x.Index,
                Signature = x.Signature
            }).ToArray();

            // Add target receiver
            var outputs = new List<TransactionOutput>
            {
                new TransactionOutput
                {
                    Amount = _totalAmount.Value,
                    Address = _outputAddress
                }
            };

            // Add change amount
            if (changeAmount > 0)
            {
                outputs.Add(new TransactionOutput {
                    Amount = changeAmount.GetValueOrDefault(),
                    Address = _changeAddress
                });
            }

            // The remaining value is the fee to be collected by the block's creator
            return new Transaction
            {
                Id = CryptoUtil.RandomString(),
                Hash = null,
                Type = _type,
                Data = new TransactionData
                {
                    Inputs = inputs,
                    Outputs = outputs.ToArray()
                }
            };
        }
    }
}