using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	[Serializable]
	public sealed class PackData : IEnumerable<FileData>, IDeserializationCallback
	{
		#region Private declarations
		
		private List<FileData> _files = new List<FileData>();
		[NonSerialized]
		private SortedDictionary<string, FileData> _names = new SortedDictionary<string, FileData>();
		[NonSerialized]
		private Dictionary<string, FileData> _fullNames = new Dictionary<string, FileData>();

		#endregion

		#region Constructors

		public PackData()
		{
			
		}

		#endregion

		#region Public properties

		public FileData this[int index] { get => _files[index]; }

		public FileData this[string name] 
		{
			get
			{
				var key = normalizeKey(name);

				if (key.Contains("\\"))
					return _fullNames[key];

				return _names[key];
			}
		}

		public SymmetricAlgorithmName Algorythm { get; internal set; }

		public int PasswordLength { get; internal set; }

		public string PasswordHash { get; internal set; }

		public int FileCount { get => _files.Count; }

		public int TotalBytes { get; private set; }

		#endregion

		#region Internal methods

		internal bool Add(FileData file) => add(file);

		internal void Remove(string name) => remove(name);

		internal void Remove(FileData file) => remove(file);

		internal void Clear() => clear();

		#region Static methods

		public static byte[] Serialize(PackData packData) => serialize(packData);

		public static PackData Deserialize(byte[] bytes) => deserialize(bytes);

		#endregion

		#endregion

		#region Interface implementations

		public IEnumerator<FileData> GetEnumerator()
		{
			return _names.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void OnDeserialization(object sender)
		{
			var files = _files;

			_files = new List<FileData>();
			_names = new SortedDictionary<string, FileData>();
			_fullNames = new Dictionary<string, FileData>();

			foreach (var file in files)
				Add(file);
		}

		#endregion

		#region Private methods

		#region Instance methods

		private void recalculateNames()
		{
			var files = copy();
			
			foreach (var file in files)
				if (file.Name != file.OriginalName)
				{
					remove(file, true);

					file.Name = calculateName(file.OriginalName);

					Add(file);
				}
		}

		private string calculateName(string name)
		{
			var lastDot = name.LastIndexOf('.');

			int index = 0;
			string ext = null;
			string shortName = null;

			if (lastDot > -1)
			{
				shortName = name.Substring(0, lastDot);
				ext = name.Substring(lastDot);
			}

			while (_names.ContainsKey(normalizeKey(name)))
			{
				if (index > 0)
					shortName = shortName.Substring(0, shortName.LastIndexOf('['));

				index++;

				shortName = $"{shortName}[{index}]";
				name = $"{shortName}{ext}";
			}

			return name;
		}

		private bool add(FileData file)
		{
			try { _fullNames.Add(normalizeKey(file.FullName), file); }
			catch { return false; }

			file.Name = calculateName(file.Name);

			_names.Add(normalizeKey(file.Name), file);
			_files.Add(file);

			TotalBytes += file.ByteCount;

			return true;
		}

		private void remove(string name)
		{
			if (_names.ContainsKey(normalizeKey(name)))
				remove(_names[name]);
		}

		private void remove(FileData file) => remove(file, false);

		private void remove(FileData file, bool skipRecalculating)
		{
			_files.Remove(file);
			_names.Remove(normalizeKey(file.Name));
			_fullNames.Remove(normalizeKey(file.FullName));

			TotalBytes -= file.ByteCount;

			if (!skipRecalculating)
				recalculateNames();
		}

		private void clear()
		{
			_files.Clear();
			_names.Clear();
			_fullNames.Clear();

			TotalBytes = 0;
		}

		private FileData[] copy()
		{
			var files = new FileData[FileCount];
			
			_names.Values.CopyTo(files, 0);

			return files;
		}

		private string normalizeKey(string key)
		{
			return key.Replace('/', '\\').ToUpper();
		}

		#endregion

		#region Static methods

		private static byte[] serialize(PackData pack)
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, pack);
				
				var base64 = Convert.ToBase64String(stream.ToArray());

				return Encoding.UTF8.GetBytes(base64);
			}
		}

		private static PackData deserialize(byte[] bytes)
		{
			BinaryFormatter formatter = new BinaryFormatter();

			var base64 = Encoding.UTF8.GetString(bytes);
			bytes = Convert.FromBase64String(base64);

			using (var stream = new MemoryStream(bytes))
				return (PackData)formatter.Deserialize(stream);
		}

		#endregion

		#endregion
	}
}
