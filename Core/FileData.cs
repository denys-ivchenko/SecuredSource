using System;
using System.IO;

namespace Telesyk.SecuredSource
{
	[Serializable]
	public sealed class FileData
	{
		#region Private declarations

		#endregion

		#region Constructors

		public FileData(string fullName)
		{
			initByName(fullName);
		}

		#endregion

		#region Public properties

		public string Name { get; internal set; }

		public string OriginalName { get; private set; }

		public string FullName { get; private set; }

		public string Path { get; private set; }

		public long ByteCount { get; private set; }

		public bool MissingReadAccess { get; private set; }

		#endregion

		#region Private methods

		private void initByName(string fullName)
		{
			FullName = fullName;
			Path = fullName.Substring(0, fullName.LastIndexOf('\\'));
			Name = OriginalName = fullName.Substring(fullName.LastIndexOf('\\') + 1);

			try
			{
				using (var stream = File.OpenRead(fullName))
					ByteCount = stream.Length;
			}
			catch { MissingReadAccess = true; }
		}

		#endregion
	}
}
