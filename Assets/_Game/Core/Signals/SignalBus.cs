using System;
using System.Collections.Generic;
using UnityEngine;

namespace OutOfOffice.Core.Signals
{
    /// <summary>
    /// Tipli sinyal bus'ı. Dinleyici listesi "copy-on-write" dizidir:
    /// Fire sırasında bellek ayırmaz, dinleyici Fire içinde aboneliğini bitirebilir.
    /// </summary>
    public sealed class SignalBus : ISignalBus
    {
        readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        public void Fire<T>(in T signal) where T : struct
        {
            if (_channels.TryGetValue(typeof(T), out var channel))
                ((Channel<T>)channel).Invoke(in signal);
        }

        public void Fire<T>() where T : struct
        {
            Fire(default(T));
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!_channels.TryGetValue(typeof(T), out var boxed))
            {
                boxed = new Channel<T>();
                _channels.Add(typeof(T), boxed);
            }

            var channel = (Channel<T>)boxed;
            channel.Add(handler);
            return new Subscription<T>(channel, handler);
        }

        sealed class Channel<T> where T : struct
        {
            Action<T>[] _handlers = Array.Empty<Action<T>>();

            public void Add(Action<T> handler)
            {
                var next = new Action<T>[_handlers.Length + 1];
                Array.Copy(_handlers, next, _handlers.Length);
                next[next.Length - 1] = handler;
                _handlers = next;
            }

            public void Remove(Action<T> handler)
            {
                int index = Array.IndexOf(_handlers, handler);
                if (index < 0) return;

                var next = new Action<T>[_handlers.Length - 1];
                Array.Copy(_handlers, 0, next, 0, index);
                Array.Copy(_handlers, index + 1, next, index, next.Length - index);
                _handlers = next;
            }

            public void Invoke(in T signal)
            {
                var handlers = _handlers;
                for (int i = 0; i < handlers.Length; i++)
                {
                    try
                    {
                        handlers[i](signal);
                    }
                    catch (Exception e)
                    {
                        // Bir dinleyicinin hatası diğerlerini susturmasın.
                        Debug.LogException(e);
                    }
                }
            }
        }

        sealed class Subscription<T> : IDisposable where T : struct
        {
            Channel<T> _channel;
            Action<T> _handler;

            public Subscription(Channel<T> channel, Action<T> handler)
            {
                _channel = channel;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_channel == null) return;
                _channel.Remove(_handler);
                _channel = null;
                _handler = null;
            }
        }
    }
}
