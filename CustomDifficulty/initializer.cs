using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomDifficulty
{
    public class initializer : MonoBehaviour
    {
        public static CustomDifficulty mod;

        GameData gameData;

        FieldInfo stamRegenField = typeof(CharacterStats).GetField("m_staminaRegen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo healthRegenField = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo manaRegenField = typeof(CharacterStats).GetField("m_manaRegen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo stamField = typeof(CharacterStats).GetField("m_maxStamina", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo healthField = typeof(CharacterStats).GetField("m_maxHealthStat", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo manaField = typeof(CharacterStats).GetField("m_maxManaStat", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo moveField = typeof(CharacterStats).GetField("m_movementSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntStamField = typeof(CharacterStats).GetField("m_burntStamina", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntHealthField = typeof(CharacterStats).GetField("m_burntHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntManaField = typeof(CharacterStats).GetField("m_burntMana", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo foodRateField = typeof(PlayerCharacterStats).GetField("m_foodDepletionRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo drinkRateField = typeof(PlayerCharacterStats).GetField("m_drinkDepletionRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo sleepRateField = typeof(PlayerCharacterStats).GetField("m_sleepDepletionRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_manaHealthReduction = typeof(CharacterStats).GetField("m_manaHealthReduction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_manaStaminaReduction = typeof(CharacterStats).GetField("m_manaStaminaReduction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_manaAugmentation = typeof(CharacterStats).GetField("m_manaAugmentation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_manaPoint = typeof(CharacterStats).GetField("m_manaPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_currentSpellCastType = typeof(Character).GetField("m_currentSpellCastType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_baseContainerCapacity = typeof(ItemContainer).GetField("m_baseContainerCapacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_generalBurdenPenaltyActive = typeof(PlayerCharacterStats).GetField("m_generalBurdenPenaltyActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_generalBurdenRatio = typeof(PlayerCharacterStats).GetField("m_generalBurdenRatio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_staminaUseModifiers = typeof(CharacterStats).GetField("m_staminaUseModifiers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_stabilityRegen = typeof(CharacterStats).GetField("m_stabilityRegen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {   
            //On.CharacterStats.RefreshVitalMaxStat += new On.CharacterStats.hook_RefreshVitalMaxStat(this.CustomDifficultyManaAugmentPatch);
            On.PlayerCharacterStats.OnStart += new On.PlayerCharacterStats.hook_OnAwake(this.CustomDifficultyPlayerStatsPatch);
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.CustomDifficultyBurntStatPatch);
            On.PlayerCharacterStats.OnUpdateWeight += new On.PlayerCharacterStats.hook_OnUpdateWeight(this.CustomDifficultyWeightPatch);
            On.ItemContainer.OnAwake += new On.ItemContainer.hook_OnAwake(this.CustomDifficultyContainerPatch);
        }

        //public void CustomDifficultyManaAugmentPatch(On.CharacterStats.orig_RefreshVitalMaxStat original, CharacterStats instance, bool _updateNeeds = false)
        //{   
        //    if (gameData.ManaStaminaReduction != 0 || gameData.ManaHealthReduction != 0 || gameData.ManaAugment != 0)
        //    {
        //        FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //        Character character = (Character)m_character.GetValue(instance);
        //        Stat maxHealthStat = (Stat)healthField.GetValue(instance);
        //        Stat maxStamina = (Stat)stamField.GetValue(instance);
        //        Stat maxManaStat = (Stat)manaField.GetValue(instance);
        //        StatStack manaHealthReduction = (StatStack)m_manaHealthReduction.GetValue(instance);
        //        StatStack manaStaminaReduction = (StatStack)m_manaStaminaReduction.GetValue(instance);
        //        StatStack manaAugmentation = (StatStack)m_manaAugmentation.GetValue(instance);
        //        if (manaHealthReduction != null && gameData.ManaHealthReduction != 0)
        //        {
        //            manaHealthReduction.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaHealthReduction + character.Inventory.Equipment.GetMaxHealthBonus());
        //        }
        //        if (manaStaminaReduction != null && gameData.ManaStaminaReduction != 0)
        //        {
        //            manaStaminaReduction.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaStaminaReduction);
        //        }
        //        if (manaAugmentation != null && gameData.ManaAugment != 0)
        //        {
        //            manaAugmentation.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaAugment);
        //        }
        //        maxHealthStat.Update();
        //        maxStamina.Update();
        //        maxManaStat.Update();
        //    }
        //    else
        //    {
        //        original.Invoke(instance, _updateNeeds);
        //    }
            
        //}

        public void CustomDifficultyPlayerStatsPatch(On.PlayerCharacterStats.orig_OnAwake original, PlayerCharacterStats instance)
        {   
            original.Invoke(instance);
            applyStatsRegen(instance);
            if (gameData.StamBoost != 0)
            {
                stamField.SetValue(instance, new Stat(instance.MaxStamina + gameData.StamBoost));
            }
            if (gameData.HealthBoost != 0)
            {
                healthField.SetValue(instance, new Stat(instance.MaxHealth + gameData.HealthBoost));
            }
            if (gameData.ManaBoost != 0)
            {
                manaField.SetValue(instance, new Stat(instance.MaxMana + gameData.ManaBoost));
            }
            if (gameData.MoveBoost != 0)
            {
                moveField.SetValue(instance, new Stat(instance.MovementSpeed + gameData.MoveBoost));
            }
            if (gameData.FoodDepleteRate != 0)
            {
                foodRateField.SetValue(instance, new Stat((float)foodRateField.GetValue(instance) - gameData.FoodDepleteRate));
            }
            if (gameData.DrinkDepleteRate != 0)
            {
                drinkRateField.SetValue(instance, new Stat((float)drinkRateField.GetValue(instance) - gameData.DrinkDepleteRate));
            }
            if (gameData.SleepDepleteRate != 0)
            {
                sleepRateField.SetValue(instance, new Stat((float)sleepRateField.GetValue(instance) - gameData.SleepDepleteRate));
            }
            if (gameData.StabRegen != 0)
            {
                m_stabilityRegen.SetValue(instance, new Stat((float)m_stabilityRegen.GetValue(instance) + gameData.StabRegen));
            }
        }

        public void CustomDifficultyWeightPatch(On.PlayerCharacterStats.orig_OnUpdateWeight original, PlayerCharacterStats instance)
        {   
            
            original.Invoke(instance);
            if (gameData.PouchCapacity != 0 || gameData.BackpackCapacity != 0)
            {   
                Character character = (Character)m_character.GetValue(instance);
                Stat s1 = (Stat)moveField.GetValue(instance);
                Stat s2 = (Stat)stamRegenField.GetValue(instance);
                Stat s3 = (Stat)m_staminaUseModifiers.GetValue(instance);
                if ((bool)m_generalBurdenPenaltyActive.GetValue(instance))
                {
                    m_generalBurdenRatio.SetValue(instance,1f);
                    m_generalBurdenPenaltyActive.SetValue(instance, false);
                    s1.RemoveMultiplierStack("Burden");
                    s2.RemoveMultiplierStack("Burden");
                    s3.RemoveMultiplierStack("Burden_Dodge");
                    s3.RemoveMultiplierStack("Burden_Sprint");
                }
                if (!character.Cheats.NotAffectedByWeightPenalties)
                {
                    float totalWeight = character.Inventory.TotalWeight;
                    if (totalWeight > gameData.PouchCapacity+gameData.BackpackCapacity)
                    {
                        m_generalBurdenPenaltyActive.SetValue(instance, true);
                        float num = totalWeight / gameData.PouchCapacity+gameData.BackpackCapacity;
                        if (num != (float)m_generalBurdenRatio.GetValue(instance))
                        {
                            m_generalBurdenRatio.SetValue(instance,num);
                            s1.AddMultiplierStack("Burden", num * -0.02f);
                            s2.AddMultiplierStack("Burden", num * -0.05f);
                            s3.AddMultiplierStack("Burden_Dodge", num * 0.05f, TagSourceManager.Dodge);
                            s3.AddMultiplierStack("Burden_Sprint", num * 0.05f, TagSourceManager.Sprint);
                        }
                    }
                }
                moveField.SetValue(instance,s1);
                stamRegenField.SetValue(instance,s2);
                m_staminaUseModifiers.SetValue(instance,s3);
            }
        }

        public void CustomDifficultyBurntStatPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {   
            if (gameData.BurntStaminaRegen != 0 || gameData.BurntHealthRegen != 0 || gameData.BurntManaRegen != 0)
            {
                Character character = (Character)m_character.GetValue(instance);
                if(gameData.EnableBurntRegenSit)
                {
                    if((Character.SpellCastType)m_currentSpellCastType.GetValue(character) == Character.SpellCastType.Sit)
                    {
                        applyBurntRegen(instance);
                    }
                } 
                else 
                {
                    applyBurntRegen(instance);
                }
            }
            original.Invoke(instance);
        }

        public void applyBurntRegen(PlayerCharacterStats instance)
        {
            if(gameData.BurntStaminaRegen != 0)
            {
                burntStamField.SetValue(instance, Mathf.Clamp((float)burntStamField.GetValue(instance) - gameData.BurntStaminaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxStamina*0.9f));
            }
            if(gameData.BurntHealthRegen != 0)
            {
                burntHealthField.SetValue(instance, Mathf.Clamp((float)burntHealthField.GetValue(instance) - gameData.BurntHealthRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxHealth*0.9f));
            }
            if(gameData.BurntManaRegen != 0)
            {
                burntManaField.SetValue(instance, Mathf.Clamp((float)burntManaField.GetValue(instance) - gameData.BurntManaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxMana*0.5f));
            }
        }

        public void applyStatsRegen(PlayerCharacterStats instance)
        {
            if (gameData.StamRegenRate != 0)
            {
                stamRegenField.SetValue(instance, new Stat(2.4f + gameData.StamRegenRate));
            }
            if (gameData.HealthRegenRate != 0)
            {
                healthRegenField.SetValue(instance, new Stat(instance.HealthRegen + gameData.HealthRegenRate));
            }
            if (gameData.ManaRegenRate != 0)
            {
                manaRegenField.SetValue(instance, new Stat(instance.ManaRegen + gameData.ManaRegenRate));
            }
        }

        public void CustomDifficultyContainerPatch(On.ItemContainer.orig_OnAwake original, ItemContainer instance)
        {
            if (instance.SpecialType == ItemContainer.SpecialContainerTypes.Pouch)
            {
                if (gameData.PouchCapacity != 0)
                {
                    m_baseContainerCapacity.SetValue(instance, (instance.ContainerCapacity + gameData.PouchCapacity));
                }
            }
            else
            {
                if (gameData.BackpackCapacity != 0)
                {
                    m_baseContainerCapacity.SetValue(instance, (instance.ContainerCapacity + gameData.BackpackCapacity));
                }
            }
            original.Invoke(instance);
        }

        private float UpdateDeltaTime<T>(T instance)
        {
            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }

        public GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader("mods/CustomDifficultyConfig.json"))
                {
                    try
                    {
                        GameData gameData = JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                        return gameData;
                    }
                    catch (ArgumentNullException ex)
                    {
                    }
                    catch (FormatException ex)
                    {
                        Debug.Log((object)"Format Exception");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.Log((object)"File Not Found Exception");
            }
            catch (IOException ex)
            {
                Debug.Log((object)"General IO Exception");
            }
            return (GameData)null;
        }
    }
}
