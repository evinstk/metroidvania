using ImGuiNET;

namespace Game.Editor.Animation
{
    class TextureMapDataManager : Manager<TextureMapData>
    {
        public override string Path => ContentPath.Textures;

        public bool FrameCombo(string textureMapId, ref string frameFilename)
        {
            var ret = false;
            if (ImGui.BeginCombo("Frame", frameFilename))
            {
                var textureMap = GetResource(textureMapId);
                if (textureMap != null)
                {
                    foreach (var spriteFrame in textureMap.frames)
                    {
                        if (ImGui.Selectable(spriteFrame.filename, spriteFrame.filename == frameFilename))
                        {
                            frameFilename = spriteFrame.filename;
                            ret = true;
                        }
                    }
                }

                ImGui.EndCombo();
            }
            return ret;
        }
    }
}
