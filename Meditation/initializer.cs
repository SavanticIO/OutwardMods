using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Meditation
{
    public class initializer : MonoBehaviour
    {
        public static Meditation mod;

        GameData gameData;

        FieldInfo burntStamField = typeof(CharacterStats).GetField("m_burntStamina", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntHealthField = typeof(CharacterStats).GetField("m_burntHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntManaField = typeof(CharacterStats).GetField("m_burntMana", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        public void Patch()
        {
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.MeditationBurntStatPatch);
        }
        public void MeditationBurntStatPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {
            if (gameData.BurntStaminaRegen != 0 || gameData.BurntHealthRegen != 0 || gameData.BurntManaRegen != 0)
            {
                FieldInfo m_currentSpellCastType = typeof(Character).GetField("m_currentSpellCastType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Character character = (Character)m_character.GetValue(instance);
                if ((Character.SpellCastType)m_currentSpellCastType.GetValue(character) == Character.SpellCastType.Sit)
                {
                    applyBurntRegen(instance);
                }
            }
            original.Invoke(instance);
        }

        public void applyBurntRegen(PlayerCharacterStats instance)
        {
            if (gameData.BurntStaminaRegen != 0)
            {
                burntStamField.SetValue(instance, Mathf.Clamp((float)burntStamField.GetValue(instance) - gameData.BurntStaminaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxStamina * 0.9f));
            }
            if (gameData.BurntHealthRegen != 0)
            {
                burntHealthField.SetValue(instance, Mathf.Clamp((float)burntHealthField.GetValue(instance) - gameData.BurntHealthRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxHealth * 0.9f));
            }
            if (gameData.BurntManaRegen != 0)
            {
                burntManaField.SetValue(instance, Mathf.Clamp((float)burntManaField.GetValue(instance) - gameData.BurntManaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxMana * 0.5f));
            }
        }

        private float UpdateDeltaTime<T>(T instance)
        {
            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }

        public GameData LoadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader("mods/MeditationConfig.json"))
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
