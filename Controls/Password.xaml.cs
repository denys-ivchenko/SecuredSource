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

		private string _password = null;
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

		public int PasswordLength { get; private set; }

		public bool IsPasswordVisible { get; private set; }

		public string Password
		{
			get { return _password; }
			private set { TextValue.Text = value; }
		}

		#endregion

		#region Events

		public event EventHandler PasswordChanged;

		#endregion

		#region Private methods

		private void init()
		{
			TextValue.MaxLength = PasswordValue.MaxLength = ApplicationSettings.Current.PasswordLength;

			setCurrentPasswordLength();
			setRequiredPasswordLength();

			ApplicationSettings.Current.AlgorythmChanged += ApplicationSettings_AlgorythmChanged;
		}

		private void setCurrentPasswordLength()
		{	
			TextQuantity.Text = $"{PasswordLength = (Password ?? string.Empty).Length}";

			if (PasswordChanged != null)
				PasswordChanged(this, EventArgs.Empty);
		}

		private void setRequiredPasswordLength()
		{
			TextRequired.Text = $"{TextValue.MaxLength = PasswordValue.MaxLength = ApplicationSettings.Current.PasswordLength}";
		}

		private void passwordValueChanged()
		{
			if (!_skipChange)
			{
				_skipChange = true;
				_password = TextValue.Text = PasswordValue.Password;

				setCurrentPasswordLength();
			}
			else
				_skipChange = false;
		}

		private void textValueChanged()
		{
			if (!_skipChange)
			{
				_skipChange = true;
				_password = PasswordValue.Password = TextValue.Text;

				setCurrentPasswordLength();
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

		private void Password_PasswordChanged(object sender, RoutedEventArgs e) => passwordValueChanged();

		private void TextValue_TextChanged(object sender, TextChangedEventArgs e) => textValueChanged();

		private void ImageVisibility_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passwordVisibilityChanged();

		private void ApplicationSettings_AlgorythmChanged(object sender, EventArgs e) => setRequiredPasswordLength();

		#endregion

		#endregion
	}
}
