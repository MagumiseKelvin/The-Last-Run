using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Builds the entire Game scene from scratch with one click.
/// Menu: The Last Run -> Build Game Scene
/// </summary>
public class SceneBuilder : EditorWindow
{
    // ── Entry Point ───────────────────────────────────────────────────────────
    [MenuItem("The Last Run/Build Game Scene")]
    public static void BuildScene()
    {
        Debug.Log("[SceneBuilder] Building scene...");

        // ── Ensure required tags exist ────────────────────────────────────────
        EnsureTag("Obstacle");
        EnsureTag("Collectible");
        EnsureTag("Player");

        // Delete old prefabs so they get recreated fresh
        DeleteAssetIfExists("Assets/Prefabs/TrackSegment_Straight.prefab");
        DeleteAssetIfExists("Assets/Prefabs/Obstacle_Barrier.prefab");
        DeleteAssetIfExists("Assets/Prefabs/Obstacle_LowBeam.prefab");
        DeleteAssetIfExists("Assets/Prefabs/Coin.prefab");
        AssetDatabase.Refresh();

        // Wipe scene objects
        ClearScene();

        // Ensure folders
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Materials");

        BuildManagers();
        GameObject player = BuildPlayer();
        BuildTrack();
        BuildCamera(player);
        BuildLighting();
        BuildUI();

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SceneBuilder] Done!");
        EditorUtility.DisplayDialog("The Last Run",
            "Scene built!\n\nPress Play to test.", "OK");
    }

    // ── Managers ──────────────────────────────────────────────────────────────
    static void BuildManagers()
    {
        CreateEmpty("GameManager").AddComponent<GameManager>();
        CreateEmpty("ScoreManager").AddComponent<ScoreManager>();
        CreateEmpty("PowerUpManager").AddComponent<PowerUpManager>();

        var obs = CreateEmpty("ObstacleSpawner");
        var col = CreateEmpty("CollectibleSpawner");
        var trk = CreateEmpty("TrackManager");
        obs.AddComponent<ObstacleSpawner>();
        col.AddComponent<CollectibleSpawner>();
        trk.AddComponent<TrackManager>();

        var am_go = CreateEmpty("AudioManager");
        var am    = am_go.AddComponent<AudioManager>();
        var mus   = am_go.AddComponent<AudioSource>();
        var sfx   = am_go.AddComponent<AudioSource>();
        mus.loop = true; mus.playOnAwake = false;
        sfx.playOnAwake = false;
        am.musicSource = mus;
        am.sfxSource   = sfx;
    }

    // ── Player ────────────────────────────────────────────────────────────────
    static GameObject BuildPlayer()
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag  = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);

        // Blue material
        ApplyMaterial(player, "Player", new Color(0.15f, 0.55f, 1f));

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        var cc    = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerCollision>();
        player.AddComponent<PlayerAnimator>();

        // Speed trail effect
        player.AddComponent<TrailRenderer>(); // required by PlayerTrailEffect
        player.AddComponent<PlayerTrailEffect>();

        // Animator controller
        EnsureFolder("Assets/Animations");
        const string animPath = "Assets/Animations/PlayerAnimatorController.controller";
        UnityEditor.Animations.AnimatorController ctrl;
        if (!File.Exists(Application.dataPath + "/Animations/PlayerAnimatorController.controller"))
        {
            ctrl = UnityEditor.Animations.AnimatorController
                       .CreateAnimatorControllerAtPath(animPath);
            ctrl.AddParameter("IsRunning",  AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("IsSliding",  AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Lean",       AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Death",      AnimatorControllerParameterType.Trigger);
        }
        else
        {
            ctrl = AssetDatabase.LoadAssetAtPath
                       <UnityEditor.Animations.AnimatorController>(animPath);
        }

        var anim = player.GetComponent<Animator>()
                   ?? player.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;

        return player;
    }

    // ── Track ─────────────────────────────────────────────────────────────────
    static void BuildTrack()
    {
        // ── Ground plane (wide, dark) ─────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position   = new Vector3(0f, -0.11f, 50f);
        ground.transform.localScale = new Vector3(6f, 1f, 120f);
        ApplyMaterial(ground, "Ground", new Color(0.06f, 0.06f, 0.08f));

        // ── Track segment prefab ──────────────────────────────────────────────
        var seg = new GameObject("TrackSegment_Straight");

        // Road surface
        var road = MakeCube("Road", seg.transform,
            new Vector3(7.5f, 0.2f, 30f), Vector3.zero,
            new Color(0.13f, 0.13f, 0.16f));
        Object.DestroyImmediate(road.GetComponent<BoxCollider>());
        // Re-add single collider for the road so player walks on it
        var roadCol = road.AddComponent<BoxCollider>();

        // Lane dividers (white dashes)
        for (int i = 0; i < 2; i++)
        {
            float x = (i == 0) ? -1.25f : 1.25f;
            var div = MakeCube($"Divider_{i}", seg.transform,
                new Vector3(0.07f, 0.21f, 30f), new Vector3(x, 0f, 0f),
                new Color(0.85f, 0.85f, 0.85f));
            Object.DestroyImmediate(div.GetComponent<BoxCollider>());
        }

        // Side kerbs (bright accent — cyan/teal)
        for (int i = 0; i < 2; i++)
        {
            float x = (i == 0) ? -3.85f : 3.85f;
            var kerb = MakeCube($"Kerb_{i}", seg.transform,
                new Vector3(0.25f, 0.3f, 30f), new Vector3(x, 0.05f, 0f),
                new Color(0.0f, 0.75f, 0.85f));
            Object.DestroyImmediate(kerb.GetComponent<BoxCollider>());
        }

        // Obstacle spawn points — staggered per lane at ground level
        for (int i = 0; i < 3; i++)
        {
            var sp = CreateEmpty($"ObstacleSpawn_{i + 1}", seg.transform);
            sp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.1f, 5f + i * 4f);
        }

        // Coin spawn points — at player chest height (0.8f), reachable by running
        for (int i = 0; i < 3; i++)
        {
            var cp = CreateEmpty($"CoinSpawn_{i + 1}", seg.transform);
            cp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.75f, 3f + i * 3f);
        }

        var ts = seg.AddComponent<TrackSegment>();
        ts.obstacleSpawnPoints    = GetChildTransforms(seg, "ObstacleSpawn");
        ts.collectibleSpawnPoints = GetChildTransforms(seg, "CoinSpawn");

        var trackPrefab = PrefabUtility.SaveAsPrefabAsset(seg, "Assets/Prefabs/TrackSegment_Straight.prefab");
        Object.DestroyImmediate(seg);

        // ── Barrier obstacle (white wall — dodge left/right) ──────────────────
        var barrier = new GameObject("Obstacle_Barrier");

        var bBody = MakeCube("Body", barrier.transform,
            new Vector3(2.2f, 1.5f, 0.3f), new Vector3(0f, 0.75f, 0f),
            new Color(0.92f, 0.92f, 0.95f));
        Object.DestroyImmediate(bBody.GetComponent<BoxCollider>());

        var bStripe = MakeCube("Stripe", barrier.transform,
            new Vector3(2.2f, 0.2f, 0.31f), new Vector3(0f, 1.2f, 0f),
            new Color(1f, 0.42f, 0f));
        Object.DestroyImmediate(bStripe.GetComponent<BoxCollider>());

        var bStripe2 = MakeCube("Stripe2", barrier.transform,
            new Vector3(2.2f, 0.2f, 0.31f), new Vector3(0f, 0.35f, 0f),
            new Color(1f, 0.42f, 0f));
        Object.DestroyImmediate(bStripe2.GetComponent<BoxCollider>());

        var bCol = barrier.AddComponent<BoxCollider>();
        bCol.isTrigger = true;
        bCol.center    = new Vector3(0f, 0.75f, 0f);
        bCol.size      = new Vector3(2.2f, 1.5f, 0.3f);
        barrier.AddComponent<ObstacleBarrier>();
        // Tag so PlayerCollision detects it without layer setup
        barrier.tag = "Obstacle";
        var barrierPrefab = PrefabUtility.SaveAsPrefabAsset(barrier, "Assets/Prefabs/Obstacle_Barrier.prefab");
        Object.DestroyImmediate(barrier);

        // ── Low beam obstacle (red bar — must slide under) ────────────────────
        var beam = new GameObject("Obstacle_LowBeam");

        var bBar = MakeCube("Bar", beam.transform,
            new Vector3(7.3f, 0.22f, 0.28f), new Vector3(0f, 1.05f, 0f),
            new Color(0.95f, 0.15f, 0.15f));
        Object.DestroyImmediate(bBar.GetComponent<BoxCollider>());

        for (int i = 0; i < 2; i++)
        {
            float x = (i == 0) ? -3.4f : 3.4f;
            var post = MakeCube($"Post_{i}", beam.transform,
                new Vector3(0.18f, 1.05f, 0.18f), new Vector3(x, 0.525f, 0f),
                new Color(0.75f, 0.75f, 0.75f));
            Object.DestroyImmediate(post.GetComponent<BoxCollider>());
        }

        var beamCol = beam.AddComponent<BoxCollider>();
        beamCol.isTrigger = true;
        beamCol.center    = new Vector3(0f, 1.05f, 0f);
        beamCol.size      = new Vector3(7.3f, 0.22f, 0.28f);
        beam.AddComponent<ObstacleLowBeam>();
        beam.tag = "Obstacle";
        var beamPrefab = PrefabUtility.SaveAsPrefabAsset(beam, "Assets/Prefabs/Obstacle_LowBeam.prefab");
        Object.DestroyImmediate(beam);

        // ── Coin prefab (gold disc, spins) ────────────────────────────────────
        var coin = new GameObject("Coin");

        var disc = MakeCylinder("Disc", coin.transform,
            new Vector3(0.42f, 0.055f, 0.42f), Vector3.zero,
            new Color(1f, 0.80f, 0f));
        Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());

        // Inner ring (slightly darker gold)
        var inner = MakeCylinder("Inner", coin.transform,
            new Vector3(0.25f, 0.06f, 0.25f), Vector3.zero,
            new Color(0.85f, 0.62f, 0f));
        Object.DestroyImmediate(inner.GetComponent<CapsuleCollider>());

        var coinCol = coin.AddComponent<SphereCollider>();
        coinCol.isTrigger = true;
        coinCol.radius    = 0.32f;

        var coinScript       = coin.AddComponent<Collectible>();
        coinScript.scoreValue = 50;
        coinScript.spin       = true;
        coinScript.spinSpeed  = 180f;
        coin.tag = "Collectible";

        var coinPrefab = PrefabUtility.SaveAsPrefabAsset(coin, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(coin);

        // ── Wire spawners ─────────────────────────────────────────────────────
        var tm = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        tm.trackSegmentPrefabs = new GameObject[] { trackPrefab };

        var os = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>();
        os.obstaclePrefabs = new GameObject[] { barrierPrefab, beamPrefab };

        var cs = GameObject.Find("CollectibleSpawner").GetComponent<CollectibleSpawner>();
        cs.coinPrefab = coinPrefab;
    }

    // ── Camera ────────────────────────────────────────────────────────────────
    static void BuildCamera(GameObject player)
    {
        var cam = GameObject.FindWithTag("MainCamera");
        if (cam == null)
        {
            cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        // Position camera behind and above player — good third-person runner view
        cam.transform.position = new Vector3(0f, 3.5f, -5f);
        cam.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        // Set camera background to dark sky color
        var camComp = cam.GetComponent<Camera>();
        camComp.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
        camComp.clearFlags      = CameraClearFlags.SolidColor;
        camComp.fieldOfView     = 70f;

        var cf    = cam.GetComponent<CameraFollow>() ?? cam.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offset = new Vector3(0f, 3.5f, -5f);
        cf.lookAtOffset = new Vector3(0f, 0.5f, 8f);

        // Camera shake on collision/game over
        if (cam.GetComponent<CameraShake>() == null)
            cam.AddComponent<CameraShake>();
    }

    // ── Lighting ──────────────────────────────────────────────────────────────
    static void BuildLighting()
    {
        var lo = GameObject.Find("Directional Light");
        if (lo == null)
        {
            lo = new GameObject("Directional Light");
            lo.AddComponent<Light>().type = LightType.Directional;
        }
        lo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        var l = lo.GetComponent<Light>();
        l.intensity = 1.1f;
        l.color     = new Color(1f, 0.95f, 0.88f);
    }

    // ── UI ────────────────────────────────────────────────────────────────────
    static void BuildUI()
    {
        // Remove old canvas completely
        var old = GameObject.Find("Canvas");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── HUD ───────────────────────────────────────────────────────────────
        // ── Score panel — dark pill, top center ──────────────────────────────
        var scoreBg = new GameObject("ScoreBackground");
        scoreBg.transform.SetParent(canvasGO.transform, false);
        scoreBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var scoreBgRt = scoreBg.GetComponent<RectTransform>();
        scoreBgRt.anchorMin = scoreBgRt.anchorMax = scoreBgRt.pivot = new Vector2(0.5f, 1f);
        scoreBgRt.anchoredPosition = new Vector2(0f, -8f);
        scoreBgRt.sizeDelta = new Vector2(200f, 95f);

        // "SCORE" label
        MakeTMP(canvasGO, "ScoreLabel", "SCORE",
            new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(190f, 28f),
            16, FontStyles.Normal, new Color(0.65f, 0.65f, 0.65f));

        // Score value — large yellow
        var scoreTxt = MakeTMP(canvasGO, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(190f, 60f),
            54, FontStyles.Bold, new Color(1f, 0.90f, 0.2f));
        scoreTxt.gameObject.AddComponent<ScoreMultiplierDisplay>();

        // ── Distance — top left ───────────────────────────────────────────────
        var distBg = new GameObject("DistBackground");
        distBg.transform.SetParent(canvasGO.transform, false);
        distBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        var distBgRt = distBg.GetComponent<RectTransform>();
        distBgRt.anchorMin = distBgRt.anchorMax = distBgRt.pivot = new Vector2(0f, 1f);
        distBgRt.anchoredPosition = new Vector2(10f, -8f);
        distBgRt.sizeDelta = new Vector2(140f, 50f);

        var distTxt = MakeTMP(canvasGO, "DistanceText", "0m",
            new Vector2(0f, 1f), new Vector2(80f, -33f), new Vector2(140f, 50f),
            26, FontStyles.Bold, Color.white);

        // ── Best score — top right ────────────────────────────────────────────
        var bestBg = new GameObject("BestBackground");
        bestBg.transform.SetParent(canvasGO.transform, false);
        bestBg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        var bestBgRt = bestBg.GetComponent<RectTransform>();
        bestBgRt.anchorMin = bestBgRt.anchorMax = bestBgRt.pivot = new Vector2(1f, 1f);
        bestBgRt.anchoredPosition = new Vector2(-10f, -8f);
        bestBgRt.sizeDelta = new Vector2(160f, 50f);

        var highTxt = MakeTMP(canvasGO, "HighScoreText", "Best: 0",
            new Vector2(1f, 1f), new Vector2(-90f, -33f), new Vector2(160f, 50f),
            24, FontStyles.Normal, new Color(0.75f, 0.75f, 0.75f));

        // ── Coins — below distance, gold ──────────────────────────────────────
        var coinBg = new GameObject("CoinBackground");
        coinBg.transform.SetParent(canvasGO.transform, false);
        coinBg.AddComponent<Image>().color = new Color(0.5f, 0.35f, 0f, 0.55f);
        var coinBgRt = coinBg.GetComponent<RectTransform>();
        coinBgRt.anchorMin = coinBgRt.anchorMax = coinBgRt.pivot = new Vector2(0f, 1f);
        coinBgRt.anchoredPosition = new Vector2(10f, -62f);
        coinBgRt.sizeDelta = new Vector2(150f, 40f);

        var coinTxt = MakeTMP(canvasGO, "CoinCountText", "Coins: 0",
            new Vector2(0f, 1f), new Vector2(85f, -82f), new Vector2(150f, 40f),
            22, FontStyles.Bold, new Color(1f, 0.85f, 0.15f));

        // Speed — bottom left
        var speedTxt = MakeTMP(canvasGO, "SpeedText", "8 m/s",
            new Vector2(0f, 0f), new Vector2(110f, 55f), new Vector2(180f, 40f),
            22, FontStyles.Normal, new Color(0.6f, 0.6f, 0.6f));

        // ── Pause button — top right corner ───────────────────────────────────
        // Use plain ASCII "II" — no special characters
        var pauseBtn = MakeButton(canvasGO, "PauseButton", "II",
            new Vector2(1f, 1f), new Vector2(-55f, -55f), new Vector2(65f, 65f),
            new Color(0.1f, 0.1f, 0.1f, 0.75f), 28);

        // ── Game Over Panel ───────────────────────────────────────────────────
        var goPanel = MakePanel(canvasGO, "GameOverPanel",
            new Color(0.04f, 0.04f, 0.06f, 0.94f),
            Vector2.zero, new Vector2(640f, 540f));
        goPanel.SetActive(false);

        MakeTMP(goPanel, "GOTitle", "GAME OVER",
            new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 75f),
            62, FontStyles.Bold, new Color(1f, 0.35f, 0.35f));

        var goScore = MakeTMP(goPanel, "GameOverScoreText", "0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(400f, 80f),
            76, FontStyles.Bold, Color.white);

        var goHigh = MakeTMP(goPanel, "GameOverHighScoreText", "Best: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(400f, 50f),
            30, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));

        var goDist = MakeTMP(goPanel, "GameOverDistanceText", "0m",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(400f, 45f),
            28, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));

        var goCoins = MakeTMP(goPanel, "GameOverCoinsText", "Coins: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(400f, 45f),
            28, FontStyles.Normal, new Color(1f, 0.85f, 0.2f));

        // New best banner — plain text, no special chars
        var newBestGO  = new GameObject("NewBestBanner");
        newBestGO.transform.SetParent(goPanel.transform, false);
        var newBestTMP = newBestGO.AddComponent<TextMeshProUGUI>();
        newBestTMP.text      = "NEW BEST!";
        newBestTMP.fontSize  = 38;
        newBestTMP.fontStyle = FontStyles.Bold;
        newBestTMP.color     = new Color(1f, 0.85f, 0.1f);
        newBestTMP.alignment = TextAlignmentOptions.Center;
        var nbRT = newBestGO.GetComponent<RectTransform>();
        nbRT.anchorMin = nbRT.anchorMax = nbRT.pivot = new Vector2(0.5f, 0.5f);
        nbRT.anchoredPosition = new Vector2(0f, 155f);
        nbRT.sizeDelta        = new Vector2(340f, 60f);
        newBestGO.SetActive(false);

        var restartBtn = MakeButton(goPanel, "RestartButton", "PLAY AGAIN",
            new Vector2(0.5f, 0f), new Vector2(-115f, 65f), new Vector2(210f, 58f),
            new Color(0.08f, 0.65f, 0.18f), 22);

        var menuBtn = MakeButton(goPanel, "MenuButton", "MAIN MENU",
            new Vector2(0.5f, 0f), new Vector2(115f, 65f), new Vector2(210f, 58f),
            new Color(0.15f, 0.35f, 0.75f), 22);

        // ── Pause Panel ───────────────────────────────────────────────────────
        var pausePanel = MakePanel(canvasGO, "PausePanel",
            new Color(0.04f, 0.04f, 0.06f, 0.95f),
            Vector2.zero, new Vector2(440f, 440f));
        pausePanel.SetActive(false);

        MakeTMP(pausePanel, "PauseTitle", "PAUSED",
            new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(340f, 65f),
            52, FontStyles.Bold, Color.white);

        var resumeBtn  = MakeButton(pausePanel, "ResumeButton", "RESUME",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 75f), new Vector2(230f, 58f),
            new Color(0.08f, 0.65f, 0.18f), 22);

        var pRestartBtn = MakeButton(pausePanel, "PauseRestartButton", "RESTART",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(230f, 58f),
            new Color(0.75f, 0.45f, 0.08f), 22);

        var pMenuBtn = MakeButton(pausePanel, "PauseMenuButton", "MAIN MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -75f), new Vector2(230f, 58f),
            new Color(0.15f, 0.35f, 0.75f), 22);

        // ── Countdown Panel ───────────────────────────────────────────────────
        var countPanel = MakePanel(canvasGO, "CountdownPanel",
            new Color(0f, 0f, 0f, 0f), Vector2.zero, new Vector2(300f, 220f));
        countPanel.SetActive(false);

        var countTxt = MakeTMP(countPanel, "CountdownText", "3",
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 200f),
            130, FontStyles.Bold, Color.white);

        // ── Wire GameHUD ──────────────────────────────────────────────────────
        var hud = canvasGO.GetComponent<GameHUD>() ?? canvasGO.AddComponent<GameHUD>();
        hud.scoreText             = scoreTxt;
        hud.highScoreText         = highTxt;
        hud.distanceText          = distTxt;
        hud.coinCountText         = coinTxt;
        hud.speedText             = speedTxt;
        hud.gameOverPanel         = goPanel;
        hud.gameOverScoreText     = goScore;
        hud.gameOverHighScoreText = goHigh;
        hud.gameOverDistanceText  = goDist;
        hud.gameOverCoinsText     = goCoins;
        hud.newHighScoreBanner    = newBestTMP;
        hud.restartButton         = restartBtn.GetComponent<Button>();
        hud.menuButton            = menuBtn.GetComponent<Button>();
        hud.pauseButton           = pauseBtn.GetComponent<Button>();
        hud.resumeButton          = resumeBtn.GetComponent<Button>();
        hud.pauseRestartButton    = pRestartBtn.GetComponent<Button>();
        hud.pauseMenuButton       = pMenuBtn.GetComponent<Button>();
        hud.pausePanel            = pausePanel;
        hud.countdownPanel        = countPanel;
        hud.countdownText         = countTxt;

        // ── Wire GameOverUI ───────────────────────────────────────────────────
        var goUI = goPanel.GetComponent<GameOverUI>() ?? goPanel.AddComponent<GameOverUI>();
        goUI.finalScoreText  = goScore;
        goUI.highScoreText   = goHigh;
        goUI.distanceText    = goDist;
        goUI.coinsText       = goCoins;
        goUI.newBestBanner   = newBestGO;
        goUI.restartButton   = restartBtn.GetComponent<Button>();
        goUI.mainMenuButton  = menuBtn.GetComponent<Button>();

        // ── ScorePopupSpawner ─────────────────────────────────────────────────
        var popupGO = CreateEmpty("ScorePopupSpawner", canvasGO.transform);
        var sps     = popupGO.AddComponent<ScorePopupSpawner>();
        sps.hudCanvas  = canvas;
        sps.mainCamera = Camera.main;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void ClearScene()
    {
        string[] keep = { "Main Camera", "Directional Light" };
        // Collect first, then destroy — avoids modifying collection during iteration
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (var go in Object.FindObjectsByType<GameObject>())
        {
            if (go.transform.parent != null) continue;
            if (System.Array.Exists(keep, n => go.name == n)) continue;
            toDestroy.Add(go);
        }
        foreach (var go in toDestroy)
            if (go != null) Object.DestroyImmediate(go);
    }

    static void DeleteAssetIfExists(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);
    }

    static void EnsureTag(string tagName)
    {
        // Read the TagManager asset
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                return; // already exists
        }

        // Add the tag
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[SceneBuilder] Created tag: {tagName}");
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    static GameObject CreateEmpty(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>
    /// Creates a material saved as an asset so URP doesn't lose the shader reference.
    /// Tries URP Lit first, falls back to Standard.
    /// </summary>
    static void ApplyMaterial(GameObject go, string matName, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;

        string path = $"Assets/Materials/TLR_{matName}.mat";

        // Delete old version so we always get a fresh one
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            AssetDatabase.DeleteAsset(path);

        // Try URP shaders in order
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (sh == null) sh = Shader.Find("Standard");

        var mat   = new Material(sh);
        mat.color = color;

        // For URP Lit, also set the base map color
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        AssetDatabase.CreateAsset(mat, path);
        r.sharedMaterial = mat;
    }

    static GameObject MakeCube(string name, Transform parent,
        Vector3 scale, Vector3 localPos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale    = scale;
        go.transform.localPosition = localPos;
        ApplyMaterial(go, name, color);
        return go;
    }

    static GameObject MakeCylinder(string name, Transform parent,
        Vector3 scale, Vector3 localPos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale    = scale;
        go.transform.localPosition = localPos;
        ApplyMaterial(go, name, color);
        return go;
    }

    static Transform[] GetChildTransforms(GameObject parent, string contains)
    {
        var list = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in parent.transform)
            if (t.name.Contains(contains)) list.Add(t);
        return list.ToArray();
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text,
        Vector2 anchor, Vector2 pos, Vector2 size,
        float fontSize, FontStyles style, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp       = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return tmp;
    }

    static GameObject MakePanel(GameObject parent, string name,
        Color color, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img   = go.AddComponent<Image>();
        img.color = color;
        var rt    = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        go.AddComponent<CanvasGroup>();
        return go;
    }

    static GameObject MakeButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 pos, Vector2 size, Color bg, float fontSize = 22)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img   = go.AddComponent<Image>();
        img.color = bg;
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        // Label — only plain ASCII characters
        var lbl = new GameObject("Text");
        lbl.transform.SetParent(go.transform, false);
        var tmp       = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;   // caller must pass plain ASCII
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        return go;
    }
}
