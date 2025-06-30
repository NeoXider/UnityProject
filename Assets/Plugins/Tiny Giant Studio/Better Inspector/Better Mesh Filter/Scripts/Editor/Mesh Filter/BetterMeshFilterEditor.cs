using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    /// <summary>
    /// Methods containing the word Setup are called once when the inspector is created.
    /// Methods containing the word Update can be called multiple times to update to reflect changes.
    ///
    ///
    /// To-do:
    /// Get editor settings from update
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeshFilter))]
    public class BetterMeshFilterEditor : Editor
    {
        #region Variable Declarations

        /// <summary>
        /// If reference is lost, retrieved from file location
        /// </summary>
        [SerializeField]
        private VisualTreeAsset visualTreeAsset;

        [SerializeField]
        private VisualTreeAsset meshPreviewTemplate;

        private readonly string _visualTreeAssetFileLocation = "Assets/Plugins/Tiny Giant Studio/Better Inspector/Better Mesh Filter/Scripts/Editor/Mesh Filter/BetterMeshFilter.uxml";

        [SerializeField]
        private StyleSheet animatedFoldoutStyleSheet;

        private readonly string animatedFoldoutStyleSheetFileLocation = "Assets/Plugins/Tiny Giant Studio/Better Inspector/Common Scripts/Editor/StyleSheets/CustomFoldout_Animated.uss";

        private VisualElement root;

        private List<Mesh> meshes = new List<Mesh>();
        private List<Transform> transforms = new List<Transform>();

        private Button settingsButton;
        private GenericDropdownMenu settingsButtonContextMenu;

        private ObjectField meshField;

        private Editor originalEditor;

        private BetterMeshPreviewManager previewManager;
        private BaseSizeFoldoutManager baseSizeFoldoutManager;
        private ActionsFoldoutManager actionsFoldoutManager;
        private DebugGizmoManager debugGizmoManager;
        private BetterMeshInspectorSettingsFoldoutManager settingsFoldoutManager;

        private BetterMeshSettings editorSettings;

        private Label assetLocationOutsideFoldout;
        private Label assetLocationLabel;

        private CustomFoldoutSetup customFoldoutSetup;

        #endregion Variable Declarations

        #region Unity Stuff

        //This is not unnecessary.
        private void OnDestroy()
        {
            CleanUp();
        }

        private void OnDisable()
        {
            CleanUp();
        }

        /// <summary>
        /// CreateInspectorGUI is called each time something else is selected with this one locked.
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            if (target == null)
                return root;

            //In-case reference to the asset is lost, retrieve it from file location
            if (visualTreeAsset == null) visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_visualTreeAssetFileLocation);

            //If can't find the Better Mesh UXML,
            //Show the default inspector
            if (visualTreeAsset == null)
            {
                LoadDefaultEditor();
                return root;
            }

            visualTreeAsset.CloneTree(root);

            editorSettings = BetterMeshSettings.instance;
            customFoldoutSetup = new CustomFoldoutSetup();

            if (animatedFoldoutStyleSheet == null)
                animatedFoldoutStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(animatedFoldoutStyleSheetFileLocation);

            if (animatedFoldoutStyleSheet != null) //This shouldn't happen though. Just added for just in case, didn't get any error
            {
                if (editorSettings.animatedFoldout)
                    root.styleSheets.Add(animatedFoldoutStyleSheet);
            }

            debugGizmoManager = new(customFoldoutSetup, editorSettings, root); //This needs to be setup before mesh field because mesh field will pass the meshes list to debug list
            baseSizeFoldoutManager = new BaseSizeFoldoutManager(customFoldoutSetup, editorSettings, root); //This needs to be setup before mesh field because mesh field will pass the meshes list to debug list

            SetupMeshField();

            actionsFoldoutManager = new(customFoldoutSetup, editorSettings, root, targets);

            settingsFoldoutManager = new(customFoldoutSetup, editorSettings, root, animatedFoldoutStyleSheet);

            settingsFoldoutManager.OnMeshFieldPositionUpdated += UpdateMeshFieldGroupPosition;
            settingsFoldoutManager.OnMeshLocationSettingsUpdated += UpdateMeshTexts;
            settingsFoldoutManager.OnDebugGizmoSettingsUpdated += debugGizmoManager.UpdateDisplayStyle;
            settingsFoldoutManager.OnActionButtonsSettingsUpdated += actionsFoldoutManager.UpdateFoldoutVisibilities;
            settingsFoldoutManager.OnBaseSizeSettingsUpdated += BaseSizeSettingUpdated;

            previewManager = new BetterMeshPreviewManager(customFoldoutSetup, editorSettings, root, meshPreviewTemplate);
            previewManager.SetupPreviewManager(meshes, targets.Length);
            settingsFoldoutManager.OnPreviewSettingsUpdated += UpdatePreviews;

            settingsButton = root.Q<Button>("SettingsButton");
            settingsButton.clicked += () => OpenContextMenu_settingsButton();

            return root;
        }

        private void BaseSizeSettingUpdated()
        {
            if (baseSizeFoldoutManager != null)
                baseSizeFoldoutManager.UpdateTargets(meshes);
        }

        private void UpdateMeshFieldGroupPosition()
        {
            GroupBox meshFieldGroupBox = root.Q<GroupBox>("MeshFieldGroupBox");
            if (editorSettings.meshFieldOnTop)
                root.Q<GroupBox>("RootHolder").Insert(2, meshFieldGroupBox);
            else
                root.Q<VisualElement>("MainContainer").Insert(0, meshFieldGroupBox);
        }

        #endregion Unity Stuff

        private void SetupMeshField()
        {
            meshField = root.Q<ObjectField>("mesh");
            assetLocationOutsideFoldout = root.Q<Label>("AssetLocationOutsideFoldout");
            assetLocationLabel = root.Q<Label>("assetLocation");

            UpdateMeshesReferences();

            meshField.schedule.Execute(() => RegisterMeshField()).ExecuteLater(1); //1000 ms = 1 s

            UpdateMeshTexts();
            UpdateMeshFieldGroupPosition();
        }

        private void RegisterMeshField()
        {
            meshField.RegisterValueChangedCallback(ev =>
            {
                UpdateMeshesReferences();
                UpdateMeshTexts();

                if (actionsFoldoutManager != null)
                    actionsFoldoutManager.MeshUpdated();

                if (previewManager != null)
                    previewManager.CreatePreviews(meshes);

                if (meshes.Count == 0)
                    HideAllFoldouts();
            });
        }

        /// <summary>
        /// This updates the tool-tip and labels with asset location
        /// </summary>
        private void UpdateMeshTexts()
        {
            string assetPath;
            if (meshes == null) assetPath = "";
            else if (meshes.Count == 0) assetPath = "No mesh found.";
            else if (meshes.Count > 1) assetPath = "";
            else if (meshes.Count == 1 && meshes[0] != null && AssetDatabase.Contains(meshes[0]))
                assetPath = AssetDatabase.GetAssetPath(meshes[0]);
            else assetPath = "The mesh is not connected to an asset.";

            meshField.tooltip = assetPath;
            assetLocationOutsideFoldout.text = assetPath;
            assetLocationLabel.text = assetPath;

            if (targets.Length != 1)
            {
                assetLocationOutsideFoldout.style.display = DisplayStyle.None;
                assetLocationLabel.style.display = DisplayStyle.None;

                return;
            }

            if (editorSettings.ShowAssetLocationBelowMesh)
                assetLocationOutsideFoldout.style.display = DisplayStyle.Flex;
            else
                assetLocationOutsideFoldout.style.display = DisplayStyle.None;

            if (editorSettings.ShowAssetLocationInFoldout)
                assetLocationLabel.style.display = DisplayStyle.Flex;
            else
                assetLocationLabel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// This updates the following:
        /// List<Mesh> meshes;
        /// Mesh mesh;
        /// MeshFilter sourceMeshFilter;
        /// string assetPath;
        /// </summary>
        private void UpdateMeshesReferences()
        {
            meshes.Clear();
            transforms.Clear();

            foreach (MeshFilter meshFilter in targets.Cast<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null)
                {
                    meshes.Add(meshFilter.sharedMesh);
                    transforms.Add(meshFilter.transform);
                }
            }

            if (debugGizmoManager != null)
                debugGizmoManager.UpdateTargets(meshes, transforms);

            if (baseSizeFoldoutManager != null)
                baseSizeFoldoutManager.UpdateTargets(meshes);
        }

        #region Foldouts

        private void HideAllFoldouts()
        {
            previewManager.HideInformationFoldout();
            debugGizmoManager.HideDebugGizmo();
            settingsFoldoutManager.HideSettings();
            baseSizeFoldoutManager.HideFoldout();
        }

        #endregion Foldouts

        #region Settings

        #region Setup

        private void UpdatePreviews()
        {
            previewManager.CreatePreviews(meshes);
        }

        #endregion Setup

        #endregion Settings

        #region Functions

        /// <summary>
        /// This cleans up memory for the previews, textures and editors that can be created by the asset
        /// </summary>
        private void CleanUp()
        {
            if (previewManager != null)
            {
                previewManager.CleanUp();
                previewManager = null;
            }

            if (originalEditor != null)
                DestroyImmediate(originalEditor);

            if (debugGizmoManager != null)
                debugGizmoManager.Cleanup();
        }

        /// <summary>
        /// If the UXML file is missing for any reason,
        /// Instead of showing an empty inspector,
        /// This loads the default one.
        /// This shouldn't ever happen.
        /// </summary>
        private void LoadDefaultEditor()
        {
            if (originalEditor != null)
                DestroyImmediate(originalEditor);

            originalEditor = Editor.CreateEditor(targets);
            IMGUIContainer inspectorContainer = new IMGUIContainer(OnGUICallback);
            root.Add(inspectorContainer);
        }

        //For the original Editor
        private void OnGUICallback()
        {
            //EditorGUIUtility.hierarchyMode = true;

            EditorGUI.BeginChangeCheck();
            originalEditor.OnInspectorGUI();
            EditorGUI.EndChangeCheck();
        }

        private void OpenContextMenu_settingsButton()
        {
            UpdateContextMenu_settingsButton();
            settingsButtonContextMenu.DropDown(GetMenuRect(settingsButton), settingsButton, true);
        }

        private void UpdateContextMenu_settingsButton()
        {
            settingsButtonContextMenu = new GenericDropdownMenu();

            bool isChecked = settingsFoldoutManager.IsInspectorSettingsIsHidden() ? false : true;

            settingsButtonContextMenu.AddItem("Settings", isChecked, () =>
                {
                    settingsFoldoutManager.ToggleInspectorSettings();
                });
        }

        private Rect GetMenuRect(VisualElement anchor)
        {
            var worldBound = anchor.worldBound;
            worldBound.xMin -= 150;
            worldBound.xMax += 0;
            return worldBound;
        }

        private bool MeshIsAnAsset(Mesh newMesh) => AssetDatabase.Contains(newMesh);

        #endregion Functions

        private void OnSceneGUI()
        {
            debugGizmoManager?.DrawGizmo();
        }
    }
}