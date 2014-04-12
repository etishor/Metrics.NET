using System;

namespace Metrics.Utils
{
    /// <summary>
    /// Utility structure that executed an action when it is disposed;
    /// </summary>
    public struct DisposableAction : IDisposable
    {
        private readonly Action action;
        private bool disposed;

        /// <summary>
        /// Creates a new instance that will execute <paramref name="action"/> when it is disposed
        /// </summary>
        /// <param name="action">Action to execute when struct is disposed.</param>
        public DisposableAction(Action action)
        {
            this.action = action;
            this.disposed = false;
        }

        /// <summary>
        /// Executed the action this instance has been constructed with.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.action();
            }
            this.disposed = true;
        }
    }
}
