using HarmonyLib;

namespace BetterCustomAngles
{
    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.CreateArbitraryFloor))]
    internal static class PatchCreateArbitraryFloor
    {
        private static void Postfix(scnEditor __instance)
        {
            Controller.HandleAnglePlaced(__instance);
        }
    }

    [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.CreateArbitraryMidspin))]
    internal static class PatchCreateArbitraryMidspin
    {
        private static void Postfix(scnEditor __instance)
        {
            Controller.HandleAnglePlaced(__instance);
        }
    }
}
