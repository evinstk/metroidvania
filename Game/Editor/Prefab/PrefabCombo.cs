using Nez;
using Nez.ImGuiTools.TypeInspectors;

namespace Game.Editor.Prefab
{
    [CustomInspector(typeof(PrefabComboInspector))]
    class PrefabCombo
    {
        public string PrefabId;
    }

    class PrefabComboInspector : AbstractTypeInspector
    {
        PrefabManager _prefabManager;

        public override void Initialize()
        {
            _prefabManager = Core.GetGlobalManager<PrefabManager>();
        }

        public override void DrawMutable()
        {
            var obj = GetValue<PrefabCombo>();
            _prefabManager.Combo($"{_name}", ref obj.PrefabId);
        }
    }
}
