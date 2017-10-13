﻿using System.Collections.Generic;
using System.Diagnostics;
using NaiveChain.Models;
using NaiveChain.Serialization;
using NaiveCoin.Models.Exceptions;

namespace NaiveCoin.Models
{
	public class Transaction : IBlockSerialized
	{
		public string Id { get; set; }
		public byte[] Hash { get; set; }
		public TransactionType Type { get; set; }
		public TransactionData Data { get; set; }

		public Transaction() { }

		public void Check(IHashProvider hashProvider, CoinSettings coinSettings)
		{
			// Check if the transaction hash is correct
			if (Hash != hashProvider.ComputeHashBytes(this))
				throw new TransactionAssertionException($"Invalid transaction hash '{Hash}'");

			Data.Check(coinSettings, hashProvider);
		}

		public void Serialize(BlockSerializeContext context)
		{
			context.bw.Write(Id);
			context.bw.WriteBuffer(Hash);
			context.bw.Write((byte)Type);

			if (context.bw.WriteBoolean(Data != null))
			{
				Debug.Assert(Data != null);
				SerializeTransactionItems(context, Data.Inputs);
				SerializeTransactionItems(context, Data.Outputs);
			}
		}

		private static void SerializeTransactionItems(BlockSerializeContext context, IList<TransactionItem> items)
		{
			if (context.bw.WriteBoolean(items != null))
			{
				Debug.Assert(items != null);
				context.bw.Write(items.Count);
				foreach (var input in items)
				{
					context.bw.Write(input.Index);
					context.bw.Write(input.TransactionId);
					context.bw.Write((byte)input.Type);
					context.bw.WriteBuffer(input.Address);
					context.bw.Write(input.Amount);
					context.bw.WriteBuffer(input.Signature);
				}
			}
		}

		public Transaction(BlockDeserializeContext context)
		{
			Id = context.br.ReadString();
			Hash = context.br.ReadBuffer();
			Type = (TransactionType)context.br.ReadByte();

			if (context.br.ReadBoolean())
			{
				Data = new TransactionData
				{
					Inputs = DeserializeTransactionItems(context),
					Outputs = DeserializeTransactionItems(context)
				};
			}
		}

		private static IList<TransactionItem> DeserializeTransactionItems(BlockDeserializeContext context)
		{
			var list = new List<TransactionItem>();

			if (context.br.ReadBoolean())
			{
				var count = context.br.ReadInt32();
				for (var i = 0; i < count; i++)
				{
					var item = new TransactionItem
					{
						Index = context.br.ReadInt64(),
						TransactionId = context.br.ReadString(),
						Type = (TransactionDataType)context.br.ReadByte(),
						Address = context.br.ReadBuffer(),
						Amount = context.br.ReadInt64(),
						Signature = context.br.ReadBuffer()
					};

					list.Add(item);
				}
			}

			return list;
		}
	}
}