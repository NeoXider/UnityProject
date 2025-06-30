using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace TinyGiantStudio.BetterInspector.BetterMesh
{
    public class BaseSizeFoldoutManager
    {
        GroupBox sizeFoldout;
        BetterMeshSettings editorSettings;

        public BaseSizeFoldoutManager(CustomFoldoutSetup customFoldoutSetup, BetterMeshSettings editorSettings, VisualElement root)
        {
            this.editorSettings = editorSettings;

            sizeFoldout = root.Q<GroupBox>("MeshSize"); ;
            customFoldoutSetup.SetupFoldout(sizeFoldout);
        }

        public void HideFoldout()
        {
            sizeFoldout.style.display = DisplayStyle.None;
        }

        public void UpdateTargets(List<Mesh> meshes)
        {
            if (meshes.Count != 1 || !editorSettings.ShowSizeFoldout)
            {
                sizeFoldout.style.display = DisplayStyle.None;
                return;
            }

            sizeFoldout.style.display = DisplayStyle.Flex;

            Mesh newMesh = meshes[0];
            if (newMesh == null) return;

            DropdownField meshUnitDropdown = sizeFoldout.parent.Q<DropdownField>("MeshUnit");

            if (ScalesFinder.MyScales().GetAvailableUnits().ToList().Count == 0) ScalesFinder.MyScales().Reset();
            meshUnitDropdown.choices = ScalesFinder.MyScales().GetAvailableUnits().ToList();

            meshUnitDropdown.index = editorSettings.SelectedUnit;

            meshUnitDropdown.RegisterCallback<ChangeEvent<string>>(ev =>
            {
                editorSettings.SelectedUnit = meshUnitDropdown.index;
                UpdateValues(" " + ev.newValue);
            });

            UpdateValues(" " + ScalesFinder.MyScales().GetAvailableUnits()[editorSettings.SelectedUnit]);

            void UpdateValues(string selectedUnitName)
            {

                editorSettings.SelectedUnit = meshUnitDropdown.index;
                Bounds meshBound = newMesh.MeshSizeEditorOnly(ScalesFinder.MyScales().UnitValue(editorSettings.SelectedUnit));

                GroupBox meshSizeTemplateContainer = sizeFoldout.Q<GroupBox>("MeshSize");

                GroupBox lengthGroup = meshSizeTemplateContainer.Q<GroupBox>("LengthGroup");
                Label lengthValue = lengthGroup.Q<Label>("Value");
                lengthValue.text = RoundedFloat(meshBound.size.x).ToString(CultureInfo.InvariantCulture);
                lengthValue.tooltip = meshBound.size.x.ToString(CultureInfo.InvariantCulture);

                GroupBox heightGroup = meshSizeTemplateContainer.Q<GroupBox>("HeightGroup");
                Label heightValue = heightGroup.Q<Label>("Value");
                heightValue.text = RoundedFloat(meshBound.size.y).ToString(CultureInfo.InvariantCulture);
                heightValue.tooltip = meshBound.size.y.ToString(CultureInfo.InvariantCulture);

                GroupBox depthGroup = meshSizeTemplateContainer.Q<GroupBox>("DepthGroup");
                Label depthValue = depthGroup.Q<Label>("Value");
                depthValue.text = RoundedFloat(meshBound.size.z).ToString(CultureInfo.InvariantCulture);
                depthValue.tooltip = meshBound.size.z.ToString(CultureInfo.InvariantCulture);

                Label centerLabel = sizeFoldout.Q<Label>("Center");
                string centerText = RoundedFloat(meshBound.center.x) + ", " + RoundedFloat(meshBound.center.y) + ", " + RoundedFloat(meshBound.center.z);
                centerLabel.text = centerText;
                centerLabel.tooltip = "Number is rounded after 4 digits";
            }

            float RoundedFloat(float rawFloat) => (float)System.Math.Round(rawFloat, 4);
        }
    }
}