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

		private List<UIElement> _controls = new List<UIElement>();
		private List<IEnablingState> _containers = new List<IEnablingState>();

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

		//public void RegisterForEncriptionProcess(IEnablingState container) => registerForEncriptionProcess(container);

		#endregion

		#region Internal methods

		internal void DisableForEncryptionProcess() => disableForEncryptionProcess();

		internal void EnableForEncryptionProcess() => enableForEncryptionProcess();

		#endregion

		#region Private methods

		private void registerForEncryptionProcess(params UIElement[] controls)
		{
			foreach(var control in controls)
				if (control is IEnablingState && !_containers.Contains(control as IEnablingState))
					_containers.Add(control as IEnablingState);
				else if (!(control is IEnablingState) && !_controls.Contains(control))
					_controls.Add(control);
		}

		private void unregisterForEncryptionProcess(params UIElement[] controls)
		{
			foreach (var control in controls)
				if (_containers.Contains(control as IEnablingState))
					_containers.Remove(control as IEnablingState);
				else if (_controls.Contains(control))
					_controls.Remove(control);
		}

		private void disableForEncryptionProcess()
		{
			_controls.ForEach(c => c.IsEnabled = false);
			_containers.ForEach(c => c.SetEnablingState(true));

			IsInProcess = true;
		}

		private void enableForEncryptionProcess()
		{
			_controls.ForEach(c => c.IsEnabled = true);
			_containers.ForEach(c => c.SetEnablingState(false));

			IsInProcess = false;
		}

		#endregion
	}
}
