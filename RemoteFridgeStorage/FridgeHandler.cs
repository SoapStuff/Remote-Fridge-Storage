using System.Collections.Generic;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace RemoteFridgeStorage
{
    /// <summary>
    /// Takes care of adding and removing elements for crafting.
    /// </summary>
    internal class FridgeHandler
    {
        private readonly List<ChestIndex> _chestIndices = new List<ChestIndex>();
        private bool _active;

        /// <summary>
        /// Unloads the items from the chest from the fridge,
        /// and update the chests to remove used ingridients.
        /// </summary>
        public void RemoveItems()
        {
            if (!_active) return;
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;

            UpdateStorage();

            farmHouse.fridge.Value.items.Clear();
            farmHouse.fridge.Value.items.AddRange(_chestIndices[0].Chest.items);
            _active = false;
        }

        /// <summary>
        /// Updates the chests to remove used ingridients.
        /// </summary>
        public void UpdateStorage()
        {
            if (!_active) return;
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;

            foreach (var chestIndex in _chestIndices)
            {
                for (var i = 0; i < chestIndex.Count; i++)
                {
                    chestIndex.Chest.items[i] = farmHouse.fridge.Value.items[chestIndex.Start + i];
                }
            }
        }

        /// <summary>
        /// Loads the items from the chest into the fridge.
        /// </summary>
        public void LoadItems()
        {
            if (_active) return;
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;

            _chestIndices.Clear();


            var fridge = new Chest();
            fridge.items.AddRange(farmHouse.fridge.Value.items);
            
            _chestIndices.Add(new ChestIndex(fridge, 0, fridge.items.Count));

            foreach (var chest in GetChests())
            {
                _chestIndices.Add(new ChestIndex(chest, farmHouse.fridge.Value.items.Count, chest.items.Count));
                farmHouse.fridge.Value.items.AddRange(chest.items);
            }

            _active = true;
        }

        private IEnumerable<Chest> GetChests()
        {
            var chests = new List<Chest>();
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return chests;
            
            foreach (var objectsValue in farmHouse.Objects.Values)
            {
                if (!(objectsValue is Chest chest) || !chest.playerChest.Value) continue;
                chests.Add(chest);
            }

            return chests;
        }
    }

    /// <summary>
    /// Represents a chest that was added to the fridge.
    /// </summary>
    internal class ChestIndex
    {
        public readonly Chest Chest;
        public readonly int Start;
        public readonly int Count;

        public ChestIndex(Chest chest1, int tempItemsCount, int size)
        {
            Chest = chest1;
            Start = tempItemsCount;
            Count = size;
        }
    }
}