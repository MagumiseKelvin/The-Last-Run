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
        SetColor(ground, new Color(0.22f, 0.22f, 0.25f));

        // ── Track Segment Prefab ──────────────────────────────────────────────
        EnsureFolder("Assets/Prefabs");

        GameObject trackSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trackSeg.name = "TrackSegment_Straight";
        trackSeg.transform.localScale = new Vector3(7.5f, 0.2f, 30f);
        SetColor(trackSeg, new Color(0.28f, 0.28f, 0.32f));
        trackSeg.AddComponent<TrackSegment>();

        // Obstacle spawn points
        for (int i = 0; i < 3; i++)
        {
            GameObject sp = CreateEmpty($"ObstacleSpawn_{i + 1}", trackSeg.transform);
            sp.transform.localPosition = new Vector3((i - 1) * 2.5f, 0.6f, (i % 3) * 5f - 5f);
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

        // ── Obstacle Prefab ───────────────────────────────────────────────────
        GameObject barrierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrierObj.name = "Obstacle_Barrier";
        barrierObj.transform.localScale = new Vector3(2.4f, 1.5f, 0.4f);
        SetColor(barrierObj, new Color(0.85f, 0.1f, 0.1f));
        barrierObj.AddComponent<ObstacleBarrier>();
        barrierObj.GetComponent<BoxCollider>().isTrigger = true;
        GameObject barrierPrefab = PrefabUtility.SaveAsPrefabAsset(barrierObj, "Assets/Prefabs/Obstacle_Barrier.prefab");
        Object.DestroyImmediate(barrierObj);
        obstacleSpawnerObj.GetComponent<ObstacleSpawner>().obstaclePrefabs = new GameObject[] { barrierPrefab };

        // ── Coin Prefab ───────────────────────────────────────────────────────
        GameObject coinObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coinObj.name = "Coin";
        coinObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        SetColor(coinObj, new Color(1f, 0.85f, 0f));
        coinObj.AddComponent<Collectible>();
        coinObj.GetComponent<SphereCollider>().isTrigger = true;
        GameObject coinPrefab = PrefabUtility.SaveAsPrefabAsset(coinObj, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(coinObj);
        collectibleSpawnerObj.GetComponent<CollectibleSpawner>().coinPrefab = coinPrefab;

        // ── Player ────────────────────────────────────────────────────────────
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.tag = "Player";
        SetColor(player, new Color(1f, 0.47f, 0f));
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
        newBest.text = "★ NEW BEST ★"; newBest.fontSize = 36; newBest.fontStyle = FontStyles.Bold;
        newBest.color = Color.yellow; newBest.alignment = TextAlignmentOptions.Center;
        var newBestRt = newBestObj.GetComponent<RectTransform>();
        newBestRt.anchorMin = new Vector2(0.5f,0.5f); newBestRt.anchorMax = new Vector2(0.5f,0.5f);
        newBestRt.pivot = new Vector2(0.5f,0.5f);
        newBestRt.anchoredPosition = new Vector2(0f,145f); newBestRt.sizeDelta = new Vector2(320f,65f);
        newBestObj.SetActive(false);
        GameObject restartBtn = CreateButton(goPanel, "RestartButton", "PLAY AGAIN", new Vector2(0.5f,0f), new Vector2(-110f,60f), new Vector2(200f,55f), new Color(0.1f,0.7f,0.2f));
        GameObject menuBtn    = CreateButton(goPanel, "MenuButton",    "MAIN MENU",  new Vector2(0.5f,0f), new Vector2(110f,60f),  new Vector2(200f,55f), new Color(0.2f,0.4f,0.8f));

        // Pause button
        GameObject pauseBtn = CreateButton(canvasObj, "PauseButton", "❚❚", new Vector2(1f,1f), new Vector2(-50f,-50f), new Vector2(60f,60f), new Color(0.2f,0.2f,0.2f,0.7f));

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
        hud.newHighScoreBanner    = newBestObj;
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

    private static void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
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
