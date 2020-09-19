using Microsoft.Xna.Framework.Graphics;
using RemoteFridgeStorage.controller;
using RemoteFridgeStorage.controller.saving;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace RemoteFridgeStorage
{
    /// <inheritdoc />
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /// <summary>The mod configuration from the player.</summary>
        public Config Config;

        public FridgeController FridgeController;

        public ChestController ChestController;

        public SaveManager SaveManager;
        
        private CompatibilityInfo _compatibilityInfo;

        public static ModEntry Instance { get; private set; }

        /// <inheritdoc />
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = helper.ReadConfig<Config>();

            var textures = LoadAssets(helper);
            _compatibilityInfo = GetCompatibilityInfo(helper);

            ChestController = new ChestController(textures, _compatibilityInfo, Config);
            FridgeController = new FridgeController(ChestController);
            SaveManager = new SaveManager(ChestController);
            
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Display.RenderingActiveMenu += OnRenderingActiveMenu;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.SaveLoaded += SaveManager.SaveLoaded;
            Helper.Events.GameLoop.Saving += SaveManager.Saving;
        }
        
        /// <summary>
        /// Check for loaded mods for compatibility reasons
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        private static CompatibilityInfo GetCompatibilityInfo(IModHelper helper)
        {
            // Compatibility checks
            bool cookingSkillLoaded = helper.ModRegistry.IsLoaded("spacechase0.CookingSkill");
            bool categorizeChestsLoaded = helper.ModRegistry.IsLoaded("CategorizeChests");
            bool convenientChestsLoaded = helper.ModRegistry.IsLoaded("aEnigma.ConvenientChests");
            bool megaStorageLoaded = helper.ModRegistry.IsLoaded("Alek.MegaStorage");

            var compatibilityInfo = new CompatibilityInfo
            {
                CategorizeChestLoaded = categorizeChestsLoaded,
                ConvenientChestLoaded = convenientChestsLoaded,
                CookingSkillLoaded = cookingSkillLoaded,
                MegaStorageLoaded = megaStorageLoaded
            };
            return compatibilityInfo;
        }

        /// <summary>
        /// Load the textures
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        private static Textures LoadAssets(IModHelper helper)
        {
            // Assets
            var fridgeSelected = helper.Content.Load<Texture2D>("assets/fridge.png");
            var fridgeSelectedAlt = helper.Content.Load<Texture2D>("assets/fridge-flipped.png");
            var fridgeDeselected = helper.Content.Load<Texture2D>("assets/fridge2.png");
            var fridgeDeselectedAlt = helper.Content.Load<Texture2D>("assets/fridge2-flipped.png");

            var textures = new Textures
            {
                FridgeSelected = fridgeSelected,
                FridgeDeselected = fridgeDeselected,
                FridgeSelectedAlt = fridgeSelectedAlt,
                FridgeDeselectedAlt = fridgeDeselectedAlt
            };
            return textures;
        }

        /// <summary>
        /// Draw the icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            ChestController.DrawFridgeIcon();
        }

        /// <summary>
        /// Add chests to fridge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (e.NewMenu == e.OldMenu || e.NewMenu == null)
                return;

            // If The (Cooking) Crafting page is opened
            if (!_compatibilityInfo.CookingSkillLoaded && e.NewMenu is StardewValley.Menus.CraftingPage &&
                Helper.Reflection.GetField<bool>(e.NewMenu, "cooking", false) != null &&
                Helper.Reflection.GetField<bool>(e.NewMenu, "cooking").GetValue())
            {
                FridgeController.InjectItems();
            }

            // If the Cooking Skill Page is opened.
            if (_compatibilityInfo.CookingSkillLoaded && e.NewMenu.GetType().ToString() == "CookingSkill.NewCraftingPage")
            {
                FridgeController.InjectItems();
            }
        }

        /// <summary>
        /// Handle clicking of the fridge icons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            ChestController.HandleClick(e.Cursor);
        }

        /// <summary>
        /// Container for textures
        /// </summary>
        public struct Textures
        {
            public Texture2D FridgeSelected;
            public Texture2D FridgeSelectedAlt;
            public Texture2D FridgeDeselected;
            public Texture2D FridgeDeselectedAlt;
        }

        /// <summary>
        /// Container for modInfo
        /// </summary>
        public struct CompatibilityInfo
        {
            public bool CookingSkillLoaded;
            public bool CategorizeChestLoaded;
            public bool ConvenientChestLoaded;
            public bool MegaStorageLoaded;
        }

        public void Log(string s)
        {
            Monitor.Log(s);
        }
    }
}