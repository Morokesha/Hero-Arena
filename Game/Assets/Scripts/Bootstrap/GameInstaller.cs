using System;
using Core.Battle;
using Services.AssetServices;
using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] 
        private GameBootstrapper gameBootstrapper;
        [Header("Для сцены меню")]
        [SerializeField]
        private MenuWindowController menuController;
        [Header("Для игровой сцены")]
        [SerializeField]
        private BattleSpawner battleSpawner;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();
            builder.Register<AssetProvider>(Lifetime.Singleton).As<IAssetProvider>();
            builder.Register<StaticDataService>(Lifetime.Singleton).As<IStaticDataService>();
            builder.Register<GameFactoryService>(Lifetime.Singleton).As<IGameFactoryService>();
            builder.Register<FactoryUIService>(Lifetime.Singleton).As<IFactoryUIService>();
            
            RegistrationComponents(builder);
        }

        private void RegistrationComponents(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameBootstrapper);
            
            if (menuController != null)
                builder.RegisterComponent(menuController);
            if (battleSpawner != null) builder.RegisterComponent(battleSpawner);
        }
    }
}