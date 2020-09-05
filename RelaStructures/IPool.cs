using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{
    public interface IPool<T> where T : IPoolable
    {
        T Request();
        void Return(T obj);
        void Clear();
    }
}
