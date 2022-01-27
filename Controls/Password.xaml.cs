using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Threading.Tasks;

using Telesyk.Cryptography;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class PasswordControl : UserControl
	{
		#region Private declarations

		private bool _skipChange = false;

		#endregion

		#region Constructors

		public PasswordControl()
		{
			InitializeComponent();

			init();
		}

		#endregion

		#region Public properties

		public bool IsPasswordVisible { get; private set; }

		public string Password { get; private set; }

		public byte[] PasswordBytes { get; private set; }

		public byte[] PasswordHashBytes { get; private set; }

		public string PasswordHash { get; private set; }

		public ApplicationMode Mode { get; set; }

		#endregion

		#region Public events

		public event ValueProcessedEventHandler<string> PasswordChanged;

		public event ValueProcessedEventHandler<string> PasswordHashChanged;

		#endregion

		#region Private methods

		private void init()
		{
			ControlStateOperator.Operator.RegisterForEncryptionProcess(PasswordValue, TextValue);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ensureMode();
		}

		private void ensureMode ()
		{
			displayCurrentPasswordLength();

			if (Mode == ApplicationMode.Encryption)
			{
				TextValue.MaxLength = PasswordValue.MaxLength = ApplicationSettings.Current.RequiredPasswordLength;

				displayRequiredPasswordLength();

				ApplicationSettings.Current.AlgorithmChanged += ApplicationSettings_AlgorithmChanged;
			}
			else
			{
				TextValue.MaxLength = PasswordValue.MaxLength = 32;
				TextLengthSeparator.Text = TextRequiredLength.Text = null;
			}
		}

		private void displayCurrentPasswordLength()
		{	
			TextLength.Text = $"{(Password ?? string.Empty).Length}";
		}

		private void displayRequiredPasswordLength()
		{
			TextValue.MaxLength = PasswordValue.MaxLength = ApplicationSettings.Current.PasswordSize.MaxSize;
			TextRequiredLength.Text = null;

			if (ApplicationSettings.Current.PasswordSize.Skip != 1)
				for (var i = 0; i < ApplicationSettings.Current.PasswordSize.Quantity; i++)
					TextRequiredLength.Text += $"{(i > 0 ? "," : null)}{ApplicationSettings.Current.PasswordSize[i]}";
			else
				TextRequiredLength.Text = $"{ApplicationSettings.Current.PasswordSize.MinSize}-{ApplicationSettings.Current.PasswordSize.MaxSize}";
		}

		private void setPassword(Func<string> func)
		{
			if (!_skipChange)
			{
				_skipChange = true;

				Password = func();

				PasswordBytes = Encoding.UTF8.GetBytes(Password);
				PasswordHashBytes = SymmetricEncryptor.ComputeHash(PasswordBytes);
				PasswordHash = Convert.ToBase64String(PasswordBytes);

				displayCurrentPasswordLength();

				if (Mode == ApplicationMode.Encryption)
					ApplicationSettings.Current.PasswordLength = Password.Length;

				PasswordChanged?.Invoke(this, new ValueProcessedEventArgs<string>(Password));
				PasswordHashChanged?.Invoke(this, new ValueProcessedEventArgs<string>(PasswordHash));
			}
			else
				_skipChange = false;
		}

		private void passwordVisibilityChanged()
		{
			IsPasswordVisible = !IsPasswordVisible;

			if (IsPasswordVisible)
			{
				ImageVisibility.Style = (Style)FindResource("UnvisiblePasswordImage");
				TextValue.Visibility = Visibility.Visible;
				PasswordValue.Visibility = Visibility.Collapsed;
			}
			else
			{
				ImageVisibility.Style = (Style)FindResource("UnvisiblePasswordImage"); ;
				TextValue.Visibility = Visibility.Collapsed;
				PasswordValue.Visibility = Visibility.Visible;
			}
		}

		#region Handlers

		private void Password_PasswordChanged(object sender, RoutedEventArgs e) => setPassword(() => TextValue.Text = PasswordValue.Password);

		private void TextValue_TextChanged(object sender, TextChangedEventArgs e) => setPassword(() => PasswordValue.Password = TextValue.Text);

		private void ImageVisibility_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passwordVisibilityChanged();

		private void ApplicationSettings_AlgorithmChanged(object sender, EventArgs e) => displayRequiredPasswordLength();

		#endregion

		#endregion
	}
}
