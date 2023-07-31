using AlexejheroYTB.Common;
using Harmony;
using QModManager.API;
using QModManager.API.ModLoading;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;

namespace AlexejheroYTB.EnhancedStillsuit
{
    [QModCore]
    public static class Mod
    {
        [QModPatch]
        public static void Patch()
        {
            HarmonyHelper.Patch();
            new ESS().Patch();

            Common.Logger.Log("Patched");
        }
    }

    public class ESS : Craftable
    {
        public ESS() : base("enhancedstillsuit", "Enhanced Stillsuit", "Just like a normal stillsuit, but it automatically injects the reclaimed water into your system.")
        {
            base.OnStartedPatching += OnStartedPatching;
            base.OnFinishedPatching += OnFinishedPatching;
        }

        public new void OnStartedPatching()
        {
            if (!QModServices.Main.ModPresent("MoreModifiedItems"))
            {
                CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "BodyMenu", "Suit Upgrades", SpriteManager.Get(TechType.Stillsuit));
            }
        }

        public new void OnFinishedPatching()
        {
            CraftDataHandler.SetItemSize(this.TechType, 2, 2);
            CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.Body);
            SpriteHandler.RegisterSprite(this.TechType, SpriteManager.Get(TechType.Stillsuit));
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.Stillsuit, 1),
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.CopperWire, 2),
                new Ingredient(TechType.Silver, 1),
            },
            };
        }

        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;
        public override string[] StepsToFabricatorTab => new string[] { "BodyMenu" };

        public override TechCategory CategoryForPDA => TechCategory.Equipment;
        public override TechGroup GroupForPDA => TechGroup.Personal;

        public override TechType RequiredForUnlock => TechType.Stillsuit;

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(TechType.Stillsuit);
            GameObject obj = GameObject.Instantiate(prefab);

            obj.AddComponent<ESSBehaviour>();

            return obj;
        }
    }

    public class ESSBehaviour : MonoBehaviour { }

    [HarmonyPatch(typeof(Stillsuit), nameof(Stillsuit.UpdateEquipped))]
    public static class StillSuit_UpdateEquipped
    {
        [HarmonyPrefix]
        public static bool Prefix(Stillsuit __instance)
        {
            if (!__instance.GetComponent<ESSBehaviour>())
            {
                return true;
            }

            Survival survival = Player.main.GetComponent<Survival>();

            if (GameModeUtils.RequiresSurvival() && !survival.freezeStats)
            {
                __instance.waterCaptured += Time.deltaTime / 18f * 0.75f;
                if (__instance.waterCaptured >= 1f)
                {
                    survival.water += __instance.waterCaptured;
                    __instance.waterCaptured -= __instance.waterCaptured;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Equipment), nameof(Equipment.GetTechTypeInSlot))]
    public static class Equipment_GetTechTypeInSlot
    {
        [HarmonyPostfix]
        public static void Postfix(ref TechType __result)
        {
            __result = TechTypeExtensions.FromString("enhancedstillsuit", out TechType enhancedstillsuit, true) && __result == enhancedstillsuit ? TechType.Stillsuit : __result;
        }
    }
}