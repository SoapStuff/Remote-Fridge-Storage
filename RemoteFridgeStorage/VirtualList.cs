using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RemoteFridgeStorage
{
    /// <summary>
    /// Unused virtual list to wrap some lists.
    /// Sadly the fridge items is a list instead of Ilist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VirtualList<T> : IList<T>
    {
        private readonly List<IList<T>> _lists;

        public VirtualList(List<IList<T>> lists)
        {
            _lists = lists;
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<T> list = new List<T>();
            foreach (var list1 in _lists)
            {
                list.AddRange(list1);
            }

            return list.GetEnumerator();
        }

        
        public void Add(T item)
        {
            _lists[_lists.Count - 1].Add(item);
        }

        public void Clear()
        {
            foreach (var list in _lists)
            {
                list.Clear();
            }
        }

        public bool Contains(T item)
        {
            return _lists.Any(list => list.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < arrayIndex + Count; i++)
            {
                array[i] = this[i - arrayIndex];
            }
        }

        public bool Remove(T item)
        {
            foreach (var list in _lists)
            {
                if (list.Remove(item)) return true;
            }

            return false;
        }

        public int Count => CalcCount();

        private int CalcCount()
        {
            int count = 0;
            foreach (var list in _lists)
            {
                count += list.Count;
            }

            return count;
        }

        public bool IsReadOnly { get; } = false;

        public int IndexOf(T item)
        {
            int searched = 0;
            foreach (var list in _lists)
            {
                var ind = list.IndexOf(item);
                if (ind != -1)
                {
                    return searched + ind;
                }

                searched += list.Count;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            ListIndexPair pair = LocalIndex(index);
            pair.List.Insert(pair.Index, item);
        }

        public void RemoveAt(int index)
        {
            ListIndexPair pair = LocalIndex(index);
            pair.List.RemoveAt(pair.Index);
        }

        public T this[int index]
        {
            get
            {
                ListIndexPair pair = LocalIndex(index);
                return pair.List[pair.Index];
            }
            set
            {
                ListIndexPair pair = LocalIndex(index);
                pair.List[pair.Index] = value;
            }
        }

        private ListIndexPair LocalIndex(int index)
        {
            if (index < 0 | index >= Count)
            {
                throw new ArgumentOutOfRangeException($"Index was {index} but size was {Count}");
            }

            int iterateIndex = 0;
            while (iterateIndex < _lists.Count)
            {
                if (index < _lists[iterateIndex].Count)
                {    
                    return new ListIndexPair(_lists[iterateIndex],index);
                }

                index -= _lists.Count;

                iterateIndex++;
            }
            //It should not reach this.
            return new ListIndexPair(null,-1);
        }

        private struct ListIndexPair
        {
            public ListIndexPair(IList<T> list1, int i)
            {
                List = list1;
                Index = i;
            }

            public IList<T> List { get; }
            public int Index { get; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
