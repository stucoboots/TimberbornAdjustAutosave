using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using Timberborn.Autosaving;

namespace TimberbornAdjustAutosave
{
    [BepInPlugin("net.buhichan.stuco.plugins.timberbornadjustautosave", "Adjust Autosave", "1.0.0")]
    [BepInProcess("Timberborn.exe")]
    public class TimberbornAdjustAutosave : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("net.buhichan.stuco.plugins.timberbornadjustautosave");
        private static string consoleOutPrefix = "[Adjust Autosave] ";

        private static ConfigEntry<int> autosaveCountConfig;
        private static int autosaveCountLowerBound = 1;
        private static int autosaveCountUpperBound = 255;

        private static ConfigEntry<float> autosaveFrequencyConfig; // game internally stores this as float but appears to round in use
        private static float autosaveFrequencyLowerBound = 1f; // 1 minute
        private static float autosaveFrequencyUpperBound = 1440f; // 24 hours

        void Awake()
        {
            autosaveCountConfig = Config.Bind<int>("Autosave Count", "autosaveCount", 3, "Number of autosaves to keep per settlement. Must be between 1 and 255.");
            autosaveFrequencyConfig = Config.Bind<float>("Autosave Frequency", "autosaveFrequency", 10f, "Autosave frequency (in minutes). Must be between 1 and 1440.");
            harmony.PatchAll();
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        private static bool isInRangeInclusive(float candidate, float lowerBound, float upperBound)
        {
            return lowerBound <= candidate && candidate <= upperBound;
        }

        [HarmonyPatch(typeof(Autosaver), "Awake")]
        class Autosaver_Awake_Patch
        {
            static void Postfix(ref float ____frequencyInMinutes, ref int ____autosavesPerSettlement)
            {
                // autosave count
                if (isInRangeInclusive(autosaveCountConfig.Value, autosaveCountLowerBound, autosaveCountUpperBound))
                {
                    ____autosavesPerSettlement = autosaveCountConfig.Value;
                }
                else
                {
                    Debug.LogWarning($"{consoleOutPrefix}Autosave count not modified. Verify that your config file value is between {autosaveCountLowerBound} and {autosaveCountUpperBound} inclusive.");
                }
                Debug.Log($"{consoleOutPrefix}Using autosave count of {____autosavesPerSettlement}.");

                // autosave frequency
                if (isInRangeInclusive(autosaveFrequencyConfig.Value, autosaveFrequencyLowerBound, autosaveFrequencyUpperBound))
                {
                    ____frequencyInMinutes = autosaveFrequencyConfig.Value;
                }
                else
                {
                    Debug.LogWarning($"{consoleOutPrefix}Autosave interval not modified. Verify that your config file value is between {autosaveFrequencyLowerBound} and {autosaveFrequencyUpperBound} inclusive.");
                }
                Debug.Log($"{consoleOutPrefix}Using autosave interval of {____frequencyInMinutes} minutes.");
            }
        }
    }
}