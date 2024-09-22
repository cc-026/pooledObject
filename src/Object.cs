using System;

namespace cc_026.Pool
{
    public interface IRef
    {
        void SetObject(PoolObject o);
    }

    public abstract class PoolObject
    {
        protected static readonly NullReferenceException NullRef = new NullReferenceException("Null or In Pool");

        protected static readonly ArgumentNullException NullArg = new ArgumentNullException($"CtorParam", "Null Constructor Arg");
        
        internal uint MId;
        internal uint MVersion;
        internal bool MInPool;
        internal Pool? MPool;

        public uint Version => MVersion;
        public bool InPool => MInPool;
        public Pool? Pool => MPool;

        protected PoolObject(CtorParam ctor)
        {
            if (null == ctor) throw NullArg;
        }
        
        protected internal virtual void OnCtor() { }
        protected internal abstract void OnGet(object param);
        protected internal abstract void OnPut();

        public void Put()
        {
            MPool?.Put(this);
            MInPool = true;
        }
    }
}