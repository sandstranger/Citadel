using System.Collections.Generic;
using UnityEngine;

namespace Citadel.Game
{
    [CreateAssetMenu(fileName = "TexturesStorage", menuName = "ScriptableObjects/Create TexturesStorageScriptableObject", order = 1)]
    public sealed class TexturesStorage : ScriptableObject
    {
        [SerializeField] 
        private List<Texture> _sequenceTextures = new();

        [SerializeField]
        private List<Sprite> _logImages = new();
        
        [SerializeField]
        private List<Sprite> _usableItemsFrobIcons = new();
        
        [SerializeField]
        private List<Sprite> _usableItemsIcons = new();

        public List<Texture> SequenceTextures => _sequenceTextures;

        public List<Sprite> LOGImages => _logImages;

        public Sprite NullableItemIcon => _usableItemsIcons[0];
            
        public Sprite GetItemFrobIcon(int iconIndex)
        {
            return _usableItemsFrobIcons[iconIndex];
        }

        public Sprite GetItemIcon(int iconIndex)
        {
            return _usableItemsIcons[iconIndex];
        }
        
        public Sprite GetWeaponCursor(int weaponIndex) {
            switch(weaponIndex) {
                case 36: return _usableItemsFrobIcons[102]; // red
                case 37: return _usableItemsFrobIcons[107]; // blue
                case 38: return _usableItemsFrobIcons[102]; // red
                case 39: return _usableItemsFrobIcons[105]; // green
                case 40: return _usableItemsFrobIcons[107]; // blue
                case 41: return _usableItemsFrobIcons[103]; // orange
                case 42: return _usableItemsFrobIcons[103]; // orange
                case 43: return _usableItemsFrobIcons[102]; // red
                case 44: return _usableItemsFrobIcons[104]; // yellow
                case 45: return _usableItemsFrobIcons[102]; // red
                case 46: return _usableItemsFrobIcons[106]; // teal
                case 47: return _usableItemsFrobIcons[104]; // yellow
                case 48: return _usableItemsFrobIcons[102]; // red
                case 49: return _usableItemsFrobIcons[105]; // green
                case 50: return _usableItemsFrobIcons[107]; // blue
                case 51: return _usableItemsFrobIcons[106]; // teal
                default: return _usableItemsFrobIcons[105]; // green
            }	
        }
    }
}