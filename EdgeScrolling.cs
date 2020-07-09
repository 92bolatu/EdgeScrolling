using System;
using Harmony;
using UnityEngine;

namespace EdgeScrolling {

    [Flags]
    enum Works : int {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }

    public class EdgeScrollingPatches {

        private static bool enabled = true;

        private static Works working = Works.None;

        private static int edgeSize = 10;

        private static float keyPanningSpeed = 2f;

        private static int pressed = 0;

        [HarmonyPatch(typeof(CameraController))]
        [HarmonyPatch("OnPrefabInit")]
        public static class CameraController_OnPrefabInit_Patch {
            public static void Prefix(CameraController __instance) {
                Traverse.Create(__instance).Field("keyPanningSpeed").SetValue(keyPanningSpeed);
            }
        }

        [HarmonyPatch(typeof(CameraController))]
        [HarmonyPatch("NormalCamUpdate")]
        public class NormalCamUpdate {
            public static void Prefix(CameraController __instance) {
                if (enabled) {

                    var m = KInputManager.GetMousePos();

                    // LEFT
                    if (m.x < edgeSize) {
                        Traverse.Create(__instance).Field("panLeft").SetValue(true);
                        working = working | Works.Left;
                    }
                    else if (working.HasFlag(Works.Left)) {
                        Traverse.Create(__instance).Field("panLeft").SetValue(false);
                        working = working & ~Works.Left;
                    }


                    // RIGHT
                    if (m.x > (Screen.width - edgeSize)) {
                        Traverse.Create(__instance).Field("panRight").SetValue(true);
                        working = working | Works.Right;
                    }
                    else if (working.HasFlag(Works.Right)) {
                        Traverse.Create(__instance).Field("panRight").SetValue(false);
                        working = working & ~Works.Right;
                    }


                    // DOWN
                    if (m.y < edgeSize) {
                        Traverse.Create(__instance).Field("panDown").SetValue(true);
                        working = working | Works.Down;
                    }
                    else if (working.HasFlag(Works.Down)) {
                        Traverse.Create(__instance).Field("panDown").SetValue(false);
                        working = working & ~Works.Down;
                    }

                    // UP
                    if (m.y > (Screen.height - edgeSize)) {
                        Traverse.Create(__instance).Field("panUp").SetValue(true);
                        working = working | Works.Up;
                    }
                    else if (working.HasFlag(Works.Up)) {
                        Traverse.Create(__instance).Field("panUp").SetValue(false);
                        working = working & ~Works.Up;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CameraController))]
        [HarmonyPatch("OnKeyDown")]
        public class OnKeyDown {
            public static void Prefix(KButtonEvent e) {
                if (e.IsAction(Action.PanUp) || e.IsAction(Action.PanRight) || e.IsAction(Action.PanDown) || e.IsAction(Action.PanLeft)) {
                    enabled = false;
                    pressed++;
                }
            }
        }

        [HarmonyPatch(typeof(CameraController))]
        [HarmonyPatch("OnKeyUp")]
        public class OnKeyUp {
            public static void Prefix(KButtonEvent e) {
                if (e.IsAction(Action.PanUp) || e.IsAction(Action.PanRight) || e.IsAction(Action.PanDown) || e.IsAction(Action.PanLeft)) {
                    pressed--;
                    if (pressed <= 0) {
                        enabled = true;
                        working = Works.None;
                        pressed = 0;
                    }
                }
            }
        }
    }
}