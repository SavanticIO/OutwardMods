using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Meditation
{
    [BepInPlugin("sco.savantic.meditation", "Meditation", "2.0.0")]
    public class Meditation : BaseUnityPlugin
    {
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
            BurntStaminaRegen = Config.Bind("General",
                                     "BurntStaminaRegen",
                                     0.5f,
                                     "How quickly burnt stamina will regen while siting. Default: 0.5f");
            BurntHealthRegen = Config.Bind("General",
                                     "BurntHealthRegen",
                                     0.5f,
                                     "How quickly burnt health will regen while siting. Default: 0.5f");
            BurntManaRegen = Config.Bind("General",
                                     "BurntManaRegen",
                                     0.5f,
                                     "How quickly burnt Mana will regen while siting. Default: 0.5f");
            CurrentStaminaRegen = Config.Bind("General",
                                     "CurrentStaminaRegen",
                                     1.0f,
                                     "How quickly stamina will regen while siting. Default: 1.0f");
            CurrentHealthRegen = Config.Bind("General",
                                     "CurrentHealthRegen",
                                     1.0f,
                                     "How quickly health will regen while siting. Default: 1.0f");
            CurrentManaRegen = Config.Bind("General",
                                     "CurrentManaRegen",
                                     1.0f,
                                     "How quickly mana will regen while siting. Default: 1.0f");
            EnableSitting = Config.Bind("General",
                                     "EnableSitting",
                                     true,
                                     "Ability to toggle sitting from this mod if you prefer another mods implimentation. Default: true");
            SitKey = Config.Bind("General",
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
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("Meditation");

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

            bool burntRegenEnabled = Meditation.BurntStaminaRegen.Value != 0 || Meditation.BurntHealthRegen.Value != 0 || Meditation.BurntManaRegen.Value != 0;
            bool currentRegenEnabled = Meditation.CurrentStaminaRegen.Value != 0 || Meditation.CurrentHealthRegen.Value != 0 || Meditation.CurrentManaRegen.Value != 0;

            Character character = (Character)m_character.GetValue(__instance);
            if (character.CurrentSpellCast == Character.SpellCastType.Sit)
            {
                myLogSource.LogInfo(burntRegenEnabled);
                myLogSource.LogInfo(currentRegenEnabled);
                if (burntRegenEnabled)
                {
                    if (Meditation.BurntStaminaRegen.Value != 0)
                    {
                        burntStamField.SetValue(__instance, Mathf.Clamp((float)burntStamField.GetValue(__instance) - Meditation.BurntStaminaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxStamina * 0.9f));
                        myLogSource.LogInfo("BurntStam: " + Meditation.BurntStaminaRegen.Value);
                        myLogSource.LogInfo("BurntStam: " + Meditation.BurntStaminaRegen.Value * UpdateDeltaTime(__instance));
                        myLogSource.LogInfo("BurntStam: "+Mathf.Clamp((float)burntStamField.GetValue(__instance) - Meditation.BurntStaminaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxStamina * 0.9f));
                    }
                    if (Meditation.BurntHealthRegen.Value != 0)
                    {
                        burntHealthField.SetValue(__instance, Mathf.Clamp((float)burntHealthField.GetValue(__instance) - Meditation.BurntHealthRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxHealth * 0.9f));
                        myLogSource.LogInfo(Mathf.Clamp((float)burntHealthField.GetValue(__instance) - Meditation.BurntHealthRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxHealth * 0.9f));
                    }
                    if (Meditation.BurntManaRegen.Value != 0)
                    {
                        burntManaField.SetValue(__instance, Mathf.Clamp((float)burntManaField.GetValue(__instance) - Meditation.BurntManaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxMana * 0.5f));
                        myLogSource.LogInfo(Mathf.Clamp((float)burntManaField.GetValue(__instance) - Meditation.BurntManaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxMana * 0.5f));
                    }
                }
                if (currentRegenEnabled)
                {
                    if (Meditation.CurrentStaminaRegen.Value != 0)
                    {
                        curStamField.SetValue(__instance, Mathf.Clamp((float)curStamField.GetValue(__instance) + Meditation.CurrentStaminaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxStamina));
                        myLogSource.LogInfo(Mathf.Clamp((float)curStamField.GetValue(__instance) + Meditation.CurrentStaminaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxStamina));
                    }
                    if (Meditation.CurrentHealthRegen.Value != 0)
                    {
                        curHealthField.SetValue(__instance, Mathf.Clamp((float)curHealthField.GetValue(__instance) + Meditation.CurrentHealthRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxHealth));
                        myLogSource.LogInfo(Mathf.Clamp((float)curHealthField.GetValue(__instance) + Meditation.CurrentHealthRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxHealth));
                    }
                    if (Meditation.CurrentManaRegen.Value != 0)
                    {
                        curManaField.SetValue(__instance, Mathf.Clamp((float)curManaField.GetValue(__instance) + Meditation.CurrentManaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxMana));
                        myLogSource.LogInfo(Mathf.Clamp((float)curManaField.GetValue(__instance) + Meditation.CurrentManaRegen.Value * UpdateDeltaTime(__instance), 0f, __instance.ActiveMaxMana));
                    }
                }
            }
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            return true;
        }

        public static float UpdateDeltaTime(PlayerCharacterStats instance)
        {
            // Point in time that this class was initialised.
            FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // If the game is paused, return 0, otherwise return the difference between now and the last update time.
            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }
    }

    [HarmonyPatch(typeof(LocalCharacterControl), "UpdateInteraction")]
    class UpdateInteraction_Patch
    {
        static void Postfix(LocalCharacterControl __instance)
        {
            if (Meditation.EnableSitting.Value)
            {
                if (__instance.InputLocked)
                {
                    return;
                }
                if (Meditation.SitKey.Value.IsDown())
                {
                    __instance.Character.CastSpell(Character.SpellCastType.Sit, __instance.Character.gameObject, Character.SpellCastModifier.Immobilized, 1, -1f);
                }
            }
        }

    }
}
