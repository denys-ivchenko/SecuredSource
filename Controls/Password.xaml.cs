using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Telesyk.Cryptography;

using oper = Telesyk.SecuredSource.ApplicationOperator;

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

		public Password Password { get; set; }

		public ApplicationMode Mode { get; set; }

		#endregion

		#region Public events

		#endregion

		#region Overridies

		public override void OnApplyTemplate() => onApplyTemplate();

		#endregion

		#region Private methods

		private void init()
		{
			oper.Operator.RegisterForEncryptionProcess(PasswordValue, TextValue);

			TextValue.TextChanged += (s, a) => setPassword(() => PasswordValue.Password = TextValue.Text);
			PasswordValue.PasswordChanged += (s, a) => setPassword(() => TextValue.Text = PasswordValue.Password);
			TextPasswordHintEdit.TextChanged += (s, a) => updatePassword();
		}

		private void algorithmChanged()
		{
			updatePassword();

			if (Mode == ApplicationMode.Encryption)
				ApplicationSettings.Current.SetEncryptionPassword(Password);
			else
				ApplicationSettings.Current.SetDecryptionPassword(Password);
		}

		private void setPassword(Func<string> func)
		{
			if (!_skipChange)
			{
				_skipChange = true;

				func();

				updatePassword();
			}
			else
				_skipChange = false;
		}

		private void updatePassword()
		{
			var algoritm = Mode == ApplicationMode.Encryption ? ApplicationSettings.Current.EncryptionAlgorithm : ApplicationSettings.Current.DecryptionAlgorithm;

			if (Mode == ApplicationMode.Encryption)
			{
				Password = new Password(algoritm, TextValue.Text, TextPasswordHintEdit.Text);

				ApplicationSettings.Current.SetEncryptionPassword(Password);
			}
			else
			{
				Password = new Password(algoritm, TextValue.Text);

				ApplicationSettings.Current.SetDecryptionPassword(Password);
			}

			displayCurrentPasswordLength();
		}

		private void onApplyTemplate()
		{
			base.OnApplyTemplate();

			if (Mode == ApplicationMode.Decryption)
			{
				TextPasswordHintHeader.Visibility = TextPasswordHintEdit.Visibility = Visibility.Collapsed;

				ApplicationSettings.Current.DecryptionAlgorithmChanged += ApplicationSettings_AlgorithmChanged;

				oper.Operator.DeserializePackChanged += (s, a) =>
				{
					if (oper.Operator.DeserializePack != null && oper.Operator.DeserializePack.IsValid && oper.Operator.DeserializePack.FilePack.Password.Hint != null)
					{
						TextPasswordHintHeader.Visibility = TextPasswordHint.Visibility = Visibility.Visible;
						TextPasswordHint.Text = a.Value.FilePack.Password.Hint;
					}
					else
						TextPasswordHintHeader.Visibility = TextPasswordHint.Visibility = Visibility.Collapsed;
				};
			}
			else
				ApplicationSettings.Current.EncryptionAlgorithmChanged += ApplicationSettings_AlgorithmChanged;
		}

		private void ensureMode()
		{
			displayCurrentPasswordLength();
		}

		private void displayCurrentPasswordLength()
		{	
			TextPasswordLength.Text = $"{Password.SymbolCount}";
		}

		private void passwordVisibilityChanged()
		{
			IsPasswordVisible = !IsPasswordVisible;

			if (IsPasswordVisible)
			{
				ImageVisibility.Style = (Style)FindResource("VisiblePasswordImage");
				TextValue.Visibility = Visibility.Visible;
				PasswordValue.Visibility = Visibility.Collapsed;
			}
			else
			{
				ImageVisibility.Style = (Style)FindResource("PasswordImage"); 
				TextValue.Visibility = Visibility.Collapsed;
				PasswordValue.Visibility = Visibility.Visible;
			}
		}

		#region Handlers

		private void Password_PasswordChanged(object sender, RoutedEventArgs e) => setPassword(() => TextValue.Text = PasswordValue.Password);

		private void TextValue_TextChanged(object sender, TextChangedEventArgs e) => setPassword(() => PasswordValue.Password = TextValue.Text);

		private void ImageVisibility_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passwordVisibilityChanged();

		private void ApplicationSettings_AlgorithmChanged(object sender, EventArgs e) => algorithmChanged();

		#endregion

		#endregion
	}
}
