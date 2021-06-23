using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Meditation
{
    [BepInPlugin("sco.savantic.meditation", "Meditation", "2.0.3")]
    public class Meditation : BaseUnityPlugin
    {
        public static ConfigEntry<bool> EnableBurntSitRegen;
        public static ConfigEntry<bool> EnableCurrentSitRegen;
        public static ConfigEntry<float> BurntStaminaRegen;
        public static ConfigEntry<float> BurntHealthRegen;
        public static ConfigEntry<float> BurntManaRegen;
        public static ConfigEntry<float> CurrentStaminaRegen;
        public static ConfigEntry<float> CurrentHealthRegen;
        public static ConfigEntry<float> CurrentManaRegen;
        public static ConfigEntry<bool> EnableSitting;
        public static ConfigEntry<KeyboardShortcut> SitKey;
        void Awake()
        {
            EnableBurntSitRegen = Config.Bind("Burnt Stat Regen",
                                     "EnableBurntSitRegen",
                                     true,
                                     "Enable or disable the regeneration of burnt stats while sitting");
            EnableCurrentSitRegen = Config.Bind("Current Stat Regen",
                                     "EnableCurrentSitRegen",
                                     true,
                                     "Enable or disable the regeneration of current(non-burnt) stats while sitting");
            BurntStaminaRegen = Config.Bind("Burnt Stat Regen",
                                     "BurntStaminaRegen",
                                     0.5f,
                                     "How quickly burnt stamina will regen while siting. Default: 0.5f");
            BurntHealthRegen = Config.Bind("Burnt Stat Regen",
                                     "BurntHealthRegen",
                                     0.5f,
                                     "How quickly burnt health will regen while siting. Default: 0.5f");
            BurntManaRegen = Config.Bind("Burnt Stat Regen",
                                     "BurntManaRegen",
                                     0.5f,
                                     "How quickly burnt Mana will regen while siting. Default: 0.5f");
            CurrentStaminaRegen = Config.Bind("Current Stat Regen",
                                     "CurrentStaminaRegen",
                                     1.0f,
                                     "How quickly stamina will regen while siting. Default: 1.0f");
            CurrentHealthRegen = Config.Bind("Current Stat Regen",
                                     "CurrentHealthRegen",
                                     1.0f,
                                     "How quickly health will regen while siting. Default: 1.0f");
            CurrentManaRegen = Config.Bind("Current Stat Regen",
                                     "CurrentManaRegen",
                                     1.0f,
                                     "How quickly mana will regen while siting. Default: 1.0f");
            EnableSitting = Config.Bind("Sitting",
                                     "EnableSitting",
                                     true,
                                     "Ability to toggle sitting from this mod if you prefer another mods implimentation. Default: true");
            SitKey = Config.Bind("Sitting",
                                     "SitKey",
                                     new KeyboardShortcut(KeyCode.X),
                                     "Keyboard shortcut for the sitting action. Default: X");
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("Meditation");
            myLogSource.LogInfo("Awaken");
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            new Harmony("sco.savantic.meditation").PatchAll();
        }
    }

    [HarmonyPatch(typeof(PlayerCharacterStats), "OnUpdateStats")]
    class OnUpdateStats_Patch
    {
        static bool Prefix(PlayerCharacterStats __instance)
        {
            FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);        

            Character character = (Character)m_character.GetValue(__instance);
            if (character.CurrentSpellCast == Character.SpellCastType.Sit)
            {
                ApplySittingRegen(__instance);
            }
            return true;
        }

        private static void ApplySittingRegen(PlayerCharacterStats instance)
        {
            if (Meditation.EnableBurntSitRegen.Value)
            {
                UpdateStats(instance, "m_burntStamina", Meditation.BurntStaminaRegen.Value, instance.MaxStamina, "BURNT");
                UpdateStats(instance, "m_burntHealth", Meditation.BurntHealthRegen.Value, instance.MaxHealth, "BURNT");
                UpdateStats(instance, "m_burntMana", Meditation.BurntManaRegen.Value, instance.MaxMana, "BURNT");
            }
            if (Meditation.EnableCurrentSitRegen.Value)
            {
                UpdateStats(instance, "m_stamina", Meditation.CurrentStaminaRegen.Value, instance.ActiveMaxStamina, "CURRENT");
                UpdateStats(instance, "m_health", Meditation.CurrentHealthRegen.Value, instance.ActiveMaxHealth, "CURRENT");
                UpdateStats(instance, "m_mana", Meditation.CurrentManaRegen.Value, instance.ActiveMaxMana, "CURRENT");
            }                   
        }

        private static float GetUpdateValue(PlayerCharacterStats instance, FieldInfo field, float configValue, string statType)
        {
            float value;
            switch (statType)
            {
                case "CURRENT":
                    value = configValue;
                    break;
                case "BURNT":
                    value = -configValue;
                    break;
                default:
                    value = 0.0f;
                    break;
            }
            return (float)field.GetValue(instance) + value * UpdateDeltaTime(instance);
        }

        private static void UpdateStats(PlayerCharacterStats instance, string fieldName, float configValue, float maxValue, string statType)
        {
            FieldInfo field = typeof(CharacterStats).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (configValue != 0)
            {
                float updateValue = GetUpdateValue(instance, field, configValue, statType);
                field.SetValue(instance, Mathf.Clamp(updateValue, 0f, maxValue));
            }
        }

        public static float UpdateDeltaTime(PlayerCharacterStats instance)
        {
            FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }
    }

    [HarmonyPatch(typeof(LocalCharacterControl), "UpdateInteraction")]
    class UpdateInteraction_Patch
    {
        static void Postfix(LocalCharacterControl __instance)
        {
            if (__instance.InputLocked)
            {
                return;
            }
            if (Meditation.EnableSitting.Value && Meditation.SitKey.Value.IsDown())
            {
                __instance.Character.CastSpell(Character.SpellCastType.Sit, __instance.Character.gameObject, Character.SpellCastModifier.Immobilized, 1, -1f);
            }
        }
    }
}
