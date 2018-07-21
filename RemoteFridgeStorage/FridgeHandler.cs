using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewMod
{
    /// <summary>
    /// Takes care of adding and removing elements for crafting.
    /// </summary>
    internal class FridgeHandler
    {
        private List<Item> items;
        private List<ChestIndex> ChestIndices = new List<ChestIndex>();

        /// <summary>
        /// Unloads the items from the chest from the fridge,
        /// and update the chests to remove used ingridients.
        /// </summary>
        public void RemoveItems()
        {
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;

            UpdateStorage();

            farmHouse.fridge.items = items;
        }

        /// <summary>
        /// Updates the chests to remove used ingridients.
        /// </summary>
        public void UpdateStorage()
        {
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;
            
            foreach (var chestIndex in ChestIndices)
            {
                for (var i = 0; i < chestIndex.Count; i++)
                {
                    chestIndex.Chest.items[i] = farmHouse.fridge.items[chestIndex.Start + i];
                }
            }
        }

        /// <summary>
        /// Loads the items from the chest into the fridge.
        /// </summary>
        public void LoadItems()
        {
            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)) return;

            items = farmHouse.fridge.items;
            ChestIndices.Clear();

            var tempItems = new List<Item>();
            
            var fridge = new Chest {items = items};
            ChestIndices.Add(new ChestIndex(fridge, 0, items.Count));
            
            tempItems.AddRange(items);
            foreach (var chest in GetChests())
            {
                ChestIndices.Add(new ChestIndex(chest, tempItems.Count, chest.items.Count));
                tempItems.AddRange(chest.items);
            }


            farmHouse.fridge.items = tempItems;
        }

        private IEnumerable<Chest> GetChests()
        {
            var chests = new List<Chest>();
            if (Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)
            {
                foreach (var pair in farmHouse.Objects)
                {
                    if (!(pair.Value is Chest chest) || !chest.playerChest) continue;
                    chests.Add(chest);
                }
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