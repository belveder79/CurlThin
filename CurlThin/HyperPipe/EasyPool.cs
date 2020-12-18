using System;
using System.Collections.Concurrent;
#if !UNITY
using System.Collections.Immutable;
#else
using System.Collections.Generic;
#endif
using CurlThin.SafeHandles;

namespace CurlThin.HyperPipe
{
    internal class EasyPool<T> : IDisposable
    {
        private readonly ConcurrentDictionary<SafeEasyHandle, T> _busy;
        private readonly ConcurrentBag<SafeEasyHandle> _free;
#if !UNITY
        private readonly ImmutableDictionary<IntPtr, SafeEasyHandle> _pool;
#else
        private readonly Dictionary<IntPtr, SafeEasyHandle> _pool;
#endif

        public EasyPool(int size)
        {
            Size = size;
#if !UNITY
            var poolBuilder = ImmutableDictionary.CreateBuilder<IntPtr, SafeEasyHandle>(
                new IntPtrEqualityComparer());

            for (var i = 0; i < size; i++)
            {
                var handle = CurlNative.Easy.Init();
                poolBuilder.Add(handle.DangerousGetHandle(), handle);
            }

            _pool = poolBuilder.ToImmutable();
            _free = new ConcurrentBag<SafeEasyHandle>(_pool.Values);
            _busy = new ConcurrentDictionary<SafeEasyHandle, T>();
#else
            _pool = new Dictionary<IntPtr, SafeEasyHandle>(new IntPtrEqualityComparer());
            for (var i = 0; i < size; i++)
            {
                var handle = CurlNative.Easy.Init();
                _pool.Add(handle.DangerousGetHandle(), handle);
            }
            _free = new ConcurrentBag<SafeEasyHandle>(_pool.Values);
            _busy = new ConcurrentDictionary<SafeEasyHandle, T>();
#endif
        }

        /// <summary>
        ///     Size of requests pool.
        /// </summary>
        public int Size { get; }

        /// <summary>
        ///     Closes native cURL easy handles.
        /// </summary>
        public void Dispose()
        {
            foreach (var easyHandle in _pool)
            {
                CurlNative.Easy.Cleanup(easyHandle.Key);
            }
        }

        /// <summary>
        ///     Retruns wrapped <see cref="SafeEasyHandle" /> for given dangerous handle pointer.
        /// </summary>
        public SafeEasyHandle GetSafeHandleFromPtr(IntPtr handle)
        {
            return _pool[handle];
        }

        /// <summary>
        ///     Try get unused cURL easy handle.
        /// </summary>
        /// <returns>TRUE if successful, FALSE if all handles are busy.</returns>
        public bool TryTakeFree(out SafeEasyHandle easy)
        {
            return _free.TryTake(out easy);
        }

        /// <summary>
        ///     Change status of cURL easy handle from busy to free.
        /// </summary>
        public void Free(SafeEasyHandle easy)
        {
            _free.Add(easy);
        }

        /// <summary>
        ///     Assigns request context to given easy handle.
        /// </summary>
        public void AssignContext(SafeEasyHandle easy, T context)
        {
            if (!_busy.TryAdd(easy, context))
            {
                throw new Exception("Trying to assign request context to handle that already has its context.");
            }
        }

        /// <summary>
        ///     Retrieves request context for given easy handle.
        /// </summary>
        public void GetAssignedContext(SafeEasyHandle easy, out T context)
        {
            if (!_busy.TryGetValue(easy, out context))
            {
                throw new Exception("Cannot find request context for given easy handle.");
            }
        }

        /// <summary>
        ///     Unassigns request context for given easy handle.
        /// </summary>
        public void UnassignContext(SafeEasyHandle easy)
        {
            if (!_busy.TryRemove(easy, out T _))
            {
                throw new Exception("Given handle doesn't have stored any request context.");
            }
        }
    }
}