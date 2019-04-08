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

        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {
            On.PlayerCharacterStats.OnAwake += new On.PlayerCharacterStats.hook_OnAwake(this.CustomDifficultyPlayerStatsPatch);
            On.ItemContainer.OnAwake += new On.ItemContainer.hook_OnAwake(this.CustomDifficultyContainerPatch);
        }

        public void CustomDifficultyPlayerStatsPatch(On.PlayerCharacterStats.orig_OnAwake original, PlayerCharacterStats instance)
        {
            instance.RefreshVitalMaxStat(true);
            FieldInfo stamRegenField = typeof(CharacterStats).GetField("m_staminaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo HealthRegenField = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo ManaRegenField = typeof(CharacterStats).GetField("m_manaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo stamField = typeof(CharacterStats).GetField("m_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo healthField = typeof(CharacterStats).GetField("m_maxHealthStat", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo manaField = typeof(CharacterStats).GetField("m_maxManaStat", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo pouchField = typeof(CharacterStats).GetField("m_pouchCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo moveField = typeof(CharacterStats).GetField("m_movementSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo breakField = typeof(PlayerCharacterStats).GetField("m_usedBreakthroughCount", BindingFlags.Instance | BindingFlags.NonPublic);
            stamRegenField.SetValue(instance, new Stat(instance.StaminaRegen + gameData.StamRegenRate));
            HealthRegenField.SetValue(instance, new Stat(instance.HeatRegenRate + gameData.HealthRegenRate));
            ManaRegenField.SetValue(instance, new Stat(instance.ManaRegen + gameData.ManaRegenRate));
            stamField.SetValue(instance, new Stat(instance.MaxStamina + gameData.StamBoost));
            healthField.SetValue(instance, new Stat(instance.MaxHealth + gameData.HealthBoost));
            manaField.SetValue(instance, new Stat(instance.MaxMana + gameData.ManaBoost));
            pouchField.SetValue(instance, new Stat(instance.PouchCapacity - gameData.BackpackCapacity + gameData.PouchCapacity));
            moveField.SetValue(instance, new Stat(instance.MovementSpeed + gameData.MoveBoost));
            original.Invoke(instance);
        }

        public void CustomDifficultyContainerPatch(On.ItemContainer.orig_OnAwake original, ItemContainer instance)
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
