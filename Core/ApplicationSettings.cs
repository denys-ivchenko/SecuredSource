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
		private const string _NODENAME_WINDOW_WIDTH = "window-width";
		private const string _NODENAME_WINDOW_HEIGHT = "window-height";
		private const string _NODENAME_WINDOW_TOP = "window-top";
		private const string _NODENAME_WINDOW_LEFT = "window-left";
		private const string _NODENAME_MODE = "mode";
		private const string _NODENAME_PANEL_WIDTH = "panel-width";
		private const string _NODENAME_DIRECTORY = "directory";
		private const string _NODENAME_ALGORYTHM = "algorythm";
		private const string _NODENAME_AES_PASSWORD_LENGTH = "aes-password-length";

		#endregion

		private ApplicationMode _mode = ApplicationMode.SimpleAndFast;
		private int _windowWidth = 600;
		private int _windowHeight = 400;
		private int _windowTop = 200;
		private int _windowLeft = 400;
		private int _panelWidth = 160;
		private string _directory = null;
		private int _aesPasswordLength = 16;
		private EncryptionAlgorythm _algorythm = EncryptionAlgorythm.RC5;

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

		public ApplicationMode Mode
		{
			get { return _mode; }
			set { _mode = writeSettingValue(_NODENAME_MODE, value); }
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
			set { _windowTop = writeSettingValue(_NODENAME_WINDOW_TOP, value); }
		}

		public int WindowLeft
		{
			get => _windowLeft;
			set { _windowLeft = writeSettingValue(_NODENAME_WINDOW_LEFT, value); }
		}

		public int PanelWidth
		{
			get => _panelWidth;
			set { _panelWidth = writeSettingValue(_NODENAME_PANEL_WIDTH, value); }
		}

		public string Directory
		{
			get => _directory;
			set { _directory = writeSettingValue(_NODENAME_DIRECTORY, value); }
		}

		public EncryptionAlgorythm Algorythm
		{
			get => _algorythm;
			set { _algorythm = writeSettingValue(_NODENAME_ALGORYTHM, value); }
		}

		public int AesPasswordLength
		{
			get => _aesPasswordLength;
			set { _aesPasswordLength = writeSettingValue(_NODENAME_AES_PASSWORD_LENGTH, value); }
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

			readEnumSetting<ApplicationMode>(_NODENAME_MODE, ref _mode);
			readIntegerSetting(_NODENAME_WINDOW_WIDTH, ref _windowWidth);
			readIntegerSetting(_NODENAME_WINDOW_HEIGHT, ref _windowHeight);
			readIntegerSetting(_NODENAME_WINDOW_TOP, ref _windowTop);
			readIntegerSetting(_NODENAME_WINDOW_LEFT, ref _windowLeft);
			readIntegerSetting(_NODENAME_PANEL_WIDTH, ref _panelWidth);
			readStringSetting(_NODENAME_DIRECTORY, ref _directory);
			readEnumSetting<EncryptionAlgorythm>(_NODENAME_ALGORYTHM, ref _algorythm);
			readIntegerSetting(_NODENAME_AES_PASSWORD_LENGTH, ref _aesPasswordLength);
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

		private void readStringSetting(string nodeName, ref string value) => readStringSetting(nodeName, ref value, false);

		private void readStringSetting(string nodeName, ref string value, bool skipWriteIfEmpty)
		{
			var node = getSettingNode("last/" + nodeName);

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

			writeSettingValue(_NODENAME_MODE, Mode, true);
			writeSettingValue(_NODENAME_WINDOW_WIDTH, WindowWidth, true);
			writeSettingValue(_NODENAME_WINDOW_HEIGHT, WindowHeight, true);
			writeSettingValue(_NODENAME_WINDOW_TOP, WindowTop, true);
			writeSettingValue(_NODENAME_WINDOW_LEFT, WindowLeft, true);
			writeSettingValue(_NODENAME_PANEL_WIDTH, PanelWidth, true);
			writeSettingValue(_NODENAME_DIRECTORY, Directory, true);
			writeSettingValue(_NODENAME_ALGORYTHM, Algorythm, true);
			writeSettingValue(_NODENAME_AES_PASSWORD_LENGTH, AesPasswordLength, true);

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

		#endregion
	}
}
