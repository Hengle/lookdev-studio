﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace LookDev.Editor
{
    public class LookDevWelcomeWindow : EditorWindow
    {
#if !DISABLE_LOOKDEV_WELCOME_WINDOW
        static readonly string _installHdrpButtonName = "InstallHdrpButton";
        static readonly string _installUrpButtonName = "InstallUrpButton";

        static readonly string _installHdrpAssetsButtonName = "InstallHdrpAssetsButton";
        static readonly string _installUrpAssetsButtonName = "InstallUrpAssetsButton";

        static readonly string _hdrpPackageAddress = "com.unity.render-pipelines.high-definition";
        static readonly string _urpPackageAddress = "com.unity.render-pipelines.universal";

        static bool _isInstallingAssets;

        const string urlTemplate =
            "https://github.com/Unity-Technologies/lookdev-studio/releases/download/v{0}/Extension-{1}.unitypackage";

        const string LDS_PACKAGE_NAME = "com.unity.lookdevstudio";

        const string PackageTempDirectory = "Temp";

        Button SetupButton => rootVisualElement.Query<Button>("SetupButton").First();
        Button OpenButton => rootVisualElement.Query<Button>("OpenLookDevButton").First();

        public static event Action PipelineExtensionSetup;

        [MenuItem("LookDev Studio/Welcome")]
        public static void ShowWindow()
        {
            GetWindow<LookDevWelcomeWindow>(typeof(SceneView));
        }

        [MenuItem("LookDev Studio DEBUG/Initialized")]
        public static void ToggleInitialized()
        {
            var isInitialized = LookDevPreferences.instance.IsRenderPipelineInitialized;

            if (!isInitialized)
            {
                LookDevPreferences.instance.IsRenderPipelineInitialized = true;
                LookDevPreferences.instance.AreAssetsInstalled = true;
                var window = GetWindow<LookDevWelcomeWindow>();
                window.OpenButton.SetEnabled(true);
                window.SetupButton.SetEnabled(true);
            }
            else
            {
                LookDevPreferences.instance.IsRenderPipelineInitialized = false;
                LookDevPreferences.instance.AreAssetsInstalled = false;
                var window = GetWindow<LookDevWelcomeWindow>();
                window.OpenButton.SetEnabled(false);
                window.SetupButton.SetEnabled(false);
            }
        }

        [MenuItem("LookDev Studio DEBUG/Initialized", true)]
        public static bool ValidateInitialized()
        {
            var isInitialized = LookDevPreferences.instance.IsRenderPipelineInitialized;
            Menu.SetChecked("LookDev Studio DEBUG/Initialized", isInitialized);
            return true;
        }

        [MenuItem("LookDev Studio DEBUG/AutoLoad")]
        public static void ToggleAutoLoad()
        {
            var autoloadEnabled = !File.Exists(LookDevStudioEditor.PathToLookDevDisableAutoLaunchFile);
            if (autoloadEnabled)
                File.Create(LookDevStudioEditor.PathToLookDevDisableAutoLaunchFile);
            else
                File.Delete(LookDevStudioEditor.PathToLookDevDisableAutoLaunchFile);
        }

        [MenuItem("LookDev Studio DEBUG/AutoLoad", true)]
        public static bool ValidateAutoLoad()
        {
            var autoloadEnabled = !File.Exists(LookDevStudioEditor.PathToLookDevDisableAutoLaunchFile);
            Menu.SetChecked("LookDev Studio DEBUG/AutoLoad", autoloadEnabled);
            return true;
        }

        [MenuItem("LookDev Studio DEBUG/Developer Mode")]
        public static void ToggleDeveloperMode()
        {
            LookDevPreferences.instance.EnableDeveloperMode = !LookDevPreferences.instance.EnableDeveloperMode;
        }

        [MenuItem("LookDev Studio DEBUG/Developer Mode", true)]
        public static bool ValidateDeveloperMode()
        {
            var developerModeEnabled = LookDevPreferences.instance.EnableDeveloperMode;
            Menu.SetChecked("LookDev Studio DEBUG/Developer Mode", developerModeEnabled);
            return true;
        }

        [MenuItem("LookDev Studio DEBUG/Create Asset Package")]
        public static void CreateAssetPackage()
        {
            AssetDatabase.ExportPackage("Assets/LookDev", "Extensions/ExportedAssetPackage.unitypackage",
                ExportPackageOptions.Recurse);
        }

        void CreateGUI()
        {
            var uxmlTemplate =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"Packages/com.unity.lookdevstudio/UI/LookDevWelcome.uxml");

            var ui = uxmlTemplate.CloneTree();
            rootVisualElement.Add(ui);

            var installHdrpButton = rootVisualElement.Query<Button>(_installHdrpButtonName).First();
            installHdrpButton.clicked += () => { InstallPackage(_hdrpPackageAddress); };

            var installUrpButton = rootVisualElement.Query<Button>(_installUrpButtonName).First();
            installUrpButton.clicked += () => { InstallPackage(_urpPackageAddress); };

            var installHdrpAssetsButton = rootVisualElement.Query<Button>(_installHdrpAssetsButtonName).First();
            installHdrpAssetsButton.clicked += () => { InstallAssets(this, "HDRP"); };
            installHdrpAssetsButton.SetEnabled(IsPackageInstalled(_hdrpPackageAddress));

            var installUrpAssetsButton = rootVisualElement.Query<Button>(_installUrpAssetsButtonName).First();
            installUrpAssetsButton.clicked += () => { InstallAssets(this, "URP"); };
            installUrpAssetsButton.SetEnabled(IsPackageInstalled(_urpPackageAddress));


            var setupButton = SetupButton;
            setupButton.clicked += () => { Setup(this); };
            setupButton.SetEnabled(LookDevPreferences.instance.AreAssetsInstalled);

            var openLookDevButton = OpenButton;
            openLookDevButton.clicked += OpenLookDev;
            openLookDevButton.SetEnabled(LookDevPreferences.instance.IsRenderPipelineInitialized);
        }

        static PackageCollection lastQueriedPackageList = null;

        static void InstallPackage(string address)
        {
            if (!IsPackageInstalled(address))
            {
                var addRequest = Client.Add(address);
                while (!addRequest.IsCompleted)
                {
                }
            }
        }

        static bool IsPackageInstalled(string address)
        {
            ListRequest listRequest = Client.List();
            while (!listRequest.IsCompleted)
            {
            }

            lastQueriedPackageList = listRequest.Result;
            var requestedPackage = listRequest.Result.FirstOrDefault(x => x.name == address);

            return listRequest.Status == StatusCode.Success && requestedPackage != null;
        }

        static async void InstallAssets(LookDevWelcomeWindow window, string expectedSourceFolderName)
        {
            if (_isInstallingAssets)
                return;

            _isInstallingAssets = true;

            if (LookDevPreferences.instance.EnableDeveloperMode)
            {
                EditorPrefs.SetBool("DirectoryMonitoring", false);
                string sourceFolderPath = EditorUtility.OpenFolderPanel("Select Folder Source", "", "");

                if (sourceFolderPath == string.Empty)
                    return;

                string selectedFolderName = new DirectoryInfo(sourceFolderPath).Name;
                if (selectedFolderName != expectedSourceFolderName)
                {
                    Debug.LogError(
                        $"Error: Wrong folder selected. Expecting a folder called {expectedSourceFolderName}");
                    return;
                }

                SymlinkUtility.Symlink(SymlinkUtility.SymlinkType.Junction, sourceFolderPath, "Assets", "LookDev");
                LookDevPreferences.instance.AreAssetsInstalled = true;
                window.SetupButton.SetEnabled(LookDevPreferences.instance.AreAssetsInstalled);
                return;
            }

            var lookDevPkgNfo = lastQueriedPackageList.First(x => x.name == LDS_PACKAGE_NAME);

            string downloadUrl = string.Format(urlTemplate, lookDevPkgNfo.version, expectedSourceFolderName);

            using (var uwr = new UnityWebRequest(downloadUrl, UnityWebRequest.kHttpVerbGET))
            {
                EditorApplication.LockReloadAssemblies();

                uwr.downloadHandler = new DownloadHandlerBuffer();
                Debug.Log($"About to download: {uwr.url}");
                var asyncRequest = uwr.SendWebRequest();
                while (!asyncRequest.isDone)
                {
                    Debug.Log($"Progress: {asyncRequest.progress * 100}%");
                    await Task.Delay(1000);
                }

                if (uwr.downloadHandler.isDone && string.IsNullOrEmpty(uwr.downloadHandler.error))
                {
                    Debug.Log("Download Complete");

                    if (!Directory.Exists(PackageTempDirectory))
                        Directory.CreateDirectory(PackageTempDirectory);

                    var packagePath = $"{PackageTempDirectory}/CachedLookDevStudioExtension.unitypackage";
                    try
                    {
                        File.WriteAllBytes(packagePath, uwr.downloadHandler.data);
                        Debug.Log("Importing Package");
                        AssetDatabase.ImportPackage(packagePath, false);
                        Debug.Log("Importing complete");
                        LookDevPreferences.instance.AreAssetsInstalled = true;
                        window.SetupButton.SetEnabled(LookDevPreferences.instance.AreAssetsInstalled);
                        EditorApplication.UnlockReloadAssemblies();
                    }
                    catch
                    {
                        Debug.LogError("Failed to import package");
                    }
                    finally
                    {
                        if (File.Exists(packagePath))
                            File.Delete(packagePath);
                    }
                }
                else
                {
                    Debug.LogError($"Download stopped with error: {uwr.downloadHandler.error}");
                }
            }

            _isInstallingAssets = false;
        }

        static void Setup(LookDevWelcomeWindow window)
        {
            PipelineExtensionSetup?.Invoke();

            LookDevPreferences.instance.IsRenderPipelineInitialized = true;
            window.OpenButton.SetEnabled(LookDevPreferences.instance.IsRenderPipelineInitialized);
        }

        static void OpenLookDev()
        {
            LookDevStudioEditor.EnableLookDev();
        }
#endif
    }
}