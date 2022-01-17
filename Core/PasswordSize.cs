using System;
using System.Collections;
using System.Collections.ObjectModel;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource
{
	public struct PasswordSize : IEnumerable
	{
		#region Private fields

		private int[] _sizes;
		private ReadOnlyCollection<int> _sizesReadOnly;

		#endregion

		#region Constructors

		public PasswordSize(CryptoAlgorithm name, int minSize, int maxSize, int skip)
		{
			Algorithm = name;
			MinSize = minSize;
			MaxSize = maxSize;
			Skip = skip;

			_sizes = null;
			_sizesReadOnly = null;
		}

		#endregion

		#region Public properties

		public int this[int index] => Sizes[index];

		public CryptoAlgorithm Algorithm { get; private set; }

		public string Key => Algorithm.ToString().ToString().ToUpper();

		public int MinSize { get; private set; }

		public int MaxSize { get; private set; }

		public int Skip { get; private set; }

		public int Quantity => MaxSize != MinSize ? (MaxSize - MinSize) / Skip + 1 : 1;

		public ReadOnlyCollection<int> Sizes
		{
			get
			{
				if (_sizes == null)
				{
					_sizes = _createSizes();
					_sizesReadOnly = new ReadOnlyCollection<int>(_sizes);
				}

				return _sizesReadOnly;
			}
		}

		#endregion

		#region Public methods

		public bool IsValid(int size) => _isValid(size);

		#endregion

		#region Interface implementations

		public IEnumerator GetEnumerator()
		{
			return _sizes.GetEnumerator();
		}

		#endregion

		#region Private methods

		private bool _isValid(int size) => Sizes.Contains(size);

		private int[] _createSizes()
		{
			var sizes = new int[Quantity];

			for (var i = 0; i < Quantity; i++)
				sizes[i] = MinSize + i * Skip;

			return sizes;
		}

		#endregion
	}
}
