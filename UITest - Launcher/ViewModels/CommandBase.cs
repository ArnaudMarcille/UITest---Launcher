using System;

namespace UITest___Launcher.ViewModels
{
    /// <summary>
    /// Command base for ICommand implementation
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// Is the command enabled ?
        /// </summary>
        internal bool isEnabled;

        /// <summary>
        /// Can execute changed event
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raise the can execute event
        /// </summary>
        protected void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
