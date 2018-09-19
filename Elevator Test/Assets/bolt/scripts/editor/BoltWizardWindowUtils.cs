using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class BoltWizardWindow
{
    [Flags]
    enum PackageFlags
    {
        WarnForProjectOverwrite = 1 << 1,
        RunInitialSetup = 1 << 2
    }

    class BoltWizardText
    {
        internal static readonly string WINDOW_TITLE = "Bolt Wizard";
        internal static readonly string SUPPORT = "You can contact the Bolt Team or Photon Services using one of the following links. You can also go to Bolt Documentation in order to get started with Photon Bolt.";
        internal static readonly string PACKAGES = "Here you will be able to select all packages you want to use into your project. Packages marked green are already installed. \nClick to install.";
        internal static readonly string PHOTON = "In this step, you will configure your Photon Cloud credentials in order to use our servers for matchmaking, relay and much more. Please fill all fields with your desired configuration.";
        internal static readonly string PHOTON_DASH = "Go to Dashboard to create your App ID: ";

        internal static readonly string CLOSE_MSG_TITLE = "Incomplete Installation";
        internal static readonly string CLOSE_MSG_QUESTION = "Are you sure you want to exit the Wizard?";
        internal static readonly string DISCORD_TEXT = "Join the Bolt Discord Community.";
        internal static readonly string DISCORD_HEADER = "Community";
        internal static readonly string BUGTRACKER_TEXT = "Open bugtracker on github.";
        internal static readonly string BUGTRACKER_HEADER = "Bug Tracker";
        internal static readonly string DOCUMENTATION_TEXT = "Open the documentation.";
        internal static readonly string DOCUMENTATION_HEADER = "Documentation";
        internal static readonly string SAMPLES_TEXT = "Import the samples package.";
        internal static readonly string SAMPLES_HEADER = "Samples";
        internal static readonly string WIZARD_INTRO =
@"Hello! Welcome to Bolt Wizard!

We are glad that you decided to use our products and services. Here at Photon, we work hard to make your multiplayer game easier to build and much more fun to play.

This is a simple step by step configuration that you can follow and in just a few minutes you will be ready to use Bolt in all its power.

Please, feel free to click on the Next button, and get started.";
    }

    class BoltPackage
    {
        public string name;
        public string title;
        public string description;
        public Func<bool> installTest;
        public PackageFlags packageFlags = default(PackageFlags);
    }

    enum BoltSetupStage
    {
        SetupIntro = 1,
        SetupPhoton = 2,
        SetupBolt = 3,
        SetupSupport = 4
    }

    [Flags]
    enum BoltInstalls
    {
        Core = 1,
        Mobile = 2,
        Steam = 4,
        Samples = 8,
        PhotonSamples = 16,
        SteamSamples = 32,
        Monitor = 64
    }

    String PackagePath(String packageName)
    {
        return "Assets/bolt/packages/" + packageName + ".unitypackage";
    }

    Boolean PackageExists(String packageName)
    {
        return File.Exists(PackagePath(packageName));
    }

    Boolean ProjectExists()
    {
        return File.Exists("Assets/bolt/project.bytes");
    }

    Boolean MainPackageInstalled()
    {
        return File.Exists("Assets/bolt/scripts/BoltLauncher.cs");
    }

    Boolean SamplesPackageInstalled()
    {
        return Directory.Exists("Assets/bolt_samples");
    }

    Boolean PhotonCloudPackageInstalled()
    {
        return File.Exists("Assets/Plugins/Photon3DotNet.dll");
    }

    Boolean SteamPackageInstalled()
    {
        return File.Exists("Assets/Plugins/x86/CSteamworks.dll");
    }

    Boolean MobilePackageInstalled()
    {
        return Directory.Exists("Assets/Plugins/iOS") && Directory.Exists("Assets/Plugins/Android");
    }

    Boolean SteamSamplesPackageInstalled()
    {
        return Directory.Exists("Assets/bolt_samples/bolt-steam");
    }

    Boolean PhotonSamplesPackageInstalled()
    {
        return Directory.Exists("Assets/bolt_samples/PhotonCloud");
    }

    Boolean MonitorPackageInstalled()
    {
        return Directory.Exists("Assets/bolt_samples/NEW-ServerMonitor");
    }

    Boolean CanInstallPhotonCloudSamples()
    {
        return SamplesPackageInstalled() && PhotonCloudPackageInstalled();
    }

    Boolean CanInstallSteamSamples()
    {
        return SamplesPackageInstalled() && SteamPackageInstalled();
    }

    Action OpenURL(String url, params System.Object[] args)
    {
        return () =>
        {
            if (args.Length > 0)
            {
                url = String.Format(url, args);
            }

            Application.OpenURL(url);
        };
    }

    void InitialSetup()
    {
        const string SETTINGS_PATH = "Assets/bolt/resources/BoltRuntimeSettings.asset";
        const string PREFABDB_PATH = "Assets/bolt/resources/BoltPrefabDatabase.asset";

        if (!AssetDatabase.LoadAssetAtPath(SETTINGS_PATH, typeof(BoltRuntimeSettings)))
        {
            BoltRuntimeSettings settings = BoltRuntimeSettings.CreateInstance<BoltRuntimeSettings>();
            settings.masterServerGameId = Guid.NewGuid().ToString().ToUpperInvariant();

            AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            AssetDatabase.ImportAsset(SETTINGS_PATH, ImportAssetOptions.Default);
        }

        if (!AssetDatabase.LoadAssetAtPath(PREFABDB_PATH, typeof(Bolt.PrefabDatabase)))
        {
            AssetDatabase.CreateAsset(Bolt.PrefabDatabase.CreateInstance<Bolt.PrefabDatabase>(), PREFABDB_PATH);
            AssetDatabase.ImportAsset(PREFABDB_PATH, ImportAssetOptions.Default);
        }

        BoltMenuItems.RunCompiler();
    }

    static bool IsAppId(string val)
    {
        try
        {
            new Guid(val);
        }
        catch
        {
            return false;
        }
        return true;
    }
}
