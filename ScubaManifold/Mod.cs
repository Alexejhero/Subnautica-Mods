using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MAC.ScubaManifold
{
    public static class Mod
    {
        public static void Patch()
        {
            new ScubaManifoldItem().Patch();
            Console.WriteLine("[ScubaManifold] [INFO] Patched!");
        }
    }

    public class ScubaManifoldItem : Craftable
    {
        public ScubaManifoldItem() : base("ScubaManifold", "Scuba Manifold", "Combines the oxygen supply of all carried tanks")
        {
            OnFinishedPatching = () =>
            {
                CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.Tank);
                CraftDataHandler.SetCraftingTime(this.TechType, 5);
                CraftDataHandler.SetItemSize(this.TechType, 3, 2);

                ScubaManifold.techType = this.TechType;
            };
        }

        protected override TechData GetBlueprintRecipe() => new TechData() { Ingredients = new List<Ingredient>() { new Ingredient(TechType.Silicone, 1), new Ingredient(TechType.Titanium, 3), new Ingredient(TechType.Lubricant, 2)}, craftAmount = 1 };
        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;
        public override string[] StepsToFabricatorTab => "Personal/Equipment".Split('/');

        public override TechGroup GroupForPDA => TechGroup.Personal;
        public override TechCategory CategoryForPDA => TechCategory.Equipment;

        public override string AssetsFolder => Path.Combine(new DirectoryInfo(Path.Combine(Assembly.GetExecutingAssembly().Location, "..")).Name, "Assets");
        public override string IconFileName => "ScubaManifold.png";

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(TechType.Tank);
            GameObject obj = GameObject.Instantiate(prefab);

            Pickupable pickupable = obj.GetComponent<Pickupable>();
            pickupable.destroyOnDeath = false;

            ScubaManifold scuba = Player.mainObject.GetComponent<ScubaManifold>() ?? Player.mainObject.AddComponent<ScubaManifold>();

            GameObject.DestroyImmediate(obj.GetComponent<Oxygen>());

            return obj;
        }
    }

    public class ScubaManifold : MonoBehaviour
    {
        public static TechType techType;

        public void Start()
        {
            if (GameObject.FindObjectsOfType<ScubaManifold>().Length >= 2)
            {
                Console.WriteLine("[ScubaManifold] [ERROR] ScubaManifold component must be a singleton!");
                DestroyImmediate(this);
            }
        }

        public void Update()
        {
            List<InventoryItem> items = new List<InventoryItem>();
            Inventory.main.container.GetItemTypes().ForEach(type => items.AddRange(Inventory.main.container.GetItems(type)));
            List<Oxygen> sources = items.Where(item => item.item.gameObject.GetComponent<Oxygen>() != null).Select(item => item.item.gameObject.GetComponent<Oxygen>()).ToList();

            if (Inventory.main.equipment.GetItemInSlot("Tank")?.item?.GetTechType() == ScubaManifold.techType) sources.ForEach(source => Player.main.oxygenMgr.RegisterSource(source));
            else sources.ForEach(source => Player.main.oxygenMgr.UnregisterSource(source));
        }
    }
}
