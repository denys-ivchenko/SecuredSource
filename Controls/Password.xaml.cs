using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Threading;
using System.Threading.Tasks;

namespace Telesyk.SecuredSource.UI.Controls
{
	public partial class PasswordControl : UserControl
	{
		#region Private declarations

		private bool _skipChange = false;
		private ControlMode _mode = ControlMode.Encrypt;

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

		public ControlMode Mode 
		{ 
			get => _mode; 
			set
			{
				_mode = value;

				ensureMode();
			}
		}

		#endregion

		#region Private methods

		private void init()
		{
			ControlStateOperator.Operator.RegisterForEncryptionProcess(PasswordValue, TextValue);
		}

		private void ensureMode ()
		{
			displayCurrentPasswordLength();

			if (Mode == ControlMode.Encrypt)
			{
				TextValue.MaxLength = PasswordValue.MaxLength = ApplicationSettings.Current.RequiredPasswordLength;

				displayRequiredPasswordLength();

				ApplicationSettings.Current.AlgorithmChanged += ApplicationSettings_AlgorithmChanged;
			}
			else
			{
				ApplicationSettings.Current.AlgorithmChanged -= ApplicationSettings_AlgorithmChanged;

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
				
				displayCurrentPasswordLength();

				if (Mode == ControlMode.Encrypt)
					ApplicationSettings.Current.PasswordLength = Password.Length;
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
