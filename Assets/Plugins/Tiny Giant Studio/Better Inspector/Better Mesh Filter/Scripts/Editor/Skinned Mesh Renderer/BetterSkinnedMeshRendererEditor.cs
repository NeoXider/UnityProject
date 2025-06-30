using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    /// <summary>
    ///
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SkinnedMeshRenderer))]
    public class SkinnedMeshRendererEditor : Editor
    {
        #region Variables

        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        private List<Mesh> meshes = new();
        private List<Transform> transforms = new();


        /// <summary>
        /// If reference is lost, this is retrieved from file location
        /// </summary>
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        [SerializeField]
        private VisualTreeAsset meshPreviewTemplate;

        /// <summary>
        /// Location of the visualTreeAsset file
        /// </summary>
        private readonly string visualTreeAssetFileLocation = "Assets/Plugins/Tiny Giant Studio/Better Inspector/Better Mesh Filter/Scripts/Editor/Skinned Mesh Renderer/BetterSkinnedMeshRenderer.uxml";

        [SerializeField]
        private StyleSheet animatedFoldoutStyleSheet;

        private readonly string animatedFoldoutStyleSheetFileLocation = "Assets/Plugins/Tiny Giant Studio/Better Inspector/Common Scripts/Editor/StyleSheets/CustomFoldout_Animated.uss";

        private Editor originalEditor;
        private VisualElement root;

        private CustomFoldoutSetup customFoldoutSetup;

        private BetterMeshSettings editorSettings;
        private BetterMeshInspectorSettingsFoldoutManager settingsFoldoutManager;

        private BetterMeshPreviewManager previewManager;
        private BaseSizeFoldoutManager baseSizeFoldoutManager;
        private ActionsFoldoutManager actionsFoldoutManager;
        private DebugGizmoManager debugGizmoManager;



        private readonly string boundingVolumeEditButtonTooltip = "Edit bounding volume in the scene view.\n\n  - Hold Alt after clicking control handle to pin center in place.\n  - Hold Shift after clicking control handle to scale uniformly.";

        #region UI

        private ObjectField meshField;
        private HelpBox invalidMeshWarning;
        private Label assetLocationOutsideFoldout;
        private Label assetLocationLabel;

        private GroupBox blendShapesFoldout;

        private GroupBox boundsGroupBox;
        private PropertyField aabb;

        private Button settingsButton;

        #endregion UI

        private GenericDropdownMenu settingsButtonContextMenu;

        #endregion Variables

        #region Unity stuff

        //This is not unnecessary.
        private void OnDestroy()
        {
            CleanUp();
        }

        private void OnDisable()
        {
            CleanUp();
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            if (target == null)
                return root;

            skinnedMeshRenderers = new SkinnedMeshRenderer[targets.Length];
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                skinnedMeshRenderers[i] = (SkinnedMeshRenderer)targets[i];
            }

            //In-case reference to the asset is lost, retrieve it from file location
            if (visualTreeAsset == null) visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreeAssetFileLocation);

            if (visualTreeAsset == null) //if couldn't find the asset, load the default inspector instead of showing an empty section
            {
                LoadDefaultEditor(root);
                return root;
            }

            visualTreeAsset.CloneTree(root);

            editorSettings = BetterMeshSettings.instance;
            customFoldoutSetup = new();

            if (animatedFoldoutStyleSheet == null)
                animatedFoldoutStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(animatedFoldoutStyleSheetFileLocation);

            if (animatedFoldoutStyleSheet != null) //This shouldn't happen though. Just added for just in case, didn't get any error
            {
                if (editorSettings.animatedFoldout)
                    root.styleSheets.Add(animatedFoldoutStyleSheet);
            }

            debugGizmoManager = new(customFoldoutSetup, editorSettings, root);
            baseSizeFoldoutManager = new BaseSizeFoldoutManager(customFoldoutSetup, editorSettings, root);

            SetupMeshFieldGroup();

            SetupBlendShapes();

            SetupBoundingVolume();

            SetupLightingFoldout();

            SetupProbesFoldout();

            actionsFoldoutManager = new ActionsFoldoutManager(customFoldoutSetup, editorSettings, root, targets);

            SetupDebugFoldout();

            SetupAdditionalSettings();

            UpdateInspectorOverride();

            settingsFoldoutManager = new(customFoldoutSetup, editorSettings, root, animatedFoldoutStyleSheet);

            settingsFoldoutManager.OnMeshFieldPositionUpdated += UpdateMeshFieldGroupPosition;
            settingsFoldoutManager.OnMeshLocationSettingsUpdated += UpdateMeshTexts;
            settingsFoldoutManager.OnDebugGizmoSettingsUpdated += debugGizmoManager.UpdateDisplayStyle;
            settingsFoldoutManager.OnDebugGizmoSettingsUpdated += UpdateDebugFoldoutVisibility;
            settingsFoldoutManager.OnActionButtonsSettingsUpdated += actionsFoldoutManager.UpdateFoldoutVisibilities;
            settingsFoldoutManager.OnBaseSizeSettingsUpdated += BaseSizeSettingUpdated;

            previewManager = new BetterMeshPreviewManager(customFoldoutSetup, editorSettings, root, meshPreviewTemplate);
            previewManager.SetupPreviewManager(meshes, targets.Length);
            settingsFoldoutManager.OnPreviewSettingsUpdated += UpdatePreviews;

            settingsButton = root.Q<Button>("SettingsButton");
            settingsButton.clicked += () => OpenContextMenu_settingsButton();

            root.Q<Label>("BonesCounter").text = GetBonesCount().ToString(CultureInfo.InvariantCulture);

            return root;
        }

        private int GetBonesCount()
        {
            int count = 0;
            foreach (var item in skinnedMeshRenderers)
            {
                count += item.bones.Length;
            }
            return count;
        }

        private void UpdatePreviews()
        {
            previewManager.CreatePreviews(meshes);
        }

        private void BaseSizeSettingUpdated()
        {
            if (baseSizeFoldoutManager != null)
                baseSizeFoldoutManager.UpdateTargets(meshes);
        }

        private void UpdateInspectorOverride()
        {
            var overrideContainer = root.Q<VisualElement>("OverrideOfTheDefaultInspector");
            var defaultContainer = root.Q<VisualElement>("DefaultInspectorContainer");
            if (!editorSettings.showDefaultSkinnedMeshRendererInspector && skinnedMeshRenderers.Length == 1)
            {
                overrideContainer.style.display = DisplayStyle.Flex;
                defaultContainer.style.display = DisplayStyle.None;
            }
            else
            {
                overrideContainer.style.display = DisplayStyle.None;
                defaultContainer.style.display = DisplayStyle.Flex;
                LoadDefaultEditor(defaultContainer);
            }
        }

        #endregion Unity stuff

        #region MeshField

        private void SetupMeshFieldGroup()
        {
            meshField = root.Q<ObjectField>("MeshField");
            assetLocationOutsideFoldout = root.Q<Label>("AssetLocationOutsideFoldout");
            assetLocationLabel = root.Q<Label>("assetLocation");

            UpdateMeshReferences();

            meshField.schedule.Execute(() => RegsiterMeshField()).ExecuteLater(1); //1000 ms = 1 s

            UpdateMeshSelectionWarnings();
            UpdateMeshTexts();
            UpdateMeshFieldGroupPosition();
        }

        private void RegsiterMeshField()
        {
            meshField.RegisterValueChangedCallback(e =>
            {
                UpdateMeshSelectionWarnings();
                UpdateBlendShapes();

                UpdateMeshReferences();
                UpdateMeshTexts();

                if (actionsFoldoutManager != null)
                    actionsFoldoutManager.MeshUpdated();

                if (previewManager != null)
                    previewManager.CreatePreviews(meshes);

                if (meshes.Count == 0)
                    HideAllFoldouts();
            });
        }

        private void HideAllFoldouts()
        {
            previewManager.HideInformationFoldout();
            debugGizmoManager.HideDebugGizmo();
            settingsFoldoutManager.HideSettings();
            baseSizeFoldoutManager.HideFoldout();
        }

        private void UpdateMeshReferences()
        {
            meshes.Clear();
            transforms.Clear();

            foreach (SkinnedMeshRenderer s in targets.Cast<SkinnedMeshRenderer>())
            {
                if (s.sharedMesh != null)
                {
                    meshes.Add(s.sharedMesh);
                    transforms.Add(s.transform);
                }
            }

            if (debugGizmoManager != null)
                debugGizmoManager.UpdateTargets(meshes, transforms);

            if (baseSizeFoldoutManager != null)
                baseSizeFoldoutManager.UpdateTargets(meshes);
        }

        private void UpdateMeshSelectionWarnings()
        {
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i].sharedMesh != null)
                {
                    bool haveClothComponent = skinnedMeshRenderers[i].gameObject.GetComponent<Cloth>() != null;
                    if (!haveClothComponent && skinnedMeshRenderers[i].sharedMesh.blendShapeCount == 0 && (skinnedMeshRenderers[i].sharedMesh.boneWeights.Length == 0 || skinnedMeshRenderers[i].sharedMesh.bindposes.Length == 0))
                    {
                        invalidMeshWarning ??= new HelpBox("The assigned mesh is missing either bone weights with bind pose, or blend shapes. This might cause the mesh not to render in the Player. If your mesh does not have either bone weights with bind pose, or blend shapes, use a Mesh Renderer instead of Skinned Mesh Renderer.", HelpBoxMessageType.Error);
                        meshField.parent.Insert(1, invalidMeshWarning);
                        return;
                    }
                }
            }
            if (invalidMeshWarning != null)
            {
                meshField.parent.Remove(invalidMeshWarning);
                invalidMeshWarning = null;
            }
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

        #endregion MeshField

        #region Blendshapes

        #region Variables

        private Toggle blendShapesFoldoutToggle;
        private PropertyField blendShapesPropertyField;
        private bool changedBlendShapesWithSlider = false;
        private HelpBox legacyClampBlendShapeWeightsInfo;
        private List<Slider> blendShapeSliders = new();

        #endregion Variables

        private void SetupBlendShapes()
        {
            blendShapesFoldout = root.Q<GroupBox>("BlendShapesFoldout");
            blendShapesFoldoutToggle = blendShapesFoldout.Q<Toggle>("FoldoutToggle");
            customFoldoutSetup.SetupFoldout(blendShapesFoldout);

            blendShapesPropertyField = root.Q<PropertyField>("BlendShapesPropertyField");

            UpdateBlendShapes();

            blendShapesFoldout.schedule.Execute(() => UpdateBlendShapesExistingSlidersIfChangedByCode()).Every(0).ExecuteLater(0); //1000 ms = 1 s
            blendShapesPropertyField.schedule.Execute(() => UpdateBlendShapesPrefabMarkup()).Every(1000).ExecuteLater(1000); //1000 ms = 1 s
        }

        private void UpdateBlendShapesPrefabMarkup()
        {
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                int blendShapeCount = skinnedMeshRenderers[i].sharedMesh == null ? 0 : skinnedMeshRenderers[i].sharedMesh.blendShapeCount;
                if (blendShapeCount == 0)
                {
                    return;
                }
            }

            if (blendShapesPropertyField.Q<Toggle>().ClassListContains(prefabOverrideClass))
                blendShapesFoldoutToggle.AddToClassList(prefabOverrideClass);
            else
                blendShapesFoldoutToggle.RemoveFromClassList(prefabOverrideClass);

            var floatFields = blendShapesPropertyField.Query<FloatField>().ToList();

            if (blendShapeSliders.Count != blendShapeSliders.Count)
            {
                UpdateBlendShapes();
                return;
            }

            for (int i = 0; i < floatFields.Count; i++)
            {
                if (floatFields[i].ClassListContains(prefabOverrideClass))
                    blendShapeSliders[i].AddToClassList(prefabOverrideClass);
                else
                    blendShapeSliders[i].RemoveFromClassList(prefabOverrideClass);
            }
        }

        /// <summary>
        /// This runs every frame to update blend shapes
        /// </summary>
        private void UpdateBlendShapesExistingSlidersIfChangedByCode()
        {
            if (skinnedMeshRenderers.Length > 1) return;

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
               int blendShapeCount = skinnedMeshRenderers[i].sharedMesh == null ? 0 : skinnedMeshRenderers[i].sharedMesh.blendShapeCount;
                if (blendShapeCount == 0)
                {
                    //blendShapesFoldout.style.display = DisplayStyle.None;
                    return;
                }
                if (blendShapeSliders.Count != blendShapeCount)
                {
                    UpdateBlendShapes();
                    return;
                }

                for (int k = 0; k < blendShapeCount; k++)
                {
                    if (blendShapeSliders[k] == null)
                    {
                        UpdateBlendShapes();
                        break;
                    }

                    if (blendShapeSliders[k].value == skinnedMeshRenderers[i].GetBlendShapeWeight(k))
                        continue;

                    if (changedBlendShapesWithSlider)
                        continue;

                    blendShapeSliders[k].SetValueWithoutNotify(skinnedMeshRenderers[i].GetBlendShapeWeight(k));
                }
            }

        }

        private void UpdateBlendShapes()
        {
            if (skinnedMeshRenderers.Length > 1) return;

            int blendShapeCount = skinnedMeshRenderers[0].sharedMesh == null ? 0 : skinnedMeshRenderers[0].sharedMesh.blendShapeCount;
            if (blendShapeCount == 0)
            {
                blendShapesFoldout.style.display = DisplayStyle.None;
                return;
            }

            if (PlayerSettings.legacyClampBlendShapeWeights)
            {
                legacyClampBlendShapeWeightsInfo ??= new HelpBox("Note that BlendShape weight range is clamped.This can be disabled in Player Settings.", HelpBoxMessageType.Error);
                blendShapesFoldout.Q<GroupBox>("Content").Insert(0, legacyClampBlendShapeWeightsInfo);
            }
            else if (legacyClampBlendShapeWeightsInfo != null)
            {
                blendShapesFoldout.Q<GroupBox>("Content").Remove(legacyClampBlendShapeWeightsInfo);
                legacyClampBlendShapeWeightsInfo = null;
            }

            CreateBlendShapesList(blendShapeCount);
        }

        private void CreateBlendShapesList(int blendShapeCount)
        {
            if (skinnedMeshRenderers.Length > 1) return;

            GroupBox blendShapesList = blendShapesFoldout.Q<GroupBox>("BlendShapesList");
            blendShapesList.Clear();
            Mesh m = skinnedMeshRenderers[0].sharedMesh;
            for (int k = 0; k < blendShapeCount; k++)
            {
                int i = k; //this is to cache the value, otherwise it would always call the last value when registerValueChanged calls it
                string blendShapeName = m.GetBlendShapeName(i);

                // Calculate the min and max values for the slider from the frame blendshape weights
                float sliderMin = 0f, sliderMax = 0f;

                int frameCount = m.GetBlendShapeFrameCount(i);
                for (int j = 0; j < frameCount; j++)
                {
                    float frameWeight = m.GetBlendShapeFrameWeight(i, j);
                    sliderMin = Mathf.Min(frameWeight, sliderMin);
                    sliderMax = Mathf.Max(frameWeight, sliderMax);
                }

                // The SkinnedMeshRenderer blendshape weights array size can be out of sync with the size defined in the mesh
                // (default values in that case are 0)
                // The desired behaviour is to resize the blendshape array on edit.

                Slider slider = new Slider(sliderMin, sliderMax);
                slider.label = blendShapeName;
                slider.showInputField = true;
                blendShapesList.Add(slider);
                blendShapeSliders.Add(slider);

                slider.value = skinnedMeshRenderers[0].GetBlendShapeWeight(i);
                slider.RegisterValueChangedCallback(e =>
                {
                    changedBlendShapesWithSlider = true;
                    skinnedMeshRenderers[0].SetBlendShapeWeight(i, slider.value);
                });

                //// Default path when the blend shape array size is big enough.
                //if (i < arraySize)
                //{
                //    //EditorGUILayout.Slider(m_BlendShapeWeights.GetArrayElementAtIndex(i), sliderMin, sliderMax, float.MinValue, float.MaxValue, content);
                //}
                //// Fall back to 0 based editing &
                //else
                //{
                //    //    EditorGUI.BeginChangeCheck();

                //    //float value = EditorGUILayout.Slider(content, 0f, sliderMin, sliderMax, float.MinValue, float.MaxValue);
                //    //    if (EditorGUI.EndChangeCheck())
                //    //    {
                //    //        m_BlendShapeWeights.arraySize = blendShapeCount;
                //    //        arraySize = blendShapeCount;
                //    //        m_BlendShapeWeights.GetArrayElementAtIndex(i).floatValue = value;
                //    //    }
                //}
            }
        }

        #endregion Blendshapes

        private void SetupAdditionalSettings()
        {
            var additionalSettingsFoldout = root.Q<GroupBox>("AdditionalSettingsFoldout");
            customFoldoutSetup.SetupFoldout(additionalSettingsFoldout);
        }

        private void SetupLightingFoldout()
        {
            var lightingFoldout = root.Q<GroupBox>("LightningFoldout");
            customFoldoutSetup.SetupFoldout(lightingFoldout);
        }

        private void SetupDebugFoldout()
        {
            var debugFoldout = root.Q<GroupBox>("DebugFoldout");
            customFoldoutSetup.SetupFoldout(debugFoldout);

            var debugPropertiesFoldout = debugFoldout.Q<GroupBox>("DebugPropertiesFoldout");
            customFoldoutSetup.SetupFoldout(debugPropertiesFoldout);

            var content = debugPropertiesFoldout.Q<GroupBox>("Content");

            //Setting content disabled wasn't applying that disabled visual
            //content.SetEnabled(false);
            foreach (var child in content.Children())
            {
                child.SetEnabled(false);
            }

            var debugGizmosFoldout = debugFoldout.Q<GroupBox>("MeshDebugFoldout");
            customFoldoutSetup.SetupFoldout(debugGizmosFoldout);

            UpdateDebugFoldoutVisibility();
        }

        private void UpdateDebugFoldoutVisibility()
        {
            if (editorSettings.ShowDebugGizmoFoldout)
                root.Q<GroupBox>("DebugFoldout").style.display = DisplayStyle.Flex;
            else
                root.Q<GroupBox>("DebugFoldout").style.display = DisplayStyle.None;
        }

        #region Bounding Probes Volume

        private void SetupBoundingVolume()
        {
            var boundingBoxFoldout = root.Q<GroupBox>("BoundingBoxFoldout");
            customFoldoutSetup.SetupFoldout(boundingBoxFoldout);

            Button editBoundingVolumeButton = boundingBoxFoldout.Q<Button>("EditBoundingVolumeButton");
            editBoundingVolumeButton.tooltip = boundingVolumeEditButtonTooltip;

            if (EditMode.editMode == EditMode.SceneViewEditMode.None)
                editBoundingVolumeButton.RemoveFromClassList("toggledOnButton");
            else
                editBoundingVolumeButton.AddToClassList("toggledOnButton");

            editBoundingVolumeButton.clicked += () =>
            {
                if (EditMode.editMode == EditMode.SceneViewEditMode.None)
                {
                    EditMode.ChangeEditMode(EditMode.SceneViewEditMode.Collider, skinnedMeshRenderers[0].bounds, this);
                    editBoundingVolumeButton.AddToClassList("toggledOnButton");
                }
                else
                {
                    EditMode.ChangeEditMode(EditMode.SceneViewEditMode.None, skinnedMeshRenderers[0].bounds, this);
                    editBoundingVolumeButton.RemoveFromClassList("toggledOnButton");
                }
            };

            Button autoFitBoundsButton = boundingBoxFoldout.Q<Button>("AutoFitBoundsButton");
            autoFitBoundsButton.clicked += () =>
             {
                 Undo.RecordObject(skinnedMeshRenderers[0], "Auto-Fit Bounds");
                 //targetSkinnedMeshRenderer.localBounds = targetSkinnedMeshRenderer.sharedMesh != null ? targetSkinnedMeshRenderer.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);

                 skinnedMeshRenderers[0].updateWhenOffscreen = true;
                 var fitBounds = skinnedMeshRenderers[0].localBounds;
                 skinnedMeshRenderers[0].updateWhenOffscreen = false;
                 skinnedMeshRenderers[0].localBounds = fitBounds;
             };

            boundsGroupBox = boundingBoxFoldout.Q<GroupBox>("BoundsGroupBox");
            aabb = root.Q<PropertyField>("AABBBindingField");
            Vector3Field boundsCenter = boundsGroupBox.Q<Vector3Field>("BoundsCenter");
            Vector3Field boundsExtent = boundsGroupBox.Q<Vector3Field>("BoundsExtentField");
            boundsCenter.SetValueWithoutNotify(skinnedMeshRenderers[0].localBounds.center);
            boundsExtent.SetValueWithoutNotify(skinnedMeshRenderers[0].localBounds.extents);
            UpdateBoundsGroupBoxContextMenu();

            aabb.RegisterValueChangeCallback(e =>
            {
                boundsCenter.SetValueWithoutNotify(skinnedMeshRenderers[0].localBounds.center);
                boundsExtent.SetValueWithoutNotify(skinnedMeshRenderers[0].localBounds.extents);
                aabb.schedule.Execute(() => UpdateBoundsFieldPrefabOverride()).ExecuteLater(1000); //1000 ms = 1 s
                UpdateBoundsGroupBoxContextMenu();
            });

            boundsCenter.RegisterValueChangedCallback(e =>
            {
                Undo.RecordObject(skinnedMeshRenderers[0], "Modified bounds center in " + skinnedMeshRenderers[0].gameObject.name);
                var bounds = skinnedMeshRenderers[0].localBounds;
                bounds.center = e.newValue;
                skinnedMeshRenderers[0].localBounds = bounds;
                EditorUtility.SetDirty(skinnedMeshRenderers[0]);
            });
            boundsExtent.RegisterValueChangedCallback(e =>
            {
                Undo.RecordObject(skinnedMeshRenderers[0], "Modified bounds extents in " + skinnedMeshRenderers[0].gameObject.name);
                var bounds = skinnedMeshRenderers[0].localBounds;
                bounds.extents = e.newValue;
                skinnedMeshRenderers[0].localBounds = bounds;
                EditorUtility.SetDirty(skinnedMeshRenderers[0]);
            });
            aabb.schedule.Execute(() => UpdateBoundsFieldPrefabOverride()).ExecuteLater(1000); //1000 ms = 1 s
        }

        private ContextualMenuManipulator contextualMenuManipulatorForBoundsGroupBox;

        /// <summary>
        /// The right click menu
        /// </summary>
        private void UpdateBoundsGroupBoxContextMenu()
        {
            //Remove the old context menu
            if (contextualMenuManipulatorForBoundsGroupBox != null)
                boundsGroupBox.RemoveManipulator(contextualMenuManipulatorForBoundsGroupBox);

            if (targets.Length > 1) return;

            UpdateContextMenuForBoundsGroupBox();

            boundsGroupBox.AddManipulator(contextualMenuManipulatorForBoundsGroupBox);

            void UpdateContextMenuForBoundsGroupBox()
            {
                contextualMenuManipulatorForBoundsGroupBox = new ContextualMenuManipulator((evt) =>
                {
                    evt.menu.AppendAction("Copy property path", (x) => CopyBoundsPropertyPath(), DropdownMenuAction.AlwaysEnabled);
                    evt.menu.AppendSeparator();

                    if (HasPrefabOverride_bounds())
                    {
                        GameObject prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(skinnedMeshRenderers[0].gameObject);
                        string prefabName = prefab ? " to '" + prefab.name + "'" : "";

                        evt.menu.AppendAction("Apply to Prefab" + prefabName, (x) => ApplyChangesToPrefab_bounds(), DropdownMenuAction.AlwaysEnabled);
                        evt.menu.AppendAction("Revert", (x) => RevertChanges_bounds(), DropdownMenuAction.AlwaysEnabled);
                    }

                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Copy", (x) => Copy(), DropdownMenuAction.AlwaysEnabled);

                    GetBoundsFromCopyBuffer(out bool exists, out float x, out float y, out float z, out float a, out float b, out float c);

                    if (exists)
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysEnabled);
                    else
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysDisabled);
                });
            }
            void CopyBoundsPropertyPath()
            {
                EditorGUIUtility.systemCopyBuffer = "m_AABB";
            }
            void Copy()
            {
                var b = skinnedMeshRenderers[0].localBounds;
                char valueSeparators = ',';
                EditorGUIUtility.systemCopyBuffer = "Bounds(" + b.center.x + valueSeparators + b.center.y + valueSeparators + b.center.z + valueSeparators + b.extents.x + valueSeparators + b.extents.y + valueSeparators + b.extents.z + ")";
            }
            void Paste()
            {
                GetBoundsFromCopyBuffer(out bool exists, out float x, out float y, out float z, out float a, out float b, out float c);
                skinnedMeshRenderers[0].localBounds = new(new Vector3(x, y, z), new Vector3(a, b, c));
            }

            bool HasPrefabOverride_bounds()
            {
                var soTarget = new SerializedObject(target);

                if (soTarget.FindProperty("m_AABB").prefabOverride)
                    return true;
                else
                    return false;
            }

            void ApplyChangesToPrefab_bounds()
            {
                if (!HasPrefabOverride_bounds())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.ApplyPropertyOverride(soTarget.FindProperty("m_AABB"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(skinnedMeshRenderers[0].transform), InteractionMode.UserAction);
            }

            void RevertChanges_bounds()
            {
                if (!HasPrefabOverride_bounds())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.RevertPropertyOverride(soTarget.FindProperty("m_AABB"), InteractionMode.UserAction);
            }
        }

        private void GetBoundsFromCopyBuffer(out bool exists, out float x, out float y, out float z, out float a, out float b, out float c)
        {
            exists = false;
            x = 0; y = 0; z = 0;
            a = 0; b = 0; c = 0;

            string copyBuffer = EditorGUIUtility.systemCopyBuffer;
            if (copyBuffer != null)
            {
                if (copyBuffer.Contains("Bounds"))
                {
                    if (copyBuffer.Length > 17)
                    {
                        copyBuffer = copyBuffer.Substring(7, copyBuffer.Length - 8);
                        string[] valueStrings = copyBuffer.Split(',');

                        if (valueStrings.Length == 6)
                        {
                            char userDecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                            string sanitizedValueString_x = valueStrings[0].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                            if (float.TryParse(sanitizedValueString_x, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out x))
                                exists = true;

                            if (exists)
                            {
                                string sanitizedValueString_y = valueStrings[1].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                                if (!float.TryParse(sanitizedValueString_y, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out y))
                                    exists = false;
                            }

                            if (exists)
                            {
                                string sanitizedValueString_z = valueStrings[2].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                                if (!float.TryParse(sanitizedValueString_z, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out z))
                                    exists = false;
                            }

                            if (exists)
                            {
                                string sanitizedValueString_z = valueStrings[3].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                                if (!float.TryParse(sanitizedValueString_z, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out a))
                                    exists = false;
                            }

                            if (exists)
                            {
                                string sanitizedValueString_z = valueStrings[4].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                                if (!float.TryParse(sanitizedValueString_z, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out b))
                                    exists = false;
                            }

                            if (exists)
                            {
                                string sanitizedValueString_z = valueStrings[5].Replace(userDecimalSeparator == ',' ? '.' : ',', userDecimalSeparator);
                                if (!float.TryParse(sanitizedValueString_z, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out x))
                                    exists = false;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateBoundsFieldPrefabOverride()
        {
            var label = aabb.Q<Label>();
            if (label == null) return;

            if (label.ClassListContains(prefabOverrideClass))
                boundsGroupBox.AddToClassList(prefabOverrideClass);
            else
                boundsGroupBox.RemoveFromClassList(prefabOverrideClass);
        }

        #region Probes Foldout

        private IntegerField lightProbeBindingField;
        private EnumField lightProbeEnumField;
        private PropertyField lightProbeVolumeOverrideField;
        private ObjectField anchorOverrideField;
        private HelpBox invalidLightProbeWarning;

        private IntegerField reflectionProbeBindingField;
        private EnumField reflectionProbeEnumField;

        private readonly string prefabOverrideClass = "unity-binding--prefab-override";

        private void SetupProbesFoldout()
        {
            var probesFoldout = root.Q<GroupBox>("ProbesFoldout");
            customFoldoutSetup.SetupFoldout(probesFoldout);

            lightProbeBindingField = probesFoldout.Q<IntegerField>("LightProbeBindingField");
            lightProbeEnumField = probesFoldout.Q<EnumField>("LightProbeEnumField");
            lightProbeEnumField.SetValueWithoutNotify(skinnedMeshRenderers[0].lightProbeUsage);

            lightProbeVolumeOverrideField = probesFoldout.Q<PropertyField>("LightProbeVolumeOverrideField");
            anchorOverrideField = probesFoldout.Q<ObjectField>("AnchorOverrideField");

            lightProbeEnumField.RegisterValueChangedCallback(e =>
            {
                skinnedMeshRenderers[0].lightProbeUsage = (UnityEngine.Rendering.LightProbeUsage)e.newValue;
            });

            lightProbeEnumField.schedule.Execute(() => RegisterLightProbe()).ExecuteLater(0);

            lightProbeBindingField.schedule.Execute(() => UpdateLightingProbePrefabOverride()).ExecuteLater(1000); //1000 ms = 1 s

            lightProbeVolumeOverrideField.RegisterValueChangeCallback(e =>
            {
                UpdateLightProbeProxyVolumeWarning();
            });

            UpdateAnchorFieldVisibility();
            UpdateLightProbeProxyVolumeWarning();

            UpdateLightProbeContextMenu();

            reflectionProbeBindingField = probesFoldout.Q<IntegerField>("ReflectionProbeBindingField");
            reflectionProbeEnumField = probesFoldout.Q<EnumField>("ReflectionProbeEnumField");
            reflectionProbeEnumField.SetValueWithoutNotify(skinnedMeshRenderers[0].reflectionProbeUsage);

            UpdateReflectionProbeContextMenu();

            reflectionProbeEnumField.RegisterValueChangedCallback(e =>
            {
                skinnedMeshRenderers[0].reflectionProbeUsage = (UnityEngine.Rendering.ReflectionProbeUsage)e.newValue;
            });

            lightProbeEnumField.schedule.Execute(() => RegisterReflectionProbe()).ExecuteLater(0);
            reflectionProbeBindingField.schedule.Execute(() => UpdateReflectionProbePrefabOverride()).ExecuteLater(1); //1000 ms = 1 s
            reflectionProbeBindingField.schedule.Execute(() => UpdateReflectionProbePrefabOverride()).ExecuteLater(1000); //1000 ms = 1 s
        }

        private void RegisterReflectionProbe()
        {
            reflectionProbeBindingField.RegisterValueChangedCallback(e =>
            {
                UpdateAnchorFieldVisibility();
                reflectionProbeEnumField.SetValueWithoutNotify(skinnedMeshRenderers[0].reflectionProbeUsage);
                reflectionProbeBindingField.schedule.Execute(() => UpdateReflectionProbePrefabOverride()).ExecuteLater(500); //1000 ms = 1 s
                reflectionProbeBindingField.schedule.Execute(() => UpdateReflectionProbePrefabOverride()).ExecuteLater(2000); //1000 ms = 1 s
                reflectionProbeBindingField.schedule.Execute(() => UpdateReflectionProbePrefabOverride()).ExecuteLater(10000); //1000 ms = 1 s //Seems unnecessary but was required one time.
                UpdateReflectionProbeContextMenu();
            });
        }

        private void RegisterLightProbe()
        {
            lightProbeBindingField.RegisterValueChangedCallback(e =>
            {
                UpdateAnchorFieldVisibility();
                lightProbeEnumField.SetValueWithoutNotify(skinnedMeshRenderers[0].lightProbeUsage);
                UpdateLightProbeProxyVolumeWarning();
                lightProbeBindingField.schedule.Execute(() => UpdateLightingProbePrefabOverride()).ExecuteLater(500); //1000 ms = 1 s
                lightProbeBindingField.schedule.Execute(() => UpdateLightingProbePrefabOverride()).ExecuteLater(2000); //1000 ms = 1 s
                UpdateLightProbeContextMenu();
            });
        }

        private ContextualMenuManipulator contextualMenuManipulatorForLightProbeEnumField;

        /// <summary>
        /// The right click menu
        /// </summary>
        private void UpdateLightProbeContextMenu()
        {
            //Remove the old context menu
            if (contextualMenuManipulatorForLightProbeEnumField != null)
                lightProbeEnumField.RemoveManipulator(contextualMenuManipulatorForLightProbeEnumField);

            if (targets.Length > 1) return;

            UpdateContextMenuForLightProbeField();

            lightProbeEnumField.AddManipulator(contextualMenuManipulatorForLightProbeEnumField);

            void UpdateContextMenuForLightProbeField()
            {
                contextualMenuManipulatorForLightProbeEnumField = new ContextualMenuManipulator((evt) =>
                {
                    evt.menu.AppendAction("Copy property path", (x) => CopyPropertyPath(), DropdownMenuAction.AlwaysEnabled);
                    evt.menu.AppendSeparator();

                    if (HasPrefabOverride_lightProbe())
                    {
                        GameObject prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(skinnedMeshRenderers[0].gameObject);
                        string prefabName = prefab ? " to '" + prefab.name + "'" : "";

                        evt.menu.AppendAction("Apply to Prefab" + prefabName, (x) => ApplyChangesToPrefab_lightProbe(), DropdownMenuAction.AlwaysEnabled);
                        evt.menu.AppendAction("Revert", (x) => RevertChanges_lightProbe(), DropdownMenuAction.AlwaysEnabled);
                    }

                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Copy", (x) => Copy(), DropdownMenuAction.AlwaysEnabled);

                    if (HasValidValueForPaste())
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysEnabled);
                    else
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysDisabled);
                });
            }
            void CopyPropertyPath()
            {
                EditorGUIUtility.systemCopyBuffer = "m_LightProbeUsage";
            }
            void Copy()
            {
                EditorGUIUtility.systemCopyBuffer = ((int)skinnedMeshRenderers[0].lightProbeUsage).ToString();
            }
            void Paste()
            {
                if (HasValidValueForPaste())
                {
                    var value = EditorGUIUtility.systemCopyBuffer;
                    if (value == null) return;

                    if (int.TryParse(value, out int intValue))
                    {
                        if (Enum.IsDefined(typeof(LightProbeUsage), intValue))
                        {
                            skinnedMeshRenderers[0].lightProbeUsage = (UnityEngine.Rendering.LightProbeUsage)intValue;
                        }
                    }
                }
            }

            bool HasValidValueForPaste()
            {
                if (EditorGUIUtility.systemCopyBuffer == null) return false;

                if (IsValidEnumValue<LightProbeUsage>(EditorGUIUtility.systemCopyBuffer))
                {
                    return true;
                }

                return false;
            }

            bool HasPrefabOverride_lightProbe()
            {
                var soTarget = new SerializedObject(target);

                if (soTarget.FindProperty("m_LightProbeUsage").prefabOverride)
                    return true;
                else
                    return false;
            }

            void ApplyChangesToPrefab_lightProbe()
            {
                if (!HasPrefabOverride_lightProbe())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.ApplyPropertyOverride(soTarget.FindProperty("m_LightProbeUsage"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(skinnedMeshRenderers[0].transform), InteractionMode.UserAction);
            }

            void RevertChanges_lightProbe()
            {
                if (!HasPrefabOverride_lightProbe())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.RevertPropertyOverride(soTarget.FindProperty("m_LightProbeUsage"), InteractionMode.UserAction);
            }
        }

        private ContextualMenuManipulator contextualMenuManipulatorForReflectionProbeEnumField;

        /// The right click menu
        /// </summary>
        private void UpdateReflectionProbeContextMenu()
        {
            //Remove the old context menu
            if (contextualMenuManipulatorForReflectionProbeEnumField != null)
                reflectionProbeEnumField.RemoveManipulator(contextualMenuManipulatorForReflectionProbeEnumField);

            if (targets.Length > 1) return;

            UpdateContextMenuForReflectionProbeField();

            reflectionProbeEnumField.AddManipulator(contextualMenuManipulatorForReflectionProbeEnumField);

            void UpdateContextMenuForReflectionProbeField()
            {
                contextualMenuManipulatorForReflectionProbeEnumField = new ContextualMenuManipulator((evt) =>
                {
                    evt.menu.AppendAction("Copy property path", (x) => CopyPropertyPath(), DropdownMenuAction.AlwaysEnabled);
                    evt.menu.AppendSeparator();

                    if (HasPrefabOverride_reflectionProbe())
                    {
                        GameObject prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(skinnedMeshRenderers[0].gameObject);
                        string prefabName = prefab ? " to '" + prefab.name + "'" : "";

                        evt.menu.AppendAction("Apply to Prefab" + prefabName, (x) => ApplyChangesToPrefab_reflectionProbe(), DropdownMenuAction.AlwaysEnabled);
                        evt.menu.AppendAction("Revert", (x) => RevertChanges_reflectionProbe(), DropdownMenuAction.AlwaysEnabled);
                    }

                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Copy", (x) => Copy(), DropdownMenuAction.AlwaysEnabled);

                    if (HasValidValueForPaste())
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysEnabled);
                    else
                        evt.menu.AppendAction("Paste", (x) => Paste(), DropdownMenuAction.AlwaysDisabled);
                });
            }
            void CopyPropertyPath()
            {
                EditorGUIUtility.systemCopyBuffer = "m_ReflectionProbeUsage";
            }
            void Copy()
            {
                EditorGUIUtility.systemCopyBuffer = ((int)skinnedMeshRenderers[0].reflectionProbeUsage).ToString();
            }
            void Paste()
            {
                if (HasValidValueForPaste())
                {
                    var value = EditorGUIUtility.systemCopyBuffer;
                    if (value == null) return;

                    if (int.TryParse(value, out int intValue))
                    {
                        if (Enum.IsDefined(typeof(ReflectionProbeUsage), intValue))
                        {
                            skinnedMeshRenderers[0].reflectionProbeUsage = (UnityEngine.Rendering.ReflectionProbeUsage)intValue;
                        }
                    }
                }
            }

            bool HasValidValueForPaste()
            {
                if (EditorGUIUtility.systemCopyBuffer == null) return false;

                if (IsValidEnumValue<ReflectionProbeUsage>(EditorGUIUtility.systemCopyBuffer))
                {
                    return true;
                }

                return false;
            }

            bool HasPrefabOverride_reflectionProbe()
            {
                var soTarget = new SerializedObject(target);

                if (soTarget.FindProperty("m_ReflectionProbeUsage").prefabOverride)
                    return true;
                else
                    return false;
            }

            void ApplyChangesToPrefab_reflectionProbe()
            {
                if (!HasPrefabOverride_reflectionProbe())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.ApplyPropertyOverride(soTarget.FindProperty("m_ReflectionProbeUsage"), PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(skinnedMeshRenderers[0].transform), InteractionMode.UserAction);
            }

            void RevertChanges_reflectionProbe()
            {
                if (!HasPrefabOverride_reflectionProbe())
                    return;

                var soTarget = new SerializedObject(target);
                PrefabUtility.RevertPropertyOverride(soTarget.FindProperty("m_ReflectionProbeUsage"), InteractionMode.UserAction);
            }
        }

        private void UpdateLightingProbePrefabOverride()
        {
            if (lightProbeBindingField.ClassListContains(prefabOverrideClass))
                lightProbeEnumField.AddToClassList(prefabOverrideClass);
            else
                lightProbeEnumField.RemoveFromClassList(prefabOverrideClass);
        }

        private void UpdateReflectionProbePrefabOverride()
        {
            if (reflectionProbeBindingField.ClassListContains(prefabOverrideClass))
                reflectionProbeEnumField.AddToClassList(prefabOverrideClass);
            else
                reflectionProbeEnumField.RemoveFromClassList(prefabOverrideClass);
        }

        private void UpdateAnchorFieldVisibility()
        {
            if (skinnedMeshRenderers[0].lightProbeUsage == UnityEngine.Rendering.LightProbeUsage.Off && skinnedMeshRenderers[0].reflectionProbeUsage == UnityEngine.Rendering.ReflectionProbeUsage.Off)
                anchorOverrideField.style.display = DisplayStyle.None;
            else
                anchorOverrideField.style.display = DisplayStyle.Flex;

            if (skinnedMeshRenderers[0].lightProbeUsage == UnityEngine.Rendering.LightProbeUsage.UseProxyVolume)
                lightProbeVolumeOverrideField.style.display = DisplayStyle.Flex;
            else
                lightProbeVolumeOverrideField.style.display = DisplayStyle.None;
        }

        private void UpdateLightProbeProxyVolumeWarning()
        {
            if (skinnedMeshRenderers[0].lightProbeUsage == UnityEngine.Rendering.LightProbeUsage.UseProxyVolume
                && (skinnedMeshRenderers[0].lightProbeProxyVolumeOverride == null
                || skinnedMeshRenderers[0].lightProbeProxyVolumeOverride.GetComponent<LightProbeProxyVolume>() == null))
            {
                if (invalidLightProbeWarning != null)
                {
                    if (invalidLightProbeWarning.parent != null)
                        invalidLightProbeWarning.parent.Remove(invalidLightProbeWarning);
                }

                invalidLightProbeWarning = new HelpBox("A valid Light Probe Proxy Volume component could not be found", HelpBoxMessageType.Warning);
                lightProbeVolumeOverrideField.parent.Insert(lightProbeVolumeOverrideField.parent.IndexOf(lightProbeVolumeOverrideField) + 1, invalidLightProbeWarning);
            }
            else
            {
                if (invalidLightProbeWarning != null)
                {
                    if (invalidLightProbeWarning.parent != null)
                        invalidLightProbeWarning.parent.Remove(invalidLightProbeWarning);

                    invalidLightProbeWarning = null;
                }
            }
            //lightProbeVolumeOverrideField
        }

        #endregion Probes Foldout

        #endregion Bounding Probes Volume

        #region UI

        private void UpdateMeshFieldGroupPosition()
        {
            GroupBox meshFieldGroupBox = root.Q<GroupBox>("MeshFieldGroupBox");
            if (editorSettings.meshFieldOnTop)
                root.Q<GroupBox>("RootHolder").Insert(2, meshFieldGroupBox);
            else
                root.Q<VisualElement>("MainContainer").Insert(0, meshFieldGroupBox);
        }

        #endregion UI

        /// <summary>
        /// If the UXML file is missing for any reason,
        /// Instead of showing an empty inspector,
        /// This loads the default one.
        /// This shouldn't ever happen.
        /// </summary>
        private void LoadDefaultEditor(VisualElement container)
        {
            if (originalEditor != null)
                DestroyImmediate(originalEditor);

            //originalEditor = Editor.CreateEditor(targets);
            originalEditor = CreateEditor(targets, typeof(Editor).Assembly.GetType("UnityEditor.SkinnedMeshRendererEditor"));
            IMGUIContainer inspectorContainer = new IMGUIContainer(OnGUICallback);
            container.Add(inspectorContainer);
        }

        //For the original Editor
        private void OnGUICallback()
        {
            //EditorGUIUtility.hierarchyMode = true;

            EditorGUI.BeginChangeCheck();
            originalEditor.OnInspectorGUI();
            EditorGUI.EndChangeCheck();
        }

        private void CleanUp()
        {
            if (previewManager != null)
            {
                previewManager.CleanUp();
                previewManager = null;
            }

            // Clean up
            if (originalEditor != null)
                DestroyImmediate(originalEditor);

            if (debugGizmoManager != null)
                debugGizmoManager.Cleanup();
        }

        #region Settings

        private void OpenContextMenu_settingsButton()
        {
            UpdateContextMenu_settingsButton();
            settingsButtonContextMenu.DropDown(GetMenuRect(settingsButton), settingsButton, true);
        }

        private void UpdateContextMenu_settingsButton()
        {
            settingsButtonContextMenu = new GenericDropdownMenu();

            if (editorSettings.showDefaultSkinnedMeshRendererInspector)
                settingsButtonContextMenu.AddItem("Default Inspector", true, () =>
                {
                    editorSettings.showDefaultSkinnedMeshRendererInspector = false;
                    UpdateInspectorOverride();
                });
            else
                settingsButtonContextMenu.AddItem("Default Inspector", false, () =>
                {
                    editorSettings.showDefaultSkinnedMeshRendererInspector = true;
                    UpdateInspectorOverride();
                });

            settingsButtonContextMenu.AddSeparator("");

            bool isChecked = settingsFoldoutManager.IsInspectorSettingsIsHidden() ? false : true;
            settingsButtonContextMenu.AddItem("Open Settings", isChecked, () =>
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

        #endregion Settings

        private bool IsValidEnumValue<TEnum>(string input) where TEnum : struct, Enum
        {
            if (int.TryParse(input, out int intValue))
            {
                return Enum.IsDefined(typeof(TEnum), intValue);
            }

            return false;
        }

        #region Gizmo

        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        private void OnSceneGUI()
        {
            if (!target)
                return;





            SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)target;

            if (renderer.updateWhenOffscreen)
            {
                Bounds bounds = renderer.bounds;
                Vector3 center = bounds.center;
                Vector3 size = bounds.size;

                Handles.DrawWireCube(center, size);
            }
            else
            {
                //using (new Handles.DrawingScope(renderer.actualRootBone.localToWorldMatrix))
                using (new Handles.DrawingScope(renderer.rootBone.localToWorldMatrix))
                {
                    Bounds bounds = renderer.localBounds;
                    m_BoundsHandle.center = bounds.center;
                    m_BoundsHandle.size = bounds.size;

                    // only display interactive handles if edit mode is active
                    if (EditMode.editMode == EditMode.SceneViewEditMode.Collider)
                    {
                        Handles.color = new Color(0, 0.75f, 1, 1f);
                        m_BoundsHandle.wireframeColor = Color.white;
                        m_BoundsHandle.handleColor = m_BoundsHandle.wireframeColor;

                        EditorGUI.BeginChangeCheck();
                        m_BoundsHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(renderer, "Resize Bounds");
                            renderer.localBounds = new Bounds(m_BoundsHandle.center, m_BoundsHandle.size);
                        }

                        Handles.color = new Color(0, 0.75f, 1, 0.05f);
                        float pulse = 1 + Mathf.Sin((float)EditorApplication.timeSinceStartup * 5f) * 0.05f;
                        Vector3 pulseSize = m_BoundsHandle.size * pulse;
                        Handles.DrawWireCube(m_BoundsHandle.center, pulseSize);

                        Handles.color = new Color(0, 0.75f, 1, 0.2f);
                        Handles.DrawWireCube(m_BoundsHandle.center, m_BoundsHandle.size * (1 + Mathf.Sin((float)EditorApplication.timeSinceStartup * 5f) * 0.025f));

                        SceneView.RepaintAll(); // force refresh
                    }
                    else
                    {
                        m_BoundsHandle.wireframeColor = new Color(1, 1, 1, 0.5f);
                        m_BoundsHandle.handleColor = Color.clear;
                        m_BoundsHandle.DrawHandle();
                    }
                }
            }


            debugGizmoManager?.DrawGizmo(skinnedMeshRenderers);
        }

        #endregion Gizmo
    }
}