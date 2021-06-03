using System.Reflection;
using R2API;
using RoR2;
using R2API.AssetPlus;
using UnityEngine;

namespace RoR2ModTest
{
    internal static class Assets
    {
        internal static GameObject[] itemPrefabs;
        internal static ItemIndex YanisationItem;
        internal static ItemIndex YashaItem;
        internal static ItemIndex SpeedWagonItem;
        internal static int amountOfItems = 2;

        public static string ModPrefix = "@textures:";
        public static string[] PrefabPath;
        public static string[] IconPath;

        internal static void Init()
        {
        

            PrefabPath = new string[]
            {
                ModPrefix + "Assets/Textures/yanisation.prefab",
                ModPrefix + "Assets/Textures/yasha.prefab",
                ModPrefix + "Assets/Textures/speedwagon.prefab"
            };

            IconPath = new string[]
            {
                ModPrefix + "Assets/Textures/yanisation.png",
                ModPrefix + "Assets/Textures/yasha.png",
                ModPrefix + "Assets/Textures/speedwagons.png",
            };


            // First registering your AssetBundle into the ResourcesAPI with a modPrefix that'll also be used for your prefab and icon paths
            // note that the string parameter of this GetManifestResourceStream call will change depending on
            // your namespace and file name



            itemPrefabs = new GameObject[3];
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoR2ModTest.textures"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(ModPrefix.TrimEnd(':'), bundle);
                ResourcesAPI.AddProvider(provider);

                itemPrefabs[0] = bundle.LoadAsset<GameObject>("Assets/Textures/yanisation.prefab");
                itemPrefabs[1] = bundle.LoadAsset<GameObject>("Assets/Textures/yasha.prefab");
                itemPrefabs[2] = bundle.LoadAsset<GameObject>("Assets/Textures/speedwagon.prefab");
            }

            initItem();
            AddLanguageTokens();
        }

        private static void initItem()
        {
            var biscoLeashItemDef = new ItemDef
            {
                name = "BiscosLeash", // its the internal name, no spaces, apostrophes and stuff like that
                tier = ItemTier.Tier1,
                pickupModelPath = PrefabPath[0],
                pickupIconPath = IconPath[0],
                nameToken = "BISCOLEASH_NAME", // stylised name
                pickupToken = "BISCOLEASH_PICKUP",
                descriptionToken = "BISCOLEASH_DESC",
                loreToken = "BISCOLEASH_LORE",
                tags = new[]
                {
                    ItemTag.Damage
                }
            };
            YanisationItem = registerItem(biscoLeashItemDef, itemPrefabs[0]);

            var yashaDef = new ItemDef
            {
                name = "YASHA", // its the internal name, no spaces, apostrophes and stuff like that
                tier = ItemTier.Lunar,
                pickupModelPath = PrefabPath[1],
                pickupIconPath = IconPath[1],
                nameToken = "YASHA_NAME", // stylised name
                pickupToken = "YASHA_PICKUP",
                descriptionToken = "YASHA_DESC",
                loreToken = "YASHA_LORE",
                
                tags = new[]
                {
                    ItemTag.Healing
                }
            };
            YashaItem = registerItem(yashaDef, itemPrefabs[1]);

            var speedwagonDef = new ItemDef
            {
                name = "SPEEDWAGON", 
                tier = ItemTier.Lunar,
                pickupModelPath = PrefabPath[2],
                pickupIconPath = IconPath[2],
                nameToken = "SPEEDWAGON_NAME", 
                pickupToken = "SPEEDWAGON_PICKUP",
                descriptionToken = "SPEEDWAGON_DESC",
                loreToken = "SPEEDWAGON_LORE",

                tags = new[]
                {
                    ItemTag.SprintRelated
                }
            };
            SpeedWagonItem = registerItem(speedwagonDef, itemPrefabs[2]);
        }

        private static ItemIndex registerItem(ItemDef itemDef, GameObject gobj)
        {
            var itemDisplayRules = new ItemDisplayRule[1];// keep this null if you don't want the item to show up on the survivor 3d model. You can also have multiple rules !
            itemDisplayRules[0].followerPrefab = gobj; // the prefab that will show up on the survivor
            itemDisplayRules[0].childName = "Chest"; // this will define the starting point for the position of the 3d model, you can see what are the differents name available in the prefab model of the survivors
            itemDisplayRules[0].localScale = new Vector3(0.15f, 0.15f, 0.15f); // scale the model
            itemDisplayRules[0].localAngles = new Vector3(0f, 180f, 0f); // rotate the model
            itemDisplayRules[0].localPos = new Vector3(-0.35f, -0.1f, 0f); // position offset relative to the childName, here the survivor Chest

            var customItem = new CustomItem(itemDef, itemDisplayRules);

            return ItemAPI.Add(customItem); // ItemAPI sends back the ItemIndex of your item
        }


        private static void AddLanguageTokens()
        {
            R2API.LanguageAPI.Add("BISCOLEASH_NAME", "Yanisation");
            R2API.LanguageAPI.Add("BISCOLEASH_PICKUP", "NEIIIINNN!!! Was hast du angestellt!!!!!");
            R2API.LanguageAPI.Add("BISCOLEASH_DESC", "???");
            R2API.LanguageAPI.Add("BISCOLEASH_LORE", "Niemand weiss woher es kommt oder was es ist. Nur eines ist klar, es ist ein Yanic!!!!");
            

            R2API.LanguageAPI.Add("YASHA_NAME", "Yasha");
            R2API.LanguageAPI.Add("YASHA_PICKUP", "Let's suck the lifeforce");
            R2API.LanguageAPI.Add("YASHA_DESC", "Gives 2.5%(per stack) of the damage dealt as heal, if the heal exceeds the maximum health then it counts as barrier instead.");
            R2API.LanguageAPI.Add("YASHA_LORE", "It gives Live because it steals live");

            R2API.LanguageAPI.Add("SPEEDWAGON_NAME", "Speeeeeed");
            R2API.LanguageAPI.Add("SPEEDWAGON_PICKUP", "SPEEEEEEED");
            R2API.LanguageAPI.Add("SPEEDWAGON_DESC", "Increases your run speed by 40% per stack.");
            R2API.LanguageAPI.Add("SPEEDWAGON_LORE", "Best Waifu");

            R2API.LanguageAPI.Add("DIFFICULTY_NIGHTMARE", "git gud");
            R2API.LanguageAPI.Add("DIFFICULTY_NIGHTMARE_DESCRIPTION", "A liitle bit faster.");
        }

        

    }
}
