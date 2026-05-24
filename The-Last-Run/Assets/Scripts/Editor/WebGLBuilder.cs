using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// One-click WebGL build tool.
/// Menu: The Last Run -> Build WebGL Game
/// Automatically configures settings and builds to D:\TheLastRun_WebBuild
/// </summary>
public class WebGLBuilder : EditorWindow
{
    private static readonly string BUILD_PATH = @"D:\TheLastRun_WebBuild";

    [MenuItem("The Last Run/Build WebGL Game")]
    public static void BuildWebGL()
    {
        // ── Step 1: Configure Player Settings ────────────────────────────────
        PlayerSettings.companyName  = "Kelvin Magumise";
        PlayerSettings.productName  = "The Last Run";
        PlayerSettings.bundleVersion = "1.0.0";

        // WebGL specific settings
        PlayerSettings.WebGL.compressionFormat     = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        // Resolution — standard widescreen, fills browser
        PlayerSettings.defaultScreenWidth  = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.runInBackground     = true;
        PlayerSettings.fullScreenMode      = FullScreenMode.FullScreenWindow;

        // ── Step 2: Set scenes ────────────────────────────────────────────────
        // Find the Game scene
        string gameScenePath = FindScenePath("Game");
        if (gameScenePath == null)
        {
            EditorUtility.DisplayDialog("Build Failed",
                "Could not find 'Game' scene in Assets/Scenes/.\n\nMake sure you have saved the Game scene first.",
                "OK");
            return;
        }

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(gameScenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log($"[WebGLBuilder] Scene set: {gameScenePath}");

        // ── Step 3: Create output folder ──────────────────────────────────────
        if (!Directory.Exists(BUILD_PATH))
            Directory.CreateDirectory(BUILD_PATH);

        // ── Step 4: Build ─────────────────────────────────────────────────────
        Debug.Log($"[WebGLBuilder] Starting WebGL build to: {BUILD_PATH}");
        EditorUtility.DisplayDialog("Building...",
            "WebGL build starting.\n\nThis will take 5-15 minutes.\nUnity may appear frozen — that is normal.\n\nClick OK to begin.",
            "OK, Start Build");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes           = new string[] { gameScenePath },
            locationPathName = BUILD_PATH,
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        };

        BuildReport  report  = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        // ── Step 5: Result ────────────────────────────────────────────────────
        if (summary.result == BuildResult.Succeeded)
        {
            double sizeMB = summary.totalSize / 1024.0 / 1024.0;
            Debug.Log($"[WebGLBuilder] Build succeeded! Size: {sizeMB:F1} MB");

            EditorUtility.DisplayDialog("Build Succeeded!",
                $"The Last Run has been built successfully!\n\n" +
                $"Location: {BUILD_PATH}\n" +
                $"Size: {sizeMB:F1} MB\n\n" +
                $"To play it:\n" +
                $"1. Open VS Code\n" +
                $"2. Open folder: {BUILD_PATH}\n" +
                $"3. Right-click index.html\n" +
                $"4. Click 'Open with Live Server'\n\n" +
                $"OR run in Command Prompt:\n" +
                $"cd {BUILD_PATH}\n" +
                $"python -m http.server 8080\n" +
                $"Then open: http://localhost:8080",
                "Open Build Folder");

            // Open the build folder in Explorer
            System.Diagnostics.Process.Start("explorer.exe", BUILD_PATH);
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] Build FAILED: {summary.result}");
            EditorUtility.DisplayDialog("Build Failed",
                $"Build failed with result: {summary.result}\n\n" +
                $"Check the Console for error details.",
                "OK");
        }
    }

    // ── Also add a menu item to start a local server ──────────────────────────

    [MenuItem("The Last Run/Start Local Server (Play in Browser)")]
    public static void StartLocalServer()
    {
        if (!Directory.Exists(BUILD_PATH))
        {
            EditorUtility.DisplayDialog("No Build Found",
                $"No WebGL build found at:\n{BUILD_PATH}\n\nPlease run 'Build WebGL Game' first.",
                "OK");
            return;
        }

        // Start Python HTTP server
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "cmd.exe",
            Arguments       = $"/k cd /d \"{BUILD_PATH}\" && python -m http.server 8080",
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);

        // Open browser after a short delay
        EditorApplication.delayCall += () =>
        {
            Application.OpenURL("http://localhost:8080");
        };

        EditorUtility.DisplayDialog("Server Starting",
            "A local server is starting at:\nhttp://localhost:8080\n\n" +
            "Your browser will open automatically.\n" +
            "Keep the Command Prompt window open while playing.",
            "OK");
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static string FindScenePath(string sceneName)
    {
        string[] guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return path;
        }
        return null;
    }
}
