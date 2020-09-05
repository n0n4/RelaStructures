using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaStructures.UT
{
    [TestClass]
    public class ListIdPoolTests
    {
        [TestMethod]
        public void Req10Test()
        {
            ListIdPool<TestPoolable> pool = new ListIdPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void ReqTooManyTest()
        {
            ListIdPool<TestPoolable> pool = new ListIdPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 15; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);
        }

        [TestMethod]
        public void ReqAndRemoveTest()
        {
            ListIdPool<TestPoolable> pool = new ListIdPool<TestPoolable>(10,
                () => { return new TestPoolable(); },
                (obj) => { obj.Clear(); });

            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
                ids.Add(pool.RequestId());

            foreach (int id in ids)
                Assert.IsFalse(id == -1);

            for (int i = 0; i < pool.Count; i++)
                pool.Values[i].X += i;

            foreach (int id in ids)
                Assert.AreEqual(id, pool.Values[pool.IdsToIndices[id]].X);

            for (int i = 5; i < 8; i++)
                pool.ReturnId(ids[i]); // return 5, 6, and 7

            Assert.IsTrue(pool.Count == 7); // down from 10
            Assert.IsTrue(pool.Values[pool.IdsToIndices[ids[9]]].X == 9); // verify that accessing id9 still works

            // request 3 new structs, to fill back up to 10
            List<int> newids = new List<int>();
            for (int i = 0; i < 3; i++)
                newids.Add(pool.RequestId());

            Assert.IsTrue(pool.Count == 10); // back to 10
            foreach (int id in newids)
                Assert.AreEqual(0, pool.Values[pool.IdsToIndices[id]].X); // verify that returned structs were cleared
        }
    }
}
