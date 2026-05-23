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

        // Clear existing scene objects (except camera and light)
        ClearScene();

        // ── Managers ──────────────────────────────────────────────────────────
        GameObject gameManager        = CreateEmpty("GameManager");
        GameObject scoreManager       = CreateEmpty("ScoreManager");
        GameObject audioManager       = CreateEmpty("AudioManager");
        GameObject powerUpManager     = CreateEmpty("PowerUpManager");
        GameObject obstacleSpawner    = CreateEmpty("ObstacleSpawner");
        GameObject collectibleSpawner = CreateEmpty("CollectibleSpawner");
        GameObject trackManager       = CreateEmpty("TrackManager");

        gameManager.AddComponent<GameManager>();
        scoreManager.AddComponent<ScoreManager>();
        powerUpManager.AddComponent<PowerUpManager>();
        obstacleSpawner.AddComponent<ObstacleSpawner>();
        collectibleSpawner.AddComponent<CollectibleSpawner>();
        trackManager.AddComponent<TrackManager>();

        // AudioManager needs two AudioSources
        AudioManager am = audioManager.AddComponent<AudioManager>();
        AudioSource musicSrc = audioManager.AddComponent<AudioSource>();
        AudioSource sfxSrc   = audioManager.AddComponent<AudioSource>();
        musicSrc.loop        = true;
        musicSrc.playOnAwake = false;
        sfxSrc.playOnAwake   = false;
        am.musicSource       = musicSrc;
        am.sfxSource         = sfxSrc;

        // ── Ground ────────────────────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.15f, 50f);
        ground.transform.localScale = new Vector3(5f, 1f, 100f);
        SetColor(ground, new Color(0.25f, 0.25f, 0.25f));

        // ── Track Segment Prefab ──────────────────────────────────────────────
        EnsureFolder("Assets/Prefabs");
        GameObject trackSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trackSeg.name = "TrackSegment_Straight";
        trackSeg.transform.localScale = new Vector3(7.5f, 0.2f, 30f);
        SetColor(trackSeg, new Color(0.3f, 0.3f, 0.35f));
        trackSeg.AddComponent<TrackSegment>();

        // Add obstacle spawn points
        for (int i = 0; i < 3; i++)
        {
            GameObject sp = CreateEmpty($"ObstacleSpawn_{i + 1}", trackSeg.transform);
            sp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.6f, (i % 3) * 5f - 5f);
        }

        // Add collectible spawn points
        for (int i = 0; i < 3; i++)
        {
            GameObject cp = CreateEmpty($"CoinSpawn_{i + 1}", trackSeg.transform);
            cp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.8f, i * 3f);
        }

        // Wire spawn points to TrackSegment
        TrackSegment ts = trackSeg.GetComponent<TrackSegment>();
        ts.obstacleSpawnPoints    = GetChildTransforms(trackSeg, "ObstacleSpawn");
        ts.collectibleSpawnPoints = GetChildTransforms(trackSeg, "CoinSpawn");

        // Save as prefab
        string prefabPath = "Assets/Prefabs/TrackSegment_Straight.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(trackSeg, prefabPath);
        Object.DestroyImmediate(trackSeg);
        Debug.Log("[SceneBuilder] Track segment prefab saved: " + prefabPath);

        // Assign to TrackManager
        TrackManager tm = trackManager.GetComponent<TrackManager>();
        tm.trackSegmentPrefabs = new GameObject[] { prefab };

        // ── Obstacle Prefab ───────────────────────────────────────────────────
        GameObject barrierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrierObj.name = "Obstacle_Barrier";
        barrierObj.transform.localScale = new Vector3(2.4f, 1.5f, 0.4f);
        SetColor(barrierObj, new Color(0.8f, 0.1f, 0.1f));
        barrierObj.AddComponent<ObstacleBarrier>();
        BoxCollider bc = barrierObj.GetComponent<BoxCollider>();
        bc.isTrigger = true;
        string barrierPath = "Assets/Prefabs/Obstacle_Barrier.prefab";
        GameObject barrierPrefab = PrefabUtility.SaveAsPrefabAsset(barrierObj, barrierPath);
        Object.DestroyImmediate(barrierObj);

        // Assign to ObstacleSpawner
        ObstacleSpawner os = obstacleSpawner.GetComponent<ObstacleSpawner>();
        os.obstaclePrefabs = new GameObject[] { barrierPrefab };

        // ── Coin Prefab ───────────────────────────────────────────────────────
        GameObject coinObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coinObj.name = "Coin";
        coinObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        SetColor(coinObj, new Color(1f, 0.85f, 0f));
        coinObj.AddComponent<Collectible>();
        SphereCollider sc = coinObj.GetComponent<SphereCollider>();
        sc.isTrigger = true;
        string coinPath = "Assets/Prefabs/Coin.prefab";
        GameObject coinPrefab = PrefabUtility.SaveAsPrefabAsset(coinObj, coinPath);
        Object.DestroyImmediate(coinObj);

        // Assign to CollectibleSpawner
        CollectibleSpawner cs = collectibleSpawner.GetComponent<CollectibleSpawner>();
        cs.coinPrefab = coinPrefab;

        // ── Player ────────────────────────────────────────────────────────────
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.tag = "Player";
        SetColor(player, new Color(1f, 0.47f, 0f));

        // Remove default capsule collider — CharacterController handles collision
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerCollision>();
        player.AddComponent<PlayerAnimator>();

        // Animator Controller
        EnsureFolder("Assets/Animations");
        string animCtrlPath = "Assets/Animations/PlayerAnimatorController.controller";
        UnityEditor.Animations.AnimatorController animCtrl;
        if (!File.Exists(Application.dataPath + "/Animations/PlayerAnimatorController.controller"))
        {
            animCtrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animCtrlPath);
            animCtrl.AddParameter("IsRunning",  AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("IsSliding",  AnimatorControllerParameterType.Bool);
            animCtrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);
            animCtrl.AddParameter("Lean",       AnimatorControllerParameterType.Float);
            animCtrl.AddParameter("Death",      AnimatorControllerParameterType.Trigger);
        }
        else
        {
            animCtrl = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(animCtrlPath);
        }

        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        anim.runtimeAnimatorController = animCtrl;

        // ── Camera ────────────────────────────────────────────────────────────
        GameObject camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null)
        {
            camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        camObj.transform.position = new Vector3(0f, 4f, -6f);
        camObj.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

        CameraFollow cf = camObj.GetComponent<CameraFollow>();
        if (cf == null) cf = camObj.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.offset = new Vector3(0f, 3f, -6f);

        // ── Directional Light ─────────────────────────────────────────────────
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
        }
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── UI Canvas ─────────────────────────────────────────────────────────
        BuildUI(player);

        // ── Layers ────────────────────────────────────────────────────────────
        // Note: layers must be set manually in Project Settings → Tags and Layers
        // Obstacle = Layer 6, Collectible = Layer 7

        // ── Save Scene ────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SceneBuilder] ✅ Scene built successfully! Press Play to test.");
        EditorUtility.DisplayDialog("The Last Run", 
            "Scene built successfully!\n\nPress Play to test the game.", "OK");
    }

    // ── UI Builder ────────────────────────────────────────────────────────────

    private static void BuildUI(GameObject player)
    {
        // Remove old canvas if exists
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── HUD elements ──────────────────────────────────────────────────────

        // Score (top center)
        TextMeshProUGUI scoreTxt = CreateTMPText(canvasObj, "ScoreText", "0",
            new Vector2(0.5f, 1f), new Vector2(0f, -60f), 56, FontStyles.Bold);

        // Distance (top left)
        TextMeshProUGUI distTxt = CreateTMPText(canvasObj, "DistanceText", "0m",
            new Vector2(0f, 1f), new Vector2(120f, -55f), 32, FontStyles.Normal);

        // High Score (top right)
        TextMeshProUGUI highTxt = CreateTMPText(canvasObj, "HighScoreText", "Best: 0",
            new Vector2(1f, 1f), new Vector2(-120f, -55f), 28, FontStyles.Normal);

        // Coin count (top left below distance)
        TextMeshProUGUI coinTxt = CreateTMPText(canvasObj, "CoinCountText", "x0",
            new Vector2(0f, 1f), new Vector2(120f, -95f), 28, FontStyles.Normal);

        // Speed (bottom left)
        TextMeshProUGUI speedTxt = CreateTMPText(canvasObj, "SpeedText", "8 m/s",
            new Vector2(0f, 0f), new Vector2(120f, 60f), 24, FontStyles.Normal);

        // ── Game Over Panel ───────────────────────────────────────────────────
        GameObject goPanel = CreatePanel(canvasObj, "GameOverPanel",
            new Color(0f, 0f, 0f, 0.85f), Vector2.zero, new Vector2(600f, 500f));
        goPanel.SetActive(false);

        // Title
        CreateTMPText(goPanel, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 1f), new Vector2(0f, -60f), 64, FontStyles.Bold, Color.white);

        // Score
        TextMeshProUGUI goScore = CreateTMPText(goPanel, "GameOverScoreText", "0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), 72, FontStyles.Bold, Color.yellow);

        // High score
        TextMeshProUGUI goHigh = CreateTMPText(goPanel, "GameOverHighScoreText", "Best: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), 32, FontStyles.Normal, Color.white);

        // Distance
        TextMeshProUGUI goDist = CreateTMPText(goPanel, "GameOverDistanceText", "0m",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), 28, FontStyles.Normal, Color.white);

        // Coins
        TextMeshProUGUI goCoins = CreateTMPText(goPanel, "GameOverCoinsText", "x0",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), 28, FontStyles.Normal, Color.yellow);

        // New Best banner
        TextMeshProUGUI newBest = CreateTMPText(goPanel, "NewBestBanner", "★ NEW BEST ★",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), 36, FontStyles.Bold, Color.yellow);
        newBest.gameObject.SetActive(false);

        // Restart button
        GameObject restartBtn = CreateButton(goPanel, "RestartButton", "PLAY AGAIN",
            new Vector2(0.5f, 0f), new Vector2(-110f, 60f), new Vector2(200f, 55f),
            new Color(0.1f, 0.7f, 0.2f));

        // Menu button
        GameObject menuBtn = CreateButton(goPanel, "MenuButton", "MAIN MENU",
            new Vector2(0.5f, 0f), new Vector2(110f, 60f), new Vector2(200f, 55f),
            new Color(0.2f, 0.4f, 0.8f));

        // ── Pause Button (top right corner) ───────────────────────────────────
        GameObject pauseBtn = CreateButton(canvasObj, "PauseButton", "❚❚",
            new Vector2(1f, 1f), new Vector2(-50f, -50f), new Vector2(60f, 60f),
            new Color(0.2f, 0.2f, 0.2f, 0.7f));

        // ── Pause Panel ───────────────────────────────────────────────────────
        GameObject pausePanel = CreatePanel(canvasObj, "PausePanel",
            new Color(0f, 0f, 0f, 0.9f), Vector2.zero, new Vector2(400f, 400f));
        pausePanel.SetActive(false);

        CreateTMPText(pausePanel, "PauseTitle", "PAUSED",
            new Vector2(0.5f, 1f), new Vector2(0f, -60f), 52, FontStyles.Bold, Color.white);

        GameObject resumeBtn = CreateButton(pausePanel, "ResumeButton", "RESUME",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(220f, 55f),
            new Color(0.1f, 0.7f, 0.2f));

        GameObject pauseRestartBtn = CreateButton(pausePanel, "PauseRestartButton", "RESTART",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(220f, 55f),
            new Color(0.8f, 0.5f, 0.1f));

        GameObject pauseMenuBtn = CreateButton(pausePanel, "PauseMenuButton", "MAIN MENU",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(220f, 55f),
            new Color(0.2f, 0.4f, 0.8f));

        // ── Countdown Panel ───────────────────────────────────────────────────
        GameObject countPanel = CreatePanel(canvasObj, "CountdownPanel",
            new Color(0f, 0f, 0f, 0f), Vector2.zero, new Vector2(300f, 200f));
        countPanel.SetActive(false);

        TextMeshProUGUI countTxt = CreateTMPText(countPanel, "CountdownText", "3",
            new Vector2(0.5f, 0.5f), Vector2.zero, 120, FontStyles.Bold, Color.white);

        // ── Wire GameHUD ──────────────────────────────────────────────────────
        GameHUD hud = canvasObj.GetComponent<GameHUD>();
        if (hud == null) hud = canvasObj.AddComponent<GameHUD>();

        hud.scoreText         = scoreTxt;
        hud.highScoreText     = highTxt;
        hud.distanceText      = distTxt;
        hud.coinCountText     = coinTxt;
        hud.speedText         = speedTxt;
        hud.gameOverPanel     = goPanel;
        hud.gameOverScoreText = goScore;
        hud.gameOverHighScoreText = goHigh;
        hud.gameOverDistanceText  = goDist;
        hud.gameOverCoinsText     = goCoins;
        hud.newHighScoreBanner    = newBest?.gameObject;
        hud.restartButton     = restartBtn.GetComponent<Button>();
        hud.menuButton        = menuBtn.GetComponent<Button>();
        hud.pauseButton       = pauseBtn.GetComponent<Button>();
        hud.resumeButton      = resumeBtn.GetComponent<Button>();
        hud.pauseRestartButton = pauseRestartBtn.GetComponent<Button>();
        hud.pauseMenuButton   = pauseMenuBtn.GetComponent<Button>();
        hud.pausePanel        = pausePanel;
        hud.countdownPanel    = countPanel;
        hud.countdownText     = countTxt;

        // ── Wire GameOverUI ───────────────────────────────────────────────────
        GameOverUI goUI = goPanel.GetComponent<GameOverUI>();
        if (goUI == null) goUI = goPanel.AddComponent<GameOverUI>();
        goUI.finalScoreText  = goScore;
        goUI.highScoreText   = goHigh;
        goUI.distanceText    = goDist;
        goUI.coinsText       = goCoins;
        goUI.newBestBanner   = newBest?.gameObject;
        goUI.restartButton   = restartBtn.GetComponent<Button>();
        goUI.mainMenuButton  = menuBtn.GetComponent<Button>();

        // ── ScorePopupSpawner ─────────────────────────────────────────────────
        GameObject popupSpawner = CreateEmpty("ScorePopupSpawner", canvasObj.transform);
        ScorePopupSpawner sps = popupSpawner.AddComponent<ScorePopupSpawner>();
        sps.hudCanvas  = canvas;
        sps.mainCamera = Camera.main;

        Debug.Log("[SceneBuilder] UI built and wired.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ClearScene()
    {
        string[] keepNames = { "Main Camera", "Directional Light" };
        GameObject[] all = Object.FindObjectsOfType<GameObject>();
        foreach (var go in all)
        {
            if (go.transform.parent != null) continue;
            bool keep = System.Array.Exists(keepNames, n => go.name == n);
            if (!keep) Object.DestroyImmediate(go);
        }
    }

    private static GameObject CreateEmpty(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        r.material = mat;
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
            if (child.name.Contains(nameContains))
                list.Add(child);
        return list.ToArray();
    }

    private static TextMeshProUGUI CreateTMPText(GameObject parent, string name, string text,
        Vector2 anchor, Vector2 anchoredPos, float fontSize, FontStyles style,
        Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = fontSize;
        tmp.fontStyle  = style;
        tmp.color      = color ?? Color.white;
        tmp.alignment  = TextAlignmentOptions.Center;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(300f, 60f);

        return tmp;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color,
        Vector2 anchor, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        Image img  = go.AddComponent<Image>();
        img.color  = color;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchor;
        rt.sizeDelta = size;

        go.AddComponent<CanvasGroup>();
        return go;
    }

    private static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = bgColor * 1.3f;
        cb.pressedColor     = bgColor * 0.7f;
        btn.colors = cb;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // Button label
        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        return go;
    }
}
