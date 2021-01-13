using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Game.Editor.Tool;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools.TypeInspectors;
using System;
using System.Collections.Generic;

namespace Game.Editor
{
    static class EditorState
    {
        public static RoomData RoomData => Core.GetGlobalManager<RoomManager>().GetResource(SelectedRoomId);
        public static string SelectedRoomId = null;

        public static PrefabData SelectedPrefab => Core.GetGlobalManager<PrefabManager>().GetResource(SelectedPrefabId);
        public static string SelectedPrefabId = null;

        public static Tools CurrentTool { get; set; } = Tools.Brush;

        public static string SelectedAnimationId = null;

        public static string TilesetTextureFile = null;
        public static Point TileSelection;

        public static RoomLayer SelectedLayer =>
            SelectedLayerIndex >= 0 && SelectedLayerIndex < RoomData?.Layers.Count
            ? RoomData.Layers[SelectedLayerIndex]
            : null;
        public static int SelectedLayerIndex = 0;

        public static RoomEntity SelectedEntity => RoomData?.Entities.Find(e => e.Id == SelectedEntityId);
        public static string SelectedEntityId = null;

        public static ScriptableObject SelectedScriptableObject
        {
            get => _selectedScriptableObject;
            set
            {
                _selectedScriptableObject = value;
                SelectedScriptableObjectInspectors = null;
                if (value != null)
                {
                    SelectedScriptableObjectInspectors = TypeInspectorUtils.GetInspectableProperties(value);
                }
            }
        }
        static ScriptableObject _selectedScriptableObject;
        public static List<AbstractTypeInspector> SelectedScriptableObjectInspectors = null;

        public static Exception Exception;
    }
}
