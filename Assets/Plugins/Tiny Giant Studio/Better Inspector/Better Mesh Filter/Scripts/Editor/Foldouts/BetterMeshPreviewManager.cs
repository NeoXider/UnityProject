using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    /// <summary>
    /// The editors pass mesh to this and this script handles creating and cleaning up previews.
    /// </summary>
    public class BetterMeshPreviewManager
    {
        // URL to documentation or help page related to runtime memory usage
        private string learnMoreAboutRuntimeMemoryUsageLink = "https://ferdowsur.gitbook.io/better-mesh/full-feature-list/runtime-memory-size";

        // Stores a list of mesh preview data used for drawing previews in the inspector. This is used to clean up memory when the inspector is deselected.
        private List<MeshPreview> meshPreviews = new();

        // UXML template used for generating each mesh preview UI
        private VisualTreeAsset previewTemplate;

        // Path to the UXML file for mesh previews
        private readonly string previewTemplateLocation = "Assets/Plugins/Tiny Giant Studio/Better Inspector/Better Mesh Filter/Scripts/Editor/Templates/MeshPreview.uxml";

        // Custom GUIStyle used when drawing elements with colored backgrounds
        private GUIStyle style;

        // Total height allocated for the mesh previews
        private float previewHeight;

        // Root container of the UI;
        private VisualElement container;

        // Contains the IMGUI preview boxes
        private VisualElement previewsGroupBox;

        private GroupBox allSelectedMeshCombinedDetails;
        private Label maxPreviewCount;

        /// <summary>
        /// Container for the IMGUI preview section. Its height can be resized using the drag handle.
        /// </summary>
        private VisualElement imguiPreviewContainer;

        // Drag handle UI element for resizing the IMGUI preview container
        private VisualElement _dragHandle;

        private GroupBox informationFoldout;
        private GroupBox assetLocationInInformationFoldoutGroupBox;

        // Stores reference to the current user settings for Better Mesh
        private BetterMeshSettings editorSettings;

        /// <summary>
        /// Frees up allocated memory and UI elements when the editor window is closed.
        /// Must be called during tear-down to prevent memory leaks.
        /// </summary>
        public void CleanUp()
        {
            // Clear UI hierarchy if initialized
            if (previewsGroupBox != null)
                previewsGroupBox.Clear();

            // Dispose of all mesh preview elements
            for (int i = 0; i < meshPreviews.Count; i++)
            {
                meshPreviews[i]?.Dispose();
            }

            // Clear the list to release references
            meshPreviews.Clear();
        }

        public BetterMeshPreviewManager(CustomFoldoutSetup customFoldoutSetup, BetterMeshSettings editorSettings, VisualElement root, VisualTreeAsset meshPreviewTemplate)
        {
            this.editorSettings = editorSettings;

            container = root.Q<TemplateContainer>("MeshPreviewContainers");
            previewsGroupBox = container.Q<ScrollView>("PreviewsGroupBox");

            if (meshPreviewTemplate != null)
            {
                previewTemplate = meshPreviewTemplate;
            }
            else
            {
                previewTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(previewTemplateLocation);
                if (previewTemplate == null)
                {
                    var objects = Resources.FindObjectsOfTypeAll(typeof(VisualTreeAsset));
                    foreach (var obj in objects)
                    {
                        var v = (VisualTreeAsset)obj;
                        if (v.name == "MeshPreview")
                        {
                            previewTemplate = v;
                            break;
                        }
                    }
                }
            }

            // Initialize preview style with background color from settings
            style ??= new GUIStyle { normal = { background = BackgroundTexture(BetterMeshSettings.instance.PreviewBackgroundColor) } };
            previewHeight = GetMeshPreviewHeight();

            // Setup background color field UI and hook into color change callback
            ColorField previewColor = previewsGroupBox.parent.Q<ColorField>("PreviewColorField");
            previewColor.value = editorSettings.PreviewBackgroundColor;
            previewColor.RegisterValueChangedCallback(ev =>
            {
                editorSettings.PreviewBackgroundColor = ev.newValue;
                style.normal.background = BackgroundTexture(ev.newValue);
            });

            allSelectedMeshCombinedDetails = container.Q<GroupBox>("AllSelectedMeshCombinedDetails");
            maxPreviewCount = allSelectedMeshCombinedDetails.Q<Label>("MaxPreviewCount");

            informationFoldout = root.Q<GroupBox>("Informations");
            customFoldoutSetup.SetupFoldout(informationFoldout);

            assetLocationInInformationFoldoutGroupBox = informationFoldout.Q<GroupBox>("AssetLocationInFoldoutGroupBox");
            informationFoldout.Q<Button>("RuntimeMemoryUsageExplaination").clicked += () => { Application.OpenURL(learnMoreAboutRuntimeMemoryUsageLink); };

        }

        private void UpdateInformationFoldout(List<Mesh> meshes)
        {
            if (!editorSettings.ShowInformationFoldout)
            {
                informationFoldout.style.display = DisplayStyle.None;
                return;
            }

            informationFoldout.style.display = DisplayStyle.Flex;

            UpdateMeshDataGroup(informationFoldout.Q<TemplateContainer>("MeshDataGroup"), meshes, true);

            assetLocationInInformationFoldoutGroupBox.style.display = editorSettings.ShowAssetLocationInFoldout ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void HideInformationFoldout()
        {
            informationFoldout.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Sets up the preview manager with settings and UI elements, but does not generate the actual mesh previews.
        /// </summary>
        /// <param name="meshes">The list of meshes to preview.</param>
        /// <param name="container">The VisualElement that will host the previews.</param>
        public void SetupPreviewManager(List<Mesh> meshes, int targetCount)
        {
            Label TotalSelectionCount = container.Q<Label>("TotalSelectionCount");
            if (targetCount > 0)
            {
                TotalSelectionCount.style.display = DisplayStyle.Flex;
                TotalSelectionCount.text = "" + targetCount + " targets selected";
            }
            else
            {
                TotalSelectionCount.style.display = DisplayStyle.None;
            }

            SetupHeightResizeDragHandles(meshes);
            CreatePreviews(meshes);
        }

        private void SetupHeightResizeDragHandles(List<Mesh> meshes)
        {
            _dragHandle = previewsGroupBox.parent.Q<VisualElement>("DragHandle");

            if (meshes.Count > 1)
            {
                // Hide drag handle for multi-mesh view (fixed size layout)
                _dragHandle.style.display = DisplayStyle.None;
            }
            else
            {
                // Enable drag handle for single mesh view
                _dragHandle.style.display = DisplayStyle.Flex;
                _dragHandle.RegisterCallback<MouseDownEvent>(OnMouseDown);
                _dragHandle.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                _dragHandle.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }
        }

        public void CreatePreviews(List<Mesh> meshes)
        {
            CleanUp();

            if (meshes.Count > 0 && editorSettings.ShowMeshPreview)
            {
                container.style.display = DisplayStyle.Flex;
            }
            else
            {
                container.style.display = DisplayStyle.None;

                if (editorSettings.ShowInformationFoldout)
                    UpdateMeshDataGroup(informationFoldout.Q<TemplateContainer>("MeshDataGroup"), meshes, editorSettings.ShowMeshDetailsUnderPreview);

                return;
            }


            if (meshes.Count > 1 && editorSettings.ShowMeshDetailsUnderPreview)
            {
                allSelectedMeshCombinedDetails.style.display = DisplayStyle.Flex;
                allSelectedMeshCombinedDetails.Q<Label>("TotalMeshCount").text = " with " + meshes.Count + " meshes.";
                UpdateMeshDataGroup(allSelectedMeshCombinedDetails.Q<TemplateContainer>("MeshDataGroup"), meshes, editorSettings.ShowMeshDetailsUnderPreview);
            }
            else
            {
                allSelectedMeshCombinedDetails.style.display = DisplayStyle.None;
            }

            int previewAmount = meshes.Count;

            if (editorSettings.MaxPreviewCount < previewAmount)
            {
                previewAmount = editorSettings.MaxPreviewCount;
                maxPreviewCount.text = "Max preview count " + editorSettings.MaxPreviewCount + " reached. Change amount from settings.";
                maxPreviewCount.style.display = DisplayStyle.Flex;
            }
            else
            {
                maxPreviewCount.style.display = DisplayStyle.None;
            }

            for (int i = 0; i < previewAmount; i++)
            {
                CreatePreviewForMesh(meshes[i]);
            }


            UpdateInformationFoldout(meshes);
        }

        public void HidePreview()
        {
            CleanUp();
            if (container != null)
                container.style.display = DisplayStyle.None;
        }

        private void CreatePreviewForMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

            // Clone template and add to the UI
            var previewBase = new VisualElement();
            previewTemplate.CloneTree(previewBase);
            previewBase.style.flexGrow = 1;
            previewsGroupBox.Add(previewBase);

            // Cache required UI elements
            IMGUIContainer previewContainer = previewBase.Q<IMGUIContainer>("PreviewContainer");
            IMGUIContainer previewSettingsContainer = previewBase.Q<IMGUIContainer>("PreviewSettingsContainer");

            previewContainer.style.height = previewHeight;
            // Cached to enable resize by dragging the handle. Is not useful when multiple mesh is selected
            imguiPreviewContainer = previewContainer;

            // Initialize and store the preview logic
            MeshPreview meshPreview = new MeshPreview(mesh);

            // If failed to create preview, return.
            if (meshPreview == null)
                return;

            meshPreviews.Add(meshPreview);

            previewSettingsContainer.onGUIHandler += () =>
            {
                //GUI.contentColor = Color.white;
                //GUI.color = Color.white;

                //GUILayout.BeginHorizontal("Box");
                GUILayout.BeginHorizontal();
                meshPreview.OnPreviewSettings();
                GUILayout.EndHorizontal();
            };

            previewContainer.onGUIHandler += () =>
            {
                if (previewContainer.contentRect.height <= 0)
                    previewContainer.style.height = 50;

                if (previewContainer.contentRect.height > 0 && previewContainer.contentRect.width > 0) //Should be unnecessary. But still fixes a bug with height and width being negative.
                    meshPreview.OnPreviewGUI(previewContainer.contentRect, style);
            };

            UpdateMeshDataGroup(previewBase.Q<GroupBox>("MeshDataGroup"), mesh);
        }

        private Texture2D BackgroundTexture(Color color)
        {
            Texture2D newTexture = new(1, 1);
            newTexture.SetPixel(0, 0, color);
            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        /// This is for the total information counter.
        /// </summary>
        /// <param name="meshDataGroup"></param>
        /// <param name="meshes"></param>
        private void UpdateMeshDataGroup(TemplateContainer meshDataGroup, List<Mesh> meshes, bool show)
        {
            if (!show)
            {
                meshDataGroup.style.display = DisplayStyle.None;
                return;
            }

            GroupBox verticesGroup = meshDataGroup.Q<GroupBox>("VerticesGroup");
            if (!editorSettings.ShowVertexInformation) verticesGroup.style.display = DisplayStyle.None;
            else
            {
                verticesGroup.style.display = DisplayStyle.Flex;

                int counter = 0;
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        counter += mesh.vertexCount;
                }
                verticesGroup.Q<Label>("Value").text = counter.ToString(CultureInfo.InvariantCulture);

                GroupBox submeshGroup = meshDataGroup.Q<GroupBox>("SubmeshGroup");
                if (meshes.Count == 1)
                {
                    int[] subMeshVertexCounts = meshes[0].SubMeshVertexCount();
                    if (subMeshVertexCounts != null && subMeshVertexCounts.Length > 1)
                    {
                        submeshGroup.style.display = DisplayStyle.Flex;
                        submeshGroup.Q<Label>("SubmeshValue").text = subMeshVertexCounts.Length.ToString(CultureInfo.InvariantCulture);
                        Label submeshVertices = submeshGroup.Q<Label>("SubmeshVertices");
                        submeshVertices.text = "(";
                        for (int i = 0; i < subMeshVertexCounts.Length; i++)
                        {
                            submeshVertices.text += subMeshVertexCounts[i];

                            if (i + 1 != subMeshVertexCounts.Length)
                                submeshVertices.text += ", ";
                        }
                        submeshVertices.text += ")";
                    }
                    else
                    {
                        submeshGroup.style.display = DisplayStyle.None;
                    }
                }
                else
                {
                    submeshGroup.style.display = DisplayStyle.None;
                }
            }

            GroupBox trianglesGroup = meshDataGroup.Q<GroupBox>("TrianglesGroup");
            if (!editorSettings.ShowTriangleInformation) trianglesGroup.style.display = DisplayStyle.None;
            else
            {
                trianglesGroup.style.display = DisplayStyle.Flex;

                int counter = 0;
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        counter += mesh.TrianglesCount();
                }

                trianglesGroup.Q<Label>("Value").text = counter.ToString(CultureInfo.InvariantCulture);
            }

            GroupBox edgeGroup = meshDataGroup.Q<GroupBox>("EdgeGroup");
            if (!editorSettings.ShowEdgeInformation) edgeGroup.style.display = DisplayStyle.None;
            else
            {
                edgeGroup.style.display = DisplayStyle.Flex;

                int counter = 0;
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        counter += mesh.EdgeCount();
                }

                edgeGroup.Q<Label>("Value").text = counter.ToString(CultureInfo.InvariantCulture);
            }

            GroupBox tangentsGroup = meshDataGroup.Q<GroupBox>("TangentsGroup");
            if (!editorSettings.ShowTangentInformation) tangentsGroup.style.display = DisplayStyle.None;
            else
            {
                tangentsGroup.style.display = DisplayStyle.Flex;

                int counter = 0;
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        counter += mesh.tangents.Length;
                }

                tangentsGroup.Q<Label>("Value").text = counter.ToString(CultureInfo.InvariantCulture);
            }

            GroupBox faceGroup = meshDataGroup.Q<GroupBox>("FaceGroup");
            if (!editorSettings.ShowFaceInformation) faceGroup.style.display = DisplayStyle.None;
            else
            {
                faceGroup.style.display = DisplayStyle.Flex;

                int counter = 0;
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        counter += mesh.FaceCount();
                }

                faceGroup.Q<Label>("Value").text = counter.ToString(CultureInfo.InvariantCulture);
            }

            GroupBox meshMemoryGroupBox = meshDataGroup.Q<GroupBox>("RuntimeMemoryUsageGroupBox");
            if (editorSettings.runtimeMemoryUsageUnderPreview)
            {
                meshMemoryGroupBox.style.display = DisplayStyle.Flex;
                long memoryUsageInByte = 0;
                foreach (Mesh mesh in meshes)
                {
                    memoryUsageInByte += MemoryUsageInByte(mesh);
                }
                meshMemoryGroupBox.Q<Label>("MemoryUsageInFoldout").text = ByteToReadableString(memoryUsageInByte);
                var label = meshMemoryGroupBox.Q<Button>();
                if (editorSettings.showRunTimeMemoryUsageLabel)
                {
                    label.style.display = DisplayStyle.Flex;
                    meshMemoryGroupBox.Q<Button>().clicked += () => { Application.OpenURL(learnMoreAboutRuntimeMemoryUsageLink); };
                }
                else
                {
                    label.style.display = DisplayStyle.None;
                }
            }
            else
                meshMemoryGroupBox.style.display = DisplayStyle.None;
        }

        private void UpdateMeshDataGroup(VisualElement meshDataGroup, Mesh mesh)
        {
            if (!editorSettings.ShowMeshDetailsUnderPreview)
            {
                meshDataGroup.style.display = DisplayStyle.None;
                return;
            }

            GroupBox verticesGroup = meshDataGroup.Q<GroupBox>("VerticesGroup");
            if (!editorSettings.ShowVertexInformation) verticesGroup.style.display = DisplayStyle.None;
            else
            {
                verticesGroup.style.display = DisplayStyle.Flex;
                verticesGroup.Q<Label>("Value").text = mesh.vertexCount.ToString(CultureInfo.InvariantCulture);

                GroupBox submeshGroup = meshDataGroup.Q<GroupBox>("SubmeshGroup");
                int[] subMeshVertexCounts = mesh.SubMeshVertexCount();
                if (subMeshVertexCounts != null && subMeshVertexCounts.Length > 1)
                {
                    submeshGroup.style.display = DisplayStyle.Flex;
                    submeshGroup.Q<Label>("SubmeshValue").text = subMeshVertexCounts.Length.ToString(CultureInfo.InvariantCulture);
                    Label submeshVertices = submeshGroup.Q<Label>("SubmeshVertices");
                    submeshVertices.text = "(";
                    for (int i = 0; i < subMeshVertexCounts.Length; i++)
                    {
                        submeshVertices.text += subMeshVertexCounts[i];
                        if (i + 1 != subMeshVertexCounts.Length)
                            submeshVertices.text += ", ";
                    }
                    submeshVertices.text += ")";
                }
                else
                {
                    submeshGroup.style.display = DisplayStyle.None;
                }
            }

            GroupBox trianglesGroup = meshDataGroup.Q<GroupBox>("TrianglesGroup");
            if (!editorSettings.ShowTriangleInformation) trianglesGroup.style.display = DisplayStyle.None;
            else
            {
                trianglesGroup.style.display = DisplayStyle.Flex;
                trianglesGroup.Q<Label>("Value").text = mesh.TrianglesCount().ToString(CultureInfo.InvariantCulture);
            }

            GroupBox edgeGroup = meshDataGroup.Q<GroupBox>("EdgeGroup");
            if (!editorSettings.ShowEdgeInformation) edgeGroup.style.display = DisplayStyle.None;
            else
            {
                edgeGroup.style.display = DisplayStyle.Flex;
                edgeGroup.Q<Label>("Value").text = mesh.EdgeCount().ToString(CultureInfo.InvariantCulture);
            }

            GroupBox tangentsGroup = meshDataGroup.Q<GroupBox>("TangentsGroup");
            if (!editorSettings.ShowTangentInformation) tangentsGroup.style.display = DisplayStyle.None;
            else
            {
                tangentsGroup.style.display = DisplayStyle.Flex;
                tangentsGroup.Q<Label>("Value").text = mesh.tangents.Length.ToString(CultureInfo.InvariantCulture);
            }

            GroupBox faceGroup = meshDataGroup.Q<GroupBox>("FaceGroup");
            if (!editorSettings.ShowFaceInformation) faceGroup.style.display = DisplayStyle.None;
            else
            {
                faceGroup.style.display = DisplayStyle.Flex;
                faceGroup.Q<Label>("Value").text = mesh.FaceCount().ToString(CultureInfo.InvariantCulture);
            }

            GroupBox meshMemoryGroupBox = meshDataGroup.Q<GroupBox>("RuntimeMemoryUsageGroupBox");
            if (editorSettings.runtimeMemoryUsageUnderPreview)
            {
                meshMemoryGroupBox.style.display = DisplayStyle.Flex;
                meshMemoryGroupBox.Q<Label>("MemoryUsageInFoldout").text = GetMemoryUsage(mesh);

                var label = meshMemoryGroupBox.Q<Button>();

                if (editorSettings.showRunTimeMemoryUsageLabel)
                {
                    label.style.display = DisplayStyle.Flex;
                    meshMemoryGroupBox.Q<Button>().clicked += () => { Application.OpenURL(learnMoreAboutRuntimeMemoryUsageLink); };
                }
                else
                {
                    label.style.display = DisplayStyle.None;
                }
            }
            else
                meshMemoryGroupBox.style.display = DisplayStyle.None;
        }

        private float GetMeshPreviewHeight()
        {
            if (BetterMeshSettings.instance.MeshPreviewHeight == 0) return 2;

            return Mathf.Abs(BetterMeshSettings.instance.MeshPreviewHeight);
        }

        public string GetMemoryUsage(Mesh mesh)
        {
            long usageInByte = MemoryUsageInByte(mesh);

            string usage = ByteToReadableString(usageInByte);

            return usage;
        }

        private string ByteToReadableString(long usageInByte)
        {
            long megabytes = usageInByte / 1024;
            long remainingKilobytes = usageInByte % 1024;

            string usage = "";
            if (megabytes > 0) usage += megabytes + "MB ";
            if (remainingKilobytes > 0) usage += remainingKilobytes + "KB";
            return usage;
        }

        public long MemoryUsageInByte(Mesh mesh)
        {
            if (mesh == null) return 0;

            return GetMemoryUsageInByte(mesh);
        }

        private long GetMemoryUsageInByte(Mesh mesh)
        {
            return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
        }

        private bool _isDragging;
        private Vector2 _startMousePosition;
        private Vector2 _startSize;

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // Left mouse button
            {
                _isDragging = true;
                _startMousePosition = evt.mousePosition;
                _startSize = new Vector2(
                    imguiPreviewContainer.resolvedStyle.width,
                    imguiPreviewContainer.resolvedStyle.height
                );

                // Capture mouse to get events even when outside element
                _dragHandle.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                Vector2 delta = evt.mousePosition - _startMousePosition;

                //_resizableContainer.style.width = Mathf.Max(50, _startSize.x + delta.x);
                imguiPreviewContainer.style.height = Mathf.Max(50, _startSize.y + delta.y);

                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (_isDragging && evt.button == 0)
            {
                _isDragging = false;
                _dragHandle.ReleaseMouse();

                previewHeight = imguiPreviewContainer.resolvedStyle.height;
                BetterMeshSettings.instance.MeshPreviewHeight = previewHeight;
                evt.StopPropagation();
            }
        }
    }
}