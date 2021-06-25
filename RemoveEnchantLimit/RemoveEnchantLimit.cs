using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RemoveEnchantLimit
{
    [BepInPlugin("sco.savantic.removeenchantlimit", "RemoveEnchantLimit", "0.9.0")]
    public class RemoveEnchantLimit : BaseUnityPlugin
    {
        void Awake()
        {
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("RemoveEnchantLimit");
            myLogSource.LogInfo("Awaken");
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            new Harmony("sco.savantic.removeenchantlimit").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Equipment), "AddEnchantment")]
    class AddEnchantment_Patch
    {
        static bool Prefix(Equipment __instance, ref int _enchantmentID, bool _fromSync = false)
        {
            Enchantment enchantment = ResourcesPrefabManager.Instance.GenerateEnchantment(_enchantmentID, __instance.transform);
            if (!(bool)(UnityEngine.Object)enchantment)
                return false;
            enchantment.ApplyEnchantment(__instance);
            EnchantmentRecipe enchantmentRecipeForId = RecipeManager.Instance.GetEnchantmentRecipeForID(_enchantmentID);
            enchantment.AppliedIncenses = enchantmentRecipeForId.Incenses;
            __instance.m_enchantmentIDs.Add(_enchantmentID);
            __instance.m_activeEnchantments.Add(enchantment);
            __instance.m_enchantmentsHaveChanged = !_fromSync;
            if (_fromSync)
                return false;
            float durabilityRatio = __instance.DurabilityRatio;
            __instance.RefreshEnchantmentModifiers();
            if ((double)durabilityRatio == (double)__instance.DurabilityRatio)
                return false;
            __instance.SetDurabilityRatio(durabilityRatio);
            return false;
        }
    }

    [HarmonyPatch(typeof(Item), "OnReceiveNetworkSync")]
    class OnReceiveNetworkSync_Patch
    {
        static bool Prefix(Item __instance, ref string[] _infos)
        {
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("RemoveEnchantLimit");
            myLogSource.LogInfo("OnReceiveNetworkSync");
            __instance.m_synced = false;
            __instance.m_receivedInfos = _infos;
            __instance.m_lastReceivedExtraData.Clear();
            bool enchantFound = false;
            int enchantIndex = 999;
            string[] strArray1 = __instance.m_receivedInfos[12].Split(';');
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (strArray1[index].Contains("Enchantment"))
                {
                    enchantFound = true;
                    enchantIndex = index;
                }
                if (!strArray1[index].Contains("Enchantment") && strArray1[index].Length > 1)
                {
                    enchantFound = false;
                    enchantIndex = 999;
                }
                if (strArray1[index].Length == 1 && enchantFound && enchantIndex < 999)
                {
                    strArray1[enchantIndex] = strArray1[enchantIndex] + ";" + strArray1[index];
                }
            }
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray1[index]))
                {
                    string[] strArray2 = strArray1[index].Split('/');
                    if (strArray2.Length == 2 && !string.IsNullOrEmpty(strArray2[0]) && !__instance.m_lastReceivedExtraData.ContainsKey(strArray2[0]))
                        __instance.m_lastReceivedExtraData.Add(strArray2[0], strArray2[1]);
                }
            }
            if (!NetworkLevelLoader.Instance.IsOverallLoadingDone && __instance.m_startLoadingInfoTime == -999.0)
                __instance.m_startLoadingInfoTime = Time.time;
            string info = _infos[13];
            if (string.IsNullOrEmpty(info))
                return false;
            string[] strArray3 = info.Split(':');
            for (int index = 0; index < strArray3.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray3[index]))
                {
                    string[] _array = strArray3[index].Split(';');
                    ItemExtension _outValue;
                    if (__instance.m_extensions.TryGetValue(_array[0], out _outValue))
                        _outValue.OnReceiveNetworkSync(_array.Skip(1));
                }
            }
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            return false;
        }
    }

    [HarmonyPatch(typeof(EnchantmentMenu), "TryEnchant")]
    class TryEnchant_Patch
    {
        static bool Prefix(EnchantmentMenu __instance)
        {
            var myLogSource = BepInEx.Logging.Logger.CreateLogSource("RemoveEnchantLimit");
            myLogSource.LogInfo(__instance.m_refItemInChest.Name);
            Equipment equipment = __instance.m_refItemInChest as Equipment;
            myLogSource.LogInfo(equipment.ActiveEnchantments);
            myLogSource.LogInfo(equipment.ActiveEnchantments.Count);
            myLogSource.LogInfo((bool)(UnityEngine.Object)__instance.m_refItemInChest);
            if ((bool)(UnityEngine.Object)__instance.m_refItemInChest)
            {
                int enchantmentId = __instance.GetEnchantmentID();
                myLogSource.LogInfo(enchantmentId);
                myLogSource.LogInfo(__instance.m_refEnchantmentStation.Name);
                if (enchantmentId != -1)
                {
                    __instance.m_refEnchantmentStation.StartEnchanting(enchantmentId);
                    __instance.UpdateProgressVisibility(0.0f);
                }
                else
                    __instance.m_characterUI.ShowInfoNotificationLoc("Notification_Enchantment_NoMatchingRecipe");
            }
            else
                __instance.m_characterUI.ShowInfoNotificationLoc("Notification_Enchantment_EmptyTable");
            BepInEx.Logging.Logger.Sources.Remove(myLogSource);
            return false;
        }
    }

    [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshEnchantmentDetails")]
    class RefreshEnchantmentDetails_Patch
    {
        static bool Prefix(ItemDetailsDisplay __instance)
        {
            int _index = 0;
            if ((bool)(UnityEngine.Object)__instance.m_lastItem && __instance.m_lastItem.IsEnchanted && __instance.m_lastItem is Equipment lastItem)
            {
                for (int i = 0; i < lastItem.ActiveEnchantments.Count; ++i)
                {
                    Enchantment activeEnchantment = lastItem.ActiveEnchantments[i];
                    if (lastItem is Weapon weapon && (activeEnchantment.PresetID == 3 || activeEnchantment.PresetID == 4))
                    {
                        int num1;
                        switch (weapon.Type)
                        {
                            case Weapon.WeaponType.Dagger_OH:
                                num1 = VampiricTransmutationTable.DAGGER_THIRST_THRESHOLD;
                                break;
                            case Weapon.WeaponType.Bow:
                                num1 = VampiricTransmutationTable.BOW_THIRST_THRESHOLD;
                                break;
                            default:
                                num1 = VampiricTransmutationTable.DEFAULT_THIRST_THRESHOLD;
                                break;
                        }
                        float num2 = (float)weapon.DamageDealtTracking / (float)num1;
                        if ((double)num2 >= 0.0 && (double)num2 < 0.330000013113022)
                        {
                            __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("Vampiric_01"), "");
                            ++_index;
                        }
                        else if ((double)num2 >= 0.330000013113022 && (double)num2 < 0.660000026226044)
                        {
                            __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("Vampiric_02"), "");
                            ++_index;
                        }
                        else if ((double)num2 >= 0.660000026226044 && (double)num2 < 1.0)
                        {
                            __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("Vampiric_03"), "");
                            ++_index;
                        }
                        else if ((double)num2 >= 1.0)
                        {
                            __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("Vampiric_04"), "");
                            ++_index;
                        }
                        if (MenuManager.Instance.DisplayDebugInfo)
                        {
                            __instance.GetEnchantmentRow(_index).SetInfo("DEBUG Damage", (float)weapon.DamageDealtTracking);
                            ++_index;
                        }
                    }
                    DamageList _damages1 = new DamageList();
                    for (int index = 0; index < activeEnchantment.Effects.Length; ++index)
                    {
                        if (activeEnchantment.Effects[index] is AddStatusEffectBuildUp effect)
                        {
                            string loc = LocalizationManager.Instance.GetLoc("EnchantmentDescription_InflictStatus", string.Empty);
                            __instance.GetEnchantmentRow(_index).SetInfo(loc, effect.Status.StatusName);
                            ++_index;
                        }
                        else if (activeEnchantment.Effects[index] is AffectStatusEffectBuildUpResistance effect1)
                        {
                            string loc = LocalizationManager.Instance.GetLoc("EnchantmentDescription_StatusResist", effect1.StatusEffect.StatusName);
                            __instance.GetEnchantmentRow(_index).SetInfo(loc, effect1.Value.ToString() + "%");
                            ++_index;
                        }
                        else if (activeEnchantment.Effects[index] is ShootEnchantmentBlast effect2 && lastItem is Weapon weapon1)
                        {
                            DamageType.Types overrideDtype = effect2.BaseBlast.GetComponentInChildren<WeaponDamage>().OverrideDType;
                            float _damage = weapon1.GetDisplayedDamage().TotalDamage * effect2.DamageMultiplier;
                            _damages1.Add(new DamageType(overrideDtype, _damage));
                        }
                    }
                    if (_damages1.Count > 0)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("EnchantmentDescription_Blast"), _damages1);
                        ++_index;
                    }
                    DamageList _damages2 = new DamageList();
                    if (lastItem is Weapon weapon2)
                        _damages2 = weapon2.GetEnchantmentDamageBonuses();
                    if (_damages2.Count > 0)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Damage"), _damages2);
                        ++_index;
                    }
                    if (activeEnchantment.DamageModifier.Count != 0)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_DamageModifier"), activeEnchantment.DamageModifier, _displayPercent: true);
                        ++_index;
                    }
                    if (activeEnchantment.ElementalResistances.Count != 0)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_DamageResistance"), activeEnchantment.ElementalResistances);
                        ++_index;
                    }
                    if ((double)activeEnchantment.GlobalStatusResistance != 0.0)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_StatusResist"), activeEnchantment.GlobalStatusResistance);
                        ++_index;
                    }
                    if ((double)activeEnchantment.ManaAbsorbRatio != 0.0)
                    {
                        string loc = LocalizationManager.Instance.GetLoc("EnchantmentDescription_Absorb", activeEnchantment.ManaAbsorbRatio.ToString(), LocalizationManager.Instance.GetLoc("CharacterStat_Mana"));
                        __instance.GetEnchantmentRow(_index).SetInfo(loc, "");
                        ++_index;
                    }
                    if ((double)activeEnchantment.HealthAbsorbRatio != 0.0)
                    {
                        string loc = LocalizationManager.Instance.GetLoc("EnchantmentDescription_Absorb", activeEnchantment.HealthAbsorbRatio.ToString(), LocalizationManager.Instance.GetLoc("CharacterStat_Health"));
                        __instance.GetEnchantmentRow(_index).SetInfo(loc, "");
                        ++_index;
                    }
                    if ((double)activeEnchantment.StaminaAbsorbRatio != 0.0)
                    {
                        string loc = LocalizationManager.Instance.GetLoc("EnchantmentDescription_Absorb", activeEnchantment.StaminaAbsorbRatio.ToString(), LocalizationManager.Instance.GetLoc("CharacterStat_Stamina"));
                        __instance.GetEnchantmentRow(_index).SetInfo(loc, "");
                        ++_index;
                    }
                    for (int index = 0; index < activeEnchantment.StatModifications.Count; ++index)
                    {
                        float num = activeEnchantment.StatModifications[index].Value;
                        if (activeEnchantment.StatModifications[index].Name == Enchantment.Stat.CooldownReduction)
                            num = -num;
                        string _dataValue = ((double)num > 0.0 ? "+" : "") + num.ToString();
                        if (activeEnchantment.StatModifications[index].Type == Enchantment.StatModification.BonusType.Modifier)
                            _dataValue += "%";
                        switch (activeEnchantment.StatModifications[index].Name)
                        {
                            case Enchantment.Stat.Weight:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Weight"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.Durability:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Durability"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.ManaCostReduction:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_ManaCost"), _dataValue.Replace("+", "-"));
                                ++_index;
                                break;
                            case Enchantment.Stat.StaminaCostReduction:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_StaminaCost"), _dataValue.Replace("+", "-"));
                                ++_index;
                                break;
                            case Enchantment.Stat.CooldownReduction:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_CooldownReduction"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.MovementSpeed:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_MovementPenalty"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.AttackSpeed:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_AttackSpeed"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.Impact:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Impact"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.StabilityRegen:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_StabilityRegen"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.HealthRegen:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_HealthRegen"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.ManaRegen:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_ManaRegen"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.Protection:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_Defense_Protection"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.CorruptionResistance:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_CorruptionResistance"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.FoodDepletionRate:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_FoodEfficiency"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.DrinkDepletionRate:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_DrinkEfficiency"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.SleepDepletionRate:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_SleepEfficiency"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.PouchCapacity:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_PouchCapacity"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.ImpactResistance:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("CharacterStat_Defense_ImpactResistance"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.HeatProtection:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_HeatProtection"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.ColdProtection:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_ColdProtection"), _dataValue);
                                ++_index;
                                break;
                            case Enchantment.Stat.Barrier:
                                __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("ItemStat_Barrier"), _dataValue);
                                ++_index;
                                break;
                        }
                    }
                    if (activeEnchantment.Indestructible)
                    {
                        __instance.GetEnchantmentRow(_index).SetInfo(LocalizationManager.Instance.GetLoc("EnchantmentDescription_Indestructible"), "");
                        ++_index;
                    }
                }
            }
            for (int index = _index; index < __instance.m_enchantmentDetailRows.Count; ++index)
            {
                if (__instance.m_enchantmentDetailRows[index].IsDisplayed)
                    __instance.m_enchantmentDetailRows[index].Hide();
            }
            return false;
        }
    }
}
