using System.Collections.Generic;
using UnityEngine;

namespace Citadel.Game
{
    public interface ITexturesStorage
    {
        IReadOnlyList<Sprite> BlockedBySecuritySprites { get; }
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