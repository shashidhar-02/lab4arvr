using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Assignment4
{
    /// <summary>
    /// Automated test verifier for Phase 6, Phase 7, Phase 9 of Assignment 4.
    /// Runs diagnostics in Play Mode to ensure all required components and behaviors are operational.
    /// </summary>
    public class Assignment4LocomotionTest : MonoBehaviour
    {
        [Header("Test Status Output")]
        public bool teleportationPass = false;
        public bool continuousMovementPass = false;
        public bool switchingBothPass = false;
        public bool obstacleInteractionPass = false;
        public bool invalidTeleportationPreventionPass = false;

        private void Start()
        {
            RunVerification();
        }

        [ContextMenu("Run Verification Now")]
        public void RunVerification()
        {
            Debug.Log("<color=cyan><b>[Assignment 4] Running Locomotion Verification...</b></color>");

            // 1. Verify Teleportation
            var teleportProvider = Object.FindAnyObjectByType<TeleportationProvider>();
            var teleportAreas = Object.FindObjectsByType<TeleportationArea>(FindObjectsInactive.Include);
            bool hasTeleportProvider = teleportProvider != null;
            bool hasTeleportSurfaces = teleportAreas != null && teleportAreas.Length > 0;
            teleportationPass = hasTeleportProvider && hasTeleportSurfaces;
            Debug.Log($"[Assignment 4] Teleportation Provider: {(hasTeleportProvider ? "FOUND" : "MISSING")}, Valid Teleport Areas: {teleportAreas?.Length ?? 0} -> {(teleportationPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 2. Verify Continuous Movement
            var moveProvider = Object.FindAnyObjectByType<ContinuousMoveProvider>();
            var charController = Object.FindAnyObjectByType<CharacterController>();
            bool hasMoveProvider = moveProvider != null && moveProvider.moveSpeed > 0;
            bool hasCharController = charController != null && charController.enabled;
            continuousMovementPass = hasMoveProvider && hasCharController;
            Debug.Log($"[Assignment 4] Continuous Move Provider: {(hasMoveProvider ? "FOUND (Speed=" + moveProvider.moveSpeed + ")" : "MISSING")}, CharacterController: {(hasCharController ? "ACTIVE" : "MISSING")} -> {(continuousMovementPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 3. Verify Switching/Testing Both
            switchingBothPass = teleportationPass && continuousMovementPass;
            Debug.Log($"[Assignment 4] Dual Locomotion Support: {(switchingBothPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 4. Verify Obstacle Interaction
            var obstacles = GameObject.Find("Obstacle_Maze_And_Walls");
            bool obstaclesConfigured = false;
            if (obstacles != null)
            {
                var colliders = obstacles.GetComponentsInChildren<Collider>();
                obstaclesConfigured = colliders.Length > 0;
            }
            obstacleInteractionPass = obstaclesConfigured;
            Debug.Log($"[Assignment 4] Obstacle Colliders Present: {(obstacleInteractionPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            // 5. Verify Invalid Teleportation Prevention
            var hazard = GameObject.Find("Restricted_Hazard_Area");
            bool hazardConfigured = false;
            if (hazard != null)
            {
                var hazardCol = hazard.GetComponent<Collider>();
                var hazardTeleport = hazard.GetComponent<TeleportationArea>();
                hazardConfigured = hazardCol != null && hazardTeleport == null;
            }
            invalidTeleportationPreventionPass = hazardConfigured;
            Debug.Log($"[Assignment 4] Restricted / Invalid Teleport Area Prevention: {(invalidTeleportationPreventionPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>")}");

            Debug.Log("==================================================");
            Debug.Log($"ASSIGNMENT 4 SUMMARY:\n" +
                      $"Teleportation: {(teleportationPass ? "PASS" : "FAIL")}\n" +
                      $"Continuous movement: {(continuousMovementPass ? "PASS" : "FAIL")}\n" +
                      $"Switching/testing both: {(switchingBothPass ? "PASS" : "FAIL")}\n" +
                      $"Obstacle interaction: {(obstacleInteractionPass ? "PASS" : "FAIL")}\n" +
                      $"Invalid teleportation prevention: {(invalidTeleportationPreventionPass ? "PASS" : "FAIL")}");
            Debug.Log("==================================================");
        }
    }
}
