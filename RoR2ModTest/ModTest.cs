using BepInEx;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using On.RoR2;
using UnityEngine;

using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace RoR2ModTest
{
    

    [BepInDependency("com.bepis.r2api")]
    //Change these
    [R2APISubmoduleDependency(nameof(DifficultyAPI))]

    [BepInPlugin("com.Arozal.ModTest", "Arozal Mod", "0.0.1")]
    public class ModTest : BaseUnityPlugin
    {
        public static int amountOfYanics = 0;
        public static int amountOfItems = 0;
        public static float time = 0;

        public static List<string> dpsName = new List<string>();
        public static List<decimal> dpsValues = new List<decimal>();
        public static List<decimal> biggestHit = new List<decimal>();
        public static List<decimal> biggestStage = new List<decimal>();

        public static string steamName = "";

        private static object GetInstanceField<T>(T instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(T).GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }


        public void Awake()
        {
            Assets.Init();
            time = Time.time;


            On.RoR2.UI.HUD.Awake += new On.RoR2.UI.HUD.hook_Awake(StatsUI.initStatsDisplay);

            On.RoR2.CharacterBody.Start += new On.RoR2.CharacterBody.hook_Start(StageStart);
            On.RoR2.CharacterBody.OnInventoryChanged += new On.RoR2.CharacterBody.hook_OnInventoryChanged(CharacterBody_OnInventoryChanged);
            On.RoR2.CharacterBody.Update += new On.RoR2.CharacterBody.hook_Update(statsDisplayUpdate);
            On.RoR2.NetworkUser.Start += new On.RoR2.NetworkUser.hook_Start(characterSelection);
            On.RoR2.CharacterModel.EnableItemDisplay += new On.RoR2.CharacterModel.hook_EnableItemDisplay(disableSkin);

            On.RoR2.CharacterBody.OnTakeDamageServer += new On.RoR2.CharacterBody.hook_OnTakeDamageServer(onEnemyTakeDmg);
            RoR2.DifficultyDef diffi = new RoR2.DifficultyDef(1000f, "DIFFICULTY_NIGHTMARE", "@textures:Assets/Textures/gitgud.png", "DIFFICULTY_NIGHTMARE_DESCRIPTION", RoR2.ColorCatalog.GetColor(RoR2.ColorCatalog.ColorIndex.Tier3ItemDark), "mn", false);
            DifficultyAPI.AddDifficulty(diffi);
        }

        private static void disableSkin(On.RoR2.CharacterModel.orig_EnableItemDisplay orig, RoR2.CharacterModel self, ItemIndex itemIndex)
        {
            if(itemIndex != Assets.YanisationItem){
                itemIndex = ItemIndex.None;
            }

            orig.Invoke(self, itemIndex);
        }

        private void OnDestroy()
        {
            On.RoR2.UI.HUD.Awake -= StatsUI.initStatsDisplay;
        }

        private static void characterSelection(On.RoR2.NetworkUser.orig_Start orig, RoR2.NetworkUser self)
        {
            orig.Invoke(self);
            
            amountOfYanics = 0;
            amountOfItems = 0;
            dpsName.Clear();
            dpsValues.Clear();
            biggestHit.Clear();
            biggestStage.Clear();
        }

        private static void statsDisplayUpdate(On.RoR2.CharacterBody.orig_Update orig, RoR2.CharacterBody self)
        {
            orig.Invoke(self);

            if (self.isPlayerControlled == false)
                return;

            steamName = RoR2.LocalUserManager.GetFirstLocalUser().userProfile.name;

            if (self.GetUserName() != steamName)
                return;


            if(Time.time - time > 1.0f)
            {
                time = Time.time;
                return;
            }
            
            StatsUI.addStats("Dmg", self.damage.ToString(), "RRGGBB");
            StatsUI.addStats("Crit", self.crit.ToString(), "RRGGBB");
            StatsUI.addStats("Armor", self.armor.ToString(), "RRGGBB");
            StatsUI.addStats("Atk Speed", self.attackSpeed.ToString(), "RRGGBB");
            StatsUI.addStats("Mov Speed", self.moveSpeed.ToString(), "RRGGBB");
            StatsUI.addStats("Acc Speed", self.acceleration.ToString(), "RRGGBB");
            StatsUI.addStats("Jump Power", self.jumpPower.ToString(), "RRGGBB");
            StatsUI.addStats("Jump Count", self.maxJumpCount.ToString(), "RRGGBB");
            StatsUI.addEmptyLine();
            for(int i = 0; i < dpsName.Count; i++)
            {
                ulong val = (ulong)dpsValues[i];
                ulong biggestVal = (ulong)biggestHit[i];
                ulong biggestStageValue = (ulong)biggestStage[i];

                StatsUI.addSingleValue(dpsName[i], "RRGGBB");
                StatsUI.addStats("MaxD", biggestVal.ToString(), "RRGGBB");
                StatsUI.addStats("DmgS", biggestStageValue.ToString(), "RRGGBB");
                StatsUI.addStats("DmgW", val.ToString(), "RRGGBB");

                StatsUI.addEmptyLine();
            }
            
            StatsUI.applyValues();
        }

        private static void onEnemyTakeDmg(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, RoR2.CharacterBody self, RoR2.DamageReport damageReport){

            orig.Invoke(self, damageReport);

            string type = damageReport.attackerBody.tag;
            int yashaCount = damageReport.attackerBody.inventory.GetItemCount(Assets.YashaItem);

            RoR2.CharacterBody cb = damageReport.attackerBody;

            if (yashaCount > 0)
            {
                
                float percentage = damageReport.damageDealt * 0.025f * yashaCount;

                float diff = cb.maxHealth - cb.healthComponent.health;

                if (diff > percentage)
                    diff = percentage;

                cb.healthComponent.health += diff;

                float barrierValue = (percentage - diff);

                if (barrierValue > 0)
                    barrierValue /= 2.0f;

                cb.healthComponent.barrier += barrierValue;
            }

            if (cb.isPlayerControlled)
            {
                if(dpsName.Contains(cb.GetUserName()))
                {
                    int index = dpsName.IndexOf(cb.GetUserName());
                    dpsValues[index] += (decimal)damageReport.damageDealt;
                    biggestStage[index] += (decimal)damageReport.damageDealt;
                    if ((decimal)damageReport.damageDealt > biggestHit[index])
                    {
                        biggestHit[index] = (decimal)damageReport.damageDealt;
                    }
        
                }
                else
                {
                    dpsName.Add(cb.GetUserName());
                    dpsValues.Add((decimal)damageReport.damageDealt);
                    biggestHit.Add((decimal)damageReport.damageDealt);
                    biggestStage.Add((decimal)damageReport.damageDealt);
                }
            }

            

        }


        private static void StageStart(On.RoR2.CharacterBody.orig_Start orig, RoR2.CharacterBody self)
        {
            orig.Invoke(self);
            string type = self.tag;


            if (self.inventory == null)
                return;

            int amountOfSpeeds = self.inventory.GetItemCount(Assets.SpeedWagonItem);
            if (amountOfSpeeds > 0)
            {
                self.sprintingSpeedMultiplier = 1.4f + (0.4f * amountOfSpeeds);
            }
            
            if (amountOfYanics > 0)
            {
                for (int i = 0; i < amountOfItems; i++)
                {
                    recalcStats(self);
                }
            }
            
        }



        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig.Invoke(self);
            
            amountOfYanics = self.inventory.GetItemCount(Assets.YanisationItem);
            if(amountOfYanics > 0)
            {
                amountOfItems++;
                RoR2.Chat.AddMessage("Yanic Level: " + amountOfItems);
                recalcStats(self);
            }

            int speedAmount = self.inventory.GetItemCount(Assets.SpeedWagonItem);
            if(speedAmount > 0)
            {
                self.sprintingSpeedMultiplier = 1.4f + (0.4f * speedAmount);
            }
        }

        public static void recalcStats(RoR2.CharacterBody self)
        {
            
            int speedAmount = self.inventory.GetItemCount(Assets.SpeedWagonItem);
            if (speedAmount > 0)
            {
                self.sprintingSpeedMultiplier = 1.4f + (0.4f * speedAmount);
            }


            if (amountOfYanics <= 0)
                return;

            self.baseJumpCount++;
            self.baseAttackSpeed *= 1.05f;
            self.baseDamage *= 1.2f;

            if(self.baseAttackSpeed > 3.0f)
            {
                self.baseAttackSpeed = 3.0f;
            }

            if (self.baseDamage > 250.0f)
            {
                self.baseDamage = 250.0f;
            }
        }
    }
}