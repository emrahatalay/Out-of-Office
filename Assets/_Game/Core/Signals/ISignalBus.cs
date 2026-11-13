using System;

namespace OutOfOffice.Core.Signals
{
    /// <summary>
    /// Sistemler arası tek haberleşme kanalı. Yayınlayan ve dinleyen birbirini tanımaz,
    /// ikisi de yalnızca sinyal tipini bilir.
    /// </summary>
    public interface ISignalBus
    {
        void Fire<T>(in T signal) where T : struct;

        /// <summary>Veri taşımayan sinyaller için kısa yol.</summary>
        void Fire<T>() where T : struct;

        /// <summary>Dönen nesne Dispose edildiğinde abonelik biter.</summary>
        IDisposable Subscribe<T>(Action<T> handler) where T : struct;
    }
}
