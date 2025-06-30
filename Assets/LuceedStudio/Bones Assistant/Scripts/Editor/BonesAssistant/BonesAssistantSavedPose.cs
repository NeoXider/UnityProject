// Bones Assistant from Luceed Studio - https://luceed.studio
// Documentation - https://luceed.studio/bones-assistant

using System;

namespace LuceedStudio_BonesAssistant
{
    [Serializable]
    public class BonesAssistantSavedPose
    {
        public string Name;
        public float[] Pose;
        public float[] RootValues;

        public BonesAssistantSavedPose(float[] pose, float[] rootValues, string name = "Pose")
        {
            this.Name = name;
            this.Pose = pose;
            this.RootValues = rootValues;
        }
    }
}

