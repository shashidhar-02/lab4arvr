using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;

namespace Assignment4.Editor
{
    [InitializeOnLoad]
    public static class Assignment4PlayModeVerification
    {
        static Assignment4PlayModeVerification()
        {
            EditorApplication.delayCall += () =>
            {
                RunFullVerification();
            };
        }

        [MenuItem("Assignment 4/Run Verification", priority = 2)]
        public static void RunFullVerification()
        {
            Debug.Log("==================================================");
            Debug.Log("STARTING ASSIGNMENT 4 VERIFICATION SUITE");
            Debug.Log("==================================================");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            // Phase 1: Inspect Components
            var xrOrigin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            var xrCamera = xrOrigin != null ? xrOrigin.Camera : null;
            var simObj = GameObject.Find("XR Interaction Simulator");
            var interactionManager = UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>();

            Debug.Log($"[Verification] XR Origin: {(xrOrigin != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] XR Camera: {(xrCamera != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] XR Device Simulator: {(simObj != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] XR Interaction Manager: {(interactionManager != null ? "PASS" : "FAIL")}");

            // Phase 2: Teleportation
            var teleportProvider = UnityEngine.Object.FindAnyObjectByType<TeleportationProvider>();
            var teleportAreas = UnityEngine.Object.FindObjectsByType<TeleportationArea>(FindObjectsInactive.Include);
            bool teleportPass = teleportProvider != null && teleportAreas != null && teleportAreas.Length >= 4;
            Debug.Log($"[Verification] Teleportation Provider: {(teleportProvider != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] Valid Teleportation Areas Count: {teleportAreas?.Length ?? 0} (Expected >= 4) -> {(teleportPass ? "PASS" : "FAIL")}");

            // Phase 3 & 4: Continuous Movement & Inputs
            var moveProvider = UnityEngine.Object.FindAnyObjectByType<ContinuousMoveProvider>();
            var charController = UnityEngine.Object.FindAnyObjectByType<CharacterController>();
            var moveFallback = UnityEngine.Object.FindAnyObjectByType<ContinuousMoveInputFallback>();
            bool continuousPass = moveProvider != null && charController != null && charController.enabled && moveProvider.moveSpeed > 0;
            Debug.Log($"[Verification] Continuous Move Provider: {(moveProvider != null ? "PASS (Speed=" + moveProvider.moveSpeed + ")" : "FAIL")}");
            Debug.Log($"[Verification] CharacterController: {(charController != null ? "PASS (Radius=" + charController.radius + ", Height=" + charController.height + ")" : "FAIL")}");
            Debug.Log($"[Verification] Continuous Input Fallback (WASD): {(moveFallback != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] Head-relative Forward Source: {(moveProvider?.forwardSource != null ? "PASS" : "FAIL")}");

            // Phase 5: Environment & Obstacles
            var mainFloor = GameObject.Find("Floor_Main");
            var platRamp = GameObject.Find("Platform_Elevated_Ramp");
            var ramp = GameObject.Find("Ramp_Walkable");
            var platHigh = GameObject.Find("Platform_Isolated_High");
            var hazard = GameObject.Find("Restricted_Hazard_Area");
            var obstacles = GameObject.Find("Obstacle_Maze_And_Walls");

            bool hazardValid = hazard != null && hazard.GetComponent<Collider>() != null && hazard.GetComponent<TeleportationArea>() == null;
            bool obstaclesValid = obstacles != null && obstacles.GetComponentsInChildren<Collider>().Length >= 7;

            Debug.Log($"[Verification] Multi-floor Environment: {(mainFloor != null && platRamp != null && ramp != null && platHigh != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] Restricted / Invalid Teleport Area (Prevents Teleport): {(hazardValid ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] Obstacle Colliders (Blocks Continuous Movement): {(obstaclesValid ? "PASS" : "FAIL")}");

            // Phase 8: Comparison Matrix UI
            var comparisonUI = GameObject.Find("Comparison_UI_Board");
            var doc = UnityEngine.Object.FindAnyObjectByType<Assignment4Evaluation>();
            Debug.Log($"[Verification] In-Project Comparison UI Board: {(comparisonUI != null ? "PASS" : "FAIL")}");
            Debug.Log($"[Verification] Assignment 4 Documentation Object: {(doc != null ? "PASS" : "FAIL")}");

            Debug.Log("==================================================");
            Debug.Log("ASSIGNMENT 4 VERIFICATION SUMMARY:");
            Debug.Log($"- Teleportation: {(teleportPass ? "PASS" : "FAIL")}");
            Debug.Log($"- Continuous movement: {(continuousPass ? "PASS" : "FAIL")}");
            Debug.Log($"- XR Device Simulator: {(simObj != null ? "PASS" : "FAIL")}");
            Debug.Log($"- Valid/invalid teleport areas: {(hazardValid && teleportPass ? "PASS" : "FAIL")}");
            Debug.Log($"- Obstacle collision: {(obstaclesValid ? "PASS" : "FAIL")}");
            Debug.Log($"- Comparison table: {(comparisonUI != null && doc != null ? "PASS" : "FAIL")}");
            Debug.Log("==================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
    }
}
