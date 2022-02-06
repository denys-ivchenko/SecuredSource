using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	public sealed class EncryptionPack
	{
		#region Constructors

		private EncryptionPack(string filePath, ApplicationMode mode, PackData pack, string outputPath)
		{
			FilePath = filePath;
			Mode = mode;
			OutputPath = outputPath;
			FilePack = pack;

			init();
		}

		public EncryptionPack(string filePath, PackData filePackData)
			: this(filePath, ApplicationMode.Encryption, filePackData, null)
		{

		}

		public EncryptionPack(string filePath, string outputPath)
			: this(filePath, ApplicationMode.Decryption, null, outputPath)
		{
		}

		#endregion

		#region Public properties

		public ApplicationMode Mode { get; private set; }

		public PackData FilePack { get; private set; }

		public Exception Error { get; private set; }

		public ReadOnlyCollection<Exception> Errors { get; private set; }

		public string FilePath { get; private set; }

		public string OutputPath { get; private set; }

		public long TotalBytes { get; private set; }

		public bool SerializeFilePackToBase64 { get; set; }

		public bool IsFinished { get; private set; }

		public bool IsCompleted{ get; private set; }

		public bool IsCanceled { get; private set; }

		public bool IsFaulted { get; private set; }

		public bool IsValid { get; private set; }

		public long BytesComplitted { get; private set; }

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

		public event ValueProcessedEventHandler<int> Processed;

		public event EventHandler Finished;

		public event EventHandler Completted;

		public event EventHandler Canceled;

		public event EventHandler Faulted;

		#endregion

		#region Private methods

		private void init()
		{
			if (Mode == ApplicationMode.Decryption)
				read();
		}

		private void resetForStart()
		{
			IsFinished = IsCompleted = IsCanceled = IsFaulted = false;

			BytesComplitted = 0;

			Error = null;
			Errors = null;
		}

		private void cancel()
		{
			var encryptor = Encryptor;

			if (encryptor != null && !encryptor.IsCanceled)
				encryptor.Cancel();
		}

		private void read()
		{
			resetForStart();
			
			try { readMetaData(); }
			catch (Exception error) { Error = error; }
		}

		private void readMetaData()
		{
			using (Encryptor = SymmetricEncryptor.CreateDecryptor(FilePath, ApplicationSettings.Current.DecryptionAlgorithm, ApplicationSettings.Current.DecryptionPassword))
			{
				Errors = Encryptor.Errors;
				
				var length = Encryptor.Read();

				byte[] bytes = Encryptor.Read(length);

				FilePack = PackData.Deserialize(bytes);

				TotalBytes = 4 + bytes.Length + FilePack.TotalBytes;

				IsValid = true;
			}
		}

		private async void serialize()
		{
			resetForStart();

			var pack = PackData.Serialize(FilePack);

			TotalBytes = 4 + pack.Length + FilePack.TotalBytes;

			Task task = null;

			ApplicationOperator.Operator.DisableForEncryptionProcess();

			using (Encryptor = SymmetricEncryptor.CreateEncryptor(FilePath, ApplicationSettings.Current.EncryptionAlgorithm, ApplicationSettings.Current.EncryptionPassword))
			{
				Encryptor.Processed += Encryptor_Processed;

				await (task = Task.Run(() => Encryptor.WriteAsync(pack.Length)));

				await (task = Task.Run(() => Encryptor.WriteAsync(pack)));

				processed(4 + pack.Length);

				foreach (var file in FilePack)
					await (task = Task.Run(() => Encryptor.EncryptAsync(file.FullName)));

				Error = task.Exception;

				IsFinished = true;

				IsCompleted = task.IsCompleted;
				IsCanceled = task.IsCanceled;
				IsFaulted = task.IsFaulted;
			}

			ApplicationOperator.Operator.EnableForEncryptionProcess();

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

			ApplicationOperator.Operator.DisableForEncryptionProcess();

			using (Encryptor = SymmetricEncryptor.CreateDecryptor(FilePath, ApplicationSettings.Current.DecryptionAlgorithm, ApplicationSettings.Current.DecryptionPassword))
			{
				Encryptor.Processed += Encryptor_Processed;

				var taskLength = Task.Run(() => Encryptor.ReadAsync());
				await taskLength;
				
				var taskBytes = Task.Run(() => Encryptor.ReadAsync(taskLength.Result));
				await taskBytes;

				FilePack = PackData.Deserialize(taskBytes.Result);
				
				TotalBytes = 4 + taskBytes.Result.LongLength + FilePack.TotalBytes;

				processed(4 + (int)taskBytes.Result.LongLength);

				Task taskFile = null;

				foreach (var file in FilePack)
					await (taskFile = Task.Run(() => Encryptor.DecryptAsync(Path.Combine(OutputPath, file.Name), file.ByteCount)));

				Error = taskFile.Exception;

				IsFinished = true;

				IsCompleted = taskFile.IsCompleted;
				IsCanceled = taskFile.IsCanceled;
				IsFaulted = taskFile.IsFaulted;
			}

			processed((int)TotalBytes);

			ApplicationOperator.Operator.EnableForEncryptionProcess();

			if (Finished != null)
				Finished(this, EventArgs.Empty);

			if (IsCompleted && Completted != null)
				Completted(this, EventArgs.Empty);

			if (IsCanceled && Canceled != null)
				Canceled(this, EventArgs.Empty);

			if (IsFaulted && Faulted != null)
				Faulted(this, EventArgs.Empty);
		}

		private void processed(int bytesComplitted)
		{
			if (Processed != null)
			{
				BytesComplitted += bytesComplitted;

				Processed(this, new ValueProcessedEventArgs<int>((int)(BytesComplitted / (decimal)TotalBytes * 100)));
			}
		}

		#region Handlers

		private void Encryptor_Processed(object sender, ValueProcessedEventArgs<int> args) => processed(args.Value);

		#endregion

		#endregion
	}
}
