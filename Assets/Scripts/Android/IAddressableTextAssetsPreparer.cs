using Cysharp.Threading.Tasks;

namespace Citadel.Game
{
    internal interface IAddressableTextAssetsPreparer
    {
        UniTask<bool> PrepareAsync();
    }
}