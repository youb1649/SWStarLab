using System.Collections.Generic;

namespace MNF
{
    public class TObjectPool<TObject> where TObject : class, new()
    {
        LinkedList<TObject> pool;

        public int Count
        {
            get { return pool.Count; }
        }

        public TObjectPool()
        {
            pool = new LinkedList<TObject>();
        }

        public TObject Alloc()
        {
            TObject tObject = default(TObject);
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    tObject = pool.First.Value;
                    pool.RemoveFirst();
                }
                else
                {
                    tObject = new TObject();
                }
            }
            return tObject;
        }

        public void Free(TObject tObject)
        {
            lock (pool)
            {
                pool.AddLast(tObject);
            }
        }
    }
}
