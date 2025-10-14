using UnityEngine;

namespace Citadel.Game
{
    public interface ITexturesStorage
    {
        int SequencesTexturesCount { get; }
        Sprite NullableItemIcon { get; }
        Texture GetScreenCode(int index);
        Sprite GetLogSprite(int position);
        Texture GetSequencesTexture(int position);
        Sprite GetItemFrobIcon(int iconIndex);
        Sprite GetItemIcon(int iconIndex);
        Sprite GetWeaponCursor(int weaponIndex);
    }
}