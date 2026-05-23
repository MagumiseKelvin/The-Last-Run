using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Editor tool that builds the Main Menu scene.
/// Menu: The Last Run -> Build Main Menu Scene
/// </summary>
public class MainMenuBuilder : EditorWindow
{
    [MenuItem("The Last Run/Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        // Clear scene
        foreach (var go in Object.FindObjectsByType<GameObject>())
        {
            if (go.transform.parent == null)
                Object.DestroyImmediate(go);
        }

        // Camera
        var cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>().backgroundColor = new Color(0.04f, 0.04f, 0.08f);
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0f, 0f, -10f);

        // Light
        var light = new GameObject("Directional Light");
        var l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Bootstrap
        var boot = new GameObject("Bootstrap");
        boot.AddComponent<GameBootstrap>();

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Background panel
        var bg = MakePanel(canvasGO, "Background",
            new Color(0.04f, 0.04f, 0.08f, 1f),
            Vector2.zero, new Vector2(1920f, 1080f));
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Title
        MakeTMP(canvasGO, "TitleText", "THE LAST RUN",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 120f),
            88, FontStyles.Bold, Color.white);

        // Subtitle
        MakeTMP(canvasGO, "SubtitleText", "How far can you run?",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(600f, 60f),
            32, FontStyles.Normal, new Color(0.6f, 0.6f, 0.7f));

        // High score display
        float hs = PlayerPrefs.GetFloat("TheLastRun_HighScore", 0f);
        MakeTMP(canvasGO, "HighScoreText", $"Best: {hs:F0}",
            new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(400f, 50f),
            28, FontStyles.Normal, new Color(1f, 0.85f, 0.2f));

        // Play button
        var playBtn = MakeButton(canvasGO, "PlayButton", "PLAY",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(280f, 75f),
            new Color(0.08f, 0.65f, 0.18f), 36);

        // Settings button
        var settingsBtn = MakeButton(canvasGO, "SettingsButton", "SETTINGS",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -125f), new Vector2(280f, 60f),
            new Color(0.15f, 0.15f, 0.2f), 26);

        // Quit button
        var quitBtn = MakeButton(canvasGO, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(280f, 60f),
            new Color(0.5f, 0.1f, 0.1f), 26);

        // Version text
        MakeTMP(canvasGO, "VersionText", $"v{Application.version}",
            new Vector2(0f, 0f), new Vector2(80f, 30f), new Vector2(150f, 35f),
            18, FontStyles.Normal, new Color(0.4f, 0.4f, 0.4f));

        // Wire MainMenuUI
        var menuUI = canvasGO.AddComponent<MainMenuUI>();
        menuUI.playButton    = playBtn.GetComponent<Button>();
        menuUI.settingsButton = settingsBtn.GetComponent<Button>();
        menuUI.quitButton    = quitBtn.GetComponent<Button>();
        menuUI.gameSceneName = "Game";

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[MainMenuBuilder] Main Menu built!");
        EditorUtility.DisplayDialog("The Last Run", "Main Menu built!\n\nSave this as 'MainMenu' scene.", "OK");
    }

    static GameObject MakePanel(GameObject parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = color;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    static TextMeshProUGUI MakeTMP(GameObject parent, string name, string text,
        Vector2 anchor, Vector2 pos, Vector2 size, float fontSize, FontStyles style, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
        tmp.color = color; tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return tmp;
    }

    static GameObject MakeButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 pos, Vector2 size, Color bg, float fontSize = 24)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var lbl = new GameObject("Text");
        lbl.transform.SetParent(go.transform, false);
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        return go;
    }
}
