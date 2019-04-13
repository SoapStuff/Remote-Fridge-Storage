using System.Collections.Generic;
using StardewValley;

namespace RemoteFridgeStorage.apis
{
    public class RemoteFridgeApi : IRemoteFridgeApi
    {
        private readonly FridgeHandler _handler;
        private readonly ModEntry _modEntry;

        public RemoteFridgeApi(FridgeHandler handler, ModEntry modEntry)
        {
            _handler = handler;
            _modEntry = modEntry;
        }

        public IList<Item> Fridge()
        {
            return _handler.FridgeList;
        }

        public void UseCustomCraftingMenu(bool enabled = false)
        {
            _modEntry.EnableCustomCraftingPage = enabled;
        }
    }
}