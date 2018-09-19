using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public partial class BoltWizardWindow : EditorWindow
{
    BoltSetupStage currentStage = BoltSetupStage.SetupIntro;
    bool corePackagesShow = true;
    bool extraPackagesShow = false;

    static Boolean? ready;
    static Single? firstCall;

    static String FirstStartupKey
    {
        get { return "$Bolt$First$Startup$Wizard/" + BoltNetwork.Version; }
    }

    static Vector2 WindowSize;
    static Vector2 WindowPosition;

    [NonSerialized]
    Func<bool> beforeNextCallback;

    [NonSerialized]
    Dictionary<BoltInstalls, BoltPackage> packageInfo;

    [NonSerialized]
    int ButtonWidth;

    [NonSerialized]
    int NavMenuWidth;

    // GUI

    [NonSerialized]
    Texture2D introIcon;

    [NonSerialized]
    Texture2D packagesIcon;

    [NonSerialized]
    Texture2D photonCloudIcon;

    [NonSerialized]
    Texture2D installIcon;

    [NonSerialized]
    Texture2D boltIcon;

    [NonSerialized]
    Texture2D activeIcon;

    [NonSerialized]
    Texture2D inactiveIcon;

    [NonSerialized]
    Texture2D bugtrackerIcon;

    [NonSerialized]
    GUIContent bugtrackerHeader;

    [NonSerialized]
    GUIContent bugtrackerText;

    [NonSerialized]
    Texture2D discordIcon;

    [NonSerialized]
    GUIContent discordHeader;

    [NonSerialized]
    GUIContent discordText;

    [NonSerialized]
    Texture2D documentationIcon;

    [NonSerialized]
    GUIContent documentationHeader;

    [NonSerialized]
    GUIContent documentationText;

    [NonSerialized]
    Texture2D samplesIcon;

    [NonSerialized]
    GUIContent samplesHeader;

    [NonSerialized]
    GUIContent samplesText;

    [NonSerialized]
    GUIStyle iconSection;

    [NonSerialized]
    GUIStyle stepStyle;

    [NonSerialized]
    GUIStyle headerStyle;

    [NonSerialized]
    GUIStyle headerLabel;

    [NonSerialized]
    GUIStyle headerLargeLabel;

    [NonSerialized]
    GUIStyle textLabel;

    [NonSerialized]
    GUIStyle centerInputText;

    [NonSerialized]
    GUIStyle minimalButton;

    [NonSerialized]
    GUIStyle introStyle;

    [NonSerialized]
    Vector2 scrollPosition;

    static BoltWizardWindow()
    {
        EditorApplication.update -= ShowWizardWindow;
        EditorApplication.update += ShowWizardWindow;

        WindowPosition = new Vector2(100, 100);
    }

    static void ShowWizardWindow()
    {
        if (firstCall.HasValue == false)
        {
            firstCall = Time.realtimeSinceStartup;
            return;
        }

        if ((Time.realtimeSinceStartup - firstCall.Value) > 1)
        {
            if (!EditorPrefs.GetBool(FirstStartupKey, false))
            {
                Open();
            }

            EditorApplication.update -= ShowWizardWindow;
        }
    }

    [MenuItem("Window/Bolt/Wizard")]
    public static void Open()
    {
        BoltWizardWindow window = GetWindow<BoltWizardWindow>(true, BoltWizardText.WINDOW_TITLE, true);
        window.position = new Rect(WindowPosition, WindowSize);
        window.Show();
    }

    static void ReOpen()
    {
        if (ready.HasValue && ready.Value == false)
        {
            Open();
        }

        EditorApplication.update -= ReOpen;
    }

    void OnEnable()
    {
        WindowSize = new Vector2(600, 550);

        minSize = WindowSize;

        NavMenuWidth = 210;
        ButtonWidth = 100;

        ready = false;

        beforeNextCallback = null;
    }

    void OnDestroy()
    {
        if (!EditorPrefs.GetBool(FirstStartupKey, false))
        {
            if (!EditorUtility.DisplayDialog(BoltWizardText.CLOSE_MSG_TITLE,
                                            BoltWizardText.CLOSE_MSG_QUESTION, "Yes", "Back"))
            {
                EditorApplication.update += ReOpen;
            }
        }

        ready = false;
    }

    void InitContent()
    {
        if (ready.HasValue && ready.Value) { return; }

        introIcon = Resources.Load<Texture2D>("icons_welcome/information");
        packagesIcon = Resources.Load<Texture2D>("icons_welcome/samples");
        photonCloudIcon = Resources.Load<Texture2D>("PhotonCloudIcon");
        installIcon = Resources.Load<Texture2D>("icons_welcome/install_icon");

        boltIcon = Resources.Load<Texture2D>("BoltIcon");

        activeIcon = Resources.Load<Texture2D>("icons_welcome/bullet_green");
        inactiveIcon = Resources.Load<Texture2D>("icons_welcome/bullet_black");

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.gray);
        texture.Apply();

        introStyle = new GUIStyle(EditorStyles.helpBox)
        {
            fontSize = 15,

            padding = new RectOffset(10, 10, 10, 10)
        };

        stepStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 0, 5, 0),
        };

        headerLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            padding = new RectOffset(10, 0, 0, 0),
            margin = new RectOffset()
        };

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(),
            padding = new RectOffset(10, 0, 0, 0)
        };

        headerLargeLabel = new GUIStyle(EditorStyles.boldLabel)
        {
            padding = new RectOffset(10, 0, 0, 0),
            margin = new RectOffset(),
            fontSize = 18,
        };
        headerLargeLabel.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0xf2 / 255f, 0xad / 255f, 0f) : new Color(30 / 255f, 99 / 255f, 183 / 255f);

        textLabel = new GUIStyle()
        {
            wordWrap = true,
            margin = new RectOffset(),
            padding = new RectOffset(10, 0, 0, 0)
        };

        centerInputText = new GUIStyle(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fixedHeight = 26
        };

        minimalButton = new GUIStyle(EditorStyles.miniButton)
        {
            fixedWidth = 130
        };

        iconSection = new GUIStyle
        {
            margin = new RectOffset(0, 0, 0, 0)
        };

        discordIcon = Resources.Load<Texture2D>("icons_welcome/community");
        discordText = new GUIContent(BoltWizardText.DISCORD_TEXT);
        discordHeader = new GUIContent(BoltWizardText.DISCORD_HEADER);

        bugtrackerIcon = Resources.Load<Texture2D>("icons_welcome/bugtracker");
        bugtrackerText = new GUIContent(BoltWizardText.BUGTRACKER_TEXT);
        bugtrackerHeader = new GUIContent(BoltWizardText.BUGTRACKER_HEADER);

        documentationIcon = Resources.Load<Texture2D>("icons_welcome/documentation");
        documentationText = new GUIContent(BoltWizardText.DOCUMENTATION_TEXT);
        documentationHeader = new GUIContent(BoltWizardText.DOCUMENTATION_HEADER);

        samplesIcon = Resources.Load<Texture2D>("icons_welcome/samples");
        samplesText = new GUIContent(BoltWizardText.SAMPLES_TEXT);
        samplesHeader = new GUIContent(BoltWizardText.SAMPLES_HEADER);

        // Package List

        packageInfo = new Dictionary<BoltInstalls, BoltPackage>
        {
            {
                BoltInstalls.Core,
                new BoltPackage()
                {
                    name = "bolt_install",
                    title = "Core Package",
                    description = "Install core bolt package",
                    installTest = MainPackageInstalled,
                    packageFlags = PackageFlags.RunInitialSetup
                }
            },

            {
                BoltInstalls.Mobile,
                new BoltPackage()
                {
                    name = "bolt_mobile_plugins",
                    title = "Mobile Plugins",
                    installTest = MobilePackageInstalled,
                    description = "Install iOS / Android socket plugins"
                }
            },

            {
                BoltInstalls.Steam,
                new BoltPackage()
                {
                    name = "bolt_steam",
                    title = "Steam",
                    installTest = SteamPackageInstalled,
                    description = "Install Steam support"
                }
            },

            {
                BoltInstalls.Samples,
                new BoltPackage()
                {
                    name = "bolt_samples",
                    title = "Samples",
                    description = "Install bolt samples",
                    installTest = SamplesPackageInstalled,
                    packageFlags = PackageFlags.WarnForProjectOverwrite
                }
            },

            {
                BoltInstalls.PhotonSamples,
                new BoltPackage()
                {
                    name = "bolt_photon_cloud_samples",
                    title = "Photon Cloud Samples",
                    description = "Install Photon Cloud samples",
                    installTest = PhotonSamplesPackageInstalled,
                    packageFlags = PackageFlags.WarnForProjectOverwrite
                }
            },

            {
                BoltInstalls.SteamSamples,
                new BoltPackage()
                {
                    name = "bolt_steam_samples",
                    title = "Steam Samples",
                    description = "Install Steam samples",
                    installTest = SteamSamplesPackageInstalled,
                    packageFlags = PackageFlags.WarnForProjectOverwrite
                }
            },

            {
                BoltInstalls.Monitor,
                new BoltPackage()
                {
                    name = "bolt_servermonitor",
                    title = "Server Monitor",
                    installTest = MonitorPackageInstalled,
                    description = "Install server monitor example"
                }
            }
        };

        ready = true;
    }

    void OnGUI()
    {
        InitContent();

        WindowPosition = position.position;

        EditorGUILayout.BeginVertical();
        DrawHeader();

        // Content
        EditorGUILayout.BeginHorizontal();

        // Nav menu
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(NavMenuWidth), GUILayout.MinWidth(NavMenuWidth));
        DrawNavMenu();
        EditorGUILayout.EndVertical();

        // Main Content
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawContent();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            Save();
        }
    }

    private void DrawContent()
    {
        switch (currentStage)
        {
            case BoltSetupStage.SetupIntro:
                DrawIntro();
                break;
            case BoltSetupStage.SetupPhoton:
                DrawSetupPhoton();
                break;
            case BoltSetupStage.SetupBolt:
                DrawSetupBolt();
                break;
            case BoltSetupStage.SetupSupport:
                DrawSupport();
                break;
        }

        GUILayout.FlexibleSpace();
        DrawFooter();
    }

    private void DrawIntro()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(BoltWizardText.WIZARD_INTRO, introStyle);
        GUILayout.EndVertical();
    }

    private void DrawSetupBolt()
    {
        DrawInputWithLabel("Bolt Setup", () =>
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label(BoltWizardText.PACKAGES, textLabel);
            GUILayout.EndVertical();
        }, false);
        GUILayout.Space(15);

        corePackagesShow = EditorGUILayout.Foldout(corePackagesShow, "Core Packages", true);

        if (corePackagesShow)
        {
            extraPackagesShow = false;

            DrawInstallOption(BoltInstalls.Core);

            EditorGUI.BeginDisabledGroup(!IsInstalled(BoltInstalls.Core));

            // MOBILE
            DrawInstallOption(BoltInstalls.Mobile);

            // STEAM
            DrawInstallOption(BoltInstalls.Steam);

            EditorGUI.EndDisabledGroup();
        }
        else
        {
            extraPackagesShow = true;
        }

        extraPackagesShow = EditorGUILayout.Foldout(extraPackagesShow, "Extra Packages", true);

        if (extraPackagesShow)
        {
            corePackagesShow = false;

            // SAMPLES
            EditorGUI.BeginDisabledGroup(!IsInstalled(BoltInstalls.Core));

            DrawInstallOption(BoltInstalls.Samples);

            // PHOTON CLOUD

            EditorGUI.BeginDisabledGroup(!IsInstalled(BoltInstalls.Core, BoltInstalls.Samples));

            DrawInstallOption(BoltInstalls.PhotonSamples);

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!IsInstalled(BoltInstalls.Core, BoltInstalls.Steam, BoltInstalls.Samples));

            DrawInstallOption(BoltInstalls.SteamSamples);

            EditorGUI.EndDisabledGroup();

            // SERVER MONITOR
            DrawInstallOption(BoltInstalls.Monitor);

            EditorGUI.EndDisabledGroup();
        }
        else
        {
            corePackagesShow = true;
        }

        // Action

        if (beforeNextCallback == null)
        {
            beforeNextCallback = () =>
            {
                if (!IsInstalled(BoltInstalls.Core))
                {
                    ShowNotification(new GUIContent("You must install at least the Bolt Core package."));
                    return false;
                }

                return true;
            };
        }
    }

    private void DrawSetupPhoton()
    {
        DrawInputWithLabel("Photon Cloud Setup", () =>
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label(BoltWizardText.PHOTON, textLabel);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(BoltWizardText.PHOTON_DASH, textLabel);
            if (GUILayout.Button("Visit Dashboard", minimalButton))
            {
                OpenURL("https://dashboard.photonengine.com/")();
            }
            GUILayout.EndHorizontal();

        }, false);
        GUILayout.Space(15);

        BoltRuntimeSettings settings = BoltRuntimeSettings.instance;

        DrawInputWithLabel("Photon Bolt App ID", () =>
        {
            GUILayout.BeginVertical();

            settings.photonAppId = EditorGUILayout.TextField(settings.photonAppId, centerInputText);

            GUILayout.EndVertical();

        }, false, true);

        DrawInputWithLabel("Connection Mode", () =>
        {
            settings.photonUseOnPremise = BoltEditorGUI.ToggleDropdown("Custom Hosted", "Photon Cloud", settings.photonUseOnPremise);
        }, true, true);

        if (settings.photonUseOnPremise)
        {
            DrawInputWithLabel("Master Server IP Address", () =>
            {
                GUILayout.BeginVertical();
                settings.photonOnPremiseIpAddress = EditorGUILayout.TextField(settings.photonOnPremiseIpAddress);
                GUILayout.EndVertical();
            }, true, true);
        }
        else
        {
            DrawInputWithLabel("Region", () =>
            {
                settings.photonCloudRegionIndex = EditorGUILayout.Popup(settings.photonCloudRegionIndex, BoltRuntimeSettings.photonCloudRegions);
            }, true, true);
        }

        DrawInputWithLabel("NAT Punchthrough Enabled", () =>
        {
            settings.photonUsePunch = BoltEditorGUI.Toggle(settings.photonUsePunch);
        }, true, true);

        // Action

        if (beforeNextCallback == null)
        {
            beforeNextCallback = () =>
            {
                if (!IsAppId(settings.photonAppId))
                {
                    ShowNotification(new GUIContent("Please specify a valid Bolt App ID."));
                    return false;
                }

                return true;
            };
        }
    }

    private void DrawSupport()
    {
        DrawInputWithLabel("Bolt Support", () =>
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label(BoltWizardText.SUPPORT, textLabel);
            GUILayout.EndVertical();
        }, false);
        GUILayout.Space(15);

        DrawStepOption(discordIcon, discordHeader, discordText, callback: OpenURL("https://discord.gg/0ya6ZpOvnShSCtbb"));
        DrawStepOption(bugtrackerIcon, bugtrackerHeader, bugtrackerText, callback: OpenURL("https://github.com/BoltEngine/Bolt-Tracker"));
        DrawStepOption(documentationIcon, documentationHeader, documentationText, callback: OpenURL("https://doc.photonengine.com/en-us/bolt/current/setup/overview"));
    }

    private void DrawNavMenu()
    {
        GUILayout.Space(5);
        DrawMenuHeader("Installation Steps");
        GUILayout.Space(10);

        DrawStepOption(introIcon, new GUIContent("Bolt Wizard Intro"),
                       active: currentStage == BoltSetupStage.SetupIntro); // callback: () => currentStage = BoltSetupStage.SetupIntro

        DrawStepOption(photonCloudIcon, new GUIContent("Photon Cloud"),
                       active: currentStage == BoltSetupStage.SetupPhoton); // callback: () => currentStage = BoltSetupStage.SetupPhoton

        DrawStepOption(boltIcon, new GUIContent("Bolt"),
                       active: currentStage == BoltSetupStage.SetupBolt); // callback: () => currentStage = BoltSetupStage.SetupBolt

        DrawStepOption(discordIcon, new GUIContent("Support"),
                       active: currentStage == BoltSetupStage.SetupSupport); // callback: () => currentStage = BoltSetupStage.SetupSupport

        GUILayout.FlexibleSpace();
        GUILayout.Label(string.Format("Bolt {0} v{1}", BoltNetwork.VersionDescription, BoltNetwork.Version), textLabel);
        GUILayout.Space(5);
    }

    void DrawHeader()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(Resources.Load<Texture2D>("BoltLogo"), GUILayout.Width(200), GUILayout.Height(109));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void DrawFooter()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup((int)currentStage == 1);

        if (GUILayout.Button("Back", GUILayout.Width(ButtonWidth)))
        {
            beforeNextCallback = null;
            BackStep();
        }

        EditorGUI.EndDisabledGroup();

        var nextLabel = currentStage == BoltSetupStage.SetupSupport ? "Done" : "Next";

        if (GUILayout.Button(nextLabel, GUILayout.Width(ButtonWidth)))
        {
            if (beforeNextCallback == null || beforeNextCallback())
            {
                if (currentStage == BoltSetupStage.SetupSupport)
                {
                    EditorPrefs.SetBool(FirstStartupKey, true);
                    Close();
                }

                NextStep();
                beforeNextCallback = null;
            }
        }

        GUILayout.Space(5);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    // Utils

    private void Save()
    {
        EditorUtility.SetDirty(BoltRuntimeSettings.instance);
        AssetDatabase.SaveAssets();
    }

    void DrawMenuHeader(String text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.Label(text, headerLargeLabel);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void DrawInputWithLabel(String label, Action gui, bool horizontal = true, bool box = false)
    {
        GUILayout.Space(10);

        if (horizontal)
        {
            if (box)
            {
                GUILayout.BeginHorizontal(stepStyle);
            }
            else
            {
                GUILayout.BeginHorizontal();
            }
        }
        else
        {
            if (box)
            {
                GUILayout.BeginVertical(stepStyle);
            }
            else
            {
                GUILayout.BeginVertical();
            }
        }

        GUILayout.Label(label, headerStyle, GUILayout.Width(220));
        GUILayout.Space(5);

        gui();

        GUILayout.Space(5);

        if (horizontal)
        {
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.EndVertical();
        }
    }

    void DrawInstallOption(BoltInstalls install)
    {
        BoltPackage package = packageInfo[install];

        Action action = () =>
        {
            if (package.installTest())
            {
                Debug.LogWarning("Package already installed");
                return;
            }

            Install(package);
        };

        if (PackageExists(package.name))
        {
            DrawStepOption(samplesIcon, new GUIContent(package.title), new GUIContent(package.description), package.installTest(), action);
        }
    }

    void DrawStepOption(Texture2D icon, GUIContent header, GUIContent description = null, bool? active = null, System.Action callback = null)
    {
        GUILayout.BeginHorizontal(stepStyle);

        if (icon != null)
        {
            GUILayout.Label(icon, iconSection, GUILayout.Width(32), GUILayout.Height(32));
        }

        var height = icon != null ? 32 : 16;

        GUILayout.BeginVertical(GUILayout.MinHeight(height));
        GUILayout.FlexibleSpace();

        GUILayout.Label(header, headerLabel, GUILayout.MinWidth(120));

        if (description != null)
        {
            GUILayout.Label(description, textLabel);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        if (active == true)
        {
            GUILayout.Label(activeIcon, iconSection, GUILayout.Width(height), GUILayout.Height(height));
        }
        else if (active == false)
        {
            GUILayout.Label(inactiveIcon, iconSection, GUILayout.Width(height), GUILayout.Height(height));
        }

        GUILayout.EndHorizontal();

        if (callback != null)
        {
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                callback();
                GUIUtility.ExitGUI();
            }
        }
    }

    void NextStep()
    {
        currentStage += (int)currentStage < Enum.GetValues(typeof(BoltSetupStage)).Length ? 1 : 0;
    }

    void BackStep()
    {
        currentStage -= (int)currentStage > 1 ? 1 : 0;
    }

    bool IsInstalled(params BoltInstalls[] installs)
    {
        foreach (var pack in installs)
        {
            if (!packageInfo[pack].installTest())
            {
                return false;
            }
        }

        return true;
    }

    void Install(BoltPackage package)
    {
        string packageName = package.name;
        PackageFlags flags = package.packageFlags;

        if ((flags & PackageFlags.WarnForProjectOverwrite) == PackageFlags.WarnForProjectOverwrite)
        {
            if (ProjectExists())
            {
                if (EditorUtility.DisplayDialog("Warning", "Importing this package will overwrite the existing bolt project file that contains all your states, events, etc. Are you sure?", "Yes", "No") == false)
                {
                    return;
                }
            }
        }

        if ((flags & PackageFlags.RunInitialSetup) == PackageFlags.RunInitialSetup)
        {
            InitialSetup();
        }

        AssetDatabase.ImportPackage(PackagePath(packageName), false);

        currentStage = BoltSetupStage.SetupBolt;
    }
}
