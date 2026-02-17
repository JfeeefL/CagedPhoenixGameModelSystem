using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Caged_Phoenix.Scripts.GameModelSystem.Config
{
    [Serializable]
    public class GameModelConfig
    {
        
        [CustomValueDrawer("DrawGMIDSelector")]
        public uint TargetGMID;

        protected GameModelDefBaseSO DefSo;

        protected GameModelFieldDef GetDef() => DefSo.GetDef(TargetGMID);


        protected uint DrawGMIDSelector(uint value, GUIContent label)
        {
            return default;
        }
    }
}