using System;
using System.IO;
using System.Xml;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	public sealed class ApplicationSettings
	{
		#region Private declarations
		
		private static Lazy<ApplicationSettings> _instance = new Lazy<ApplicationSettings>(() => new ApplicationSettings());
	
		#region Constants

		private const string _APPDIRECTORY_PATH = "Telesyk/Enigma";
		private const string _SETTINGS_FILE = "settings.config";
		private const string _NODENAME_WINDOW_WIDTH = "window-width";
		private const string _NODENAME_WINDOW_HEIGHT = "window-height";
		private const string _NODENAME_WINDOW_TOP = "window-top";
		private const string _NODENAME_WINDOW_LEFT = "window-left";
		private const string _NODENAME_MODE = "mode";
		private const string _NODENAME_PANEL_WIDTH = "panel-width";
		private const string _NODENAME_ENCRYPTION_DIRECTORY = "directory";
		private const string _NODENAME_FILE_NAME = "file-name";
		private const string _NODENAME_ALGORITHM = "algorithm";
		private const string _NODENAME_DECRYPTION_DIRECTORY = "decryption-directory";
		private const string _NODENAME_DECRYPTION_PACK_PATH = "decryption-pack-path";
		private const string _NODENAME_DECRYPTION_ALGORITHM = "decryption-algorithm";
		private const string _NODENAME_REQUIRED_PASSWORD_LENGTH = "required-password-length";

		#endregion

		private ApplicationMode _mode = ApplicationMode.Decryption;
		private SymmetricAlgorithmName _encryptionAlgorithm = SymmetricAlgorithmName.Aes;
		private SymmetricAlgorithmName _decryptionAlgorithm = SymmetricAlgorithmName.Aes;
		private int _windowWidth = 600;
		private int _windowHeight = 420;
		private int _windowTop = 200;
		private int _windowLeft = 400;
		private int _panelWidth = 160;
		private string _encryptionDirectory = null;
		private string _fileName = null;
		private int _requiredPasswordLength = 16;
		private int _fileQuantity;
		private string _decryptionPackPath;
		private string _decryptionDirectory;

		#endregion

		#region Constructors

		private ApplicationSettings()
		{
			init();
		}

		#endregion

		#region Internal properties

		internal string XmlFilePath { get; private set; }

		internal XmlDocument SettingsXml { get; private set; }

		internal Password EncryptionPassword { get; private set; }

		internal Password DecryptionPassword { get; private set; }

		#endregion

		#region Public properties

		public static ApplicationSettings Current { get => _instance.Value; }

		public string AppVersion { get; set; }

		public ApplicationMode Mode
		{
			get { return _mode; }
			set { _mode = setMode(value); }
		}

		public SymmetricAlgorithmName EncryptionAlgorithm { get => _encryptionAlgorithm; }

		public SymmetricAlgorithmName DecryptionAlgorithm { get => _decryptionAlgorithm; }

		public string EncryptionDirectory
		{
			get => _encryptionDirectory;
			set { _encryptionDirectory = writeSettingValue(_NODENAME_ENCRYPTION_DIRECTORY, value); }
		}

		public string DecryptionDirectory
		{
			get => _decryptionDirectory;
			set
			{
				_decryptionDirectory = writeSettingValue(_NODENAME_DECRYPTION_DIRECTORY, value);

				DecryptionDirectoryChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int WindowWidth
		{
			get => _windowWidth;
			set { _windowWidth = writeSettingValue(_NODENAME_WINDOW_WIDTH, value); }
		}

		public int WindowHeight
		{
			get => _windowHeight;
			set { _windowHeight = writeSettingValue(_NODENAME_WINDOW_HEIGHT, value); }
		}

		public int WindowTop
		{
			get => _windowTop;
			set { _windowTop = writeSettingValue(_NODENAME_WINDOW_TOP, value < 0 ? WindowTop : value); }
		}

		public int WindowLeft
		{
			get => _windowLeft;
			set { _windowLeft = writeSettingValue(_NODENAME_WINDOW_LEFT, value < 0 ? WindowLeft : value); }
		}

		public int PanelWidth
		{
			get => _panelWidth;
			set { _panelWidth = writeSettingValue(_NODENAME_PANEL_WIDTH, value); }
		}

		public string FileName
		{
			get => _fileName;
			set { _fileName = writeSettingValue(_NODENAME_FILE_NAME, value); }
		}

		public int FileQuantity
		{
			get => _fileQuantity;
			set { changeFileQuantity(value); }
		}

		public string DecryptionPackFilePath { get; set; }

		#endregion

		#region Internal methods

		public void SetEncryptionPassword(Password password)
			=> setEncryptionPassword(password);

		public void SetDecryptionPassword(Password password)
			=> setDecryptionPassword(password);

		public void SetEncryptionAlgorithm(SymmetricAlgorithmName algorithm)
			=> setEncryptionAlgorithm(algorithm);

		public void SetDecryptionAlgorithm(SymmetricAlgorithmName algorithm)
			=> setDecryptionAlgorithm(algorithm);

		#endregion

		#region Events

		public event EventHandler ModeChanged;

		public event EventHandler EncryptionAlgorithmChanged;

		public event EventHandler EncryptionPasswordChanged;

		public event EventHandler DecryptionPasswordChanged;

		public event EventHandler FileQuantityChanged;

		public event EventHandler DecryptionPackChanged;

		public event EventHandler DecryptionDirectoryChanged;

		public event EventHandler DecryptionAlgorithmChanged;

		#endregion

		#region Private methods

		#region Init

		private void init()
		{
			ensureSettingsXml();
			readSettings();
		}

		#endregion

		#region Read settings

		private void readSettings()
		{

			readEnumSetting<ApplicationMode>(_NODENAME_MODE, ref _mode);
			readIntegerSetting(_NODENAME_WINDOW_WIDTH, ref _windowWidth);
			readIntegerSetting(_NODENAME_WINDOW_HEIGHT, ref _windowHeight);
			readIntegerSetting(_NODENAME_WINDOW_TOP, ref _windowTop);
			readIntegerSetting(_NODENAME_WINDOW_LEFT, ref _windowLeft);
			readIntegerSetting(_NODENAME_PANEL_WIDTH, ref _panelWidth);
			readStringSetting(_NODENAME_ENCRYPTION_DIRECTORY, ref _encryptionDirectory);
			readStringSetting(_NODENAME_FILE_NAME, ref _fileName);
			readStringSetting(_NODENAME_DECRYPTION_PACK_PATH, ref _decryptionPackPath);
			readStringSetting(_NODENAME_DECRYPTION_DIRECTORY, ref _decryptionDirectory);
			readEnumSetting<SymmetricAlgorithmName>(_NODENAME_ALGORITHM, ref _encryptionAlgorithm);
			readEnumSetting<SymmetricAlgorithmName>(_NODENAME_DECRYPTION_ALGORITHM, ref _decryptionAlgorithm);
			readIntegerSetting(_NODENAME_REQUIRED_PASSWORD_LENGTH, ref _requiredPasswordLength);

			_encryptionDirectory = !string.IsNullOrEmpty(_encryptionDirectory) ? _encryptionDirectory : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			_decryptionDirectory = !string.IsNullOrEmpty(_decryptionDirectory) ? _decryptionDirectory : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		}

		#endregion

		#region Xml operations

		private XmlNode getSettingNode(string nodeName)
		{
			return SettingsXml.LastChild.SelectSingleNode(nodeName);
		}

		private XmlNode createSettingNode(string nodeName, ref string value)
		{
			XmlNode node = SettingsXml.CreateElement(nodeName);
			SettingsXml.LastChild.AppendChild(node);

			node.InnerText = value;

			SettingsXml.Save(XmlFilePath);

			return node;
		}

		private void readStringSetting(string nodeName, ref string value) => readStringSetting(nodeName, ref value, false);

		private void readStringSetting(string nodeName, ref string value, bool skipWriteIfEmpty)
		{
			var node = getSettingNode(nodeName);

			if (node == null)
				node = createSettingNode(nodeName, ref value);

			if (!skipWriteIfEmpty && string.IsNullOrWhiteSpace(node.InnerText) && !string.IsNullOrWhiteSpace(value))
				writeSettingValue<string>(nodeName, value);
			else
				value = node.InnerText;
		}

		private void readEnumSetting<T>(string nodeName, ref T value)
			where T : struct
		{
			string data = null;
			readStringSetting(nodeName, ref data);

			if (!Enum.TryParse<T>(data, out T result))
				writeSettingValue(nodeName, value);
			else
				value = result;
		}

		private void readIntegerSetting(string nodeName, ref int value)
		{
			string data = null;
			readStringSetting(nodeName, ref data, true);

			if (!int.TryParse(data, out int result))
				writeSettingValue(nodeName, value);
			else
				value = result;
		}

		private T writeSettingValue<T>(string nodeName, T value) => writeSettingValue(nodeName, value, false);

		private T writeSettingValue<T>(string nodeName, T value, bool withoutSaving)
		{
			var text = value != null ? value.ToString() : string.Empty;
			var node = getSettingNode(nodeName);

			if (node != null && node.InnerText != text)
			{
				node.InnerText = value.ToString();

				if (!withoutSaving)
					SettingsXml.Save(XmlFilePath);
			}
			else if (node == null)
				createSettingNode(nodeName, ref text);

				return value;
		}

		#endregion

		#region IO operation

		private void createSettingsXml()
		{
			SettingsXml = new XmlDocument();
			SettingsXml.AppendChild(SettingsXml.CreateElement("settings"));

			writeSettingValue(_NODENAME_MODE, Mode, true);
			writeSettingValue(_NODENAME_WINDOW_WIDTH, WindowWidth, true);
			writeSettingValue(_NODENAME_WINDOW_HEIGHT, WindowHeight, true);
			writeSettingValue(_NODENAME_WINDOW_TOP, WindowTop, true);
			writeSettingValue(_NODENAME_WINDOW_LEFT, WindowLeft, true);
			writeSettingValue(_NODENAME_PANEL_WIDTH, PanelWidth, true);
			writeSettingValue(_NODENAME_ENCRYPTION_DIRECTORY, EncryptionDirectory, true);
			writeSettingValue(_NODENAME_FILE_NAME, FileName, true);
			writeSettingValue(_NODENAME_ALGORITHM, EncryptionAlgorithm, true);

			SettingsXml.Save(XmlFilePath);
		}

		private void reCreateSettingsXml()
		{
			deleteSettingsXml();
			createSettingsXml();
		}

		private void deleteSettingsXml()
		{
			try { File.Delete(XmlFilePath); }
			catch { }
		}

		private string ensureAppDirectory()
		{
			var paths = _APPDIRECTORY_PATH.Split('/');
			var appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			for (var i = 0; i < paths.Length; i++)
			{
				appPath = Path.Combine(appPath, paths[i]);

				if (!System.IO.Directory.Exists(appPath))
					System.IO.Directory.CreateDirectory(appPath);
			}

			return appPath;
		}

		private void ensureSettingsXml()
		{
			var path = ensureAppDirectory();
			XmlFilePath = Path.Combine(path, _SETTINGS_FILE);

			SettingsXml = new XmlDocument();

			if (File.Exists(XmlFilePath))
			{
				try { SettingsXml.Load(XmlFilePath); }
				catch { reCreateSettingsXml(); }
			}

			if (!File.Exists(XmlFilePath))
				createSettingsXml();
		}

		#endregion

		#region Password

		private void setEncryptionAlgorithm(SymmetricAlgorithmName algorithm)
		{
			_encryptionAlgorithm = writeSettingValue(_NODENAME_ALGORITHM, algorithm);

			EncryptionAlgorithmChanged?.Invoke(this, EventArgs.Empty);
		}

		private void setDecryptionAlgorithm(SymmetricAlgorithmName algorithm)
		{
			_decryptionAlgorithm = writeSettingValue(_NODENAME_DECRYPTION_ALGORITHM, algorithm);

			DecryptionAlgorithmChanged?.Invoke(this, EventArgs.Empty);
		}

		private void setEncryptionPassword(Password password)
		{
			EncryptionPassword = password;

			EncryptionPasswordChanged?.Invoke(this, EventArgs.Empty);
		}

		private void setDecryptionPassword(Password password)
		{
			DecryptionPassword = password;

			DecryptionPasswordChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		private ApplicationMode setMode(ApplicationMode mode)
		{
			writeSettingValue(_NODENAME_MODE, mode);

			ModeChanged?.Invoke(this, EventArgs.Empty);

			return mode;
		}

		private void changeDecryptionPack(string filePath)
		{
			_decryptionPackPath = writeSettingValue(_NODENAME_DECRYPTION_PACK_PATH, filePath);

			DecryptionPackChanged?.Invoke(this, EventArgs.Empty);
		}

		private void changeFileQuantity(int value)
		{
			_fileQuantity = value;

			if (FileQuantityChanged != null)
				FileQuantityChanged(this, new ValueProcessedEventArgs<int>(FileQuantity));
		}

		#endregion
	}
}
