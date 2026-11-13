using System;
using System.Collections.Generic;

namespace OutOfOffice.Core.State
{
    /// <summary>
    /// Küçük, genel amaçlı durum makinesi. Her durum için giriş/çıkış davranışı ve
    /// izin verilen geçişler tanımlanır; izin verilmeyen geçiş sessizce reddedilir.
    /// </summary>
    public sealed class StateMachine<TState> where TState : struct, Enum
    {
        sealed class Node
        {
            public Action OnEnter;
            public Action OnExit;
            public readonly HashSet<TState> Next = new HashSet<TState>();
        }

        readonly Dictionary<TState, Node> _nodes = new Dictionary<TState, Node>();

        public TState Current { get; private set; }

        /// <summary>(önceki, yeni) durum.</summary>
        public event Action<TState, TState> Changed;

        public StateMachine(TState initial)
        {
            Current = initial;
        }

        public StateMachine<TState> Configure(TState state, Action onEnter = null, Action onExit = null, params TState[] next)
        {
            var node = new Node { OnEnter = onEnter, OnExit = onExit };
            for (int i = 0; i < next.Length; i++) node.Next.Add(next[i]);
            _nodes[state] = node;
            return this;
        }

        public bool CanGo(TState to)
        {
            return _nodes.TryGetValue(Current, out var node) && node.Next.Contains(to);
        }

        public bool TryGo(TState to)
        {
            if (!CanGo(to)) return false;

            var from = Current;
            _nodes[from].OnExit?.Invoke();
            Current = to;
            if (_nodes.TryGetValue(to, out var target)) target.OnEnter?.Invoke();
            Changed?.Invoke(from, to);
            return true;
        }
    }
}
