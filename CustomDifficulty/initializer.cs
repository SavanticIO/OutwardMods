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

        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {   
            On.CharacterStats.OnRefreshVitalMaxStat += new On.CharacterStats.hook_OnRefreshVitalMaxStat(this.CustomDifficultyManaAugmentPatch);
            On.PlayerCharacterStats.OnAwake += new On.PlayerCharacterStats.hook_OnAwake(this.CustomDifficultyPlayerStatsPatch);
            On.ItemContainer.OnAwake += new On.ItemContainer.hook_OnAwake(this.CustomDifficultyContainerPatch);
            On.PlayerCharacterStats.OnStart += new On.PlayerCharacterStats.hook_OnStart(this.CustomDifficultyPlayerNeedsPatch);
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.CustomDifficultyPlayerBurntPatch);
        }

        public void CustomDifficultyManaAugmentPatch(On.CharacterStats.orig_OnRefreshVitalMaxStat original, CharacterStats instance)
        {   
            FieldInfo m_character = instance.GetType().GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Character character = (Character)field.GetValue(instance);
            Stat maxHealthStat = (Stat)healthField.GetValue(instance);
            Stat maxStamina = (Stat)stamField.GetValue(instance);
            Stat maxManaStat = (Stat)manaField.GetValue(instance);
            StatStack manaHealthReduction = (StatStack)m_manaHealthReduction.GetValue(instance);
            StatStack manaStaminaReduction = (StatStack)m_manaStaminaReduction.GetValue(instance);
            StatStack manaAugmentation = (StatStack)m_manaAugmentation.GetValue(instance);
            if (manaHealthReduction != null)
            {
                manaHealthReduction.Refresh((float)m_manaPoint.GetValue(instance) * gameData.ManaHealthReduction + character.Inventory.Equipment.GetMaxHealthBonus());
            }
            if (manaStaminaReduction != null)
            {
                manaStaminaReduction.Refresh((float)m_manaPoint.GetValue(instance) * gameData.ManaStaminaReduction);
            }
            if (manaAugmentation != null)
            {
                manaAugmentation.Refresh((float)m_manaPoint.GetValue(instance) * gameData.ManaAugment);
            }
            maxHealthStat.Update();
		    maxStamina.Update();
		    maxManaStat.Update();
        }
        public void CustomDifficultyPlayerStatsPatch(On.PlayerCharacterStats.orig_OnAwake original, PlayerCharacterStats instance)
        {
            instance.RefreshVitalMaxStat(true);
            stamRegenField.SetValue(instance, new Stat(instance.StaminaRegen + gameData.StamRegenRate));
            healthRegenField.SetValue(instance, new Stat(instance.HealthRegen + gameData.HealthRegenRate));
            manaRegenField.SetValue(instance, new Stat(instance.ManaRegen + gameData.ManaRegenRate));
            stamField.SetValue(instance, new Stat(instance.MaxStamina + gameData.StamBoost));
            healthField.SetValue(instance, new Stat(instance.MaxHealth + gameData.HealthBoost));
            manaField.SetValue(instance, new Stat(instance.MaxMana + gameData.ManaBoost));
            pouchField.SetValue(instance, new Stat(instance.PouchCapacity - gameData.BackpackCapacity + gameData.PouchCapacity));
            moveField.SetValue(instance, new Stat(instance.MovementSpeed + gameData.MoveBoost));
            original.Invoke(instance);
        }
        public void CustomDifficultyPlayerNeedsPatch(On.PlayerCharacterStats.orig_OnStart original, PlayerCharacterStats instance)
        {
            foodRateField.SetValue(instance, new Stat(foodRateField.GetValue(instance) - gameData.FoodDepleteRate));
            drinkRateField.SetValue(instance, new Stat(foodRateField.GetValue(instance) - gameData.DrinkDepleteRate));
            sleepRateField.SetValue(instance, new Stat(foodRateField.GetValue(instance) - gameData.SleepDepleteRate));
            original.Invoke(instance);
        }

        public void CustomDifficultyPlayerBurntPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {   
            FieldInfo m_currentSpellCastType = instance.GetType().GetField("m_currentSpellCastType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo m_character = instance.GetType().GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Character character = (Character)field.GetValue(instance);
            if(gameData.EnableSit)
            {
                if(m_currentSpellCastType.GetValue(instance) == Character.SpellCastType.Sit)
                {
                    applyBurntRegen(instance,character);
                }
            } 
            else 
            {
                applyBurntRegen(instance,character);
            }
            original.Invoke(instance);
        }

        public void applyBurntRegen<T>(T instance, Character character)
        {
            if(gameData.BurntStaminaRegen > 0f)
            {
                burntStamField.SetValue(instance, Mathf.Clamp(burntStamField.GetValue(instance) - gameData.BurntStaminaRegen * character.UpdateDeltaTime, 0f, instance.ActiveMaxStamina*0.9f));
            }
            if(gameData.BurntHealthRegen > 0f)
            {
                burntHealthField.SetValue(instance, Mathf.Clamp(burntHealthField.GetValue(instance) - gameData.BurntHealthRegen * character.UpdateDeltaTime, 0f, instance.ActiveMaxHealth*0.9f));
            }
            if(gameData.BurntManaRegen > 0f)
            {
                burntManaField.SetValue(instance, Mathf.Clamp(burntManaField.GetValue(instance) - gameData.BurntManaRegen * character.UpdateDeltaTime, 0f, instance.ActiveMaxMana*0.5f));
            }
        }
        public void CustomDifficultyContainerPatch(On.ItemContainer.orig_OnAwake original, PlayerCharacterStats instance)
        {
            FieldInfo bagField = typeof(ItemContainer).GetField("m_baseContainerCapacity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bagField.SetValue(instance, (instance.ContainerCapacity + gameData.BackpackCapacity));
            original.Invoke(instance);
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
