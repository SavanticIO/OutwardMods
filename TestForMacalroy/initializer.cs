using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace Test
{
    public class initializer : MonoBehaviour
    {
        GameData gameData;

        public static Test mod;

        internal static Test Mod { get => mod; set => mod = value; }

        public void Initialize()
        {
            gameData = LoadSettings();
            Patch();
        }

        public void Patch()
        {
            On.PlayerCharacterStats.OnAwake += new On.PlayerCharacterStats.hook_OnAwake(StatPatch);
        }

        public void StatPatch(On.PlayerCharacterStats.orig_OnAwake original, PlayerCharacterStats instance)
        {
            Debug.Log("working");

            Debug.Log("settings loaded");
            instance.RefreshVitalMaxStat(true);

            LoadStats(instance);
            Debug.Log("stats successfully loaded");

            Debug.Log(JsonUtility.ToJson(new GameData()));

            original.Invoke(instance);

        }

        public GameData LoadSettings()
        {
            using (StreamReader streamReader = new StreamReader("mods/CustomStats.json"))
            {
                try
                {
                    GameData gameDat = JsonUtility.FromJson<GameData>(streamReader.ReadToEnd());
                    Debug.Log("success loading into streamreader" + gameDat);
                    return gameDat;
                }
                catch (FileNotFoundException ex)
                {
                    Debug.Log("File not found");
                }
                catch (ArgumentNullException ex)
                {
                    Debug.Log("Null argument");
                }
                catch (FormatException ex2)
                {
                    Debug.Log("Format Exception");
                }
            }
            return null;
        }

        public void LoadStats(CharacterStats instance)
        {
            instance.RefreshVitalMaxStat(true);
            System.IO.StreamWriter file = new System.IO.StreamWriter("mods/Debug.txt");
            FieldInfo movSpeed = typeof(CharacterStats).GetField("m_movementSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo maxHealth = typeof(CharacterStats).GetField("m_maxHealthStat", BindingFlags.Instance | BindingFlags.NonPublic);
            // FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo maxStamina = typeof(CharacterStats).GetField("m_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo staminaRegen = typeof(CharacterStats).GetField("m_staminaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            /*FieldInfo staminaCostRedux = typeof(CharacterStats).GetField("m_staminaCostReduction", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo maxMana = typeof(CharacterStats).GetField("m_maxManaStat", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo manaRegen = typeof(CharacterStats).GetField("m_manaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo impact = typeof(CharacterStats).GetField("m_impactModifier", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo allDamages = typeof(CharacterStats).GetField("m_damagesModifier", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo phyiscalDam = typeof(CharacterStats).GetField("m_damageTypesModifier[0]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo etherealDam = typeof(CharacterStats).GetField("m_damageTypesModifier[1]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo decayDamage = typeof(CharacterStats).GetField("m_damageTypesModifier[2]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo electricDam = typeof(CharacterStats).GetField("m_damageTypesModifier[3]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo frostDamage = typeof(CharacterStats).GetField("m_damageTypesModifier[4]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fireDamage = typeof(CharacterStats).GetField("m_damageTypesModifier[5]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo darkDamage = typeof(CharacterStats).GetField("m_damageTypesModifier[2]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo lightDamage = typeof(CharacterStats).GetField("m_damageTypesModifier[3]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo physicalProt = typeof(CharacterStats).GetField("m_damageProtection[0]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo etherealProt = typeof(CharacterStats).GetField("m_damageProtection[1]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo decayProt = typeof(CharacterStats).GetField("m_damageProtection[2]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo electricProt = typeof(CharacterStats).GetField("m_damageProtection[3]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo frostProt = typeof(CharacterStats).GetField("m_damageProtection[4]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fireProt = typeof(CharacterStats).GetField("m_damageProtection[5]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo darkProt = typeof(CharacterStats).GetField("m_damageProtection[6]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo lightProt = typeof(CharacterStats).GetField("m_damageProtection[7]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo allResist = typeof(CharacterStats).GetField("m_resistanceModifiers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo physResist = typeof(CharacterStats).GetField("m_damageResistance[0]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo etherealResist = typeof(CharacterStats).GetField("m_damageResistance[1]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo decayResist = typeof(CharacterStats).GetField("m_damageResistance[2]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo electricResist = typeof(CharacterStats).GetField("m_damageResistance[3]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo frostResist = typeof(CharacterStats).GetField("m_damageResistance[4]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fireResist = typeof(CharacterStats).GetField("m_damageResistance[5]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo darkResist = typeof(CharacterStats).GetField("m_damageResistance[6]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo lightResist = typeof(CharacterStats).GetField("m_damageResistance[7]", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo impactResist = typeof(CharacterStats).GetField("m_impactResistance", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo stabilityRegen = typeof(CharacterStats).GetField("m_stabilityRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo envColdProt = typeof(CharacterStats).GetField("m_coldProtection", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo envHeatProt = typeof(CharacterStats).GetField("m_heatProtection", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo coldRegen = typeof(CharacterStats).GetField("m_coldRegenRate", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo heatRegen = typeof(CharacterStats).GetField("m_heatRegenRate", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo waterProof = typeof(CharacterStats).GetField("m_waterproof", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo corruptProt = typeof(CharacterStats).GetField("m_corruptionProtection", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo tempModifier = typeof(CharacterStats).GetField("m_temperatureModifier", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo speed = typeof(CharacterStats).GetField("m_speedModifier", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo attackSpeed = typeof(CharacterStats).GetField("m_attackSpeedModifier", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo dectectability = typeof(CharacterStats).GetField("m_detectability", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo visualDetectability = typeof(CharacterStats).GetField("m_visualDetectability", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo pouchCapacity = typeof(CharacterStats).GetField("m_pouchCapacity", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo foodEffectEfficiency = typeof(CharacterStats).GetField("m_foodEffectEfficiency", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo runicSwordLifeSpan = typeof(RunicBlade).GetField("SummonLifeSpan", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo maxHealth = typeof(CharacterStats).GetField("m_maxHealthStat", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            //FieldInfo healthRegen = typeof(CharacterStats).GetField("m_healthRegen", BindingFlags.Instance | BindingFlags.NonPublic);*/

            //maxHealth.SetValue(instance, new Stat(instance.MaxHealth + gameData.MaxHealth));
            //healthRegen.SetValue(instance, new Stat(instance.HealthRegen + gameData.HealthRegen));
            movSpeed.SetValue(instance, new Stat(instance.MovementSpeed + gameData.MovementSpeed));
            //maxStamina.SetValue(instance, new Stat(instance.MaxStamina + gameData.MaxStamina));
            staminaRegen.SetValue(instance, new Stat(instance.StaminaRegen + gameData.StaminaRegen));


            file.WriteLine("MovSpeed" + gameData.MovementSpeed);
            file.WriteLine("StamRegen" + gameData.MovementSpeed);
            Debug.Log("Movspeed value " + gameData.MovementSpeed);
            Debug.Log("StamRegen value " + gameData.StaminaRegen);
            //FieldInfo field = typeof(CharacterStats).GetField("m_staminaRegen", BindingFlags.Instance | BindingFlags.NonPublic);
            // field.SetValue(instance, new Stat(gameData.StaminaRegen));
        }

    }
}
