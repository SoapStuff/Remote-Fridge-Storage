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


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            var textureFridge = helper.Content.Load<Texture2D>("assets/fridge.png", ContentSource.ModFolder);
            var textureFridge2 = helper.Content.Load<Texture2D>("assets/fridge2.png", ContentSource.ModFolder);

            if (textureFridge == null || textureFridge2 == null)
                Monitor.Log("One of the images could not be loaded", LogLevel.Warn);

            var categorizeChestsLoaded = helper.ModRegistry.IsLoaded("CategorizeChests") ||
                                         helper.ModRegistry.IsLoaded("aEnigma.ConvenientChests");
            _handler = new FridgeHandler(textureFridge, textureFridge2, categorizeChestsLoaded);

            MenuEvents.MenuChanged += MenuChanged_Event;
            MenuEvents.MenuClosed += MenuClosed_Event;
            PlayerEvents.InventoryChanged += InventoryChanged_Event;
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
            _handler.Update();
        }

        private void AfterSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.AfterSave();
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.Save();
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _handler.LoadSave();
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

        private void InventoryChanged_Event(object sender, EventArgsInventoryChanged e)
        {
            if (!Context.IsWorldReady) return;
            _handler.UpdateStorage();
        }

        /// <summary>
        /// If the closed menu was a crafting menu, call the handler to reset the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClosed_Event(object sender, EventArgsClickableMenuClosed e)
        {
            if (!Context.IsWorldReady)
                return;
            try
            {
                if (e.PriorMenu is IClickableMenu page && Helper.Reflection.GetField<bool>(page, "cooking").GetValue())
                {
                    _handler.RemoveItems();
                }
            }
            catch (InvalidOperationException ignore)
            {
                
            }
            
        }

        /// <summary>
        /// If the opened menu was a crafting menu, call the handler to add load the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChanged_Event(object sender, EventArgsClickableMenuChanged e)
        {
            if (!Context.IsWorldReady)
                return;
            try
            {
                if (e.NewMenu is IClickableMenu page && Helper.Reflection.GetField<bool>(page, "cooking") != null &&
                    Helper.Reflection.GetField<bool>(page, "cooking").GetValue())
                {
                    _handler.LoadItems();
                }
            }
            catch (InvalidOperationException ignore)
            {
                //This is a temporary work around.
            }
            
        }
    }
}