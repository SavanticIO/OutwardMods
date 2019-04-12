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
        FieldInfo pouchField = typeof(CharacterStats).GetField("m_pouchCapacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {   
            On.CharacterStats.RefreshVitalMaxStat += new On.CharacterStats.hook_RefreshVitalMaxStat(this.CustomDifficultyManaAugmentPatch);
            On.PlayerCharacterStats.OnStart += new On.PlayerCharacterStats.hook_OnStart(this.CustomDifficultyPlayerStatsPatch);
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.CustomDifficultyBurntStatPatch);
            On.ItemContainer.OnAwake += new On.ItemContainer.hook_OnAwake(this.CustomDifficultyContainerPatch);
        }

        public void CustomDifficultyManaAugmentPatch(On.CharacterStats.orig_RefreshVitalMaxStat original, CharacterStats instance, bool _updateNeeds = false)
        {   
            if (gameData.ManaStaminaReduction != 0 || gameData.ManaHealthReduction != 0 || gameData.ManaAugment != 0)
            {
                FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Character character = (Character)m_character.GetValue(instance);
                Stat maxHealthStat = (Stat)healthField.GetValue(instance);
                Stat maxStamina = (Stat)stamField.GetValue(instance);
                Stat maxManaStat = (Stat)manaField.GetValue(instance);
                StatStack manaHealthReduction = (StatStack)m_manaHealthReduction.GetValue(instance);
                StatStack manaStaminaReduction = (StatStack)m_manaStaminaReduction.GetValue(instance);
                StatStack manaAugmentation = (StatStack)m_manaAugmentation.GetValue(instance);
                if (manaHealthReduction != null && gameData.ManaHealthReduction != 0)
                {
                    manaHealthReduction.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaHealthReduction + character.Inventory.Equipment.GetMaxHealthBonus());
                }
                if (manaStaminaReduction != null && gameData.ManaStaminaReduction != 0)
                {
                    manaStaminaReduction.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaStaminaReduction);
                }
                if (manaAugmentation != null && gameData.ManaAugment != 0)
                {
                    manaAugmentation.Refresh((int)m_manaPoint.GetValue(instance) * gameData.ManaAugment);
                }
                maxHealthStat.Update();
                maxStamina.Update();
                maxManaStat.Update();
            }
            else
            {
                original.Invoke(instance);
            }
            
        }

        public void CustomDifficultyPlayerStatsPatch(On.PlayerCharacterStats.orig_OnStart original, PlayerCharacterStats instance)
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
            if (gameData.PouchCapacity+gameData.BackpackCapacity != 0)
            {
                pouchField.SetValue(instance, new Stat(instance.PouchCapacity - gameData.BackpackCapacity + gameData.PouchCapacity));
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
        }

        public void CustomDifficultyBurntStatPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {   
            if (gameData.BurntStaminaRegen != 0 || gameData.BurntHealthRegen != 0 || gameData.BurntManaRegen != 0)
            {
                FieldInfo m_currentSpellCastType = instance.GetType().GetField("m_currentSpellCastType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Debug.Log("m_currentSpellCastType Value: "+m_currentSpellCastType.GetValue(instance));
                if(gameData.EnableSit)
                {
                //    if((Character.SpellCastType)m_currentSpellCastType.GetValue(instance) == Character.SpellCastType.Sit)
                //    {
                //        applyBurntRegen(instance);
                //    }
                } 
                else 
                {
                //    applyBurntRegen(instance);
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
                stamRegenField.SetValue(instance, new Stat(instance.StaminaRegen + gameData.StamRegenRate));
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
            if (gameData.BackpackCapacity != 0)
            {
                FieldInfo bagField = typeof(ItemContainer).GetField("m_baseContainerCapacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bagField.SetValue(instance, (instance.ContainerCapacity + gameData.BackpackCapacity));
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
