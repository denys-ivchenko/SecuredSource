using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Telesyk.Cryptography;

namespace Tests
{
	class Program
	{
		private static CancellationTokenSource _cancel;
		private static bool _stop = false;
		private static int _iterations = 0;

		static void Main(string[] args)
		{
			testStringGen();
			
			//writeSizes();
		}

		private static void testStringGen()
		{
			while (true)
			{
				var sentense = CryptographyUtils.GenerateSentence(2048);

				Console.WriteLine($"{sentense}");
				Console.WriteLine();
				Console.WriteLine($"Symbols: {sentense.Length}");
				Console.WriteLine($"Letters: {sentense.Split(' ').Length}");
				Console.WriteLine();

				var password = new Password(SymmetricAlgorithmName.RC2, sentense);

				Console.WriteLine($"Key hash: {password.Hash}");
				Console.WriteLine($"Key size: {password.KeySize}");
				
				string bytes = null;

				foreach (var byt in password.Key)
					bytes += (bytes != null ? ", " : "") + byt.ToString();

				Console.WriteLine($"Key bytes: {bytes}");
				Console.WriteLine();

				Console.WriteLine($"Vector: {password.Vector}");
				Console.WriteLine($"Vector size: {password.BlockSize}");

				bytes = null;

				foreach (var byt in password.IV)
					bytes += (bytes != null ? ", " : "") + byt.ToString();

				Console.WriteLine($"Vector bytes: {bytes}");

				Console.ReadLine();
				Console.Clear();
			}
		}

		private static void testKeys2()
		{
			//var pk1 = new Password(SymmetricAlgorithmName.Aes, 32, "ВсемПриветКакВсегда");
			//var pk2 = new Password(SymmetricAlgorithmName.Aes, 32, "ВсемПриветКакВсегда", "ИОстакльнымТоже");
			//var pk3 = new Password(SymmetricAlgorithmName.Aes, 32);
			//var pk7 = new Password(SymmetricAlgorithmName.Rijndael, 32, "Без вины виноватые");
			//var pk9 = new Password(SymmetricAlgorithmName.Rijndael, 32, "Без вины виноватые", "Как всегда");
			//var pk4 = new Password(SymmetricAlgorithmName.Rijndael, 32);
			//var pk8 = new Password(SymmetricAlgorithmName.TripleDES, 32, "Без вины виноватые");
			//var pk10 = new Password(SymmetricAlgorithmName.TripleDES, 32, "Без вины виноватые", "Как всегда");
			//var pk5 = new Password(SymmetricAlgorithmName.TripleDES, 32); 
			//var pk6 = new Password(SymmetricAlgorithmName.Aes, 32, "Без вины виноватые");
			//var pk11 = new Password(SymmetricAlgorithmName.RC2, 1, "Без вины виноватые");

			//Console.ReadLine();
		}

		private static void testKeys()
		{
			//Password s = new Password(SymmetricAlgorithmName.Aes, 32, "Glory to Ukraine! Glory to Heros!");
			//Console.WriteLine(s.Hash);
			//Console.WriteLine(s.Vector);
			//Console.WriteLine($"{s.Core.LegalKeySizes[0].MinSize}-{s.Core.LegalKeySizes[0].MaxSize}-{s.Core.LegalKeySizes[0].SkipSize}");
			//Console.WriteLine($"{s.Core.LegalBlockSizes[0].MinSize}-{s.Core.LegalBlockSizes[0].MaxSize}-{s.Core.LegalBlockSizes[0].SkipSize}");
			//Console.ReadLine();
		}

		private static void readArchive()
		{
			using (var file = new FileStream(@"C:\Users\Denys\Desktop\temp-archive.zip", FileMode.Open))
			using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
			using (var target = new FileStream(@"C:\Users\Denys\Desktop\re___file.txt", FileMode.Create))
			{
				var entry = archive.GetEntry("temp.txt");

				byte[] data = new byte[entry.Length];

				target.Write(data, 0, data.Length);
			}
		}

		private static void createArchive()
		{
			using (var file = new FileStream(@"C:\Users\Denys\Desktop\temp-archive.zip", FileMode.Create))
			using (var archive = new GZipStream(file, CompressionLevel.Fastest))
			using(var source = new FileStream(@"C:\Users\Denys\Desktop\___file.txt", FileMode.Open))
			{
				byte[] data = new byte[source.Length];

				source.Read(data, 0, (int)source.Length);

				archive.Write(data, 0, data.Length);
			}
		}

		static void createBigText()
		{
			using (var file = new System.IO.StreamWriter(@"C:\Users\Denys\Desktop\file.txt"))
			{
				for (var i = 0; i < 300000; i++)
					file.WriteLine("У попа была собака он ее любил, она съела кусок мяса, он ее убил, закопал и написал:");
			}
		}

		private static void testAsPro()
		{
			testAsProAsync();
		}

		private static async void testAsProAsync()
		{
			await testAsMethodAsync();
		}


		private static async Task testAsMethodAsync()
		{

			await Task.Run(() =>
			{
				start();
			});

		}

		private static void start()
		{
			Console.WriteLine($"start(1)");

			start1();

			Console.WriteLine($"start(2)");

			while (!_stop)
			{
				_iterations++;

				Console.WriteLine($"Iteration: {_iterations} - {_stop}");

				if (_iterations == 7)
				{
					_cancel.Cancel();

					Console.WriteLine("Cancel");
				}

				Thread.Sleep(200);
			}
		}

		private static async void start1()
		{
			_cancel = new CancellationTokenSource();

			await Task.Run(() => { process(); }, _cancel.Token);
			//await process();

			Console.WriteLine($"End task");
			//_stop = true;
		}

		private static async void Test()
		{
			var r = await Task.Run<int>(() => {
				var resultRnd = (byte)1;
				var resultGen = new byte[1];
				var q = 0;

				var rnd = new Random();
				var gen = System.Security.Cryptography.RandomNumberGenerator.Create();

				while (resultRnd != resultGen[0])
				{
					resultRnd = (byte)rnd.Next(0, 256);
					gen.GetBytes(resultGen);

					q++;

					Thread.Sleep(10);

					//TextRequired.Text = $"+{q}";
				}

				return q;
			});
		}

		private static void process()
		{
			Console.WriteLine($"process");

			for (var i = 0; i < 1000; i++)
			{
				_cancel.Token.ThrowIfCancellationRequested();

				Thread.Sleep(700);

				Console.WriteLine($"Process async: {i}");
			}

			Console.WriteLine($"process END");
		}

		private static void writeSizes()
		{
			writeSizes(Aes.Create());
			writeSizes(DES.Create());
			writeSizes(TripleDES.Create());
			writeSizes(Rijndael.Create());
			writeSizes(RC2.Create());

			Console.ReadLine();
		}
		private static void writeSizes(SymmetricAlgorithm alg)
		{
			Console.WriteLine($"{alg.GetType().Name}");

			Console.WriteLine();
			Console.WriteLine($"Key sizes:");

			foreach (var size in alg.LegalKeySizes)
				Console.WriteLine($"Min: {size.MinSize}, Max: {size.MaxSize}, Skip: {size.SkipSize}");

			Console.WriteLine();
			Console.WriteLine($"Block sizes:");

			foreach (var size in alg.LegalBlockSizes)
				Console.WriteLine($"Min: {size.MinSize}, Max: {size.MaxSize}, Skip: {size.SkipSize}");

			Console.WriteLine();
			Console.WriteLine("---------------------------------");
			Console.WriteLine();
		}
	}
}
