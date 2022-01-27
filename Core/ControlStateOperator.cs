using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Telesyk.SecuredSource
{
	public sealed class ControlStateOperator
	{
		#region Private declarations

		private static Lazy<ControlStateOperator> _operator = new Lazy<ControlStateOperator>(() => new ControlStateOperator());

		private List<UIElement> _encryptionControls = new List<UIElement>();
		private List<IEnablingState> _encryptionContainers = new List<IEnablingState>();

		//private List<UIElement> _decryptionControls = new List<UIElement>();
		//private List<IEnablingState> _decryptionContainers = new List<IEnablingState>();

		#endregion

		#region Constructors

		private ControlStateOperator()
		{

		}

		#endregion

		#region Public properties

		public static ControlStateOperator Operator => _operator.Value;

		public bool IsInProcess { get; private set; }

		#endregion

		#region Public methods

		public void RegisterForEncryptionProcess(params UIElement[] controls) => registerForEncryptionProcess(controls);

		public void UnregisterForEncryptionProcess(params UIElement[] controls) => unregisterForEncryptionProcess(controls);

		//public void RegisterForDecryptionProcess(params UIElement[] controls) => registerForDecryptionProcess(controls);

		//public void UnregisterForDecryptionProcess(params UIElement[] controls) => unregisterForDecryptionProcess(controls);

		//public void RegisterForEncriptionProcess(IEnablingState container) => registerForEncriptionProcess(container);

		#endregion

		#region Internal methods

		internal void DisableForEncryptionProcess() => disableForEncryptionProcess();

		internal void EnableForEncryptionProcess() => enableForEncryptionProcess();

		//internal void DisableForDecryptionProcess() => disableForDecryptionProcess();

		//internal void EnableForDecryptionProcess() => enableForDecryptionProcess();

		#endregion

		#region Private methods

		private void registerForEncryptionProcess(params UIElement[] controls)
		{
			foreach(var control in controls)
				if (control is IEnablingState && !_encryptionContainers.Contains(control as IEnablingState))
					_encryptionContainers.Add(control as IEnablingState);
				else if (!(control is IEnablingState) && !_encryptionControls.Contains(control))
					_encryptionControls.Add(control);
		}

		private void unregisterForEncryptionProcess(params UIElement[] controls)
		{
			foreach (var control in controls)
				if (_encryptionContainers.Contains(control as IEnablingState))
					_encryptionContainers.Remove(control as IEnablingState);
				else if (_encryptionControls.Contains(control))
					_encryptionControls.Remove(control);
		}

		private void disableForEncryptionProcess()
		{
			_encryptionControls.ForEach(c => c.IsEnabled = false);
			_encryptionContainers.ForEach(c => c.SetEnablingState(true));

			IsInProcess = true;
		}

		private void enableForEncryptionProcess()
		{
			_encryptionControls.ForEach(c => c.IsEnabled = true);
			_encryptionContainers.ForEach(c => c.SetEnablingState(false));

			IsInProcess = false;
		}

		//private void registerForDecryptionProcess(params UIElement[] controls)
		//{
		//	foreach(var control in controls)
		//		if (control is IEnablingState && !_decryptionContainers.Contains(control as IEnablingState))
		//			_decryptionContainers.Add(control as IEnablingState);
		//		else if (!(control is IEnablingState) && !_decryptionControls.Contains(control))
		//			_decryptionControls.Add(control);
		//}

		//private void unregisterForDecryptionProcess(params UIElement[] controls)
		//{
		//	foreach (var control in controls)
		//		if (_decryptionContainers.Contains(control as IEnablingState))
		//			_decryptionContainers.Remove(control as IEnablingState);
		//		else if (_decryptionControls.Contains(control))
		//			_decryptionControls.Remove(control);
		//}

		//private void disableForDecryptionProcess()
		//{
		//	_decryptionControls.ForEach(c => c.IsEnabled = false);
		//	_decryptionContainers.ForEach(c => c.SetEnablingState(true));

		//	IsInProcess = true;
		//}

		//private void enableForDecryptionProcess()
		//{
		//	_decryptionControls.ForEach(c => c.IsEnabled = true);
		//	_decryptionContainers.ForEach(c => c.SetEnablingState(false));

		//	IsInProcess = false;
		//}

		#endregion
	}
}
