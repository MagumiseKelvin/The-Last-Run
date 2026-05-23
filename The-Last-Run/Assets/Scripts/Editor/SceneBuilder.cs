using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Editor tool that builds the entire Game scene automatically.
/// Run it from the Unity menu: The Last Run → Build Game Scene
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("The Last Run/Build Game Scene")]
    public static void BuildScene()
    {
        Debug.Log("[SceneBuilder] Starting scene build...");

        ClearScene();

        // ── Managers ──────────────────────────────────────────────────────────
        CreateEmpty("GameManager").AddComponent<GameManager>();
        CreateEmpty("ScoreManager").AddComponent<ScoreManager>();
        CreateEmpty("PowerUpManager").AddComponent<PowerUpManager>();

        GameObject obstacleSpawnerObj    = CreateEmpty("ObstacleSpawner");
        GameObject collectibleSpawnerObj = CreateEmpty("CollectibleSpawner");
        GameObject trackManagerObj       = CreateEmpty("TrackManager");

        obstacleSpawnerObj.AddComponent<ObstacleSpawner>();
        collectibleSpawnerObj.AddComponent<CollectibleSpawner>();
        trackManagerObj.AddComponent<TrackManager>();

        // AudioManager needs two AudioSources
        GameObject audioManagerObj = CreateEmpty("AudioManager");
        AudioManager am  = audioManagerObj.AddComponent<AudioManager>();
        AudioSource music = audioManagerObj.AddComponent<AudioSource>();
        AudioSource sfx   = audioManagerObj.AddComponent<AudioSource>();
        music.loop = true; music.playOnAwake = false;
        sfx.playOnAwake = false;
        am.musicSource = music;
        am.sfxSource   = sfx;

        // ── Ground ────────────────────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position   = new Vector3(0f, -0.15f, 50f);
        ground.transform.localScale = new Vector3(5f, 1f, 100f);
        SetColor(ground, new Color(0.08f, 0.08f, 0.10f)); // near-black dark slate

        // ── Track Segment Prefab ──────────────────────────────────────────────
        EnsureFolder("Assets/Prefabs");

        GameObject trackSeg = new GameObject("TrackSegment_Straight");

        // Road surface — dark asphalt
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.SetParent(trackSeg.transform, false);
        road.transform.localScale    = new Vector3(7.5f, 0.2f, 30f);
        road.transform.localPosition = Vector3.zero;
        SetColor(road, new Color(0.12f, 0.12f, 0.14f));

        // Lane dividers — two thin white strips
        for (int i = 0; i < 2; i++)
        {
            GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = $"LaneDivider_{i}";
            divider.transform.SetParent(trackSeg.transform, false);
            divider.transform.localScale    = new Vector3(0.08f, 0.21f, 30f);
            divider.transform.localPosition = new Vector3((i == 0 ? -1.25f : 1.25f), 0f, 0f);
            SetColor(divider, new Color(0.9f, 0.9f, 0.9f));
            Object.DestroyImmediate(divider.GetComponent<BoxCollider>());
        }

        // Side walls — dark grey borders
        for (int i = 0; i < 2; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"SideWall_{i}";
            wall.transform.SetParent(trackSeg.transform, false);
            wall.transform.localScale    = new Vector3(0.3f, 0.8f, 30f);
            wall.transform.localPosition = new Vector3((i == 0 ? -3.9f : 3.9f), 0.3f, 0f);
            SetColor(wall, new Color(0.18f, 0.18f, 0.22f));
            Object.DestroyImmediate(wall.GetComponent<BoxCollider>());
        }

        trackSeg.AddComponent<TrackSegment>();

        // Obstacle spawn points — one per lane
        for (int i = 0; i < 3; i++)
        {
            GameObject sp = CreateEmpty($"ObstacleSpawn_{i + 1}", trackSeg.transform);
            sp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.1f, (i % 3) * 6f - 6f);
        }
        // Coin spawn points
        for (int i = 0; i < 3; i++)
        {
            GameObject cp = CreateEmpty($"CoinSpawn_{i + 1}", trackSeg.transform);
            cp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.8f, i * 3f);
        }

        TrackSegment ts = trackSeg.GetComponent<TrackSegment>();
        ts.obstacleSpawnPoints    = GetChildTransforms(trackSeg, "ObstacleSpawn");
        ts.collectibleSpawnPoints = GetChildTransforms(trackSeg, "CoinSpawn");

        string prefabPath = "Assets/Prefabs/TrackSegment_Straight.prefab";
        GameObject trackPrefab = PrefabUtility.SaveAsPrefabAsset(trackSeg, prefabPath);
        Object.DestroyImmediate(trackSeg);

        trackManagerObj.GetComponent<TrackManager>().trackSegmentPrefabs = new GameObject[] { trackPrefab };

        // ── Barrier Obstacle (full-height wall — must dodge left/right) ───────
        GameObject barrierObj = new GameObject("Obstacle_Barrier");
        // Main body
        GameObject barrierBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrierBody.name = "Body";
        barrierBody.transform.SetParent(barrierObj.transform, false);
        barrierBody.transform.localScale    = new Vector3(2.2f, 1.6f, 0.35f);
        barrierBody.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        SetColor(barrierBody, new Color(0.95f, 0.95f, 1.0f));  // bright white
        // Warning stripes — orange accent bar
        GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stripe.name = "Stripe";
        stripe.transform.SetParent(barrierObj.transform, false);
        stripe.transform.localScale    = new Vector3(2.2f, 0.18f, 0.36f);
        stripe.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        SetColor(stripe, new Color(1f, 0.45f, 0f));  // orange
        Object.DestroyImmediate(stripe.GetComponent<BoxCollider>());
        // Trigger collider on root
        BoxCollider barrierCol = barrierObj.AddComponent<BoxCollider>();
        barrierCol.isTrigger = true;
        barrierCol.center = new Vector3(0f, 0.8f, 0f);
        barrierCol.size   = new Vector3(2.2f, 1.6f, 0.35f);
        barrierObj.AddComponent<ObstacleBarrier>();
        GameObject barrierPrefab = PrefabUtility.SaveAsPrefabAsset(barrierObj, "Assets/Prefabs/Obstacle_Barrier.prefab");
        Object.DestroyImmediate(barrierObj);

        // ── Low Beam Obstacle (must slide under) ──────────────────────────────
        GameObject beamObj = new GameObject("Obstacle_LowBeam");
        // Horizontal beam
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "Beam";
        beam.transform.SetParent(beamObj.transform, false);
        beam.transform.localScale    = new Vector3(7.4f, 0.25f, 0.3f);
        beam.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        SetColor(beam, new Color(1f, 0.2f, 0.2f));  // red beam
        // Left post
        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postL.name = "PostL";
        postL.transform.SetParent(beamObj.transform, false);
        postL.transform.localScale    = new Vector3(0.2f, 1.1f, 0.2f);
        postL.transform.localPosition = new Vector3(-3.5f, 0.55f, 0f);
        SetColor(postL, new Color(0.8f, 0.8f, 0.8f));
        Object.DestroyImmediate(postL.GetComponent<BoxCollider>());
        // Right post
        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postR.name = "PostR";
        postR.transform.SetParent(beamObj.transform, false);
        postR.transform.localScale    = new Vector3(0.2f, 1.1f, 0.2f);
        postR.transform.localPosition = new Vector3(3.5f, 0.55f, 0f);
        SetColor(postR, new Color(0.8f, 0.8f, 0.8f));
        Object.DestroyImmediate(postR.GetComponent<BoxCollider>());
        // Trigger collider
        BoxCollider beamCol = beamObj.AddComponent<BoxCollider>();
        beamCol.isTrigger = true;
        beamCol.center = new Vector3(0f, 1.1f, 0f);
        beamCol.size   = new Vector3(7.4f, 0.25f, 0.3f);
        beamObj.AddComponent<ObstacleLowBeam>();
        GameObject beamPrefab = PrefabUtility.SaveAsPrefabAsset(beamObj, "Assets/Prefabs/Obstacle_LowBeam.prefab");
        Object.DestroyImmediate(beamObj);

        obstacleSpawnerObj.GetComponent<ObstacleSpawner>().obstaclePrefabs = new GameObject[] { barrierPrefab, beamPrefab };

        // ── Coin Prefab — gold spinning disc ─────────────────────────────────
        GameObject coinObj = new GameObject("Coin");
        GameObject coinDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coinDisc.name = "Disc";
        coinDisc.transform.SetParent(coinObj.transform, false);
        coinDisc.transform.localScale    = new Vector3(0.45f, 0.06f, 0.45f);
        coinDisc.transform.localPosition = Vector3.zero;
        SetColor(coinDisc, new Color(1f, 0.82f, 0f));  // gold
        Object.DestroyImmediate(coinDisc.GetComponent<CapsuleCollider>());
        // Trigger collider on root
        SphereCollider coinCol = coinObj.AddComponent<SphereCollider>();
        coinCol.isTrigger = true;
        coinCol.radius = 0.35f;
        Collectible coinScript = coinObj.AddComponent<Collectible>();
        coinScript.scoreValue = 50;
        coinScript.spin = true;
        coinScript.spinSpeed = 200f;
        GameObject coinPrefab = PrefabUtility.SaveAsPrefabAsset(coinObj, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(coinObj);
        collectibleSpawnerObj.GetComponent<CollectibleSpawner>().coinPrefab = coinPrefab;

        // ── Player ────────────────────────────────────────────────────────────
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.tag = "Player";
        SetColor(player, new Color(0.2f, 0.6f, 1.0f));  // cool blue
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerCollision>();
        player.AddComponent<PlayerAnimator>();

        // Animator Controller
        EnsureFolder("Assets/Animations");
        string animPath = "Assets/Animations/PlayerAnimatorController.controller";
        UnityEditor.Animations.AnimatorController animCtrl;
        if (!File.Exists(Application.dataPath + "/Animations/PlayerAnimatorController.controller"))
        {
            animCtrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animPath);
            animCtrl.AddParameter("IsRunning",  AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("IsSliding",  AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
            animCtrl.AddParameter("Lean",       AnimatorControllerParameterType.Float);
            animCtrl.AddParameter("Death",      AnimatorControllerParameterType.Trigger);
        }
        else
        {
            animCtrl = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(animPath);
        }
        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        anim.runtimeAnimatorController = animCtrl;

        // ── Camera ────────────────────────────────────────────────────────────
        GameObject camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null) { camObj = new GameObject("Main Camera"); camObj.AddComponent<Camera>(); camObj.tag = "MainCamera"; }
        camObj.transform.position = new Vector3(0f, 4f, -6f);
        camObj.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        CameraFollow cf = camObj.GetComponent<CameraFollow>() ?? camObj.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offset = new Vector3(0f, 3f, -6f);

        // ── Light ─────────────────────────────────────────────────────────────
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null) { lightObj = new GameObject("Directional Light"); lightObj.AddComponent<Light>().type = LightType.Directional; }
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        lightObj.GetComponent<Light>().intensity = 1.2f;

        // ── UI ────────────────────────────────────────────────────────────────
        BuildUI(player);

        // ── Save ──────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SceneBuilder] Scene built successfully!");
        EditorUtility.DisplayDialog("The Last Run", "Scene built successfully!\n\nPress Play to test.", "OK");
    }

    // ── UI Builder ────────────────────────────────────────────────────────────

    private static void BuildUI(GameObject player)
    {
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // HUD texts
        TextMeshProUGUI scoreTxt  = CreateTMPText(canvasObj, "ScoreText",     "0",       new Vector2(0.5f,1f), new Vector2(0f,-60f),   56, FontStyles.Bold);
        TextMeshProUGUI distTxt   = CreateTMPText(canvasObj, "DistanceText",  "0m",      new Vector2(0f,1f),   new Vector2(120f,-55f),  32, FontStyles.Normal);
        TextMeshProUGUI highTxt   = CreateTMPText(canvasObj, "HighScoreText", "Best: 0", new Vector2(1f,1f),   new Vector2(-120f,-55f), 28, FontStyles.Normal);
        TextMeshProUGUI coinTxt   = CreateTMPText(canvasObj, "CoinCountText", "x0",      new Vector2(0f,1f),   new Vector2(120f,-95f),  28, FontStyles.Normal);
        TextMeshProUGUI speedTxt  = CreateTMPText(canvasObj, "SpeedText",     "8 m/s",   new Vector2(0f,0f),   new Vector2(120f,60f),   24, FontStyles.Normal);

        // Game Over Panel
        GameObject goPanel = CreatePanel(canvasObj, "GameOverPanel", new Color(0f,0f,0f,0.88f), Vector2.zero, new Vector2(620f,520f));
        goPanel.SetActive(false);
        CreateTMPText(goPanel, "GameOverTitle",     "GAME OVER", new Vector2(0.5f,1f),   new Vector2(0f,-55f),  64, FontStyles.Bold,   Color.white);
        TextMeshProUGUI goScore = CreateTMPText(goPanel, "GameOverScoreText",     "0",       new Vector2(0.5f,0.5f), new Vector2(0f,90f),   72, FontStyles.Bold,   Color.yellow);
        TextMeshProUGUI goHigh  = CreateTMPText(goPanel, "GameOverHighScoreText", "Best: 0", new Vector2(0.5f,0.5f), new Vector2(0f,25f),   32, FontStyles.Normal, Color.white);
        TextMeshProUGUI goDist  = CreateTMPText(goPanel, "GameOverDistanceText",  "0m",      new Vector2(0.5f,0.5f), new Vector2(0f,-15f),  28, FontStyles.Normal, Color.white);
        TextMeshProUGUI goCoins = CreateTMPText(goPanel, "GameOverCoinsText",     "x0",      new Vector2(0.5f,0.5f), new Vector2(0f,-50f),  28, FontStyles.Normal, Color.yellow);
        GameObject newBestObj = new GameObject("NewBestBanner");
        newBestObj.transform.SetParent(goPanel.transform, false);
        TextMeshProUGUI newBest = newBestObj.AddComponent<TextMeshProUGUI>();
        newBest.text = "** NEW BEST **"; newBest.fontSize = 36; newBest.fontStyle = FontStyles.Bold;
        newBest.color = Color.yellow; newBest.alignment = TextAlignmentOptions.Center;
        var newBestRt = newBestObj.GetComponent<RectTransform>();
        newBestRt.anchorMin = new Vector2(0.5f,0.5f); newBestRt.anchorMax = new Vector2(0.5f,0.5f);
        newBestRt.pivot = new Vector2(0.5f,0.5f);
        newBestRt.anchoredPosition = new Vector2(0f,145f); newBestRt.sizeDelta = new Vector2(320f,65f);
        newBestObj.SetActive(false);
        GameObject restartBtn = CreateButton(goPanel, "RestartButton", "PLAY AGAIN", new Vector2(0.5f,0f), new Vector2(-110f,60f), new Vector2(200f,55f), new Color(0.1f,0.7f,0.2f));
        GameObject menuBtn    = CreateButton(goPanel, "MenuButton",    "MAIN MENU",  new Vector2(0.5f,0f), new Vector2(110f,60f),  new Vector2(200f,55f), new Color(0.2f,0.4f,0.8f));

        // Pause button
        GameObject pauseBtn = CreateButton(canvasObj, "PauseButton", "II", new Vector2(1f,1f), new Vector2(-50f,-50f), new Vector2(60f,60f), new Color(0.15f,0.15f,0.15f,0.8f));

        // Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj, "PausePanel", new Color(0f,0f,0f,0.92f), Vector2.zero, new Vector2(420f,420f));
        pausePanel.SetActive(false);
        CreateTMPText(pausePanel, "PauseTitle", "PAUSED", new Vector2(0.5f,1f), new Vector2(0f,-55f), 52, FontStyles.Bold, Color.white);
        GameObject resumeBtn       = CreateButton(pausePanel, "ResumeButton",       "RESUME",    new Vector2(0.5f,0.5f), new Vector2(0f,70f),  new Vector2(220f,55f), new Color(0.1f,0.7f,0.2f));
        GameObject pauseRestartBtn = CreateButton(pausePanel, "PauseRestartButton", "RESTART",   new Vector2(0.5f,0.5f), new Vector2(0f,0f),   new Vector2(220f,55f), new Color(0.8f,0.5f,0.1f));
        GameObject pauseMenuBtn    = CreateButton(pausePanel, "PauseMenuButton",    "MAIN MENU", new Vector2(0.5f,0.5f), new Vector2(0f,-70f), new Vector2(220f,55f), new Color(0.2f,0.4f,0.8f));

        // Countdown Panel
        GameObject countPanel = CreatePanel(canvasObj, "CountdownPanel", new Color(0f,0f,0f,0f), Vector2.zero, new Vector2(300f,200f));
        countPanel.SetActive(false);
        TextMeshProUGUI countTxt = CreateTMPText(countPanel, "CountdownText", "3", new Vector2(0.5f,0.5f), Vector2.zero, 120, FontStyles.Bold, Color.white);

        // Wire GameHUD
        GameHUD hud = canvasObj.GetComponent<GameHUD>() ?? canvasObj.AddComponent<GameHUD>();
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
        hud.newHighScoreBanner    = newBest;
        hud.restartButton         = restartBtn.GetComponent<Button>();
        hud.menuButton            = menuBtn.GetComponent<Button>();
        hud.pauseButton           = pauseBtn.GetComponent<Button>();
        hud.resumeButton          = resumeBtn.GetComponent<Button>();
        hud.pauseRestartButton    = pauseRestartBtn.GetComponent<Button>();
        hud.pauseMenuButton       = pauseMenuBtn.GetComponent<Button>();
        hud.pausePanel            = pausePanel;
        hud.countdownPanel        = countPanel;
        hud.countdownText         = countTxt;

        // Wire GameOverUI
        GameOverUI goUI = goPanel.GetComponent<GameOverUI>() ?? goPanel.AddComponent<GameOverUI>();
        goUI.finalScoreText  = goScore;
        goUI.highScoreText   = goHigh;
        goUI.distanceText    = goDist;
        goUI.coinsText       = goCoins;
        goUI.newBestBanner   = newBestObj;
        goUI.restartButton   = restartBtn.GetComponent<Button>();
        goUI.mainMenuButton  = menuBtn.GetComponent<Button>();

        // ScorePopupSpawner
        GameObject popupObj = CreateEmpty("ScorePopupSpawner", canvasObj.transform);
        ScorePopupSpawner sps = popupObj.AddComponent<ScorePopupSpawner>();
        sps.hudCanvas  = canvas;
        sps.mainCamera = Camera.main;

        Debug.Log("[SceneBuilder] UI built and wired.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ClearScene()
    {
        string[] keep = { "Main Camera", "Directional Light" };
        foreach (var go in Object.FindObjectsByType<GameObject>())
        {
            if (go.transform.parent != null) continue;
            if (System.Array.Exists(keep, n => go.name == n)) continue;
            Object.DestroyImmediate(go);
        }
    }

    private static GameObject CreateEmpty(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static Material _urpLit;

    /// <summary>
    /// Gets the correct URP Lit shader. Tries several known shader names for Unity 6.
    /// Creates and saves a base material asset so it persists correctly.
    /// </summary>
    private static Material GetURPMaterial(Color color, string assetName)
    {
        EnsureFolder("Assets/Materials");
        string path = $"Assets/Materials/{assetName}.mat";

        // Try to find the right shader — Unity 6 URP uses different names
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader");
        if (shader == null) shader = Shader.Find("URP/Lit");
        if (shader == null) shader = Shader.Find("Standard"); // absolute fallback

        Material mat = new Material(shader);
        mat.color = color;

        // Save as asset so Unity doesn't lose the reference
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    private static void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        string safeName = go.name.Replace(" ", "_").Replace("/", "_");
        r.sharedMaterial = GetURPMaterial(color, safeName);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Transform[] GetChildTransforms(GameObject parent, string nameContains)
    {
        var list = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in parent.transform)
            if (child.name.Contains(nameContains)) list.Add(child);
        return list.ToArray();
    }

    private static TextMeshProUGUI CreateTMPText(GameObject parent, string name, string text,
        Vector2 anchor, Vector2 pos, float size, FontStyles style, Color? color = null)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = color ?? Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320f, 65f);
        return tmp;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f,0.5f); rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<CanvasGroup>();
        return go;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 pos, Vector2 size, Color bg)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var labelObj = new GameObject("Text");
        labelObj.transform.SetParent(go.transform, false);
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 22; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        var lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        return go;
    }
}
