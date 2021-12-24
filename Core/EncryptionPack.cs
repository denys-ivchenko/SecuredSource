using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	public sealed class EncryptionPack
	{
		#region Private declarations

		private CancellationTokenSource _cancel;
		private int _totalByteComplitted;
		private int _lastPercentage;

		#endregion

		#region Constructors

		private EncryptionPack(string filePath, string password, EncryptedPackMode mode)
		{
			FilePath = filePath;
			Password = password;
			Mode = mode;

			init();
		}

		public EncryptionPack(string filePath, string password, PackData filePackData)
			: this(filePath, password, EncryptedPackMode.Serialize)
		{
			FilePack = filePackData;
		}

		public EncryptionPack(string filePath, string password, string outputPath)
			: this(filePath, password, EncryptedPackMode.Deserialize)
		{
			OutputPath = outputPath;
		}

		#endregion

		#region Public properties

		public EncryptedPackMode Mode { get; private set; }

		public string FilePath { get; private set; }

		public string Password { get; private set; }

		public string OutputPath { get; private set; }

		public byte[] PasswordBytes { get; private set; }

		public PackData FilePack { get; private set; }

		public int TotalBytes { get; private set; }

		public int BytesComplitted
		{
			get => _totalByteComplitted + CurrentByteComplitted;
			private set
			{
				_totalByteComplitted = value;
				CurrentByteComplitted = 0;
			}
		}

		public int CurrentByteComplitted { get; private set; }

		public bool IsCanceled { get; private set; }

		#endregion

		#region Public methods

		public void Process() => process();

		public void Cancel() => cancel();

		#endregion

		#region Public events

		public event ProgressEventHandler Progress;

		public event EventHandler Completted;

		public event EventHandler Canceled;

		public event EventHandler Finished;

		#endregion

		#region Interface implementations

		//public void Dispose()
		//{

		//}

		#endregion

		#region Overrided methods

		#endregion

		#region Private methods

		private void init()
		{
			PasswordBytes = Encoding.UTF8.GetBytes(Password);

			if (Mode == EncryptedPackMode.Deserialize)
				using (FileStream stream = new FileStream(FilePath, FileMode.Open))
					TotalBytes = (int)stream.Length;

			//Crypton.Instance.Progress += Crypton_Progress;
		}

		private async void process()
		{
			_cancel = new CancellationTokenSource();
			_lastPercentage = 0;

			await processAsync();
		}

		private async Task processAsync()
		{
			Debug.WriteLine("process()");
			
			Action action = () => serialize();
			
			if (Mode == EncryptedPackMode.Deserialize)
				action = () => deserialize();

			await Task.Run(action, _cancel.Token);

			if (Completted != null)
				Completted(this, EventArgs.Empty);

			if (Finished != null)
				Finished(this, EventArgs.Empty);

			Debug.WriteLine("process.");
		}

		private void cancel()
		{
			_cancel.Cancel();

			if (Canceled != null)
				Canceled(this, EventArgs.Empty);

			if (Finished != null)
				Finished(this, EventArgs.Empty);
		}

		private void ensureCanceling()
		{
			//Debug.WriteLine("ensureCanceling()");

			_cancel.Token.ThrowIfCancellationRequested();

			//Debug.WriteLine("ensureCanceling.");
		}

		private async Task serialize()
		{
			Debug.WriteLine("serialize()");

			BinaryFormatter formatter = new BinaryFormatter();

			var packBytes = PackData.Serialize(FilePack);

			TotalBytes = 4 + packBytes.Length;

			foreach (var file in FilePack)
				TotalBytes += file.ByteCount * 5;

			using (FileStream file = new FileStream(FilePath, FileMode.Create))
			{
				BinaryWriter writer = new BinaryWriter(file);
				writer.Write(packBytes.Length);

				progress((int)file.Position, true);

				int position = 0;

				while (position < packBytes.Length)
				{
					var length = Crypton.BUFFER_SIZE;
					var over = position + Crypton.BUFFER_SIZE - packBytes.Length;

					if (over > 0)
						length -= over;

					file.Write(packBytes, position, length);

					progress(position);

					position += Crypton.BUFFER_SIZE;
				}

				progress(packBytes.Length, true);

				foreach(var fileData in FilePack)
				{
					var fileBytes = new byte[fileData.ByteCount];

					position = 0;

					using (var stream = new FileStream(fileData.FullName, FileMode.Open))
					{
						while (position < stream.Length)
						{
							var length = Crypton.BUFFER_SIZE;
							var over = position + Crypton.BUFFER_SIZE - (int)stream.Length;

							if (over > 0)
								length -= over;

							stream.Read(fileBytes, position, length);

							progress(position);

							position += Crypton.BUFFER_SIZE;
						}
					}

					progress(fileData.ByteCount, true);

					fileBytes = encryptFile(fileBytes);

					progress(fileData.ByteCount, true);

					position = 0;

					while (position < fileBytes.Length)
					{
						var length = Crypton.BUFFER_SIZE;
						var over = position + Crypton.BUFFER_SIZE - (int)fileBytes.Length;

						if (over > 0)
							length -= over;

						writer.Write(fileBytes, position, length);

						progress(position);

						position += Crypton.BUFFER_SIZE;
					}

					progress(fileData.ByteCount, true);
				}
			}

			Debug.WriteLine("serialize.");
		}

		private void deserialize()
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (FileStream stream = new FileStream(FilePath, FileMode.Open))
			{
				BinaryReader reader = new BinaryReader(stream);
				var lengthData = reader.ReadInt32();

				using (var memoryData = new MemoryStream(reader.ReadBytes(lengthData)))
				{
					FilePack = (PackData)formatter.Deserialize(memoryData);

					foreach (var fileData in FilePack)
					{
						var fileBytes = reader.ReadBytes(fileData.ByteCount);

						fileBytes = decryptFile(fileBytes);

						using (var file = new FileStream(Path.Combine(OutputPath, fileData.Name), FileMode.Create))
							file.Write(fileBytes, 0, fileData.ByteCount);
					}
				}
			}
		}

		private void progress(int byteCount) => progress(byteCount, false);

		private void progress(int byteCount, bool isTotal)
		{
			if (isTotal)
				BytesComplitted = BytesComplitted + byteCount;
			else
				CurrentByteComplitted = byteCount;

			var percentage = (int)((BytesComplitted / (decimal)TotalBytes) * 100);

			if (Progress != null && percentage > _lastPercentage)
			{
				_lastPercentage = percentage;

				Progress(this, new ProgressEventArgs(percentage, 0));
			}

			ensureCanceling();
		}

		private void cryptonProgress(int bytesComplitted)
		{
			if (Progress != null)
			{
				CurrentByteComplitted = bytesComplitted;

				Progress(this, new ProgressEventArgs((int)((BytesComplitted / TotalBytes) * 100), BytesComplitted));
			}
		}

		private byte[] encryptFile(byte[] fileBytes)
		{
			Crypton.Instance.GenerateAesKey();
			Crypton.Instance.AesKey = PasswordBytes;

			return Crypton.Instance.EncryptByAes(fileBytes);
		}

		private byte[] decryptFile(byte[] fileBytes)
		{
			Crypton.Instance.GenerateAesKey();
			Crypton.Instance.AesKey = PasswordBytes;

			return Crypton.Instance.DecryptByAes(fileBytes);
		}

		#region Handlers

		private void Crypton_Progress(object sender, ProgressEventArgs args) => cryptonProgress(args.BytesComplitted);

		#endregion

		#endregion

		#region Enum EncryptedPackMode

		public enum EncryptedPackMode
		{
			Serialize,
			Deserialize
		}

		#endregion
	}
}
