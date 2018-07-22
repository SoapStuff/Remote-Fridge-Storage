using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Events;
using StardewValley.Menus;

namespace RemoteFridgeStorage
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private IModHelper _helper;
        private FridgeHandler _handler;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _handler = new FridgeHandler();
            MenuEvents.MenuChanged += MenuChanged_Event;
            MenuEvents.MenuClosed += MenuClosed_Event;
            PlayerEvents.InventoryChanged += InventoryChanged_Event;
        }

        private void InventoryChanged_Event(object sender, EventArgsInventoryChanged e)
        {
            _handler.UpdateStorage();
        }

        /// <summary>
        /// If the closed menu was a crafting menu, call the handler to reset the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClosed_Event(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is CraftingPage page && _helper.Reflection.GetField<bool>(page, "cooking").GetValue())
            {
                _handler.RemoveItems();
            }
        }

        /// <summary>
        /// If the opened menu was a crafting menu, call the handler to add load the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChanged_Event(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is CraftingPage page && _helper.Reflection.GetField<bool>(page, "cooking").GetValue())
            {
                _handler.LoadItems();
            }
        }
    }
}