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

		private long _totalByteComplitted;

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

		public long TotalBytes { get; private set; }

		public long BytesComplitted
		{
			get => _totalByteComplitted + CurrentByteComplitted;
			private set
			{
				_totalByteComplitted = value;
				CurrentByteComplitted = 0;
			}
		}

		public AggregateException Exception { get; private set; }

		public long CurrentByteComplitted { get; private set; }

		public bool IsFinished { get; private set; }

		public bool IsCompleted{ get; private set; }

		public bool IsCanceled { get; private set; }

		public bool IsFaulted { get; private set; }

		#endregion

		#region Internal properties

		internal SymmetricEncryptor Encryptor { get; private set; }

		#endregion

		#region Public methods

		public void Serialize() => serialize();

		public void Deserialize() => deserialize();

		public void Cancel() => cancel();

		#endregion

		#region Public events

		//public event ProgressEventHandler Progress;

		public event ValueProcessedEventHandler<int> Processed;

		public event EventHandler Finished;

		public event EventHandler Completted;

		public event EventHandler Canceled;

		public event EventHandler Faulted;

		#endregion

		#region Interface implementations

		//public void Dispose()
		//{

		//}

		#endregion

		#region Private methods

		private void init()
		{
			PasswordBytes = Encoding.UTF8.GetBytes(Password);

			if (Mode == EncryptedPackMode.Deserialize)
				using (FileStream stream = new FileStream(FilePath, FileMode.Open))
					TotalBytes = (int)stream.Length;
		}

		private void resetForStart()
		{
			IsFinished = IsCompleted = IsCanceled = IsFaulted = false;
			Exception = null;
		}

		private void cancel()
		{
			var encryptor = Encryptor;

			if (encryptor != null && !encryptor.IsCanceled)
				encryptor.Cancel();
		}

		private async void serialize()
		{
			resetForStart();

			var pack = PackData.Serialize(FilePack);

			TotalBytes = 4 + pack.Length + FilePack.TotalBytes;

			Task task = null;

			ControlStateOperator.Operator.DisableForEncryptionProcess();

			using (Encryptor = new SymmetricEncryptor(FilePath, FileMode.Create, ApplicationSettings.Current.Algorithm, PasswordBytes))
			{
				Encryptor.Processed += Encryptor_Processed;

				await (task = Task.Run(() => Encryptor.WriteAsync(pack.Length)));

				await (task = Task.Run(() => Encryptor.WriteAsync(pack)));

				foreach (var file in FilePack)
					await (task = Task.Run(() => Encryptor.EncryptAsync(file.FullName)));

				Exception = task.Exception;

				IsFinished = true;

				IsCompleted = task.IsCompleted;
				IsCanceled = task.IsCanceled;
				IsFaulted = task.IsFaulted;
			}

			ControlStateOperator.Operator.EnableForEncryptionProcess();

			if (Finished != null)
				Finished(this, EventArgs.Empty);

			if (IsCompleted && Completted != null)
				Completted(this, EventArgs.Empty);

			if (IsCanceled && Canceled != null)
				Canceled(this, EventArgs.Empty);

			if (IsFaulted && Faulted != null)
				Faulted(this, EventArgs.Empty);
		}

		private async void deserialize()
		{
			resetForStart();

			ControlStateOperator.Operator.DisableForEncryptionProcess();

			using (Encryptor = new SymmetricEncryptor(FilePath, FileMode.Open, ApplicationSettings.Current.Algorithm, PasswordBytes))
			{
				Encryptor.Processed += Encryptor_Processed;

				var taskLength = Task.Run(() => Encryptor.ReadAsync());
				await taskLength;
				
				var taskBytes = Task.Run(() => Encryptor.ReadAsync(taskLength.Result));
				await taskBytes;

				FilePack = PackData.Deserialize(taskBytes.Result);
				
				TotalBytes = 4 + taskBytes.Result.LongLength + FilePack.TotalBytes;

				Task taskFile = null;

				foreach (var file in FilePack)
					await (taskFile = Task.Run(() => Encryptor.DecryptAsync(Path.Combine(OutputPath, file.Name), file.ByteCount)));

				Exception = taskFile.Exception;

				IsFinished = true;

				IsCompleted = taskFile.IsCompleted;
				IsCanceled = taskFile.IsCanceled;
				IsFaulted = taskFile.IsFaulted;
			}

			ControlStateOperator.Operator.EnableForEncryptionProcess();

			if (Finished != null)
				Finished(this, EventArgs.Empty);

			if (IsCompleted && Completted != null)
				Completted(this, EventArgs.Empty);

			if (IsCanceled && Canceled != null)
				Canceled(this, EventArgs.Empty);

			if (IsFaulted && Faulted != null)
				Faulted(this, EventArgs.Empty);
		}

		private void encryptorProcessed(int bytesComplitted)
		{
			if (Processed != null)
			{
				BytesComplitted += bytesComplitted;

				Processed(this, new ValueProcessedEventArgs<int>((int)(BytesComplitted / (decimal)TotalBytes * 100)));
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

		//private void Crypton_Progress(object sender, ProgressEventArgs args) => cryptonProgress(args.Percent);

		private void Encryptor_Processed(object sender, ValueProcessedEventArgs<int> args) => encryptorProcessed(args.Value);

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
