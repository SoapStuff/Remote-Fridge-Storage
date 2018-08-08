using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RemoteFridgeStorage
{
    /// <summary>
    /// Takes care of adding and removing elements for crafting.
    /// </summary>
    internal class FridgeHandler
    {
        /// <summary>
        /// List of pairs with the chests that currently are added to the items in the fridge.
        /// </summary>
        private readonly List<ChestIndex> _chestIndices = new List<ChestIndex>();
        /// <summary>
        /// If the player currently is crafting
        /// </summary>
        private bool _active;

        /// <summary>
        /// Set of all chests that are included.
        /// </summary>
        private readonly HashSet<Chest> _chests = new HashSet<Chest>();
        
        /// <summary>
        /// Texture button state included.
        /// </summary>
        private readonly ClickableTextureComponent _fridge1;
        /// <summary>
        /// Texture button state excluded (default state)
        /// </summary>
        private readonly ClickableTextureComponent _fridge2;

        /// <summary>
        /// If the chest is currently open.
        /// </summary>
        private bool _opened;

        /// <summary>
        /// Creats a new handler for fridge items.
        /// </summary>
        /// <param name="textureFridge"></param>
        /// <param name="textureFridge2"></param>
        public FridgeHandler(Texture2D textureFridge, Texture2D textureFridge2)
        {
            _opened = false;
            _fridge1 = new ClickableTextureComponent(Rectangle.Empty, textureFridge, Rectangle.Empty, 1f);
            _fridge2 = new ClickableTextureComponent(Rectangle.Empty, textureFridge2, Rectangle.Empty, 1f);
            UpdatePos();
        }

        /// <summary>
        /// Update the position of the button.
        /// </summary>
        private void UpdatePos()
        {
            var menu = Game1.activeClickableMenu;
            if (menu == null) return;

            _fridge1.bounds = _fridge2.bounds = new Rectangle(menu.xPositionOnScreen - 17 * Game1.pixelZoom,
                menu.yPositionOnScreen + Game1.tileSize + Game1.pixelZoom * 5, 16 * Game1.pixelZoom,
                16 * Game1.pixelZoom);
        }

        /// <summary>
        /// Unloads the items from the chest from the fridge,
        /// and update the chests to remove used ingridients.
        /// </summary>
        public void RemoveItems()
        {
            if (!_active) return;
            
            var farmHouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;

            UpdateStorage();

            farmHouse?.fridge.Value.items.Clear();
            //The first index was always that of the fridge.
            farmHouse?.fridge.Value.items.AddRange(_chestIndices[0].Chest.items);
            _active = false;
        }

        /// <summary>
        /// Updates the chests to remove used ingridients.
        /// </summary>
        public void UpdateStorage()
        {
            if (!_active) return;
            
            var farmHouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;

            foreach (var chestIndex in _chestIndices)
            {
                for (var i = 0; i < chestIndex.Count; i++)
                {
                    chestIndex.Chest.items[i] = farmHouse?.fridge.Value.items[chestIndex.Start + i];
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

        /// <summary>
        /// Gets all the chests that are available for crafting.
        /// </summary>
        /// <returns>All the chests</returns>
        private IEnumerable<Chest> GetChests()
        {
            return _chests;
        }

        /// <summary>
        /// Handle the click event if it was on the fridge icon.
        /// </summary>
        /// <param name="eventArgsInput"></param>
        public void HandleClick(EventArgsInput eventArgsInput)
        {
            var chest = GetOpenChest();
            if (chest == null) return;

            var screenPixels = eventArgsInput.Cursor.ScreenPixels;

            if (!_fridge1.containsPoint((int) screenPixels.X, (int) screenPixels.Y)) return;

            Game1.playSound("smallSelect");
            
            if (_chests.Contains(chest))
            {
                _chests.Remove(chest);
            }
            else
            {
                _chests.Add(chest);
            }
        }

       /// <summary>
        /// Gets the chest that is currently open or null if no chest is open.
        /// </summary>
        /// <returns>The chest that is open</returns>
        private static Chest GetOpenChest()
        {
            if (Game1.activeClickableMenu == null)
                return null;

            if (!(Game1.activeClickableMenu is ItemGrabMenu)) return null;

            var menu = (ItemGrabMenu) Game1.activeClickableMenu;
            if (menu.behaviorOnItemGrab?.Target is Chest chest)
                return chest;

            return null;
        }

        /// <summary>
        /// Draw the fridge button
        /// </summary>
        public void DrawFridge()
        {
            var openChest = GetOpenChest();
            if (openChest == null) return;

            var farmHouse = Game1.getLocationFromName("farmHouse") as FarmHouse;

            if (openChest == farmHouse?.fridge.Value || Game1.activeClickableMenu == null ||
                !openChest.playerChest.Value) return;

            UpdatePos();
            if (_chests.Contains(openChest))
                _fridge1.draw(Game1.spriteBatch);
            else
                _fridge2.draw(Game1.spriteBatch);

            Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()),
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero,
                4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Load all fridges.
        /// </summary>
        public void LoadSave()
        {
            //
            _chests.Clear();
            var farmHouse = Game1.getLocationFromName("farmHouse") as FarmHouse;
            foreach (var gameLocation in Game1.locations)
            {
                foreach (var gameLocationObject in gameLocation.objects.Values)
                {
                    if (!(gameLocationObject is Chest chest)) continue;

                    if (chest.fridge.Value && chest != farmHouse?.fridge.Value)
                    {
                        _chests.Add(chest);
                        chest.fridge.Value = false;
                    }
                }
            }
        }

        /// <summary>
        /// Hacky way to store which chests are selected
        /// </summary>
        public void Save()
        {
            foreach (var chest in _chests)
            {
                chest.fridge.Value = true;
            }
        }

        /// <summary>
        /// Reset the fridge booleans
        /// </summary>
        public void AfterSave()
        {
            //Reset the fridge flag for all chests.
            foreach (var chest in _chests)
            {
                chest.fridge.Value = false;
            }
        }

        /// <summary>
        /// Listen to ticks update to determine when a chest opens or closes.
        /// </summary>
        public void Update()
        {
            var chest = GetOpenChest();
            if (chest == null && _opened)
            {
                //Chest close event;
            }

            if (chest != null && !_opened)
            {
                //Chest open event
                UpdatePos();
            }

            _opened = chest != null;
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