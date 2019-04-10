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

        FieldInfo stamRegenField = typeof(CharacterStats).GetField("m_staminaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo healthRegenField = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo manaRegenField = typeof(CharacterStats).GetField("m_manaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo stamField = typeof(CharacterStats).GetField("m_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo healthField = typeof(CharacterStats).GetField("m_maxHealthStat", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo manaField = typeof(CharacterStats).GetField("m_maxManaStat", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo pouchField = typeof(CharacterStats).GetField("m_pouchCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo moveField = typeof(CharacterStats).GetField("m_movementSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo burntStamField = typeof(CharacterStats).GetField("m_burntStamina", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo burntHealthField = typeof(CharacterStats).GetField("m_burntHealth", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo burntManaField = typeof(CharacterStats).GetField("m_burntMana", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo foodRateField = typeof(PlayerCharacterStats).GetField("m_foodDepletionRate", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo drinkRateField = typeof(PlayerCharacterStats).GetField("m_drinkDepletionRate", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo sleepRateField = typeof(PlayerCharacterStats).GetField("m_sleepDepletionRate", BindingFlags.Instance | BindingFlags.NonPublic);

        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {
            On.PlayerCharacterStats.OnAwake += new On.PlayerCharacterStats.hook_OnAwake(this.CustomDifficultyPlayerStatsPatch);
            On.ItemContainer.OnAwake += new On.ItemContainer.hook_OnAwake(this.CustomDifficultyContainerPatch);
            On.PlayerCharacterStats.OnStart += new On.PlayerCharacterStats.hook_OnStart(this.CustomDifficultyPlayerNeedsPatch);
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.CustomDifficultyPlayerBurntPatch);
        }

        public void CustomDifficultyPlayerStatsPatch(On.PlayerCharacterStats.orig_OnAwake original, PlayerCharacterStats instance)
        {
            instance.RefreshVitalMaxStat(true);
            stamRegenField.SetValue(instance, new Stat(instance.StaminaRegen + gameData.StamRegenRate));
            healthRegenField.SetValue(instance, new Stat(instance.HeatRegenRate + gameData.HealthRegenRate));
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
            foodRateField.SetValue(instance, new Stat(instance.FoodDepleteRate - gameData.FoodDepleteRate));
            drinkRateField.SetValue(instance, new Stat(instance.DrinkDepleteRate - gameData.DrinkDepleteRate));
            sleepRateField.SetValue(instance, new Stat(instance.SleepDepleteRate - gameData.SleepDepleteRate));
            original.Invoke(instance);
        }

        public void CustomDifficultyPlayerBurntPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {   
            if(gameData.BurntStaminaRegen > 0f)
            {
                burntStamField.SetValue(instance, Mathf.Clamp(instance.StaminaBurn - gameData.BurntStaminaRegen * instance.UpdateDeltaTime, 0f, instance.ActiveMaxStamina*0.9f));
            }
            if(gameData.BurntHealthRegen > 0f)
            {
                burntHealthField.SetValue(instance, Mathf.Clamp(instance.HealthBurn - gameData.BurntHealthRegen * instance.UpdateDeltaTime, 0f, instance.ActiveMaxHealth*0.9f));
            }
            if(gameData.BurntManaRegen > 0f)
            {
                burntManaField.SetValue(instance, Mathf.Clamp(instance.ManaBurn - gameData.BurntManaRegen * instance.UpdateDeltaTime, 0f, instance.ActiveMaxMana*0.5f));
            }
            original.Invoke(instance);
        }
        public void CustomDifficultyContainerPatch(On.ItemContainer.orig_OnAwake original, PlayerCharacterStats instance)
        {
            FieldInfo bagField = typeof(ItemContainer).GetField("m_baseContainerCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
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
