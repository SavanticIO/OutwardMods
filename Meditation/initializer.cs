using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Meditation
{
    public class initializer : MonoBehaviour
    {
        // Unity GameObject containing this class as a component.
        public static Meditation mod;

        // Parsed contents of the configuration file.
        GameData gameData;

        // Point in time that this class was initialised.
        FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Current spell (for sitting check).
        FieldInfo m_currentSpellCastType = typeof(Character).GetField("m_currentSpellCastType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Character reference.
        FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Burnt stats current values.
        FieldInfo burntStamField = typeof(CharacterStats).GetField("m_burntStamina", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntHealthField = typeof(CharacterStats).GetField("m_burntHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo burntManaField = typeof(CharacterStats).GetField("m_burntMana", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Rate of current value regeneration.
        FieldInfo curStamField = typeof(CharacterStats).GetField("m_stamina", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo curHealthField = typeof(CharacterStats).GetField("m_health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo curManaField = typeof(CharacterStats).GetField("m_mana", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Use Monobehaviour's `Initialize` function.
        /// </summary>
        public void Initialize()
        {
            gameData = this.LoadSettings();
            this.Patch();
        }

        /// <summary>
        /// Hook onto the PlayerCharacterStats instance and add a hook to run the patch when stats are being updated. 
        /// </summary>
        public void Patch()
        {
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.MeditationBurntStatPatch);
        }

        /// <summary>
        /// Regens stats if any of them aren't at full.
        /// </summary>
        /// <param name="original">Update stats function which this is hooked onto.</param>
        /// <param name="instance">Instance of the player's stats.</param>
        public void MeditationBurntStatPatch(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {
            bool burntRegenEnabled = gameData.BurntStaminaRegen != 0 || gameData.BurntHealthRegen != 0 || gameData.BurntManaRegen != 0;
            bool currentRegenEnabled = gameData.CurrentStaminaRegen != 0 || gameData.CurrentHealthRegen != 0 || gameData.CurrentManaRegen != 0;
            
            if (burntRegenEnabled || currentRegenEnabled)
            {
                // Get the player.
                Character character = (Character)m_character.GetValue(instance);

                // Apply regen if we're sitting.
                if ((Character.SpellCastType)m_currentSpellCastType.GetValue(character) == Character.SpellCastType.Sit)
                {
                    if (burntRegenEnabled)
                    {
                        applyBurntRegen(instance);
                    }
                    if (currentRegenEnabled)
                    {
                        applyCurrentRegen(instance);
                    }
                }
            }
            
            // Call the hooked function with the overridden player stats.
            original.Invoke(instance);
        }

        /// <summary>
        /// Regens burnt stats by the amount requested within the config file (per second), until the burn has completely worn off.
        /// </summary>
        /// <param name="instance">Instance of the player's stats.</param>
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

        /// <summary>
        /// Sets the current regen amount to the amount specified within the config file (per second).
        /// Doesn't need to handle maximum values as the game does that internally.
        /// </summary>
        /// <param name="instance">Instance of the player's stats.</param>
        public void applyCurrentRegen(PlayerCharacterStats instance)
        {
            if (gameData.CurrentStaminaRegen != 0)
            {
                curStamField.SetValue(instance, Mathf.Clamp((float)curStamField.GetValue(instance) + gameData.CurrentStaminaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxStamina));
            }
            if (gameData.CurrentHealthRegen != 0)
            {
                curHealthField.SetValue(instance, Mathf.Clamp((float)curHealthField.GetValue(instance) + gameData.CurrentHealthRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxHealth));
            }
            if (gameData.CurrentManaRegen != 0)
            {
                curManaField.SetValue(instance, Mathf.Clamp((float)curManaField.GetValue(instance) + gameData.CurrentManaRegen * UpdateDeltaTime(instance), 0f, instance.ActiveMaxMana));
            }
        }

        /// <summary>
        /// Decide whether or not the regeneration should occur based on whether or not the game is paused.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">Always a PlayerCharacterStats instance.</param>
        /// <returns></returns>
        private float UpdateDeltaTime<T>(T instance)
        {
            // If the game is paused, return 0, otherwise return the difference between now and the last update time.
            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }

        /// <summary>
        /// Loads the GameData from the configuration file located within the mods folder.
        /// </summary>
        /// <returns>Populated GameData.</returns>
        public GameData LoadSettings()
        {
            try
            {
                // Read the configuration file (path is relative to exe dir).
                using (StreamReader streamReader = new StreamReader("Mods/MeditationConfig.json"))
                {
                    try
                    {
                        GameData gameData = JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                        return gameData;
                    }
                    catch (ArgumentNullException ex)
                    {
                        Debug.Log((object)"Argument null exception");
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
            catch (Exception ex)
            {
                Debug.Log("Meditation exception: " + ex.Message);
            }

            // If it's made it this far something is wrong, return null.
            return (GameData)null;
        }
    }
}
