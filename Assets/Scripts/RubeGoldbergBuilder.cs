using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class RubeGoldbergBuilder
{
    public const string RootObjectName = "RubeGoldbergMachine";
#if UNITY_EDITOR
    private const string GeneratedAssetsFolder = "Assets/Materials/RubeGoldberg";
#endif

    public static RubeGoldbergController BuildMachineInCurrentScene()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureEditorFolderExists();
        }
#endif

        GameObject existing = GameObject.Find(RootObjectName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject root = new GameObject(RootObjectName);
        RubeGoldbergController controller = root.AddComponent<RubeGoldbergController>();

        // Create Physics Materials
        PhysicsMaterial lowFrictionMat = CreatePhysicsMaterial("LowFrictionTrack", 0.04f, 0.04f, 0.15f);
        PhysicsMaterial ballPhysMat = CreatePhysicsMaterial("RollingBallPhys", 0.05f, 0.05f, 0.25f);
        PhysicsMaterial dominoPhysMat = CreatePhysicsMaterial("DominoPhys", 0.35f, 0.35f, 0.05f);
        PhysicsMaterial cupPhysMat = CreatePhysicsMaterial("GoalCupPhys", 0.6f, 0.6f, 0.0f);

        // Create Visual Materials (URP Lit compatible)
        Material floorMat = CreateLitMaterial("FloorMat", new Color(0.16f, 0.18f, 0.22f), 0.1f, 0.35f);
        Material backdropMat = CreateLitMaterial("BackdropMat", new Color(0.12f, 0.14f, 0.18f), 0.05f, 0.2f);
        Material pillarMat = CreateLitMaterial("PillarMat", new Color(0.26f, 0.29f, 0.34f), 0.2f, 0.4f);
        Material woodTrackMat = CreateLitMaterial("TrackMat", new Color(0.82f, 0.64f, 0.42f), 0.05f, 0.55f);
        Material railMat = CreateLitMaterial("RailMat", new Color(0.32f, 0.38f, 0.46f), 0.35f, 0.6f);
        Material gateMat = CreateLitMaterial("ReleaseGateMat", new Color(0.95f, 0.75f, 0.15f), 0.2f, 0.7f);
        Material starterBallMat = CreateLitMaterial("StarterBallMat", new Color(0.95f, 0.26f, 0.18f), 0.65f, 0.85f);
        Material pendulumMat = CreateLitMaterial("PendulumMat", new Color(0.20f, 0.68f, 0.92f), 0.55f, 0.8f);
        Material finalBallMat = CreateLitMaterial("FinalBallMat", new Color(1.0f, 0.84f, 0.18f), 0.75f, 0.9f);
        Material funnelMat = CreateLitMaterial("FunnelMat", new Color(0.56f, 0.36f, 0.88f), 0.25f, 0.65f);
        Material cupMat = CreateLitMaterial("GoalCupMat", new Color(0.22f, 0.78f, 0.52f), 0.25f, 0.65f);
        Material beaconMat = CreateLitMaterial("GoalBeaconMat", new Color(0.95f, 0.45f, 0.15f), 0.3f, 0.75f);

        // ---------------------------------------------------------
        // 0. Environment & Support Frame
        // ---------------------------------------------------------
        Transform envGroup = CreateGroup(root.transform, "00_Environment");
        CreateBox("WorkshopFloor", envGroup, new Vector3(1.2f, -0.25f, 0f), new Vector3(20f, 0.5f, 8f), Quaternion.identity, floorMat, lowFrictionMat);
        CreateBox("BackdropWall", envGroup, new Vector3(1.2f, 3.8f, 1.8f), new Vector3(20f, 8.2f, 0.25f), Quaternion.identity, backdropMat, null);

        // Support pillars down to Y = 0
        CreateBox("Pillar_Start", envGroup, new Vector3(-6.2f, 2.8f, 0f), new Vector3(0.35f, 5.6f, 0.6f), Quaternion.identity, pillarMat, null);
        CreateBox("Pillar_MidRamp", envGroup, new Vector3(-2.7f, 2.0f, 0f), new Vector3(0.35f, 4.0f, 0.6f), Quaternion.identity, pillarMat, null);
        CreateBox("Pillar_DominoLeft", envGroup, new Vector3(0.2f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.6f), Quaternion.identity, pillarMat, null);
        CreateBox("Pillar_DominoRight", envGroup, new Vector3(3.4f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.6f), Quaternion.identity, pillarMat, null);
        CreateBox("Pillar_ChuteMid", envGroup, new Vector3(5.8f, 1.25f, 0f), new Vector3(0.35f, 2.5f, 0.6f), Quaternion.identity, pillarMat, null);

        // ---------------------------------------------------------
        // 1. Stage 1: Rolling Ball & Angled Ramps
        // ---------------------------------------------------------
        Transform stage1 = CreateGroup(root.transform, "01_Stage1_RampsAndBalls");

        // Start Deck (slightly tilted -6 deg so ball immediately wants to roll right)
        CreateTrackWithRails(
            "StartDeck",
            stage1,
            new Vector3(-6.15f, 5.65f, 0f),
            new Vector3(1.4f, 0.18f, 0.9f),
            Quaternion.Euler(0f, 0f, -6f),
            woodTrackMat,
            railMat,
            lowFrictionMat,
            0.38f
        );

        CreateBox(
            "StartBackstop",
            stage1,
            new Vector3(-6.78f, 6.02f, 0f),
            new Vector3(0.14f, 0.55f, 0.9f),
            Quaternion.identity,
            railMat,
            lowFrictionMat
        );

        GameObject releaseGateObj = CreateBox(
            "ReleaseGate",
            stage1,
            new Vector3(-5.68f, 5.95f, 0f),
            new Vector3(0.10f, 0.55f, 0.68f),
            Quaternion.identity,
            gateMat,
            lowFrictionMat
        );
        controller.releaseGate = releaseGateObj.transform;

        // Starter Ball (Radius 0.24, Diameter 0.48)
        GameObject starterBallObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        starterBallObj.name = "StarterBall";
        starterBallObj.transform.SetParent(stage1, false);
        starterBallObj.transform.localPosition = new Vector3(-6.12f, 5.98f, 0f);
        starterBallObj.transform.localScale = Vector3.one * 0.48f;
        starterBallObj.GetComponent<Renderer>().sharedMaterial = starterBallMat;
        SphereCollider sbCol = starterBallObj.GetComponent<SphereCollider>();
        sbCol.material = ballPhysMat;

        Rigidbody starterRb = starterBallObj.AddComponent<Rigidbody>();
        starterRb.mass = 1.8f;
        starterRb.linearDamping = 0.02f;
        starterRb.angularDamping = 0.02f;
        starterRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        starterRb.interpolation = RigidbodyInterpolation.Interpolate;
        starterRb.constraints = RigidbodyConstraints.FreezePositionZ;
        controller.starterBall = starterRb;

        // Upper Ramp (-22 deg)
        CreateTrackWithRails(
            "UpperRamp",
            stage1,
            new Vector3(-4.10f, 5.02f, 0f),
            new Vector3(3.15f, 0.18f, 0.9f),
            Quaternion.Euler(0f, 0f, -22f),
            woodTrackMat,
            railMat,
            lowFrictionMat,
            0.38f
        );

        // Lower Ramp (-14 deg) catching ball from UpperRamp and feeding smoothly onto DominoDeck (top Y = 3.30)
        CreateTrackWithRails(
            "LowerRamp",
            stage1,
            new Vector3(-1.55f, 3.66f, 0f),
            new Vector3(2.55f, 0.18f, 0.9f),
            Quaternion.Euler(0f, 0f, -14f),
            woodTrackMat,
            railMat,
            lowFrictionMat,
            0.38f
        );

        // ---------------------------------------------------------
        // 2. Stage 2: Domino Topple Sequence
        // ---------------------------------------------------------
        Transform stage2 = CreateGroup(root.transform, "02_Stage2_DominoChain");

        // Flat Domino Deck: top surface at Y = 3.30 (center Y = 3.20, thickness = 0.20), X from -0.45 to +3.75
        CreateTrackWithRails(
            "DominoDeck",
            stage2,
            new Vector3(1.65f, 3.20f, 0f),
            new Vector3(4.20f, 0.20f, 0.9f),
            Quaternion.identity,
            woodTrackMat,
            railMat,
            dominoPhysMat,
            0.26f
        );

        // Create 8 Dominoes from X = 0.40 to X = 3.34 (spacing 0.42, height 0.90, thickness 0.14, width 0.52)
        int dominoCount = 8;
        List<Rigidbody> dominoBodies = new List<Rigidbody>(dominoCount);
        for (int i = 0; i < dominoCount; i++)
        {
            float t = (float)i / (dominoCount - 1);
            Color domColor = Color.Lerp(new Color(0.95f, 0.45f, 0.15f), new Color(0.20f, 0.85f, 0.65f), t);
            Material domMat = CreateLitMaterial($"DominoMat_{i + 1:00}", domColor, 0.15f, 0.7f);

            float domX = 0.40f + i * 0.42f;
            GameObject dom = CreateBox(
                $"Domino_{i + 1:00}",
                stage2,
                new Vector3(domX, 3.75f, 0f),
                new Vector3(0.14f, 0.90f, 0.52f),
                Quaternion.identity,
                domMat,
                dominoPhysMat
            );

            Rigidbody domRb = dom.AddComponent<Rigidbody>();
            domRb.mass = 0.42f;
            domRb.linearDamping = 0.05f;
            domRb.angularDamping = 0.05f;
            domRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            domRb.interpolation = RigidbodyInterpolation.Interpolate;
            domRb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            dominoBodies.Add(domRb);
        }
        controller.dominoes = dominoBodies.ToArray();

        // ---------------------------------------------------------
        // 3. Stage 3: Swinging Pendulum / Hammer Transfer
        // ---------------------------------------------------------
        Transform stage3 = CreateGroup(root.transform, "03_Stage3_PendulumHammer");

        // Gantry Frame around X = 3.95
        CreateBox("GantryPost_Front", stage3, new Vector3(3.95f, 4.40f, -0.58f), new Vector3(0.18f, 2.2f, 0.18f), Quaternion.identity, pillarMat, null);
        CreateBox("GantryPost_Back", stage3, new Vector3(3.95f, 4.40f, 0.58f), new Vector3(0.18f, 2.2f, 0.18f), Quaternion.identity, pillarMat, null);
        CreateBox("GantryCrossbar", stage3, new Vector3(3.95f, 5.42f, 0f), new Vector3(0.24f, 0.20f, 1.35f), Quaternion.identity, pillarMat, null);

        // Kinematic Pivot Anchor at (3.95, 5.32, 0)
        GameObject pivotAnchor = new GameObject("PendulumPivotAnchor");
        pivotAnchor.transform.SetParent(stage3, false);
        pivotAnchor.transform.localPosition = new Vector3(3.95f, 5.32f, 0f);
        Rigidbody pivotRb = pivotAnchor.AddComponent<Rigidbody>();
        pivotRb.isKinematic = true;

        // Pendulum Hammer Assembly (Compound Rigidbody)
        GameObject pendulumRoot = new GameObject("PendulumHammer");
        pendulumRoot.transform.SetParent(stage3, false);
        pendulumRoot.transform.localPosition = new Vector3(3.95f, 5.32f, 0f);

        CreateBox(
            "PendulumRod",
            pendulumRoot.transform,
            new Vector3(0f, -0.82f, 0f),
            new Vector3(0.10f, 1.60f, 0.10f),
            Quaternion.identity,
            railMat,
            null
        );

        CreateBox(
            "HammerHead",
            pendulumRoot.transform,
            new Vector3(0f, -1.68f, 0f),
            new Vector3(0.42f, 0.36f, 0.48f),
            Quaternion.identity,
            pendulumMat,
            ballPhysMat
        );

        Rigidbody pendulumRb = pendulumRoot.AddComponent<Rigidbody>();
        pendulumRb.mass = 1.4f;
        pendulumRb.linearDamping = 0.02f;
        pendulumRb.angularDamping = 0.02f;
        pendulumRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        pendulumRb.interpolation = RigidbodyInterpolation.Interpolate;
        pendulumRb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        HingeJoint hinge = pendulumRoot.AddComponent<HingeJoint>();
        hinge.connectedBody = pivotRb;
        hinge.anchor = Vector3.zero;
        hinge.axis = new Vector3(0f, 0f, 1f);
        hinge.useLimits = false;

        controller.pendulumBob = pendulumRb;

        // ---------------------------------------------------------
        // 4. Stage 4: Drop Chute, Funnel & Final Goal Cup
        // ---------------------------------------------------------
        Transform stage4 = CreateGroup(root.transform, "04_Stage4_DropChuteAndGoal");

        CreateTrackWithRails(
            "LaunchLedge",
            stage4,
            new Vector3(4.42f, 3.26f, 0f),
            new Vector3(0.36f, 0.20f, 0.9f),
            Quaternion.Euler(0f, 0f, -4f),
            woodTrackMat,
            railMat,
            lowFrictionMat,
            0.35f
        );

        GameObject finalBallObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        finalBallObj.name = "FinalBall";
        finalBallObj.transform.SetParent(stage4, false);
        finalBallObj.transform.localPosition = new Vector3(4.46f, 3.60f, 0f);
        finalBallObj.transform.localScale = Vector3.one * 0.48f;
        finalBallObj.GetComponent<Renderer>().sharedMaterial = finalBallMat;
        SphereCollider fbCol = finalBallObj.GetComponent<SphereCollider>();
        fbCol.material = ballPhysMat;

        Rigidbody finalRb = finalBallObj.AddComponent<Rigidbody>();
        finalRb.mass = 1.1f;
        finalRb.linearDamping = 0.02f;
        finalRb.angularDamping = 0.02f;
        finalRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        finalRb.interpolation = RigidbodyInterpolation.Interpolate;
        finalRb.constraints = RigidbodyConstraints.FreezePositionZ;
        controller.finalBall = finalRb;

        // Steep Drop Chute (-26 deg) from X ~ 4.58 down to X ~ 7.10
        CreateTrackWithRails(
            "DropChute",
            stage4,
            new Vector3(5.84f, 2.58f, 0f),
            new Vector3(2.85f, 0.18f, 0.9f),
            Quaternion.Euler(0f, 0f, -26f),
            woodTrackMat,
            railMat,
            lowFrictionMat,
            0.38f
        );

        // Funnel / Drop Hopper centered at X = 7.65, Y = 1.45
        Transform funnelGroup = CreateGroup(stage4, "FunnelHopper");
        CreateBox("FunnelBackboard", funnelGroup, new Vector3(8.22f, 1.95f, 0f), new Vector3(0.16f, 1.50f, 1.0f), Quaternion.Euler(0f, 0f, 8f), funnelMat, cupPhysMat);
        CreateBox("FunnelSlope_Left", funnelGroup, new Vector3(7.15f, 1.40f, 0f), new Vector3(0.75f, 0.14f, 0.95f), Quaternion.Euler(0f, 0f, -42f), funnelMat, lowFrictionMat);
        CreateBox("FunnelSlope_Right", funnelGroup, new Vector3(8.15f, 1.40f, 0f), new Vector3(0.75f, 0.14f, 0.95f), Quaternion.Euler(0f, 0f, 42f), funnelMat, lowFrictionMat);
        CreateBox("FunnelWall_Back", funnelGroup, new Vector3(7.65f, 1.65f, 0.45f), new Vector3(1.35f, 0.95f, 0.10f), Quaternion.identity, funnelMat, lowFrictionMat);
        CreateBox("FunnelWall_Front", funnelGroup, new Vector3(7.65f, 1.65f, -0.45f), new Vector3(1.35f, 0.45f, 0.10f), Quaternion.identity, railMat, lowFrictionMat);

        // Final Goal Cup directly under Funnel at X = 7.65, Y = 0.0
        Transform cupGroup = CreateGroup(stage4, "GoalCup");
        CreateBox("CupBase", cupGroup, new Vector3(7.65f, 0.10f, 0f), new Vector3(1.50f, 0.20f, 1.30f), Quaternion.identity, cupMat, cupPhysMat);
        CreateBox("CupWall_Left", cupGroup, new Vector3(6.98f, 0.48f, 0f), new Vector3(0.16f, 0.75f, 1.30f), Quaternion.identity, cupMat, cupPhysMat);
        CreateBox("CupWall_Right", cupGroup, new Vector3(8.32f, 0.48f, 0f), new Vector3(0.16f, 0.75f, 1.30f), Quaternion.identity, cupMat, cupPhysMat);
        CreateBox("CupWall_Back", cupGroup, new Vector3(7.65f, 0.48f, 0.58f), new Vector3(1.50f, 0.75f, 0.16f), Quaternion.identity, cupMat, cupPhysMat);
        CreateBox("CupWall_Front", cupGroup, new Vector3(7.65f, 0.36f, -0.58f), new Vector3(1.50f, 0.50f, 0.16f), Quaternion.identity, cupMat, cupPhysMat);

        // Glowing Goal Beacon Pillar behind the Goal Cup
        GameObject beaconObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beaconObj.name = "GoalBeacon";
        beaconObj.transform.SetParent(cupGroup, false);
        beaconObj.transform.localPosition = new Vector3(8.65f, 0.75f, 0f);
        beaconObj.transform.localScale = new Vector3(0.32f, 0.75f, 0.32f);
        Renderer beaconRenderer = beaconObj.GetComponent<Renderer>();
        beaconRenderer.sharedMaterial = beaconMat;

        // Celebration Particle System
        GameObject particlesObj = new GameObject("CelebrationParticles");
        particlesObj.transform.SetParent(cupGroup, false);
        particlesObj.transform.localPosition = new Vector3(7.65f, 0.65f, 0f);
        particlesObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        ConfigureCelebrationParticles(ps, finalBallMat);

        // Goal Sensor Trigger inside Goal Cup
        GameObject sensorObj = new GameObject("GoalSensor");
        sensorObj.transform.SetParent(cupGroup, false);
        sensorObj.transform.localPosition = new Vector3(7.65f, 0.42f, 0f);
        BoxCollider sensorCol = sensorObj.AddComponent<BoxCollider>();
        sensorCol.size = new Vector3(1.10f, 0.55f, 0.95f);
        sensorCol.isTrigger = true;

        GoalTrigger goalTrigger = sensorObj.AddComponent<GoalTrigger>();
        goalTrigger.controller = controller;
        goalTrigger.indicatorRenderer = beaconRenderer;
        goalTrigger.celebrationParticles = ps;
        goalTrigger.targetBall = finalBallObj.transform;
        controller.goalTrigger = goalTrigger;

        // Position Main Camera for a clear initial view of the entire machine
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = controller.overviewPosition;
            mainCam.transform.LookAt(controller.overviewLookTarget);
            controller.targetCamera = mainCam;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            AssetDatabase.SaveAssets();
        }
#endif

        controller.RecordSnapshots();
        return controller;
    }

#if UNITY_EDITOR
    private static void EnsureEditorFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        if (!AssetDatabase.IsValidFolder(GeneratedAssetsFolder))
        {
            AssetDatabase.CreateFolder("Assets/Materials", "RubeGoldberg");
        }
    }
#endif

    private static Transform CreateGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject CreateBox(
        string name,
        Transform parent,
        Vector3 localPos,
        Vector3 localScale,
        Quaternion localRot,
        Material visualMat,
        PhysicsMaterial physMat)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localRotation = localRot;
        box.transform.localScale = localScale;

        if (visualMat != null)
        {
            box.GetComponent<Renderer>().sharedMaterial = visualMat;
        }

        if (physMat != null)
        {
            box.GetComponent<BoxCollider>().material = physMat;
        }

        return box;
    }

    private static GameObject CreateTrackWithRails(
        string name,
        Transform parent,
        Vector3 position,
        Vector3 size,
        Quaternion rotation,
        Material trackMat,
        Material railMat,
        PhysicsMaterial physMat,
        float railHeight)
    {
        GameObject trackGroup = new GameObject(name);
        trackGroup.transform.SetParent(parent, false);
        trackGroup.transform.localPosition = position;
        trackGroup.transform.localRotation = rotation;

        CreateBox("Plank", trackGroup.transform, Vector3.zero, size, Quaternion.identity, trackMat, physMat);

        float railZ = (size.z * 0.5f) - 0.05f;
        float railY = (size.y * 0.5f) + (railHeight * 0.5f);
        CreateBox("Rail_Back", trackGroup.transform, new Vector3(0f, railY, railZ), new Vector3(size.x, railHeight, 0.08f), Quaternion.identity, railMat, physMat);
        CreateBox("Rail_Front", trackGroup.transform, new Vector3(0f, railY, -railZ), new Vector3(size.x, railHeight * 0.65f, 0.08f), Quaternion.identity, railMat, physMat);

        return trackGroup;
    }

    private static PhysicsMaterial CreatePhysicsMaterial(string name, float dynamicFriction, float staticFriction, float bounciness)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string assetPath = $"{GeneratedAssetsFolder}/{name}.physicMaterial";
            PhysicsMaterial existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(assetPath);
            if (existing != null)
            {
                existing.dynamicFriction = dynamicFriction;
                existing.staticFriction = staticFriction;
                existing.bounciness = bounciness;
                existing.frictionCombine = PhysicsMaterialCombine.Minimum;
                existing.bounceCombine = PhysicsMaterialCombine.Average;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            PhysicsMaterial created = new PhysicsMaterial(name)
            {
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                bounciness = bounciness,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Average
            };
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }
#endif
        return new PhysicsMaterial(name)
        {
            dynamicFriction = dynamicFriction,
            staticFriction = staticFriction,
            bounciness = bounciness,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Average
        };
    }

    private static Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            string assetPath = $"{GeneratedAssetsFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null)
            {
                ApplyMaterialProperties(existing, color, metallic, smoothness);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Material created = new Material(shader) { name = name };
            ApplyMaterialProperties(created, color, metallic, smoothness);
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }
#endif

        Material mat = new Material(shader) { name = name };
        ApplyMaterialProperties(mat, color, metallic, smoothness);
        return mat;
    }

    private static void ApplyMaterialProperties(Material mat, Color color, float metallic, float smoothness)
    {
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.color = color;
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureCelebrationParticles(ParticleSystem ps, Material particleMat)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 2.0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
        main.gravityModifier = 1.2f;
        main.maxParticles = 120;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0.0f, 65),
            new ParticleSystem.Burst(0.25f, 45)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 32f;
        shape.radius = 0.25f;

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        psRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(tempCube);
        psRenderer.mesh = cubeMesh;
        psRenderer.sharedMaterial = particleMat;
    }
}
