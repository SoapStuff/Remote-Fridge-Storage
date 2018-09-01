using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewValley;
// Just some IDE things.
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable InvertIf
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace RemoteFridgeStorage
{
    public class HarmonyRecipePatch
    {
        static bool Prefix(CraftingRecipe instance)
        {
            // Copied from CraftingRecipe consumeIngredients method.
            // The only change in this is the use of the ModEntry Fridge() method.
            // The private fields are accessed through reflection.
            ModEntry.Instance.Monitor.Log("Heeeeeeey");
            var recipeList = GetField<Dictionary<int, int>>(instance, "recipeList"); 
            for (int index1 = recipeList.Count - 1; index1 >= 0; --index1)
            {
                int recipe1 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                bool flag = false;
                for (int index2 = Game1.player.items.Count - 1; index2 >= 0; --index2)
                {
                    if (Game1.player.items[index2] != null && Game1.player.items[index2] is Object &&
                        !(bool) ((NetFieldBase<bool, NetBool>) (Game1.player.items[index2] as Object).bigCraftable) &&
                        ((int) ((NetFieldBase<int, NetInt>) Game1.player.items[index2].parentSheetIndex) ==
                         recipeList.Keys.ElementAt<int>(index1) || Game1.player.items[index2].Category ==
                         recipeList.Keys.ElementAt<int>(index1)))
                    {
                        int recipe2 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                        recipeList[recipeList.Keys.ElementAt<int>(index1)] -=
                            Game1.player.items[index2].Stack;
                        Game1.player.items[index2].Stack -= recipe2;
                        if (Game1.player.items[index2].Stack <= 0)
                            Game1.player.items[index2] = (Item) null;
                        if (recipeList[recipeList.Keys.ElementAt<int>(index1)] <= 0)
                        {
                            recipeList[recipeList.Keys.ElementAt<int>(index1)] = recipe1;
                            flag = true;
                            break;
                        }
                    }
                }

                if (instance.isCookingRecipe && !flag)
                {
                    //We do not need to check for the current location.
                    // FarmHouse currentLocation = Game1.currentLocation as FarmHouse;
                    // if (currentLocation != null)
                    {
                        // Use 
                        // var fridgeItems = currentLocation.fridge.Value.items;
                        var fridgeItems = ModEntry.Fridge();
                        for (int index2 = fridgeItems.Count - 1; index2 >= 0; --index2)
                        {
                            if (fridgeItems[index2] != null &&
                                fridgeItems[index2] is Object &&
                                ((int) ((NetFieldBase<int, NetInt>) fridgeItems[index2]
                                     .parentSheetIndex) == recipeList.Keys.ElementAt<int>(index1) ||
                                 fridgeItems[index2].Category ==
                                 recipeList.Keys.ElementAt<int>(index1)))
                            {
                                int recipe2 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                                recipeList[recipeList.Keys.ElementAt<int>(index1)] -=
                                    fridgeItems[index2].Stack;
                                fridgeItems[index2].Stack -= recipe2;
                                if (fridgeItems[index2].Stack <= 0)
                                    fridgeItems[index2] = (Item) null;
                                if (recipeList[recipeList.Keys.ElementAt<int>(index1)] <= 0)
                                {
                                    recipeList[recipeList.Keys.ElementAt<int>(index1)] = recipe1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static T GetField<T>(CraftingRecipe instance, string field)
        {
            return ModEntry.Instance.Helper.Reflection.GetField<T>(instance, field, false).GetValue();
        }
    }
}