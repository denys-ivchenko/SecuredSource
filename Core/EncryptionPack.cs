using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	public sealed class EncryptionPack
	{
		#region Private declarations

		private int _totalByteComplitted;

		#endregion

		#region Constructors

		private EncryptionPack(string filePath, string password)
		{
			FilePath = filePath;
			Password = password;

			init();
		}

		public EncryptionPack(string filePath, string password, PackData filePackData)
			: this(filePath, password)
		{
			FilePack = filePackData;
			Mode = EncryptedPackMode.Serialize;
		}

		public EncryptionPack(string filePath, string password, string outputPath)
			: this(filePath, password)
		{
			OutputPath = outputPath;
			Mode = EncryptedPackMode.Deserialize;
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

		public int ByteComplitted
		{
			get => _totalByteComplitted + CurrentByteComplitted;
			private set
			{
				_totalByteComplitted = value;
				CurrentByteComplitted = 0;
			}
		}

		public int CurrentByteComplitted { get; private set; }

		#endregion

		#region Public methods

		public void Process() => process();

		#endregion

		#region Public events

		public event ProgressEventHandler Progress;

		public event EventHandler Completted;

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

			using (FileStream stream = new FileStream(FilePath, FileMode.Open))
				TotalBytes = (int)stream.Length;

			Crypton.Instance.Progress += Instance_Progress;
		}

		private void Instance_Progress(object sender, ProgressEventArgs args)
		{
			if (Progress != null)
			{
				CurrentByteComplitted = args.ByteComplitted;

				Progress(this, new ProgressEventArgs((int)((ByteComplitted / TotalBytes) * 100), ByteComplitted));
			}
		}

		private void process()
		{
			if (Mode == EncryptedPackMode.Serialize)
				serialize();
			else
				deserialize();

			if (Completted != null)
				Completted(this, EventArgs.Empty);
		}

		private void serialize()
		{
			BinaryFormatter formatter = new BinaryFormatter();

			var packBytes = PackData.Serialize(FilePack);

			TotalBytes = 4 + packBytes.Length;

			foreach (var file in FilePack)
				TotalBytes += file.ByteCount * 6;

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
				ByteComplitted = ByteComplitted + byteCount;
			else
				CurrentByteComplitted = byteCount;

			var percentage = (int)((ByteComplitted / (decimal)TotalBytes) * 100);

			if (Progress != null)
				Progress(this, new ProgressEventArgs(percentage, 0));
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
