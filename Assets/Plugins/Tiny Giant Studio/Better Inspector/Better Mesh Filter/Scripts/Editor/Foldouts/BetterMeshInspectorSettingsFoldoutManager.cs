using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    public class BetterMeshInspectorSettingsFoldoutManager //That's one giant name, but I want names to be clear and not get mixed with other settings scripts
    {
        StyleSheet animatedFoldoutStyleSheet;

        private bool inspectorFoldoutSetupCompleted = false;

        BetterMeshSettings editorSettings;
        CustomFoldoutSetup customFoldoutSetup;

        VisualElement root;
        GroupBox inspectorSettingsFoldout;
        private Toggle inspectorSettingsFoldoutToggle;

        Toggle overrideInspectorColorToggle;
        private ColorField inspectorColorField;
        Toggle overrideFoldoutColorToggle;
        private ColorField foldoutColorField;

        private readonly string assetLink = "https://assetstore.unity.com/packages/tools/utilities/better-mesh-filter-266489?aid=1011ljxWe";
        private readonly string publisherLink = "https://assetstore.unity.com/publishers/45848?aid=1011ljxWe";
        private readonly string documentationLink = "https://ferdowsur.gitbook.io/better-mesh/";

        Toggle autoHideInspectorSettingsField;
        Toggle showMeshFieldOnTop;
        private Toggle showAssetLocationBelowPreviewField;
        private Toggle showAssetLocationInFoldoutToggle;

        private Toggle showMeshPreviewField;
        private SliderInt maxPreviewAmount;

        private Toggle showInformationOnPreviewField;
        private Toggle showRuntimeMemoryUsageUnderPreview;
        private Toggle runTimeMemoryUsageLabelToggle;

        private Toggle showMeshSizeField;

        private Toggle showMeshDetailsInFoldoutToggle;
        private Toggle showVertexInformationToggle;
        private Toggle showTriangleInformationToggle;
        private Toggle showEdgeInformationToggle;
        private Toggle showFaceInformationToggle;
        private Toggle showTangentInformationToggle;

        private Toggle showDebugGizmoFoldoutField;


        public event Action OnPreviewSettingsUpdated;
        public event Action OnMeshFieldPositionUpdated;
        public event Action OnMeshLocationSettingsUpdated;
        public event Action OnActionButtonsSettingsUpdated;
        public event Action OnBaseSizeSettingsUpdated;
        public event Action OnDebugGizmoSettingsUpdated;

        public BetterMeshInspectorSettingsFoldoutManager(CustomFoldoutSetup customFoldoutSetup, BetterMeshSettings editorSettings, VisualElement root, StyleSheet animatedFoldoutStyleSheet)
        {
            inspectorFoldoutSetupCompleted = false; //Unnecessary. This is the default value

            this.animatedFoldoutStyleSheet = animatedFoldoutStyleSheet;
            this.root = root;
            this.editorSettings = editorSettings;
            this.customFoldoutSetup = customFoldoutSetup;

            inspectorSettingsFoldout = root.Q<GroupBox>("InspectorSettings");
            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout);

            inspectorSettingsFoldoutToggle = inspectorSettingsFoldout.Q<Toggle>("FoldoutToggle");

            inspectorSettingsFoldout.style.display = DisplayStyle.None;
            UpdateInspectorColor();
        }

        public void ToggleInspectorSettings()
        {
            if (IsInspectorSettingsIsHidden())
            {
                //This completes the setup of the inspector foldout.
                //This is made to avoid referencing everything at the start,
                //Giving this a performance improvement
                if (!inspectorFoldoutSetupCompleted) Setup();

                UpdateInspectorSettingsFoldout();

                inspectorSettingsFoldout.style.display = DisplayStyle.Flex;
                inspectorSettingsFoldoutToggle.value = true;
            }
            else
            {
                HideSettings();
            }
        }

        /// <summary>
        /// Returns true when the foldout is hidden.
        /// </summary>
        /// <returns></returns>
        public bool IsInspectorSettingsIsHidden() => inspectorSettingsFoldout.style.display == DisplayStyle.None;


        void Setup()
        {
            inspectorSettingsFoldoutToggle.RegisterValueChangedCallback(ev =>
            {
                if (editorSettings.AutoHideSettings)
                {
                    if (ev.newValue)
                    {
                        inspectorSettingsFoldout.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        inspectorSettingsFoldout.style.display = DisplayStyle.None;
                    }
                }
            });

            #region Inspector Customization

            Toggle foldoutAnimationsToggle = inspectorSettingsFoldout.Q<Toggle>("FoldoutAnimationsToggle");
            foldoutAnimationsToggle.SetValueWithoutNotify(editorSettings.animatedFoldout);
            foldoutAnimationsToggle.RegisterValueChangedCallback(e =>
            {
                editorSettings.animatedFoldout = e.newValue;
                editorSettings.Save();

                if (!e.newValue)
                {
                    if (root.styleSheets.Contains(animatedFoldoutStyleSheet))
                    {
                        root.styleSheets.Remove(animatedFoldoutStyleSheet);
                    }
                }
                else
                {
                    if (!root.styleSheets.Contains(animatedFoldoutStyleSheet))
                    {
                        root.styleSheets.Add(animatedFoldoutStyleSheet);
                    }
                }
            });

            foldoutColorField = inspectorSettingsFoldout.Q<ColorField>("FoldoutColorField");
            inspectorColorField = inspectorSettingsFoldout.Q<ColorField>("InspectorColorField");

            overrideInspectorColorToggle = inspectorSettingsFoldout.Q<Toggle>("OverrideInspectorColorToggle");
            overrideInspectorColorToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.OverrideInspectorColor = ev.newValue;
                UpdateInspectorColor();
            });
            inspectorColorField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.InspectorColor = ev.newValue;
                UpdateInspectorColor();
            });
            overrideFoldoutColorToggle = inspectorSettingsFoldout.Q<Toggle>("OverrideFoldoutColorToggle");
            overrideFoldoutColorToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.OverrideFoldoutColor = ev.newValue;
                UpdateInspectorColor();
            });
            foldoutColorField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.FoldoutColor = ev.newValue;
                UpdateInspectorColor();
            });

            #endregion Inspector Customization

            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout.Q<GroupBox>("InspectorCustomizationFoldout"));
            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout.Q<GroupBox>("MeshPreviewSettingsFoldout"), "FoldoutToggle", "showMeshPreview");
            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout.Q<GroupBox>("InformationFoldoutSettingsFoldout"), "FoldoutToggle", "ShowMeshDetailsInFoldoutToggle");
            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout.Q<GroupBox>("MeshDetailsSettingsFoldout"));
            customFoldoutSetup.SetupFoldout(inspectorSettingsFoldout.Q<GroupBox>("ActionSettingsFoldout"), "FoldoutToggle", "showActionsFoldout");

            inspectorSettingsFoldout.Q<ToolbarButton>("AssetLink").clicked += () => { Application.OpenURL(assetLink); };
            inspectorSettingsFoldout.Q<ToolbarButton>("Documentation").clicked += () => { Application.OpenURL(documentationLink); };
            inspectorSettingsFoldout.Q<ToolbarButton>("OtherAssetsLink").clicked += () => { Application.OpenURL(publisherLink); };

            inspectorSettingsFoldout.Q<Button>("ResetInspectorSettings").clicked += ResetInspectorSettings;
            inspectorSettingsFoldout.Q<Button>("ResetInspectorSettings2").clicked += ResetInspectorSettings2;
            inspectorSettingsFoldout.Q<Button>("ResetInspectorSettingsToMinimal").clicked += ResetInspectorSettingsToMinimal;
            inspectorSettingsFoldout.Q<Button>("ResetInspectorSettingsToNothing").clicked += ResetInspectorSettingsToNothing;

            autoHideInspectorSettingsField = inspectorSettingsFoldout.Q<Toggle>("autoHideInspectorSettings");
            autoHideInspectorSettingsField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.AutoHideSettings = ev.newValue;
            });

            showMeshFieldOnTop = inspectorSettingsFoldout.Q<Toggle>("MeshFieldOnTop");
            showMeshFieldOnTop.RegisterValueChangedCallback(ev =>
            {
                editorSettings.meshFieldOnTop = ev.newValue;
                OnMeshFieldPositionUpdated?.Invoke();
            });

            showMeshPreviewField = inspectorSettingsFoldout.Q<Toggle>("showMeshPreview");
            showMeshPreviewField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowMeshPreview = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            maxPreviewAmount = inspectorSettingsFoldout.Q<SliderInt>("MaximumPreviewAmount");
            maxPreviewAmount.RegisterValueChangedCallback(ev =>
            {
                editorSettings.MaxPreviewCount = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });



            showInformationOnPreviewField = inspectorSettingsFoldout.Q<Toggle>("ShowInformationOnPreview");
            showInformationOnPreviewField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowMeshDetailsUnderPreview = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            showRuntimeMemoryUsageUnderPreview = inspectorSettingsFoldout.Q<Toggle>("ShowRuntimeMemoryUsageBelowPreview");
            runTimeMemoryUsageLabelToggle = inspectorSettingsFoldout.Q<Toggle>("RunTimeMemoryUsageLabelToggle");

            showRuntimeMemoryUsageUnderPreview.RegisterValueChangedCallback(ev =>
            {
                editorSettings.runtimeMemoryUsageUnderPreview = ev.newValue;
                editorSettings.Save();

                OnPreviewSettingsUpdated?.Invoke();
            });

            runTimeMemoryUsageLabelToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.showRunTimeMemoryUsageLabel = ev.newValue;
                editorSettings.Save();

                OnPreviewSettingsUpdated?.Invoke();
            });

            showAssetLocationBelowPreviewField = inspectorSettingsFoldout.Q<Toggle>("ShowAssetLocationBelowPreview");
            showAssetLocationBelowPreviewField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowAssetLocationBelowMesh = ev.newValue;
                OnMeshLocationSettingsUpdated?.Invoke();
            });

            showAssetLocationInFoldoutToggle = inspectorSettingsFoldout.Q<Toggle>("ShowAssetLocationInFoldout");
            showAssetLocationInFoldoutToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowAssetLocationInFoldout = ev.newValue;

                OnPreviewSettingsUpdated?.Invoke();
            });

            showMeshSizeField = inspectorSettingsFoldout.Q<Toggle>("ShowMeshSize");
            showMeshSizeField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowSizeFoldout = ev.newValue;

                OnBaseSizeSettingsUpdated?.Invoke();
            });




            #region Mesh Details

            showMeshDetailsInFoldoutToggle = inspectorSettingsFoldout.Q<Toggle>("ShowMeshDetailsInFoldoutToggle");
            showMeshDetailsInFoldoutToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowInformationFoldout = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            showVertexInformationToggle = inspectorSettingsFoldout.Q<Toggle>("showVertexCount");
            showVertexInformationToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowVertexInformation = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            showTriangleInformationToggle = inspectorSettingsFoldout.Q<Toggle>("showTriangleCount");
            showTriangleInformationToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowTriangleInformation = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            showEdgeInformationToggle = inspectorSettingsFoldout.Q<Toggle>("showEdgeCount");
            showEdgeInformationToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowEdgeInformation = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });

            showFaceInformationToggle = inspectorSettingsFoldout.Q<Toggle>("showFaceCount");
            showFaceInformationToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowFaceInformation = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });
            showTangentInformationToggle = inspectorSettingsFoldout.Q<Toggle>("showTangentCount");
            showTangentInformationToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowTangentInformation = ev.newValue;
                OnPreviewSettingsUpdated?.Invoke();
            });
            #endregion

            #region Actions

            var showActionsFoldoutField = inspectorSettingsFoldout.Q<Toggle>("showActionsFoldout");

            showActionsFoldoutField.SetValueWithoutNotify(editorSettings.ShowActionsFoldout);
            showActionsFoldoutField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowActionsFoldout = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });

            var showOptimizeButtonToggle = inspectorSettingsFoldout.Q<Toggle>("OptimizeMesh");
            showOptimizeButtonToggle.SetValueWithoutNotify(editorSettings.ShowOptimizeButton);
            showOptimizeButtonToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowOptimizeButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });
            Toggle recalculateNormalsToggle = inspectorSettingsFoldout.Q<Toggle>("RecalculateNormals");
            recalculateNormalsToggle.SetValueWithoutNotify(editorSettings.ShowRecalculateNormalsButton);
            recalculateNormalsToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowRecalculateNormalsButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });

            Toggle showRecalculateTangentsButtonToggle = inspectorSettingsFoldout.Q<Toggle>("RecalculateTangents");
            showRecalculateTangentsButtonToggle.SetValueWithoutNotify(editorSettings.ShowRecalculateTangentsButton);
            showRecalculateTangentsButtonToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowRecalculateTangentsButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });

            Toggle showFlipNormalsToggle = inspectorSettingsFoldout.Q<Toggle>("FlipNormals");
            showFlipNormalsToggle.SetValueWithoutNotify(editorSettings.ShowFlipNormalsButton);
            showFlipNormalsToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowFlipNormalsButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });

            Toggle showGenerateSecondaryUVButtonToggle = inspectorSettingsFoldout.Q<Toggle>("GenerateSecondaryUVSet");
            showGenerateSecondaryUVButtonToggle.SetValueWithoutNotify(editorSettings.ShowGenerateSecondaryUVButton);
            showGenerateSecondaryUVButtonToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowGenerateSecondaryUVButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });

            Toggle showSaveMeshButtonAsToggle = inspectorSettingsFoldout.Q<Toggle>("SaveMeshAs");
            showSaveMeshButtonAsToggle.SetValueWithoutNotify(editorSettings.ShowSaveMeshAsButton);
            showSaveMeshButtonAsToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowSaveMeshAsButton = ev.newValue;
                OnActionButtonsSettingsUpdated?.Invoke();
            });
            #endregion

            showDebugGizmoFoldoutField = inspectorSettingsFoldout.Q<Toggle>("showDebugGizmoFoldout");
            showDebugGizmoFoldoutField.RegisterValueChangedCallback(ev =>
            {
                editorSettings.ShowDebugGizmoFoldout = ev.newValue;
                OnDebugGizmoSettingsUpdated?.Invoke();
            });

            Button scaleSettingsButton = inspectorSettingsFoldout.Q<Button>("ScaleSettingsButton");
            scaleSettingsButton.clicked += () =>
            {
                SettingsService.OpenProjectSettings("Project/Tiny Giant Studio/Scale Settings");
                GUIUtility.ExitGUI();
            };

            inspectorFoldoutSetupCompleted = true;
        }

        /// <summary>
        /// This updates the fields that show the current inspector settings
        /// </summary>
        void UpdateInspectorSettingsFoldout()
        {
            overrideInspectorColorToggle.SetValueWithoutNotify(editorSettings.OverrideInspectorColor);
            inspectorColorField.SetValueWithoutNotify(editorSettings.InspectorColor);
            overrideFoldoutColorToggle.SetValueWithoutNotify(editorSettings.OverrideFoldoutColor);
            foldoutColorField.SetValueWithoutNotify(editorSettings.FoldoutColor);

            autoHideInspectorSettingsField.SetValueWithoutNotify(editorSettings.AutoHideSettings);
            showMeshFieldOnTop.SetValueWithoutNotify(editorSettings.meshFieldOnTop);

            showMeshPreviewField.SetValueWithoutNotify(editorSettings.ShowMeshPreview);
            maxPreviewAmount.SetValueWithoutNotify(editorSettings.MaxPreviewCount);

            showInformationOnPreviewField.SetValueWithoutNotify(editorSettings.ShowMeshDetailsUnderPreview);

            showRuntimeMemoryUsageUnderPreview.SetValueWithoutNotify(editorSettings.runtimeMemoryUsageUnderPreview);
            runTimeMemoryUsageLabelToggle.SetValueWithoutNotify(editorSettings.showRunTimeMemoryUsageLabel);

            showMeshSizeField.SetValueWithoutNotify(editorSettings.ShowSizeFoldout);
            showAssetLocationBelowPreviewField.SetValueWithoutNotify(editorSettings.ShowAssetLocationBelowMesh);
            showAssetLocationInFoldoutToggle.SetValueWithoutNotify(editorSettings.ShowAssetLocationInFoldout);

            showMeshDetailsInFoldoutToggle.SetValueWithoutNotify(editorSettings.ShowInformationFoldout);
            showVertexInformationToggle.SetValueWithoutNotify(editorSettings.ShowVertexInformation);
            showTriangleInformationToggle.SetValueWithoutNotify(editorSettings.ShowTriangleInformation);
            showEdgeInformationToggle.SetValueWithoutNotify(editorSettings.ShowEdgeInformation);
            showFaceInformationToggle.SetValueWithoutNotify(editorSettings.ShowFaceInformation);
            showTangentInformationToggle.SetValueWithoutNotify(editorSettings.ShowTangentInformation);

            showDebugGizmoFoldoutField.SetValueWithoutNotify(editorSettings.ShowDebugGizmoFoldout);
        }

        public void HideSettings()
        {
            inspectorSettingsFoldout.style.display = DisplayStyle.None;
            inspectorSettingsFoldoutToggle.value = false;
        }



        private void ResetInspectorSettings()
        {
            editorSettings.ResetToDefault();
            EditorSettingsHaveBeenReset();
        }

        private void ResetInspectorSettings2()
        {
            editorSettings.ResetToDefault2();
            EditorSettingsHaveBeenReset();
        }

        private void ResetInspectorSettingsToMinimal()
        {
            editorSettings.ResetToMinimal();
            EditorSettingsHaveBeenReset();
        }

        private void ResetInspectorSettingsToNothing()
        {
            editorSettings.ResetToNothing();
            EditorSettingsHaveBeenReset();
        }

        private void EditorSettingsHaveBeenReset()
        {
            UpdateInspectorSettingsFoldout();
            UpdateInspectorColor();

            OnPreviewSettingsUpdated?.Invoke();
            OnMeshFieldPositionUpdated?.Invoke();
            OnMeshLocationSettingsUpdated?.Invoke();
            OnActionButtonsSettingsUpdated?.Invoke();
            OnBaseSizeSettingsUpdated?.Invoke();
            OnDebugGizmoSettingsUpdated?.Invoke();
        }

        private void UpdateInspectorColor()
        {
            List<GroupBox> customFoldoutGroups = root.Query<GroupBox>(className: "custom-foldout").ToList();
            if (editorSettings.OverrideFoldoutColor)
            {
                foldoutColorField?.SetEnabled(true);

                foreach (GroupBox foldout in customFoldoutGroups)
                    foldout.style.backgroundColor = editorSettings.FoldoutColor;
            }
            else
            {
                foldoutColorField?.SetEnabled(false);
                foreach (GroupBox foldout in customFoldoutGroups)
                    foldout.style.backgroundColor = StyleKeyword.Null;
            }

            if (editorSettings.OverrideInspectorColor)
            {
                inspectorColorField?.SetEnabled(true);
                root.Q<GroupBox>("RootHolder").style.backgroundColor = editorSettings.InspectorColor;
            }
            else
            {
                inspectorColorField?.SetEnabled(false);
                root.Q<GroupBox>("RootHolder").style.backgroundColor = StyleKeyword.Null;
            }
        }




    }
}