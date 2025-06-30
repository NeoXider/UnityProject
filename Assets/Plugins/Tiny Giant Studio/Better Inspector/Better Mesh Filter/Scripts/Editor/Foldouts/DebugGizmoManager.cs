using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    public class DebugGizmoManager
    {
        #region Gizmo

        #region Variable Declarations

        private GroupBox container;

        public bool showNormals = false;
        public float normalLength = 0.1f;
        public float normalWidth = 5;
        public Color normalColor = Color.blue;

        public bool showTangents = false;
        public float tangentLength = 0.1f;
        public float tangentWidth = 5;
        public Color tangentColor = Color.red;

        public bool showUV;
        public Color uvSeamColor = Color.green;
        public float uvWidth = 5;

        private GroupBox gimzoTimeWarningBox;

        private FloatField normalsWidthField;
        private FloatField tangentWidthField;
        private FloatField uvWidthField;

        private Label lastDrawnGizmosWith;
        private Label tooMuchHandleWarningLabel;
        private Label gizmoIsOnWarningLabel;
        private BetterMeshSettings editorSettings;

        private List<Transform> transforms = new();
        private List<Mesh> meshes = new();
        public List<Mesh> bakedMeshes = new List<Mesh>();

        #endregion Variable Declarations


        /// <summary>
        /// Constructor
        /// </summary>
        public DebugGizmoManager(CustomFoldoutSetup customFoldoutSetup, BetterMeshSettings editorSettings, VisualElement root)
        {
            this.editorSettings = editorSettings;

            container = root.Q<GroupBox>("MeshDebugFoldout");
            customFoldoutSetup.SetupFoldout(container);


            DrawGizmoSettings(container);

            UpdateDisplayStyle();
        }

        public void Cleanup()
        {
            ResetMaterials();

            CleanUpBakedMeshes();
        }

        private void CleanUpBakedMeshes()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < bakedMeshes.Count; i++)
                {
                    Object.Destroy(bakedMeshes[i]);
                }
            }
            else
            {
                for (int i = 0; i < bakedMeshes.Count; i++)
                {
                    Object.DestroyImmediate(bakedMeshes[i]);
                }
            }

            bakedMeshes.Clear();
        }


        /// <summary>
        /// Turns on and off the foldout
        /// </summary>
        /// <param name="editorSettings"></param>
        public void UpdateDisplayStyle()
        {
            container.style.display = editorSettings.ShowDebugGizmoFoldout ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void HideDebugGizmo()
        {
            container.style.display = DisplayStyle.None;
        }

        private void DrawGizmoSettings(VisualElement container)
        {
            gizmoIsOnWarningLabel = container.Q<Label>("GizmoIsOnWarningLabel");

            NormalsGizmoSettings(container);

            TangentGizmoSettings(container);

            UVGizmoSettings(container);

            CheckeredUVSettings(container);

            var useAntiAliasedGizmosField = container.Q<Toggle>("UseAntiAliasedGizmosField");
            useAntiAliasedGizmosField.value = editorSettings.useAntiAliasedGizmo;
            useAntiAliasedGizmosField.RegisterValueChangedCallback(e =>
            {
                editorSettings.useAntiAliasedGizmo = e.newValue;
                editorSettings.Save();

                SwitchOnOffAAAGizmosFields();
            });

            SwitchOnOffAAAGizmosFields();

            var maximumGizmoDrawTimeField = container.Q<IntegerField>("MaximumGizmoDrawTimeField");
            maximumGizmoDrawTimeField.value = editorSettings.maximumGizmoDrawTime;
            maximumGizmoDrawTimeField.RegisterValueChangedCallback(e =>
            {
                if (e.newValue < 10 && e.newValue > 10000)
                    return;

                editorSettings.maximumGizmoDrawTime = e.newValue;
                editorSettings.Save();
            });

            var CachedDataUpdateRate = container.Q<IntegerField>("CachedDataUpdateRate");
            CachedDataUpdateRate.value = editorSettings.updateCacheEveryDashSeconds;
            CachedDataUpdateRate.RegisterValueChangedCallback(e =>
            {
                if (e.newValue < 0)
                    return;

                editorSettings.updateCacheEveryDashSeconds = e.newValue;
                editorSettings.Save();
            });

            gimzoTimeWarningBox = container.Q<GroupBox>("GimzoTimeWarningBox");
            lastDrawnGizmosWith = container.Q<Label>("LastDrawnGizmosWith");
            tooMuchHandleWarningLabel = container.Q<Label>("GizmoWarningLabel");
        }

        public void UpdateTargets(List<Mesh> meshes, List<Transform> transforms, bool bakeMesh = false)
        {
            this.meshes ??= new List<Mesh>();
            this.meshes.Clear();
            this.transforms ??= new List<Transform>();
            this.transforms.Clear();

            foreach (Mesh mesh in meshes)
            {
                if (mesh == null) continue;

                this.meshes.Add(mesh);
            }
            foreach (Transform transform in transforms)
            {
                if (transform == null) continue;

                this.transforms.Add(transform);
            }

            UpdateCachedData();
        }




        private void SwitchOnOffAAAGizmosFields()
        {
            if (editorSettings.useAntiAliasedGizmo)
            {
                normalsWidthField.SetEnabled(true);
                tangentWidthField.SetEnabled(true);
                uvWidthField.SetEnabled(true);
            }
            else
            {
                normalsWidthField.SetEnabled(false);
                tangentWidthField.SetEnabled(false);
                uvWidthField.SetEnabled(false);
            }
        }

        private void NormalsGizmoSettings(VisualElement container)
        {
            Toggle showNormalsField = container.Q<Toggle>("showNormals");
            FloatField normalsLengthField = container.Q<FloatField>("normalLength");
            normalsWidthField = container.Q<FloatField>("normalWidth");
            ColorField normalsColorField = container.Q<ColorField>("normalColor");

            if (!showNormalsField.value) HideNormalsGizmoSettings(normalsLengthField, normalsWidthField, normalsColorField);

            showNormalsField.RegisterValueChangedCallback(ev =>
            {
                showNormals = ev.newValue;

                if (ev.newValue)
                {
                    normalsLengthField.style.display = DisplayStyle.Flex;
                    normalsWidthField.style.display = DisplayStyle.Flex;
                    normalsColorField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    HideNormalsGizmoSettings(normalsLengthField, normalsWidthField, normalsColorField);
                }
                GizmoToggled();
                SceneView.RepaintAll();
            });
            normalsLengthField.RegisterValueChangedCallback(ev =>
            {
                normalLength = ev.newValue;
                SceneView.RepaintAll();
            });
            normalsWidthField.RegisterValueChangedCallback(ev =>
            {
                normalWidth = ev.newValue;
                SceneView.RepaintAll();
            });
            normalsColorField.RegisterValueChangedCallback(ev =>
            {
                normalColor = ev.newValue;
                SceneView.RepaintAll();
            });

            GizmoToggled();
        }

        private void GizmoToggled()
        {
            if (showNormals || showTangents || showUV)
                gizmoIsOnWarningLabel.style.display = DisplayStyle.Flex;
            else
                gizmoIsOnWarningLabel.style.display = DisplayStyle.None;
        }

        private void HideNormalsGizmoSettings(FloatField normalsLengthField, FloatField normalsWidthField, ColorField normalsColorField)
        {
            normalsLengthField.style.display = DisplayStyle.None;
            normalsWidthField.style.display = DisplayStyle.None;
            normalsColorField.style.display = DisplayStyle.None;
        }

        private void CheckeredUVSettings(VisualElement container)
        {
            Button setCheckeredUV = container.Q<Button>("setCheckeredUV");
            setCheckeredUV.clickable = null;
            setCheckeredUV.clicked += () =>
            {
                if (editorSettings.originalMaterials.Count > 0)
                    ResetMaterials();
                else
                    AssignCheckerMaterials();
            };
            var setCheckerField = container.Q<Toggle>("setChecker");

            setCheckerField.RegisterValueChangedCallback(ev =>
            {
                if (ev.newValue)
                {
                    AssignCheckerMaterials();
                }
                else
                {
                    ResetMaterials();
                }
            });
        }

        private bool justAppliedMaterialDoNotReset = false;

        private void AssignCheckerMaterials()
        {
            int width = container.Q<IntegerField>("UVWidth").value;
            int height = container.Q<IntegerField>("UVHeight").value;
            int cellSize = container.Q<IntegerField>("UVCellSize").value;

            Undo.SetCurrentGroupName("Set Checkered Materials");
            int group = Undo.GetCurrentGroup();

            for (int i = 0; i < transforms.Count; i++)
            {
                AssignCheckerMaterial(i, width, height, cellSize);
            }

            Undo.CollapseUndoOperations(group);

            // todo Find a cleaner way to work around this
            // If not in multi select, when only one target is selected, the inspector resets. This avoids the checker material from going away.
            if (transforms.Count == 1)
                justAppliedMaterialDoNotReset = true; //If this is true
        }

        private void AssignCheckerMaterial(int index, int width, int height, int cellSize)
        {
            Debug.Log("Assigning checker material.");
            Renderer renderer = transforms[index].GetComponent<Renderer>();

            if (renderer == null)
            {
                return;
            }

            OriginalMaterial original = new(renderer.sharedMaterials);
            Material[] tempMaterials = renderer.sharedMaterials;

            for (int i = 0; i < tempMaterials.Length; i++)
            {
                Material originalMaterial = tempMaterials[i];

                if (originalMaterial == null) continue;

                Material checkerMaterial = new Material(originalMaterial);
                checkerMaterial.name = "Checkered Material";

                checkerMaterial.mainTexture = CreateCheckeredTexture(width, height, cellSize);
                tempMaterials[i] = checkerMaterial;
            }
            editorSettings.originalMaterials.Add(original);

            Undo.RecordObject(renderer, "Set Checkered Materials");
            renderer.sharedMaterials = tempMaterials;
        }

        private void ResetMaterials()
        {
            if (justAppliedMaterialDoNotReset)
            {
                justAppliedMaterialDoNotReset = false;
                return;
            }
            for (int i = 0; transforms.Count > i; i++)
            {
                if (transforms[i] == null) continue;

                if (!transforms[i].GetComponent<Renderer>()) continue;

                if (editorSettings.originalMaterials.Count <= i) continue;

                transforms[i].GetComponent<Renderer>().sharedMaterials = editorSettings.originalMaterials[i].materials;
            }

            editorSettings.originalMaterials.Clear();
        }

        private Texture2D CreateCheckeredTexture(int width, int height, int checkSize)
        {
            Texture2D texture = new Texture2D(width, height);
            Color color1 = Color.black;
            Color color2 = Color.white;

            // Loop over the width and height.
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    // Determine which color to use based on the current x and y indices.
                    bool checkX = x / checkSize % 2 == 0;
                    bool checkY = y / checkSize % 2 == 0;
                    Color color = (checkX == checkY) ? color1 : color2;

                    // Set the pixel color.
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();

            return texture;
        }

        private void UVGizmoSettings(VisualElement container)
        {
            Toggle showUVField = container.Q<Toggle>("showUV");
            uvWidthField = container.Q<FloatField>("uvWidth");
            ColorField uvColorField = container.Q<ColorField>("uvColor");

            if (!showUVField.value) HideUVGizmoSettings(uvWidthField, uvColorField);

            showUVField.RegisterValueChangedCallback(ev =>
            {
                showUV = ev.newValue;

                if (ev.newValue)
                {
                    uvWidthField.style.display = DisplayStyle.Flex;
                    uvColorField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    HideUVGizmoSettings(uvWidthField, uvColorField);
                }
                GizmoToggled();
                SceneView.RepaintAll();
            });
            uvWidthField.RegisterValueChangedCallback(ev =>
            {
                uvWidth = ev.newValue;
                SceneView.RepaintAll();
            });
            uvColorField.RegisterValueChangedCallback(ev =>
            {
                uvSeamColor = ev.newValue;
                SceneView.RepaintAll();
            });
        }

        private void HideUVGizmoSettings(FloatField uvWidthField, ColorField uvColorField)
        {
            uvWidthField.style.display = DisplayStyle.None;
            uvColorField.style.display = DisplayStyle.None;
        }

        private void TangentGizmoSettings(VisualElement container)
        {
            Toggle showTangentsField = container.Q<Toggle>("showTangents");
            FloatField tangentLengthField = container.Q<FloatField>("tangentLength");
            tangentWidthField = container.Q<FloatField>("tangentWidth");
            ColorField tangentColorField = container.Q<ColorField>("tangentColor");

            if (!showTangentsField.value) HideTangentGizmoSettings(tangentLengthField, tangentWidthField, tangentColorField);

            showTangentsField.RegisterValueChangedCallback(ev =>
            {
                showTangents = ev.newValue;

                if (ev.newValue)
                {
                    tangentLengthField.style.display = DisplayStyle.Flex;
                    tangentWidthField.style.display = DisplayStyle.Flex;
                    tangentColorField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    HideTangentGizmoSettings(tangentLengthField, tangentWidthField, tangentColorField);
                }
                GizmoToggled();
                SceneView.RepaintAll();
            });
            tangentLengthField.RegisterValueChangedCallback(ev =>
            {
                tangentLength = ev.newValue;
                SceneView.RepaintAll();
            });
            tangentWidthField.RegisterValueChangedCallback(ev =>
            {
                tangentWidth = ev.newValue;
                SceneView.RepaintAll();
            });
            tangentColorField.RegisterValueChangedCallback(ev =>
            {
                tangentColor = ev.newValue;
                SceneView.RepaintAll();
            });
        }

        private void HideTangentGizmoSettings(FloatField tangentLengthField, FloatField tangentWidthField, ColorField tangentColorField)
        {
            tangentLengthField.style.display = DisplayStyle.None;
            tangentWidthField.style.display = DisplayStyle.None;
            tangentColorField.style.display = DisplayStyle.None;
        }

        private Stopwatch stopwatch;
        private bool useAntiAliasedHandles;
        private int drawnGizmo;
        private int maximumGizmoTime;
        private bool wasAbleToDrawEverything = true;

        public void DrawGizmo(SkinnedMeshRenderer[] skinnedMeshRenderers = null)
        {
            if (meshes == null) return;
            if (meshes.Count == 0) return;

            if (editorSettings == null) return;

            if (!editorSettings.ShowDebugGizmoFoldout) return;

            if (!showNormals && !showTangents && !showUV) return;

            if (stopwatch == null) stopwatch = new();
            else stopwatch.Reset();

            stopwatch.Start();

            useAntiAliasedHandles = editorSettings.useAntiAliasedGizmo;
            maximumGizmoTime = editorSettings.maximumGizmoDrawTime;
            drawnGizmo = 0;

            wasAbleToDrawEverything = true;

            if ((skinnedMeshRenderers != null && (bakedMeshes == null || bakedMeshes.Count == 0)) || Time.realtimeSinceStartup - timeSinceLastCachedDataUpdate > editorSettings.updateCacheEveryDashSeconds) UpdateCachedData(skinnedMeshRenderers);

            if (skinnedMeshRenderers == null)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    if (stopwatch.ElapsedMilliseconds > maximumGizmoTime)
                    {
                        stopwatch.Stop();
                        break;
                    }

                    if (i >= transforms.Count || i >= cachedMeshDatas.Count) return;

                    DrawGizmoForMesh(meshes[i], transforms[i], cachedMeshDatas[i]);
                }
            }
            else
            {
                for (int i = 0; i < bakedMeshes.Count; i++)
                {
                    if (stopwatch.ElapsedMilliseconds > maximumGizmoTime)
                    {
                        stopwatch.Stop();
                        //MaximumGizmoDrawingTimeReachedForVertexAndTriangles(vertices.Length - (i + 1));
                        break;
                    }

                    if (i >= transforms.Count || i >= cachedMeshDatas.Count) return;

                    DrawGizmoForMesh(bakedMeshes[i], skinnedMeshRenderers[i].transform, cachedMeshDatas[i]);
                }
            }


            stopwatch.Stop();
            GizmosDrawingDone(drawnGizmo, stopwatch.ElapsedMilliseconds);
        }

        private float uvSeamThreshhold = 0.001f;
        private int maxUVSeamForAAAHandle = 30000;

        private void DrawGizmoForMesh(Mesh mesh, Transform transform, CachedMeshData cachedMeshData)
        {
            

            if (mesh == null) return;
            Vector3[] vertices = mesh.vertices;

            Matrix4x4 localToWorld = transform.localToWorldMatrix;
            if (showUV)
            {
                Handles.color = uvSeamColor;

                    if (useAntiAliasedHandles && cachedMeshData.linePoints.Length < maxUVSeamForAAAHandle)
                        Handles.DrawAAPolyLine(uvWidth, cachedMeshData.linePoints);
                    else
                        Handles.DrawLines(cachedMeshData.linePoints);

                    drawnGizmo += cachedMeshData.linePoints.Length;
            }
            if (showNormals || showTangents)
            {
                Vector3[] normals = mesh.normals;
                Vector4[] tangents = mesh.tangents;

                Matrix4x4 normalMatrix = transform.localToWorldMatrix;

                bool drawNormals = showNormals && normals.Length == vertices.Length;
                bool drawTangents = showTangents && tangents.Length == vertices.Length;

                for (int i = 0; i < vertices.Length; i++)
                {
                    //Vector3 worldVertex = transform.TransformPoint(vertices[i]);
                    Vector3 worldVertex = localToWorld.MultiplyPoint3x4(vertices[i]);

                    if (drawNormals)
                    {
                        //Vector3 worldNormal = transform.TransformDirection(normals[i]);
                        Vector3 worldNormal = normalMatrix.MultiplyVector(normals[i]);
                        Handles.color = normalColor;
                        if (useAntiAliasedHandles)
                            Handles.DrawAAPolyLine(normalWidth, worldVertex, worldVertex + worldNormal * normalLength);
                        else
                            Handles.DrawLine(worldVertex, worldVertex + worldNormal * normalLength);
                    }

                    if (drawTangents)
                    {
                        //Vector3 worldTangent = transform.TransformDirection(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z));
                        Vector3 worldTangent = normalMatrix.MultiplyVector(tangents[i]);
                        Handles.color = tangentColor;
                        if (useAntiAliasedHandles)
                            Handles.DrawAAPolyLine(tangentWidth, worldVertex, worldVertex + worldTangent * tangentLength);
                        else
                            Handles.DrawLine(worldVertex, worldVertex + worldTangent * tangentLength);
                    }

                    if (stopwatch.ElapsedMilliseconds > maximumGizmoTime)
                    {
                        stopwatch.Stop();

                        if (showNormals)
                            drawnGizmo += i + 1;
                        if (showTangents)
                            drawnGizmo += i + 1;

                        MaximumGizmoDrawingTimeReachedForVertexAndTriangles(vertices.Length - (i + 1));
                        return;
                    }
                }

                if (showNormals)
                    drawnGizmo += vertices.Length;
                if (showTangents)
                    drawnGizmo += tangents.Length;
            }
        }

        float timeSinceLastCachedDataUpdate;
        private void UpdateCachedData(SkinnedMeshRenderer[] skinnedMeshRenderers = null)
        {
            if (transforms.Count != meshes.Count) return;

            cachedMeshDatas ??= new();
            cachedMeshDatas.Clear();

            if (skinnedMeshRenderers == null)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    if (meshes[i] == null || transforms[i] == null) continue;

                    UpdateCachedData(meshes[i], transforms[i]);
                }
            }
            else
            {
                CleanUpBakedMeshes();

                for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                {
                    if (skinnedMeshRenderers[i].sharedMesh == null) continue;

                    Mesh bakedMesh = new Mesh();
                    skinnedMeshRenderers[i].BakeMesh(bakedMesh);
                    bakedMeshes.Add(bakedMesh);

                    UpdateCachedData(bakedMesh, skinnedMeshRenderers[i].transform);
                }
            }


            timeSinceLastCachedDataUpdate = Time.realtimeSinceStartup;
        }

        private void UpdateCachedData(Mesh mesh, Transform transform)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            Matrix4x4 localToWorld = transform.localToWorldMatrix;

            Vector2[] uvs = mesh.uv;
            if (uvs.Length == 0) return;

            //float threshold = 0.5f * 0.5f; // Compare squared distances to avoid sqrt calculations
            float threshold = uvSeamThreshhold * uvSeamThreshhold; // Compare squared distances to avoid sqrt calculations

            int triangleCount = triangles.Length;

            Handles.color = uvSeamColor;

            List<Triangle> trianglesStruct = new();
            for (int i = 0; i < triangleCount; i += 3)
            {
                int indexA = triangles[i];
                int indexB = triangles[i + 1];
                int indexC = triangles[i + 2];

                Vector2 uvA = uvs[indexA];
                Vector2 uvB = uvs[indexB];
                Vector2 uvC = uvs[indexC];

                if ((uvA - uvB).sqrMagnitude > threshold || (uvB - uvC).sqrMagnitude > threshold || (uvC - uvA).sqrMagnitude > threshold)
                {
                    trianglesStruct.Add(new Triangle(localToWorld.MultiplyPoint3x4(vertices[indexA]), localToWorld.MultiplyPoint3x4(vertices[indexB]), localToWorld.MultiplyPoint3x4(vertices[indexC])));
                }
            }

            List<Vector3> linePoints = new();
            for (int i = 0; i < trianglesStruct.Count; i++)
            {
                var tri = trianglesStruct[i];

                linePoints.Add(tri.WorldVertexA);
                linePoints.Add(tri.WorldVertexB);

                linePoints.Add(tri.WorldVertexB);
                linePoints.Add(tri.WorldVertexC);

                linePoints.Add(tri.WorldVertexC);
                linePoints.Add(tri.WorldVertexA);
            }
            cachedMeshDatas.Add(new CachedMeshData(linePoints));
        }

        private List<CachedMeshData> cachedMeshDatas = new List<CachedMeshData>();

        private struct CachedMeshData
        {
            public Vector3[] linePoints;

            public CachedMeshData(List<Vector3> linePointsList) : this()
            {
                linePoints = linePointsList.ToArray();
            }
        }

        private struct Triangle
        {
            public Triangle(Vector3 worldVertexA, Vector3 worldVertexB, Vector3 worldVertexC) : this()
            {
                WorldVertexA = worldVertexA;
                WorldVertexB = worldVertexB;
                WorldVertexC = worldVertexC;
            }

            public Vector3 WorldVertexA { get; }
            public Vector3 WorldVertexB { get; }
            public Vector3 WorldVertexC { get; }
        }

        /// <summary>
        /// This is called at the end after everything is drawn.
        /// </summary>
        /// <param name="gizmosDrawn"></param>
        /// <param name="time"></param>
        private void GizmosDrawingDone(int gizmosDrawn, long time)
        {
            if (lastDrawnGizmosWith == null)
                return;

            if (gizmosDrawn > 0)
                lastDrawnGizmosWith.text = "Drew <b>" + gizmosDrawn + "</b> handles and it took <b>" + time + "</b>ms.";
            else
                lastDrawnGizmosWith.text = "";

            if (wasAbleToDrawEverything)
                gimzoTimeWarningBox.style.display = DisplayStyle.None;
        }

        private void MaximumGizmoDrawingTimeReachedForVertexAndTriangles(int gizmoNotDrawnFor)
        {
            wasAbleToDrawEverything = false;

            if (tooMuchHandleWarningLabel == null)
                return;

            gimzoTimeWarningBox.style.display = DisplayStyle.Flex;

            tooMuchHandleWarningLabel.text = "Didn't draw <b>" + gizmoNotDrawnFor + "</b> handles for ";
            if (showNormals && showTangents)
                tooMuchHandleWarningLabel.text += "normals and tangents.";
            else if (showNormals)
                tooMuchHandleWarningLabel.text += "normals.";
            else if (showTangents)
                tooMuchHandleWarningLabel.text += "tangents.";

            if (editorSettings.useAntiAliasedGizmo)
                tooMuchHandleWarningLabel.text += "\n\nTurning off anti aliased gizmos will help the performance";
        }

        #endregion Gizmo
    }
}