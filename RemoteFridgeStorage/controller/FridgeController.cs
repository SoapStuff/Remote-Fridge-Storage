using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RemoteFridgeStorage.controller
{
    /// <summary>
    /// Handles the items used by the fridge from the chest controller
    /// </summary>
    public class FridgeController
    {
        private readonly ChestController _chestController;

        public FridgeController(ChestController chestController)
        {
            _chestController = chestController;
        }

        //Thanks to aEnigmatic
        public void InjectItems()
        {
            var page = Game1.activeClickableMenu;

            if (page == null)
                return;

            // Find nearby chests
            var nearbyChests = _chestController.GetChests().ToList();
            if (!nearbyChests.Any())
                return;

            if (page is CraftingPage craftingPage)
            {
                craftingPage._materialContainers.AddRange(nearbyChests);
            }
        }
    }
}