using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    public class ActionsFoldoutManager
    {
        private GroupBox container;

        private GroupBox lightmapGenerateFoldout;
        private SliderInt lightmapGenerate_hardAngle;
        private SliderInt lightmapGenerate_angleError;
        private SliderInt lightmapGenerate_areaError;
        private SliderInt lightmapGenerate_packMargin;

        private BetterMeshSettings editorSettings;

        public List<MeshFilter> sourceMeshFilters = new();
        public List<SkinnedMeshRenderer> sourceSkinnedMeshRenderers = new();

        public ActionsFoldoutManager(CustomFoldoutSetup customFoldoutSetup, BetterMeshSettings editorSettings, VisualElement root, Object[] targets)
        {
            sourceMeshFilters ??= new();
            sourceSkinnedMeshRenderers ??= new();

            foreach (Object target in targets)
            {
                if (target == null) continue;
                if (target as MeshFilter != null) sourceMeshFilters.Add(target as MeshFilter);
                if (target as SkinnedMeshRenderer != null) sourceSkinnedMeshRenderers.Add(target as SkinnedMeshRenderer);
            }

            this.editorSettings = editorSettings;

            container = root.Q<GroupBox>("Buttons");
            customFoldoutSetup.SetupFoldout(container);

            UpdateDisplayStyle();

            SetupLightmapGenerateFoldout(customFoldoutSetup);

            Toggle doNotApplyActionToAssetToggle = root.Q<Toggle>("doNotApplyActionToAsset");
            doNotApplyActionToAssetToggle.value = editorSettings.DoNotApplyActionToAsset;
            doNotApplyActionToAssetToggle.RegisterValueChangedCallback(ev =>
            {
                editorSettings.DoNotApplyActionToAsset = ev.newValue;
            });

            Button optimizeMeshButton = root.Q<Button>("OptimizeMesh");
            optimizeMeshButton.tooltip = OptimizeMeshTooltip();
            optimizeMeshButton.style.display = editorSettings.ShowOptimizeButton ? DisplayStyle.Flex : DisplayStyle.None;
            optimizeMeshButton.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    OptimizeMesh(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    OptimizeMesh(i);
                }
            };

            Button recalculateNormals = root.Q<Button>("RecalculateNormals");
            recalculateNormals.style.display = editorSettings.ShowRecalculateNormalsButton ? DisplayStyle.Flex : DisplayStyle.None;
            recalculateNormals.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    RecalculateNormals(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    RecalculateNormals(i);
                }
            };

            Button recalculateTangents = root.Q<Button>("RecalculateTangents");
            recalculateTangents.style.display = editorSettings.ShowRecalculateTangentsButton ? DisplayStyle.Flex : DisplayStyle.None;
            recalculateTangents.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    RecalculateTangents(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    RecalculateTangents(i);
                }
            };

            Button flipNormals = root.Q<Button>("FlipNormals");
            flipNormals.style.display = editorSettings.ShowFlipNormalsButton ? DisplayStyle.Flex : DisplayStyle.None;
            flipNormals.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    FlipNormals(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    FlipNormals(i);
                }
            };

            Button generateSecondaryUVSet = root.Q<Button>("GenerateSecondaryUVSet");
            generateSecondaryUVSet.style.display = editorSettings.ShowGenerateSecondaryUVButton ? DisplayStyle.Flex : DisplayStyle.None;
            generateSecondaryUVSet.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    GenerateSecondaryUVSet(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ConvertMeshToInstanceIfRequired(i);
                    GenerateSecondaryUVSet(i);
                }
            };

            Button saveMeshAsField = root.Q<Button>("exportMesh");
            saveMeshAsField.style.display = editorSettings.ShowSaveMeshAsButton ? DisplayStyle.Flex : DisplayStyle.None;
            saveMeshAsField.clicked += () =>
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    ExportMesh(i);
                }
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    ExportMesh(i);
                }
            };

            /// <summary>
            /// This is used to make sure the mesh you are modifying is an instance and the user isn't accidentally modifying the asset
            /// </summary>
            void ConvertMeshToInstanceIfRequired(int index)
            {
                Mesh mesh;
                if (sourceMeshFilters.Count > index) mesh = sourceMeshFilters[index].sharedMesh;
                else mesh = sourceSkinnedMeshRenderers[index].sharedMesh;

                if (mesh == null) return;

                if (MeshIsAnAsset(mesh) && editorSettings.DoNotApplyActionToAsset)
                {
                    Mesh newMesh = new Mesh();
                    newMesh.vertices = mesh.vertices;
                    newMesh.triangles = mesh.triangles;
                    newMesh.uv = mesh.uv;
                    newMesh.uv2 = mesh.uv2;
                    newMesh.uv3 = mesh.uv3;
                    newMesh.uv4 = mesh.uv4;
                    newMesh.colors = mesh.colors;
                    newMesh.colors32 = mesh.colors32;
                    newMesh.boneWeights = mesh.boneWeights;
                    newMesh.bindposes = mesh.bindposes;
                    newMesh.normals = mesh.normals;
                    newMesh.tangents = mesh.tangents;
                    newMesh.name = mesh.name + " (Local Instance)";

                    newMesh.subMeshCount = mesh.subMeshCount;
                    for (int i = 0; i < mesh.subMeshCount; i++)
                        newMesh.SetTriangles(mesh.GetTriangles(i), i);

                    for (int shapeIndex = 0; shapeIndex < mesh.blendShapeCount; shapeIndex++)
                    {
                        string shapeName = mesh.GetBlendShapeName(shapeIndex);
                        int frameCount = mesh.GetBlendShapeFrameCount(shapeIndex);

                        for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                        {
                            float weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);

                            Vector3[] deltaVertices = new Vector3[mesh.vertexCount];
                            Vector3[] deltaNormals = new Vector3[mesh.vertexCount];
                            Vector3[] deltaTangents = new Vector3[mesh.vertexCount];

                            mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                            newMesh.AddBlendShapeFrame(shapeName, weight, deltaVertices, deltaNormals, deltaTangents);
                        }
                    }

                    newMesh.RecalculateBounds();

                    if (sourceMeshFilters.Count > index)
                    {
                        Undo.RecordObject(sourceMeshFilters[index], "Mesh instance creation");
                        sourceMeshFilters[index].sharedMesh = newMesh;
                        EditorUtility.SetDirty(sourceMeshFilters[index]);
                    }
                    else
                    {
                        Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh instance creation");
                        sourceSkinnedMeshRenderers[index].sharedMesh = newMesh;
                        EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                    }
                }
            }

            //void SubDivideMesh(ClickEvent evt)
            //{
            //    sourceMeshFilter.sharedMesh = mesh.SubDivide();
            //    mesh = sourceMeshFilter.sharedMesh;
            //    EditorUtility.SetDirty(mesh);
            //    Log("exported");
            //    UpdateFoldouts(mesh);
            //    SceneView.RepaintAll();
            //}

            void OptimizeMesh(int index)
            {
                if (sourceMeshFilters.Count > index)
                {
                    if (!sourceMeshFilters[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceMeshFilters[index], "Mesh Optimized.");
                    sourceMeshFilters[index].sharedMesh.Optimize();
                    EditorUtility.SetDirty(sourceMeshFilters[index]);
                }
                else
                {
                    if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh Optimized.");
                    sourceSkinnedMeshRenderers[index].sharedMesh.Optimize();
                    EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                }

                Debug.Log("Mesh optimized.");
            }

            void RecalculateNormals(int index)
            {
                if (sourceMeshFilters.Count > index)
                {
                    if (!sourceMeshFilters[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceMeshFilters[index], "Mesh Normals Modified.");
                    sourceMeshFilters[index].sharedMesh.RecalculateNormals();
                    EditorUtility.SetDirty(sourceMeshFilters[index]);
                }
                else
                {
                    if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh Normals Modified.");
                    sourceSkinnedMeshRenderers[index].sharedMesh.RecalculateNormals();
                    EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                }

                Debug.Log("Mesh Normals recalculated");
            }

            void RecalculateTangents(int index)
            {
                if (sourceMeshFilters.Count > index)
                {
                    if (!sourceMeshFilters[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceMeshFilters[index], "Mesh Tangents Modified.");
                    sourceMeshFilters[index].sharedMesh.RecalculateTangents();
                    EditorUtility.SetDirty(sourceMeshFilters[index]);
                }
                else
                {
                    if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh Tangents Modified.");
                    sourceSkinnedMeshRenderers[index].sharedMesh.RecalculateTangents();
                    EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                }

                Debug.Log("Mesh Tangents recalculated");
            }

            void FlipNormals(int index)
            {
                if (sourceMeshFilters.Count > index)
                {
                    if (!sourceMeshFilters[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceMeshFilters[index], "Mesh Normals Flipped.");
                    sourceMeshFilters[index].sharedMesh.FlipNormals();
                    EditorUtility.SetDirty(sourceMeshFilters[index]);
                }
                else
                {
                    if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh Normals Flipped.");
                    sourceSkinnedMeshRenderers[index].sharedMesh.FlipNormals();
                    EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                }

                Debug.Log("Mesh Normals Flipped");
                SceneView.RepaintAll();
            }

            void ExportMesh(int index)
            {
                if (sourceMeshFilters.Count > index)
                {
                    if (!sourceMeshFilters[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceMeshFilters[index], "Mesh Exported.");
                    sourceMeshFilters[index].sharedMesh = sourceMeshFilters[index].sharedMesh.ExportMesh();
                    EditorUtility.SetDirty(sourceMeshFilters[index]);
                }
                else
                {
                    if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                        return;

                    Undo.RecordObject(sourceSkinnedMeshRenderers[index], "Mesh Exported.");
                    sourceSkinnedMeshRenderers[index].sharedMesh = sourceSkinnedMeshRenderers[index].sharedMesh.ExportMesh();
                    EditorUtility.SetDirty(sourceSkinnedMeshRenderers[index]);
                }

                Debug.Log("Mesh Exported");
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Turns on and off the foldout
        /// </summary>
        /// <param name="editorSettings"></param>
        public void UpdateDisplayStyle()
        {
            container.style.display = editorSettings.ShowActionsFoldout ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetupLightmapGenerateFoldout(CustomFoldoutSetup customFoldoutSetup)
        {
            lightmapGenerateFoldout = container.Q<GroupBox>("GenerateSecondaryUVsetGroupBox");
            customFoldoutSetup.SetupFoldout(lightmapGenerateFoldout);

            lightmapGenerateFoldout.style.display = editorSettings.ShowGenerateSecondaryUVButton ? DisplayStyle.Flex : DisplayStyle.None;


            lightmapGenerate_hardAngle = lightmapGenerateFoldout.Q<SliderInt>("HardAngleSlider");
            lightmapGenerate_angleError = lightmapGenerateFoldout.Q<SliderInt>("AngleErrorSlider");
            lightmapGenerate_areaError = lightmapGenerateFoldout.Q<SliderInt>("AreaErrorSlider");
            lightmapGenerate_packMargin = lightmapGenerateFoldout.Q<SliderInt>("PackMarginSlider");

            lightmapGenerateFoldout.Q<Button>("ResetGenerateSecondaryUVSetButton").clicked += () =>
            {
                UnwrapParam unwrapParam = new UnwrapParam();
                UnwrapParam.SetDefaults(out unwrapParam);

                lightmapGenerate_hardAngle.value = Mathf.CeilToInt(unwrapParam.hardAngle);
                lightmapGenerate_angleError.value = Mathf.CeilToInt(Remap(unwrapParam.angleError, 0, 1, 1, 75));
                lightmapGenerate_areaError.value = Mathf.CeilToInt(Remap(unwrapParam.areaError, 0, 1, 1, 75));
                lightmapGenerate_packMargin.value = Mathf.CeilToInt(Remap(unwrapParam.packMargin, 0, 1, 1, 64));
            };
        }

        public void MeshUpdated()
        {
            if (!editorSettings.ShowActionsFoldout)
            {
                container.style.display = DisplayStyle.None;
                return;
            }

            if (sourceMeshFilters.Count > 0)
            {
                for (int i = 0; i < sourceMeshFilters.Count; i++)
                {
                    if (sourceMeshFilters[i].sharedMesh != null)
                    {
                        container.style.display = DisplayStyle.Flex;
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < sourceSkinnedMeshRenderers.Count; i++)
                {
                    if (sourceSkinnedMeshRenderers[i].sharedMesh != null)
                    {
                        container.style.display = DisplayStyle.Flex;
                        return;
                    }
                }
            }
            container.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Compute a unique UV layout for a Mesh, and store it in Mesh.uv2.
        /// When you import a model asset, you can instruct Unity to compute a light map UV layout for it using [[ModelImporter-generateSecondaryUV]] or the Model Import Settings Inspector. This function allows you to do the same to procedurally generated meshes.
        ///If this process requires multiple UV charts to flatten the mesh, the mesh might contain more vertices than before. If the mesh uses 16-bit indices (see Mesh.indexFormat) and the process would result in more vertices than are possible to use with 16-bit indices, this function fails and returns false.
        /// Note: Editor only
        /// </summary>
        private void GenerateSecondaryUVSet(int index)
        {
            Mesh mesh;

            if (sourceMeshFilters.Count > index)
            {
                if (!sourceMeshFilters[index].sharedMesh)
                    return;

                mesh = sourceMeshFilters[index].sharedMesh;
            }
            else
            {
                if (!sourceSkinnedMeshRenderers[index].sharedMesh)
                    return;

                mesh = sourceSkinnedMeshRenderers[index].sharedMesh;
            }

            UnwrapParam unwrapParam = new UnwrapParam();
            UnwrapParam.SetDefaults(out unwrapParam);

            unwrapParam.hardAngle = lightmapGenerate_hardAngle.value;
            unwrapParam.angleError = Remap(lightmapGenerate_angleError.value, 1f, 75f, 0f, 1f);
            unwrapParam.areaError = Remap(lightmapGenerate_areaError.value, 1f, 75f, 0f, 1f);
            unwrapParam.packMargin = Remap(lightmapGenerate_packMargin.value, 0f, 1f, 0f, 64f);

            Undo.RecordObject(mesh, "Generated Secondary UV");
            Unwrapping.GenerateSecondaryUVSet(mesh, unwrapParam);
            EditorUtility.SetDirty(mesh);

            Debug.Log("Generated Secondary UV.");

            SceneView.RepaintAll();
        }



        public void UpdateFoldoutVisibilities()
        {
            UpdateDisplayStyle();

            container.Q<Button>("OptimizeMesh").style.display = editorSettings.ShowOptimizeButton ? DisplayStyle.Flex : DisplayStyle.None;
            container.Q<Button>("RecalculateNormals").style.display = editorSettings.ShowRecalculateNormalsButton ? DisplayStyle.Flex : DisplayStyle.None;
            container.Q<Button>("RecalculateTangents").style.display = editorSettings.ShowRecalculateTangentsButton ? DisplayStyle.Flex : DisplayStyle.None;
            container.Q<Button>("FlipNormals").style.display = editorSettings.ShowFlipNormalsButton ? DisplayStyle.Flex : DisplayStyle.None;
            container.Q<GroupBox>("GenerateSecondaryUVsetGroupBox").style.display = editorSettings.ShowGenerateSecondaryUVButton ? DisplayStyle.Flex : DisplayStyle.None;
            container.Q<Button>("exportMesh").style.display = editorSettings.ShowSaveMeshAsButton ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax) => (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;


        private bool MeshIsAnAsset(Mesh newMesh) => AssetDatabase.Contains(newMesh);

        private string OptimizeMeshTooltip() => "Optimizes mesh data to improve rendering performance. You should only use this function on meshes generated procedurally in code. For regular mesh assets, it is automatically called by the import pipeline when Optimize Mesh is enabled in the mesh importer settings."
        + "\n\nThis function reorders the geometry and vertices of the mesh internally to improve vertex cache utilization on the graphics hardware, thereby enhancing rendering performance."
            + "\n\nNote that this operation can take several seconds or more for complex meshes, and it should only be used when the ordering of geometry and vertices is not significant, as both will be changed.";
    }
}