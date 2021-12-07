using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.IO;
using System.Xml;

namespace Telesyk.SecuredSource
{
	public sealed class ApplicationSettings
	{
		#region Private declarations
		
		private static Lazy<ApplicationSettings> _instance = new Lazy<ApplicationSettings>(() => new ApplicationSettings());

		#region Constants

		private const string _APPDIRECTORY_PATH = "Telesyk/SecuredSource";
		private const string _SETTINGS_FILE = "settings.config";
		private const string _NODENAME_LAST_MODE = "mode";
		private const string _NODENAME_LAST_PANEL_WIDTH = "panel-width";
		private const string _NODENAME_LAST_DIRECTORY = "directory";
		private const string _NODENAME_LAST_ALGORYTHM = "algorythm";
		private const string _NODENAME_LAST_AES_KEY_LENGTH = "aes-key-length";
		private const string _NODENAME_LAST_AES_PASSWORD_LENGTH = "aes-password-length";

		#endregion

		private ApplicationMode _lastMode = ApplicationMode.SimpleAndFast;
		private int _lastPanelWidth = 160;
		private string _lastDirectory = null;
		private int _lastAesKeyLength = 128;
		private int _lastAesPasswordLength = 16;
		private EncryptionAlgorythm _lastAlgorythm = EncryptionAlgorythm.Aes;

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

		#endregion

		#region Public properties

		public static ApplicationSettings Current { get => _instance.Value; }

		public ApplicationMode LastMode
		{
			get { return _lastMode; }
			set { _lastMode = writeSettingValue(_NODENAME_LAST_MODE, value); }
		}

		public int LastPanelWidth
		{
			get => _lastPanelWidth;
			set { _lastPanelWidth = writeSettingValue(_NODENAME_LAST_PANEL_WIDTH, value); }
		}

		public string LastDirectory
		{
			get => _lastDirectory;
			set { _lastDirectory = writeSettingValue(_NODENAME_LAST_DIRECTORY, value); }
		}

		public EncryptionAlgorythm LastAlgorythm
		{
			get => _lastAlgorythm;
			set { _lastAlgorythm = writeSettingValue(_NODENAME_LAST_ALGORYTHM, value); }
		}

		public int LastAesKeyLength
		{
			get => _lastAesKeyLength;
			set { _lastAesKeyLength = writeSettingValue(_NODENAME_LAST_AES_KEY_LENGTH, value); }
		}

		public int LastAesPasswordLength
		{
			get => _lastAesPasswordLength;
			set { _lastAesPasswordLength = writeSettingValue(_NODENAME_LAST_AES_PASSWORD_LENGTH, value); }
		}

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
			readLastModeSetting();
			readLastMenuWidthSetting();
			readLastDirectorySetting();
			readLastAlgorythmSetting();
			readLastAesKeyLengthSetting();
			readLastAesPasswordLengthSetting();
		}

		private void readLastModeSetting()
		{
			var value = readSettingValue(_NODENAME_LAST_MODE);

			if (!Enum.TryParse<ApplicationMode>(value, out ApplicationMode mode))
				writeSettingValue(_NODENAME_LAST_MODE, LastMode);
			else
				_lastMode = mode;
		}

		private void readLastMenuWidthSetting()
		{
			var value = readSettingValue(_NODENAME_LAST_PANEL_WIDTH);

			if (!int.TryParse(value, out int menuWidth))
				writeSettingValue(_NODENAME_LAST_PANEL_WIDTH, LastPanelWidth);
			else
				_lastPanelWidth = menuWidth;
		}

		private void readLastDirectorySetting()
		{
			var value = readSettingValue(_NODENAME_LAST_DIRECTORY);

			_lastDirectory = value;
		}

		private void readLastAlgorythmSetting()
		{
			var value = readSettingValue(_NODENAME_LAST_ALGORYTHM);

			if (!Enum.TryParse<EncryptionAlgorythm>(value, out EncryptionAlgorythm algorythm))
				writeSettingValue(_NODENAME_LAST_MODE, LastAlgorythm);
			else
				_lastAlgorythm = algorythm;
		}

		private void readLastAesKeyLengthSetting()
		{
			var value = readSettingValue(_NODENAME_LAST_AES_KEY_LENGTH);

			if (!int.TryParse(value, out int length))
				writeSettingValue(_NODENAME_LAST_AES_KEY_LENGTH, LastAesKeyLength);
			else
				_lastAesKeyLength = length;
		}

		private void readLastAesPasswordLengthSetting()
		{
			var value = readSettingValue(_NODENAME_LAST_AES_PASSWORD_LENGTH);

			if (!int.TryParse(value, out int length))
				writeSettingValue(_NODENAME_LAST_AES_PASSWORD_LENGTH, LastAesPasswordLength);
			else
				_lastAesPasswordLength = length;
		}

		#endregion

		#region Xml operations

		private XmlNode getSettingNode(string nodeName)
		{
			XmlNode node = SettingsXml.LastChild.SelectSingleNode(nodeName);

			if (node == null)
			{
				reCreateSettingsXml();
				node = SettingsXml.LastChild.SelectSingleNode(nodeName);
			}

			return node;
		}

		private string readSettingValue(string nodeName)
		{
			var node = getSettingNode("last/" + nodeName);

			return node.InnerText;
		}

		private T writeSettingValue<T>(string nodeName, T value) => writeSettingValue(nodeName, value, false);

		private T writeSettingValue<T>(string nodeName, T value, bool withoutSaving)
		{
			var text = value != null ? value.ToString() : string.Empty;
			var node = getSettingNode("last/" + nodeName);

			if (node.InnerText != text)
			{
				node.InnerText = value.ToString();

				if (!withoutSaving)
					SettingsXml.Save(XmlFilePath);
			}

			return value;
		}

		#endregion

		#region IO operation

		private void createSettingsXml()
		{
			SettingsXml = new XmlDocument();
			SettingsXml.LoadXml(Properties.Resources.Settings);

			writeSettingValue(_NODENAME_LAST_MODE, LastMode, true);
			writeSettingValue(_NODENAME_LAST_PANEL_WIDTH, LastPanelWidth, true);
			writeSettingValue(_NODENAME_LAST_DIRECTORY, LastDirectory, true);
			writeSettingValue(_NODENAME_LAST_ALGORYTHM, LastAlgorythm, true);
			writeSettingValue(_NODENAME_LAST_AES_KEY_LENGTH, LastAesKeyLength, true);
			writeSettingValue(_NODENAME_LAST_AES_PASSWORD_LENGTH, LastAesPasswordLength, true);

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

				if (!Directory.Exists(appPath))
					Directory.CreateDirectory(appPath);
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

		#endregion
	}
}
