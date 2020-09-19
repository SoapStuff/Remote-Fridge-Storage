using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RemoteFridgeStorage.controller
{
    /// <summary>
    /// Used for drawing and selecting chests
    /// </summary>
    public class ChestController
    {
        private readonly Config _config;
        private readonly ClickableTextureComponent _fridgeSelected;
        private readonly ClickableTextureComponent _fridgeDeselected;
        private readonly ClickableTextureComponent _fridgeSelectedAlt;
        private readonly ClickableTextureComponent _fridgeDeselectedAlt;
        private readonly bool _offsetIcon;

        private readonly HashSet<Chest> _chests;

        /// <summary>
        /// New ChestController
        /// </summary>
        /// <param name="textures">This is a struct that contains all the textures used by the mod</param>
        /// <param name="compatibilityInfo">The struct that contains information about loaded mods</param>
        /// <param name="config">The mod config</param>
        public ChestController(ModEntry.Textures textures, ModEntry.CompatibilityInfo compatibilityInfo, Config config)
        {
            _chests = new HashSet<Chest>();
            _config = config;
            _offsetIcon =
                compatibilityInfo.CategorizeChestLoaded || compatibilityInfo.ConvenientChestLoaded ||
                compatibilityInfo.MegaStorageLoaded;
            _fridgeSelected =
                new ClickableTextureComponent(Rectangle.Empty, textures.FridgeSelected, Rectangle.Empty, 1f);
            _fridgeDeselected =
                new ClickableTextureComponent(Rectangle.Empty, textures.FridgeDeselected, Rectangle.Empty, 1f);
            _fridgeSelectedAlt =
                new ClickableTextureComponent(Rectangle.Empty, textures.FridgeSelectedAlt, Rectangle.Empty, 1f);
            _fridgeDeselectedAlt =
                new ClickableTextureComponent(Rectangle.Empty, textures.FridgeDeselectedAlt, Rectangle.Empty, 1f);
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
        /// Update the position of the button based on the settings in the config.
        /// </summary>
        private void UpdatePos()
        {
            //Number values here are based on trial and error
            var menu = Game1.activeClickableMenu;
            if (menu == null) return;

            double xOffset = 0.0;
            double yOffset = 1.0;
            //A mod is loaded that places an icon on the same location so we change it.
            if (_offsetIcon)
            {
                xOffset = -1.0;
                yOffset = -0.25;
            }

            int xScaledOffset = (int) (xOffset * Game1.tileSize);
            int yScaledOffset = (int) (yOffset * Game1.tileSize);

            int screenX = menu.xPositionOnScreen - 17 * Game1.pixelZoom + xScaledOffset;
            int screenY = menu.yPositionOnScreen + yScaledOffset + Game1.pixelZoom * 5;

            //This option is used to indicate a user defined position
            if (_config.OverrideOffset)
            {
                screenX = _config.XOffset;
                screenY = _config.YOffset;
            }

            var rectangle = new Rectangle(screenX, screenY,
                (int) (_config.ImageScale * 16 * Game1.pixelZoom),
                (int) (_config.ImageScale * 16 * Game1.pixelZoom));

            _fridgeSelected.bounds = _fridgeDeselected.bounds =
                _fridgeSelectedAlt.bounds = _fridgeDeselectedAlt.bounds = rectangle;
        }

        /// <summary>
        /// Handle the click event if it was on the fridge icon.
        /// </summary>
        /// <param name="cursor">The current cursor position.</param>
        public void HandleClick(ICursorPosition cursor)
        {
            var chest = GetOpenChest();
            if (chest == null) return;

            var screenPixels = cursor.ScreenPixels;

            if (!_fridgeSelected.containsPoint((int) screenPixels.X, (int) screenPixels.Y)) return;

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
        /// Draw the icon.
        /// </summary>
        public void DrawFridgeIcon()
        {
            var openChest = GetOpenChest();
            if (openChest == null) return;

            var farmHouse = Game1.getLocationFromName("farmHouse") as FarmHouse;

            if (openChest == farmHouse?.fridge.Value || Game1.activeClickableMenu == null ||
                !openChest.playerChest.Value) return;

            UpdatePos();
            if (_chests.Contains(openChest))
            {
                if (_config.FlipImage) _fridgeSelectedAlt.draw(Game1.spriteBatch);
                else _fridgeSelected.draw(Game1.spriteBatch);
            }
            else
            {
                if (_config.FlipImage) _fridgeSelectedAlt.draw(Game1.spriteBatch);
                else _fridgeDeselected.draw(Game1.spriteBatch);
            }

            Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()),
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero,
                4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Returns the chests currently selected
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Chest> GetChests()
        {
            return _chests;
        }

        /// <summary>
        /// Update the chests
        /// </summary>
        /// <param name="chests"></param>
        public void SetChests(IEnumerable<Chest> chests)
        {
            _chests.Clear();
            foreach (var chest in chests)
            {
                _chests.Add(chest);
            }
        }

        /// <summary>
        /// Clears the list of chests.
        /// </summary>
        public void ClearChests()
        {
            _chests.Clear();
        }
    }
}