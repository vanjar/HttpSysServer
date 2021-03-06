// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NETSTANDARD1_3 // TODO: Temp copy. Remove once we target net46.
using System;
namespace System.Threading
{
    internal sealed class PreAllocatedOverlapped : IDisposable, IDeferredDisposable
    {
        internal readonly ThreadPoolBoundHandleOverlapped _overlapped;
        private DeferredDisposableLifetime<PreAllocatedOverlapped> _lifetime;
        public PreAllocatedOverlapped(IOCompletionCallback callback, object state, object pinData)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            this._overlapped = new ThreadPoolBoundHandleOverlapped(callback, state, pinData, this);
        }
        internal bool AddRef()
        {
            return this._lifetime.AddRef(this);
        }
        internal void Release()
        {
            this._lifetime.Release(this);
        }
        public void Dispose()
        {
            this._lifetime.Dispose(this);
            GC.SuppressFinalize(this);
        }
        ~PreAllocatedOverlapped()
        {
            if (!Environment.HasShutdownStarted)
            {
                this.Dispose();
            }
        }
        unsafe void IDeferredDisposable.OnFinalRelease(bool disposed)
        {
            if (disposed)
            {
                Overlapped.Free(this._overlapped._nativeOverlapped);
                return;
            }
            this._overlapped._boundHandle = null;
            this._overlapped._completed = false;
            *this._overlapped._nativeOverlapped = default(NativeOverlapped);
        }
    }
}
#endif