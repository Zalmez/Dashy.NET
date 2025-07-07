using System;
using System.Collections.Generic;

namespace Dashy.Net.Web.Helpers
{
    public class EventSubscriptionManager : IDisposable
    {
        private readonly List<Action> _unsubscriptions = new();
        private bool _isDisposed;

        public void AddSubscription(Action unsubscribeAction)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(EventSubscriptionManager));
            _unsubscriptions.Add(unsubscribeAction);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            foreach (var unsubscribe in _unsubscriptions)
            {
                try
                {
                    unsubscribe();
                }
                catch (Exception ex)
                {
                    // Log the exception if needed
                    _logger.LogError(ex, "Error during event unsubscription.");
                }
            }

            _unsubscriptions.Clear();
            _isDisposed = true;
        }
    }
}