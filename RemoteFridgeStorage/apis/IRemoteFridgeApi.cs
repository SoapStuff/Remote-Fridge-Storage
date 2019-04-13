using System.Collections.Generic;
using StardewValley;

namespace RemoteFridgeStorage.apis
{
    /// <summary>
    /// API For getting the list with items 
    /// </summary>
    public interface IRemoteFridgeApi
    {
        IList<Item> Fridge();
        void UseCustomCraftingMenu(bool enabled = false);
    }
}