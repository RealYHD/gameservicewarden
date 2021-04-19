using GameServiceWarden.Core.Collection;
using Xunit;

namespace GameServiceWarden.Core.Tests.Collection
{
    public class LRUCacheTest
    {
        [Fact]
        public void Use_SufficientSpace_StoredUsed()
        {
            //Given
            string data = "data";
            LRUCache<int, string> cache = new LRUCache<int, string>(10);
            //When
            cache.Use(0, () => data);
            //Then
            Assert.Same(data, cache.Use(0, () => "other"));
        }

        [Fact]
        public void Use_InsufficientSpace_LastUsedRemoved()
        {
            //Given
            string[] data = new string[] { "a", "b", "c" };
            LRUCache<int, string> cache = new LRUCache<int, string>(2);
            //When
            for (int i = 0; i < data.Length; i++)
            {
                cache.Use(i, () => data[i]);
            }
            //Then
            Assert.Contains("c", cache);
            Assert.Contains("b", cache);
            Assert.DoesNotContain("a", cache);
        }

        [Fact]
        public void IsCached_CachedData_True()
        {
            //Given
            string[] data = new string[] { "a", "b", "c" };
            LRUCache<int, string> cache = new LRUCache<int, string>(2);
            //When
            for (int i = 0; i < data.Length; i++)
            {
                cache.Use(i, () => data[i]);
            }
            //Then
            Assert.True(cache.IsCached(2));
            Assert.True(cache.IsCached(1));
            Assert.False(cache.IsCached(0));
        }

        [Fact]
        public void Use_CleanupDelSet_DataChanged()
        {
            //Given
            FakeDisposable[] data = new FakeDisposable[] { new FakeDisposable("a"), new FakeDisposable("b"), new FakeDisposable("c")};
            LRUCache<int, FakeDisposable> cache = new LRUCache<int, FakeDisposable>(2, (d) => d.Dispose());
            //When
            for (int i = 0; i < data.Length; i++)
            {
                cache.Use(i, () => data[i]);
            }
            //Then
            Assert.True(data[0].IsDisposed());
            Assert.False(data[1].IsDisposed());
            Assert.False(data[2].IsDisposed());
        }

    }
}