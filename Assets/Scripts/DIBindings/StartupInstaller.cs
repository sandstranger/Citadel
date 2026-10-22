using Zenject;

namespace Citadel.Game
{
    internal sealed class StartupInstaller : MonoInstaller
    {
        public override void InstallBindings() => 
            Container.Bind<IAddressableTextAssetsPreparer>()
                .FromInstance(new AddressableTextAssetsPreparer()).AsSingle();
    }
}
