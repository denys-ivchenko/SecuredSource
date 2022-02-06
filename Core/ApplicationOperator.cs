using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Telesyk.SecuredSource
{
	public sealed class ApplicationOperator
	{
		#region Private declarations

		private static Lazy<ApplicationOperator> _operator = new Lazy<ApplicationOperator>(() => new ApplicationOperator());

		private List<UIElement> _encryptionControls = new List<UIElement>();
		private List<IEnablingState> _encryptionContainers = new List<IEnablingState>();

		private PackData _encryptionPack;

		private EncryptionPack _serializePack;

		private EncryptionPack _deserializePack;

		//private List<UIElement> _decryptionControls = new List<UIElement>();
		//private List<IEnablingState> _decryptionContainers = new List<IEnablingState>();

		#endregion

		#region Constructors

		private ApplicationOperator()
		{
			init();
		}

		#endregion

		#region Public properties

		public static ApplicationOperator Operator => _operator.Value;

		public bool IsInProcess { get; private set; }

		public PackData EncryptionPack
		{
			get
			{
				if (_encryptionPack == null)
					_encryptionPack = new PackData(ApplicationSettings.Current.EncryptionPassword, ApplicationSettings.Current.AppVersion);

				return _encryptionPack;
			}
		}

		public EncryptionPack SerializePack
		{ 
			get => _serializePack; 
			internal set
			{
				_serializePack = value;

				SerializePackChanged?.Invoke(this, new ValueProcessedEventArgs<EncryptionPack>(SerializePack));
			}
		}

		public EncryptionPack DeserializePack
		{
			get => _deserializePack;
			internal set
			{
				_deserializePack = value;

				DeserializePackChanged?.Invoke(this, new ValueProcessedEventArgs<EncryptionPack>(DeserializePack));
			}
		}

		#endregion

		#region Public methods

		public void InitStartUpFile() => initStartUpFile();

		public void LoadDeserializePack(string packPath) => loadDeserializePack(packPath);

		public void RegisterForEncryptionProcess(params UIElement[] controls) => registerForEncryptionProcess(controls);

		public void UnregisterForEncryptionProcess(params UIElement[] controls) => unregisterForEncryptionProcess(controls);

		//public void RegisterForDecryptionProcess(params UIElement[] controls) => registerForDecryptionProcess(controls);

		//public void UnregisterForDecryptionProcess(params UIElement[] controls) => unregisterForDecryptionProcess(controls);

		//public void RegisterForEncriptionProcess(IEnablingState container) => registerForEncriptionProcess(container);

		#endregion

		#region Internal methods

		internal void DisableForEncryptionProcess() => disableForEncryptionProcess();

		internal void EnableForEncryptionProcess() => enableForEncryptionProcess();

		//internal void DisableForDecryptionProcess() => disableForDecryptionProcess();

		//internal void EnableForDecryptionProcess() => enableForDecryptionProcess();

		#endregion

		#region Public events

		public event ValueProcessedEventHandler<EncryptionPack> SerializePackChanged;

		public event ValueProcessedEventHandler<EncryptionPack> DeserializePackChanged;

		#endregion

		#region Private methods

		private void init()
		{
			//ApplicationSettings.Current.EncryptionAlgorithmChanged += (s, a)
			//	=> SerializePack.FilePack.Algorithm = ApplicationSettings.Current.EncryptionAlgorithm;

			ApplicationSettings.Current.EncryptionPasswordChanged += (s, a)
				=> EncryptionPack.Password = ApplicationSettings.Current.EncryptionPassword;
		}

		private void loadDeserializePack(string packPath)
		{
			ApplicationSettings.Current.DecryptionPackFilePath = null;
			
			DeserializePack = null;

			if (!string.IsNullOrEmpty(packPath))
			{
				var file = new FileInfo(packPath);

				if (file.Exists)
				{
					ApplicationSettings.Current.DecryptionPackFilePath = packPath;

					DeserializePack = new EncryptionPack(packPath, ApplicationSettings.Current.DecryptionDirectory);

					if (!DeserializePack.IsValid)
						DeserializePack = null;
				}
			}
		}

		private void initStartUpFile()
		{
			try { LoadDeserializePack(ApplicationSettings.Current.DecryptionPackFilePath); }
			catch { };

			if (DeserializePack != null)
				ApplicationSettings.Current.Mode = ApplicationMode.Decryption;
		}

		private void registerForEncryptionProcess(params UIElement[] controls)
		{
			foreach(var control in controls)
				if (control is IEnablingState && !_encryptionContainers.Contains(control as IEnablingState))
					_encryptionContainers.Add(control as IEnablingState);
				else if (!(control is IEnablingState) && !_encryptionControls.Contains(control))
					_encryptionControls.Add(control);
		}

		private void unregisterForEncryptionProcess(params UIElement[] controls)
		{
			foreach (var control in controls)
				if (_encryptionContainers.Contains(control as IEnablingState))
					_encryptionContainers.Remove(control as IEnablingState);
				else if (_encryptionControls.Contains(control))
					_encryptionControls.Remove(control);
		}

		private void disableForEncryptionProcess()
		{
			_encryptionControls.ForEach(c => c.IsEnabled = false);
			_encryptionContainers.ForEach(c => c.SetEnablingState(true));

			IsInProcess = true;
		}

		private void enableForEncryptionProcess()
		{
			_encryptionControls.ForEach(c => c.IsEnabled = true);
			_encryptionContainers.ForEach(c => c.SetEnablingState(false));

			IsInProcess = false;
		}

		//private void registerForDecryptionProcess(params UIElement[] controls)
		//{
		//	foreach(var control in controls)
		//		if (control is IEnablingState && !_decryptionContainers.Contains(control as IEnablingState))
		//			_decryptionContainers.Add(control as IEnablingState);
		//		else if (!(control is IEnablingState) && !_decryptionControls.Contains(control))
		//			_decryptionControls.Add(control);
		//}

		//private void unregisterForDecryptionProcess(params UIElement[] controls)
		//{
		//	foreach (var control in controls)
		//		if (_decryptionContainers.Contains(control as IEnablingState))
		//			_decryptionContainers.Remove(control as IEnablingState);
		//		else if (_decryptionControls.Contains(control))
		//			_decryptionControls.Remove(control);
		//}

		//private void disableForDecryptionProcess()
		//{
		//	_decryptionControls.ForEach(c => c.IsEnabled = false);
		//	_decryptionContainers.ForEach(c => c.SetEnablingState(true));

		//	IsInProcess = true;
		//}

		//private void enableForDecryptionProcess()
		//{
		//	_decryptionControls.ForEach(c => c.IsEnabled = true);
		//	_decryptionContainers.ForEach(c => c.SetEnablingState(false));

		//	IsInProcess = false;
		//}

		#endregion
	}
}
