using System;
using System.Collections.Generic;

namespace OutOfOffice.Core.Signals
{
    /// <summary>Bir sistemin tüm aboneliklerini toplar, tek Dispose ile hepsini bitirir.</summary>
    public sealed class SubscriptionBag : IDisposable
    {
        readonly List<IDisposable> _items = new List<IDisposable>();

        public void Add(IDisposable subscription)
        {
            _items.Add(subscription);
        }

        public void Dispose()
        {
            for (int i = 0; i < _items.Count; i++) _items[i].Dispose();
            _items.Clear();
        }
    }

    public static class SubscriptionExtensions
    {
        public static void AddTo(this IDisposable subscription, SubscriptionBag bag)
        {
            bag.Add(subscription);
        }
    }
}
