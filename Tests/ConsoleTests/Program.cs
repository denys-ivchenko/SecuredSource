﻿using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Telesyk.Cryptography;

//using var client = new HttpClient();
//var content = await client.GetStringAsync("http://webcode.me");

//Console.WriteLine(content);

namespace Tests
{
	class Program
	{
		private static CancellationTokenSource _cancel;
		private static bool _stop = false;
		private static int _iterations = 0;

		static void Main(string[] args)
		{
			testAsPro();
		}

		private static void testAsPro()
		{
			testAsProAsync();
		}

		private static async void testAsProAsync()
		{
			testAsMethodAsync();
		}


		private static async Task testAsMethodAsync()
		{

			await Task.Run(() =>
			{
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