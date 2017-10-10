using System;
using Microsoft.Extensions.Logging;
using NaiveChain.DataAccess;
using NaiveChain.Extensions;
using NaiveChain.Models;

namespace NaiveChain.Tests.Fixtures
{
	public class EmptyBlockchainFixture : IDisposable
	{
		public EmptyBlockchainFixture()
		{
			var @namespace = $"{Guid.NewGuid()}";

			Init(@namespace);
		}

		protected void Init(string @namespace)
		{
			var hashProvider = new ObjectHashProviderFixture().Value;
			var factory = new LoggerFactory();
			factory.AddConsole();

			var genesisBlock = new Block
			{
				Index = 0L,
				PreviousHash = new byte[] {0},
				Timestamp = 1465154705L,
				Nonce = 0L,
				Objects = new BlockObject[] { },
			};

			Value = new SqliteBlockRepository(
				@namespace,
				"blockchain",
				genesisBlock,
				hashProvider,
				factory.CreateLogger<SqliteBlockRepository>());

			genesisBlock.Index = 1;

			genesisBlock.Hash = genesisBlock.ToHashBytes(hashProvider);

			Value.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();
			
			GenesisBlock = genesisBlock;
		}

		public SqliteBlockRepository Value { get; set; }

		public Block GenesisBlock { get; private set; }

		public void Dispose()
		{
			Value.Purge();
		}
	}
}