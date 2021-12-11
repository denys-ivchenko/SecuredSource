using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace Telesyk.SecuredSource
{
	[Serializable]
	public sealed class PackData : IEnumerable<FileData>
	{
		#region Private declarations

		private List<FileData> _files = new List<FileData>();

		private SortedDictionary<string, FileData> _names = new SortedDictionary<string, FileData>();

		private Dictionary<string, FileData> _fullNames = new Dictionary<string, FileData>();

		private int _fileCount;

		#endregion

		#region Constructors

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

		public EncryptionAlgorythm Algorythm { get; internal set; }

		public int PasswordLength { get; internal set; }

		public string PasswordHash { get; internal set; }

		public int FileCount { get => _fileCount; internal set => _fileCount = value; }

		#endregion

		#region Internal methods

		internal bool Add(FileData file) => add(file);

		internal void Remove(string name) => remove(name);

		internal void Remove(FileData file) => remove(file);

		internal void Clear() => clear();

		#endregion

		#region Interface implementations

		public IEnumerator<FileData> GetEnumerator()
		{
			return _names.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region Private methods

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

			FileCount = _files.Count;

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

			FileCount = _files.Count;

			if (!skipRecalculating)
				recalculateNames();
		}

		private void clear()
		{
			_files.Clear();
			_names.Clear();
			_fullNames.Clear();

			FileCount = _files.Count;
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

		public void test(FileData file)
		{
			BinaryFormatter f = new BinaryFormatter();

			using (var stream = new MemoryStream())
			{
				f.Serialize(stream, file);
				stream.Position = 0;
				var de = (FileData)f.Deserialize(stream);
			}
		}

		#endregion
	}
}
