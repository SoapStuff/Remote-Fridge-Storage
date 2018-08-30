using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RemoteFridgeStorage
{
    internal class RemoteFridgeCraftingPage : CraftingPage
    {
        private readonly FridgeHandler _fridgeHandler;

        public RemoteFridgeCraftingPage(CraftingPage page, FridgeHandler fridgeHandler) :
            base(page.xPositionOnScreen, page.yPositionOnScreen, page.width, page.height, true)
        {
            _fridgeHandler = fridgeHandler;
        }

        protected override IList<Item> fridge()
        {
            return new VirtualList(_fridgeHandler);
        }
    }

    internal class VirtualList : IList<Item>
    {
        private readonly List<Chest> _chests;

        public VirtualList(FridgeHandler fridgeHandler)
        {
            _chests = fridgeHandler.Chests.ToList();
            if (Game1.currentLocation is FarmHouse farm) _chests.Add(farm.fridge.Value);
        }

        public IEnumerator<Item> GetEnumerator()
        {
            var list = new List<Item>();
            foreach (var chest in _chests)
            {
                list.AddRange(chest.items);
            }

            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Item item)
        {
            if (_chests.Count == 0) _chests.Add(new Chest());
            _chests[_chests.Count - 1].items.Add(item);
        }

        public void Clear()
        {
            foreach (var chest in _chests)
            {
                chest.items.Clear();
            }

            _chests.Clear();
        }

        public bool Contains(Item item)
        {
            return _chests.Any(c => c.items.Contains(item));
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < arrayIndex + Count; i++)
            {
                array[i] = this[i - arrayIndex];
            }
        }

        public bool Remove(Item item)
        {
            foreach (var chest in _chests)
            {
                if (chest.items.Remove(item)) return true;
            }

            return false;
        }

        public int Count => CalcCount();

        private int CalcCount()
        {
            var count = 0;
            foreach (var chest in _chests)
            {
                count += chest.items.Count;
            }

            return count;
        }

        public bool IsReadOnly { get; } = false;

        public int IndexOf(Item item)
        {
            var searched = 0;
            foreach (var chest in _chests)
            {
                var ind = chest.items.IndexOf(item);
                if (ind != -1)
                {
                    return searched + ind;
                }

                searched += chest.items.Count;
            }

            return -1;
        }

        public void Insert(int index, Item item)
        {
            var pair = LocalIndex(index);
            pair.Chest.items.Insert(pair.Index, item);
        }

        public void RemoveAt(int index)
        {
            var pair = LocalIndex(index);
            pair.Chest.items.RemoveAt(pair.Index);
        }

        public Item this[int index]
        {
            get
            {
                var pair = LocalIndex(index);
                return pair.Chest.items[pair.Index];
            }
            set
            {
                var pair = LocalIndex(index);
                pair.Chest.items[pair.Index] = value;
            }
        }

        private ListIndexPair LocalIndex(int index)
        {
            if (index < 0 | index >= Count)
            {
                throw new ArgumentOutOfRangeException($"Index was {index} but size was {Count}");
            }

            var iterateIndex = 0;
            while (iterateIndex < _chests.Count)
            {
                if (index < _chests[iterateIndex].items.Count)
                {
                    return new ListIndexPair(_chests[iterateIndex], index);
                }

                index -= _chests[iterateIndex].items.Count;

                iterateIndex++;
            }

            //It should not reach this.
            return new ListIndexPair(null, -1);
        }

        private struct ListIndexPair
        {
            public ListIndexPair(Chest list1, int i)
            {
                Chest = list1;
                Index = i;
            }

            public Chest Chest { get; }
            public int Index { get; }
        }
    }
}