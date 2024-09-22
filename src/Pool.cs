using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace cc_026.Pool
{
    public class CtorParam
    {
        internal CtorParam()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false)]
    public class IsAsynchronousSafe: Attribute
    {
        public IsAsynchronousSafe(bool safe)
        {
            _mSafe = safe;
        }

        private bool _mSafe;
    }
    
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false)]
    public class InitCapacity: Attribute
    {
        public InitCapacity(int capacity)
        {
            _mCapacity = capacity;
        }

        private int _mCapacity;
    }

    public abstract class Pool
    {
        private const int INIT_CAPACITY = 128;
        private readonly Dictionary<uint, PoolObject> _mActiveObj;
        private readonly Queue<PoolObject> _mInactiveObj;
        private readonly CtorParam _mCtor = new CtorParam();
        private readonly object _mLock = new object();
        private readonly PoolSettings _mPoolSettings;

        public struct PoolSettings
        {
            public static readonly PoolSettings DefaultValue = new PoolSettings
            {
                IsAsynchronousSafe = true,
                InitCapacity = INIT_CAPACITY,
            };

            public bool IsAsynchronousSafe;
            public int InitCapacity;
        }

        protected Pool() : this(PoolSettings.DefaultValue) { }

        protected Pool(PoolSettings settings)
        {
            _mPoolSettings = settings;
            _mActiveObj = new Dictionary<uint, PoolObject>(settings.InitCapacity);
            _mInactiveObj = new Queue<PoolObject>(settings.InitCapacity);
            
            PoolManager.Instance.AddPool(this);
        }

        protected abstract PoolObject New(CtorParam ctor);

        public PoolObject Get()
        {
            PoolObject @object;
            if (_mPoolSettings.IsAsynchronousSafe)
            {
                lock (_mLock)
                {
                    @object = GetImpl();
                }
            }
            else
            {
                @object = GetImpl();
            }

            @object.MInPool = false;
            @object.MPool = this;
            @object.MVersion++;

            if (@object.MVersion == 1)
            {
                @object.MId = PoolManager.Instance.GetId();
                @object.OnCtor();
            }

            return @object;
        }

        internal void Put(PoolObject o)
        {
            //if (null == o)
            //    return;

            if (_mPoolSettings.IsAsynchronousSafe)
            {
                lock (_mLock)
                {
                    PutImpl(o);
                }
            }
            else
            {
                PutImpl(o);
            }
        }

        internal void Clear()
        {
            if (_mPoolSettings.IsAsynchronousSafe)
            {
                lock (_mLock)
                {
                    ClearImpl();
                }
            }
            else
            {
                ClearImpl();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PoolObject GetImpl()
        {
            var @object = _mInactiveObj.Count > 0 ? _mInactiveObj.Dequeue() : New(_mCtor);
            _mActiveObj[@object.MId] = @object;
            return @object;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PutImpl(PoolObject o)
        {
            if (o.InPool)
                return;
            o.MInPool = true;
            
            _mActiveObj.Remove(o.MId);
            _mInactiveObj.Enqueue(o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearImpl()
        {
            foreach (var kv in _mActiveObj)
            {
                kv.Value.MPool = null;
            }

            _mActiveObj.Clear();
            foreach (var p in _mInactiveObj)
            {
                p.MPool = null;
            }

            _mInactiveObj.Clear();
        }
    }

    /// <remarks>
    ///     https://www.jb51.net/article/100783.htm
    /// </remarks>
    public class PoolManager
    {
        private PoolManager() { }

        public static PoolManager Instance => Nested._instance;

        private class Nested
        {
            static Nested() { }
            internal static readonly PoolManager _instance = new PoolManager();
        }
        
        private readonly List<Pool> _mPools = new List<Pool>();
        private readonly object _mIdLock = new object();
        private uint _mId = 0;

        public void AddPool(Pool pool)
        {
            lock (_mPools)
            {
                _mPools.Add(pool);
            }
        }

        public void Clear()
        {
            lock (_mPools)
            {
                foreach (var pool in _mPools)
                    pool.Clear();
                
                GC.Collect();
            }
        }

        internal uint GetId()
        {
            uint res = 0;
            lock (_mIdLock)
            {
                _mId++;
                res = _mId;
            }

            return res;
        }
    }
}