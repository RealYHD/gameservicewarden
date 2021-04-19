using System;

namespace GameServiceWarden.Core.Tests.Collection
{
    public class FakeDisposable : IDisposable
    {
        private string value;
        private bool disposedValue;

        public FakeDisposable(string value)
        {
            this.value = value;
        }
        
        public bool IsDisposed() {
            return disposedValue;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}