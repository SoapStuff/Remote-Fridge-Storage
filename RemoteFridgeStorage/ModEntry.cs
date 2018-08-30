using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace RemoteFridgeStorage
{
    
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private FridgeHandler _handler;
        public static ModEntry instance;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            var fridgeSelected = helper.Content.Load<Texture2D>("assets/fridge.png");
            var fridgeDeselected = helper.Content.Load<Texture2D>("assets/fridge2.png");

            var categorizeChestsLoaded = helper.ModRegistry.IsLoaded("CategorizeChests") ||
                                         helper.ModRegistry.IsLoaded("aEnigma.ConvenientChests");
            _handler = new FridgeHandler(fridgeSelected, fridgeDeselected, categorizeChestsLoaded);

            MenuEvents.MenuChanged += MenuChanged_Event;
            InputEvents.ButtonPressed += Button_Pressed_Event;

            GraphicsEvents.OnPostRenderGuiEvent += Draw;

            SaveEvents.AfterLoad += AfterLoad;
            SaveEvents.BeforeSave += BeforeSave;
            SaveEvents.AfterSave += AfterSave;
            GameEvents.UpdateTick += Game_Update;
        }

        private void Game_Update(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.Game_Update();
        }

        private void AfterSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.AfterSave();
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.BeforeSave();
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.AfterLoad();
        }


        private void Draw(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.DrawFridge();
        }

        private void Button_Pressed_Event(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady) return;

            if (e.Button == SButton.MouseLeft)
            {
                _handler.HandleClick(e);
            }
        }

        /// <summary>
        /// If the opened menu was a crafting menu, call the handler to add load the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChanged_Event(object sender, EventArgsClickableMenuChanged e)
        {
            if (!Context.IsWorldReady) return;
            if (e.NewMenu is CraftingPage && !(e.NewMenu is RemoteFridgeCraftingPage))
            {
                _handler.LoadMenu(e);
            }
        }
    }
}