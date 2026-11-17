using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using Unity.XR.CoreUtils;
using Assignment4;

namespace Assignment4.Editor
{
    /// <summary>
    /// Automated Scene Builder and Validator for Assignment 4:
    /// Comparing Teleportation and Continuous Movement in VR.
    /// </summary>
    [InitializeOnLoad]
    public static class Assignment4SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string XrOriginPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        private const string SimulatorPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/XR Interaction Simulator/XR Interaction Simulator.prefab";
        private const string InputActionsPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/XRI Default Input Actions.inputactions";

        static Assignment4SceneBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("Assignment4_SetupDone", false))
                {
                    SessionState.SetBool("Assignment4_SetupDone", true);
                    BuildAssignment4Scene();
                }
            };
        }

        [MenuItem("Assignment 4/Build and Setup Scene", priority = 1)]
        public static void BuildAssignment4Scene()
        {
            Debug.Log("<color=green><b>[Assignment 4] Starting automated scene construction...</b></color>");

            // Ensure the scene is open
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath && File.Exists(ScenePath))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            // 1. Create or verify XR Interaction Manager
            var interactionManager = UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>();
            if (interactionManager == null)
            {
                var mgrObj = new GameObject("XR Interaction Manager");
                interactionManager = mgrObj.AddComponent<XRInteractionManager>();
                Undo.RegisterCreatedObjectUndo(mgrObj, "Create XR Interaction Manager");
                Debug.Log("[Assignment 4] Created XR Interaction Manager.");
            }

            // 2. Remove standalone Main Camera if outside XR Origin
            var allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
            foreach (var cam in allCameras)
            {
                if (cam.gameObject.name == "Main Camera" && cam.GetComponentInParent<XROrigin>() == null)
                {
                    Debug.Log("[Assignment 4] Removing standalone template Main Camera.");
                    UnityEngine.Object.DestroyImmediate(cam.gameObject);
                }
            }

            // 3. Create or verify XR Interaction Simulator
            var simulator = GameObject.Find("XR Interaction Simulator");
            if (simulator == null)
            {
                var simPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPrefabPath);
                if (simPrefab != null)
                {
                    simulator = (GameObject)PrefabUtility.InstantiatePrefab(simPrefab);
                    simulator.name = "XR Interaction Simulator";
                    Debug.Log("[Assignment 4] Instantiated XR Interaction Simulator.");
                }
                else
                {
                    Debug.LogWarning("[Assignment 4] XR Interaction Simulator prefab not found at: " + SimulatorPrefabPath);
                }
            }

            // 4. Create or verify XR Origin (XR Rig)
            var xrOrigin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            GameObject xrOriginObj = null;
            if (xrOrigin == null)
            {
                var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefabPath);
                if (rigPrefab != null)
                {
                    xrOriginObj = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
                    xrOriginObj.name = "XR Origin (XR Rig)";
                    xrOrigin = xrOriginObj.GetComponent<XROrigin>();
                    Debug.Log("[Assignment 4] Instantiated XR Origin (XR Rig) from Starter Assets.");
                }
                else
                {
                    Debug.LogError("[Assignment 4] XR Origin prefab not found at: " + XrOriginPrefabPath);
                    return;
                }
            }
            else
            {
                xrOriginObj = xrOrigin.gameObject;
            }

            // Position player at initial spawn point
            xrOriginObj.transform.position = new Vector3(0, 0, -2);
            xrOriginObj.transform.rotation = Quaternion.identity;

            // Ensure CharacterController on XR Origin
            var charController = xrOriginObj.GetComponent<CharacterController>();
            if (charController == null)
            {
                charController = xrOriginObj.AddComponent<CharacterController>();
                charController.radius = 0.35f;
                charController.height = 1.8f;
                charController.center = new Vector3(0, 0.9f, 0);
                charController.minMoveDistance = 0f;
                charController.stepOffset = 0.35f;
                charController.slopeLimit = 45f;
                Debug.Log("[Assignment 4] Added CharacterController to XR Origin.");
            }

            // Find camera inside XR Origin
            var xrCamera = xrOrigin.Camera;
            if (xrCamera == null)
            {
                xrCamera = xrOriginObj.GetComponentInChildren<Camera>();
            }

            // 5. Configure Continuous Move Provider
            var moveProvider = xrOriginObj.GetComponentInChildren<ContinuousMoveProvider>();
            if (moveProvider == null)
            {
                moveProvider = xrOriginObj.AddComponent<ContinuousMoveProvider>();
                Debug.Log("[Assignment 4] Added ContinuousMoveProvider to XR Origin.");
            }
            moveProvider.moveSpeed = 3.0f;
            moveProvider.enableStrafe = true;
            if (xrCamera != null)
            {
                moveProvider.forwardSource = xrCamera.transform; // Headset relative
            }

            // Wire input actions from XRI Default Input Actions
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions != null)
            {
                var moveAction = inputActions.FindAction("XRI Left Locomotion/Move") ?? inputActions.FindAction("Move");
                if (moveAction != null)
                {
                    var actionRef = InputActionReference.Create(moveAction);
                    var reader = new XRInputValueReader<Vector2>();
                    reader.inputActionReference = actionRef;
                    moveProvider.leftHandMoveInput = reader;
                    Debug.Log("[Assignment 4] Successfully bound Left Hand Move Input Action.");
                }
            }

            // Attach ContinuousMoveInputFallback for immediate WASD keyboard driving in Editor
            var moveFallback = xrOriginObj.GetComponent<ContinuousMoveInputFallback>();
            if (moveFallback == null)
            {
                moveFallback = xrOriginObj.AddComponent<ContinuousMoveInputFallback>();
                moveFallback.moveSpeed = 3.0f;
                if (xrCamera != null) moveFallback.forwardSource = xrCamera.transform;
                Debug.Log("[Assignment 4] Attached ContinuousMoveInputFallback for smooth WASD navigation.");
            }

            // 6. Verify Teleportation Provider
            var teleportProvider = xrOriginObj.GetComponentInChildren<TeleportationProvider>();
            if (teleportProvider == null)
            {
                teleportProvider = xrOriginObj.AddComponent<TeleportationProvider>();
                Debug.Log("[Assignment 4] Added TeleportationProvider to XR Origin.");
            }

            // 7. Build Environment (Floors, Platforms, Hazard, Obstacles)
            BuildEnvironmentHierarchy(teleportProvider);

            // 8. Build World Space Comparison Board
            Assignment4ComparisonUI.CreateOrUpdateWorldCanvas(new Vector3(0, 1.8f, 2.5f), Quaternion.identity);

            // 9. Create Documentation & Evaluation Object
            var docObj = GameObject.Find("Assignment4_Documentation");
            if (docObj == null)
            {
                docObj = new GameObject("Assignment4_Documentation");
            }
            if (docObj.GetComponent<Assignment4Evaluation>() == null)
            {
                docObj.AddComponent<Assignment4Evaluation>();
            }
            if (docObj.GetComponent<Assignment4LocomotionTest>() == null)
            {
                docObj.AddComponent<Assignment4LocomotionTest>();
            }

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=green><b>[Assignment 4] Scene setup successfully completed and saved!</b></color>");
        }

        private static void BuildEnvironmentHierarchy(TeleportationProvider teleportProvider)
        {
            var envRoot = GameObject.Find("Environment");
            if (envRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(envRoot);
            }
            envRoot = new GameObject("Environment");

            // Create Materials
            var matFloor = CreateMaterial("Mat_Floor_Valid", new Color(0.24f, 0.35f, 0.47f)); // Slate Blue
            var matPlatformCyan = CreateMaterial("Mat_Platform_Cyan", new Color(0.0f, 0.71f, 0.85f)); // Cyan
            var matPlatformIndigo = CreateMaterial("Mat_Platform_Indigo", new Color(0.37f, 0.38f, 0.81f)); // Indigo
            var matHazardRed = CreateMaterial("Mat_Hazard_Red", new Color(0.90f, 0.22f, 0.27f)); // Bright Hazard Red
            var matObstacleWall = CreateMaterial("Mat_Obstacle_Wall", new Color(0.55f, 0.60f, 0.68f)); // Neutral Gray
            var matRamp = CreateMaterial("Mat_Ramp", new Color(0.18f, 0.55f, 0.65f)); // Teal

            // --- 1. MAIN GROUND FLOOR (Valid Teleport Area) ---
            var mainFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainFloor.name = "Floor_Main";
            mainFloor.transform.SetParent(envRoot.transform);
            mainFloor.transform.position = new Vector3(0, -0.1f, 0);
            mainFloor.transform.localScale = new Vector3(32, 0.2f, 32);
            mainFloor.GetComponent<Renderer>().material = matFloor;
            var mainTeleport = mainFloor.AddComponent<TeleportationArea>();
            mainTeleport.teleportationProvider = teleportProvider;

            // --- 2. ELEVATED PLATFORM WITH WALKABLE RAMP (Valid Teleport Area) ---
            var platformRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformRamp.name = "Platform_Elevated_Ramp";
            platformRamp.transform.SetParent(envRoot.transform);
            platformRamp.transform.position = new Vector3(8, 1.5f, 8);
            platformRamp.transform.localScale = new Vector3(8, 0.4f, 8);
            platformRamp.GetComponent<Renderer>().material = matPlatformCyan;
            var platRampTeleport = platformRamp.AddComponent<TeleportationArea>();
            platRampTeleport.teleportationProvider = teleportProvider;

            // Walkable Ramp connecting Ground (z=2.5) to Platform (z=4)
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp_Walkable";
            ramp.transform.SetParent(envRoot.transform);
            ramp.transform.position = new Vector3(8, 0.75f, 1.5f);
            ramp.transform.rotation = Quaternion.Euler(16.5f, 0, 0);
            ramp.transform.localScale = new Vector3(3.5f, 0.2f, 6.2f);
            ramp.GetComponent<Renderer>().material = matRamp;
            var rampTeleport = ramp.AddComponent<TeleportationArea>();
            rampTeleport.teleportationProvider = teleportProvider;

            // --- 3. ISOLATED HIGH PLATFORM (Valid Teleport Area - NO Ramp) ---
            var platformHigh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformHigh.name = "Platform_Isolated_High";
            platformHigh.transform.SetParent(envRoot.transform);
            platformHigh.transform.position = new Vector3(-8, 2.8f, 8);
            platformHigh.transform.localScale = new Vector3(6, 0.4f, 6);
            platformHigh.GetComponent<Renderer>().material = matPlatformIndigo;
            var platHighTeleport = platformHigh.AddComponent<TeleportationArea>();
            platHighTeleport.teleportationProvider = teleportProvider;

            // Support Pillars for Isolated Platform
            CreatePillar(envRoot.transform, new Vector3(-10, 1.3f, 6), matObstacleWall);
            CreatePillar(envRoot.transform, new Vector3(-6, 1.3f, 6), matObstacleWall);
            CreatePillar(envRoot.transform, new Vector3(-10, 1.3f, 10), matObstacleWall);
            CreatePillar(envRoot.transform, new Vector3(-6, 1.3f, 10), matObstacleWall);

            // --- 4. RESTRICTED / HAZARD AREA (INVALID Teleport Destination) ---
            var hazardArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazardArea.name = "Restricted_Hazard_Area";
            hazardArea.transform.SetParent(envRoot.transform);
            hazardArea.transform.position = new Vector3(-8, 0.05f, -6);
            hazardArea.transform.localScale = new Vector3(8, 0.15f, 8);
            hazardArea.GetComponent<Renderer>().material = matHazardRed;
            // Deliberately NO TeleportationArea component! BoxCollider blocks teleport ray and flags as invalid surface.

            // Warning Curbs around Hazard Area
            CreateWall(envRoot.transform, "Hazard_Curb_N", new Vector3(-8, 0.3f, -2), new Vector3(8.4f, 0.5f, 0.4f), matObstacleWall);
            CreateWall(envRoot.transform, "Hazard_Curb_S", new Vector3(-8, 0.3f, -10), new Vector3(8.4f, 0.5f, 0.4f), matObstacleWall);
            CreateWall(envRoot.transform, "Hazard_Curb_E", new Vector3(-4, 0.3f, -6), new Vector3(0.4f, 0.5f, 8.4f), matObstacleWall);
            CreateWall(envRoot.transform, "Hazard_Curb_W", new Vector3(-12, 0.3f, -6), new Vector3(0.4f, 0.5f, 8.4f), matObstacleWall);

            // --- 5. OBSTACLE MAZE & WALLS ---
            var obstaclesRoot = new GameObject("Obstacle_Maze_And_Walls");
            obstaclesRoot.transform.SetParent(envRoot.transform);

            // Perimeter Boundary Walls (surrounding 32x32 arena)
            CreateWall(obstaclesRoot.transform, "Wall_Perimeter_North", new Vector3(0, 2f, 16f), new Vector3(32.4f, 4f, 0.5f), matObstacleWall);
            CreateWall(obstaclesRoot.transform, "Wall_Perimeter_South", new Vector3(0, 2f, -16f), new Vector3(32.4f, 4f, 0.5f), matObstacleWall);
            CreateWall(obstaclesRoot.transform, "Wall_Perimeter_East", new Vector3(16f, 2f, 0), new Vector3(0.5f, 4f, 32.4f), matObstacleWall);
            CreateWall(obstaclesRoot.transform, "Wall_Perimeter_West", new Vector3(-16f, 2f, 0), new Vector3(0.5f, 4f, 32.4f), matObstacleWall);

            // Central Navigation Obstacles / Slalom Course
            CreateWall(obstaclesRoot.transform, "Barrier_Wall_A", new Vector3(-3f, 1.5f, 4.5f), new Vector3(5f, 3f, 0.5f), matObstacleWall);
            CreateWall(obstaclesRoot.transform, "Barrier_Wall_B", new Vector3(3.5f, 1.5f, -2f), new Vector3(0.5f, 3f, 5f), matObstacleWall);
            CreateWall(obstaclesRoot.transform, "Barrier_Wall_C", new Vector3(-2f, 1.5f, -2f), new Vector3(4f, 3f, 0.5f), matObstacleWall);

            // Obstacle Columns (Testing Continuous Steering vs Direct Teleport)
            CreatePillar(obstaclesRoot.transform, new Vector3(1.5f, 1.5f, 5.5f), matObstacleWall);
            CreatePillar(obstaclesRoot.transform, new Vector3(-0.5f, 1.5f, 7.5f), matObstacleWall);
            CreatePillar(obstaclesRoot.transform, new Vector3(3.5f, 1.5f, 2f), matObstacleWall);
            CreatePillar(obstaclesRoot.transform, new Vector3(-5.5f, 1.5f, 1.5f), matObstacleWall);
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = mat;
        }

        private static void CreatePillar(Transform parent, Vector3 pos, Material mat)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar_Obstacle";
            pillar.transform.SetParent(parent);
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f); // Height 3m
            pillar.GetComponent<Renderer>().material = mat;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string matPath = $"Assets/Settings/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader) { color = color };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.color = color;
            }
            return mat;
        }
    }
}
