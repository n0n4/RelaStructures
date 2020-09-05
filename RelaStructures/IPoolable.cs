using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures
{
    public interface IPoolable
    {
        void SetPoolIndex(int index);
        int GetPoolIndex();
    }
}
