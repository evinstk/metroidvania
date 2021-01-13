﻿namespace Game.Editor.Scriptable
{
    class ScriptableObjectManager<T> : Manager<T>
        where T : ScriptableObject, IResource, new()
    {
        public override string Path => ContentPath.ScriptableObjects + typeof(T).Name + "/";
    }
}