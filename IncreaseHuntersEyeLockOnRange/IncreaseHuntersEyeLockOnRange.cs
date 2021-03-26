using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IncreaseHuntersEyeLockOnRange
{
    [BepInPlugin("sco.savantic.increasehunterseyelockonrange", "IncreaseHuntersEyeLockOnRange", "1.0.0")]
    public class IncreaseHuntersEyeLockOnRange : BaseUnityPlugin
    {
        void Awake()
        {
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("IncreaseHuntersEyeLockOnRange");
            myLogSource.LogInfo("Awaken");
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            new Harmony("sco.savantic.increasehunterseyelockonrange").PatchAll();
        }
    }

    [HarmonyPatch(typeof(TargetingSystem))]
    [HarmonyPatch("TrueRange", MethodType.Getter)]
    class TrueRange_Patch
    {
        static bool Prefix(TargetingSystem __instance, ref float __result)
        {
            FieldInfo m_character = typeof(TargetingSystem).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Character character = (Character)m_character.GetValue(__instance);
            if (character != null && character.CurrentWeapon.Type == Weapon.WeaponType.Bow && character.Inventory.SkillKnowledge.IsItemLearned(8205160))
            {
                __result = 100.0f;
                return false;
            }
            if (character != null && character.CurrentWeapon.Type != Weapon.WeaponType.Bow && character.Inventory.SkillKnowledge.IsItemLearned(8205160))
            {
                __result = 40.0f;
                return false;
            }
            if (character != null && character.CurrentWeapon.Type == Weapon.WeaponType.Bow && !character.Inventory.SkillKnowledge.IsItemLearned(8205160))
            {
                __result = 40.0f;
                return false;
            }
            __result = 20.0f;
            return false;
        }
    }
}
