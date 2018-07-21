using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = System.Object;

namespace StardewMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private IModHelper helper;
        private FridgeHandler handler;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            handler = new FridgeHandler();
            MenuEvents.MenuChanged += MenuChanged_Event;
            MenuEvents.MenuClosed += MenuClosed_Event;
        }

        /// <summary>
        /// If the closed menu was a crafting menu, call the handler to reset the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClosed_Event(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is CraftingPage page && helper.Reflection.GetField<bool>(page, "cooking").GetValue())
            {
                handler.RemoveItems();
            }
        }

        /// <summary>
        /// If the opened menu was a crafting menu, call the handler to add load the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChanged_Event(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is CraftingPage page && helper.Reflection.GetField<bool>(page, "cooking").GetValue())
            {
                handler.LoadItems();
            }
        }
    }
}