using Citadel.SceneManagement;
using Zenject;

namespace Citadel.Game
{
    internal sealed class StartupInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ScenesLoader>().
                FromInstance(ScenesLoader.Default).AsSingle();
        }
    }
}
