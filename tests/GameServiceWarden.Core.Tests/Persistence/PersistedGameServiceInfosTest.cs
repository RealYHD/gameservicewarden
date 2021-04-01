using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.Core.Tests.Modules;
using GameServiceWarden.Core.Tests.Modules.Games;
using GameServiceWarden.ModuleAPI;
using Xunit;

namespace GameServiceWarden.Core.Tests.Persistence
{
    public class PersistedGameServiceInfosTest
    {
        //MethodTested_ScenarioTested_ExpectedBehavior
        [Fact]
        public void GetPathForKey_PathGen_ExpectedPathResult()
        {
            //Given
            const string TEST_DIR = "services";
            const string MODULE_NAME = "fake_module";
            const string ASSEMBLY_NAME = "fake_assembly";
            const string SERVICE_NAME = "fake_service";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            Dictionary<string, IGameServiceModule> stubAssemblyDict = new Dictionary<string, IGameServiceModule>();
            FakeGameServiceModule stubGameServiceModule = new FakeGameServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubGameServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            PersistedGameServiceInfos persistedGameServices = new PersistedGameServiceInfos(TEST_DIR, stubModulesPersistence);

            //Then
            Assert.True(persistedGameServices.GetPathForKey(SERVICE_NAME).Equals(Path.Combine(TEST_DIR, SERVICE_NAME)));
        }

        [Fact]
        public void Save_SavingService_FileCreated()
        {
            //Given
            const string TEST_DIR = "services";
            const string MODULE_NAME = "fake_module";
            const string ASSEMBLY_NAME = "fake_assembly";
            const string SERVICE_NAME = "fake_service";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            Dictionary<string, IGameServiceModule> stubAssemblyDict = new Dictionary<string, IGameServiceModule>();
            FakeGameServiceModule stubGameServiceModule = new FakeGameServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubGameServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            PersistedGameServiceInfos persistedGameServiceInfos = new PersistedGameServiceInfos(TEST_DIR, stubModulesPersistence);

            GameServiceInfo stubGameServiceInfo = new GameServiceInfo(stubModulesPersistence[ASSEMBLY_NAME][MODULE_NAME].InstantiateGameService(persistedGameServiceInfos.GetPathForKey(SERVICE_NAME), true), MODULE_NAME, ASSEMBLY_NAME);
            //When
            persistedGameServiceInfos[SERVICE_NAME] = stubGameServiceInfo;
            //Then
            Assert.True(Directory.Exists(TEST_DIR));
            Assert.True(Directory.Exists(persistedGameServiceInfos.GetPathForKey(SERVICE_NAME)));
            string[] files = Directory.GetFiles(persistedGameServiceInfos.GetPathForKey(SERVICE_NAME));
            Assert.True(files.Length == 1);
            Assert.StartsWith(SERVICE_NAME, Path.GetFileName(files[0]));

            Directory.Delete(TEST_DIR, true);
        }

        [Fact]
        public void Save_ReadingService_MetadataRead()
        {
            //Given
            const string TEST_DIR = "services";
            const string MODULE_NAME = "fake_module";
            const string ASSEMBLY_NAME = "fake_assembly";
            const string SERVICE_NAME = "fake_service";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            Dictionary<string, IGameServiceModule> stubAssemblyDict = new Dictionary<string, IGameServiceModule>();
            FakeGameServiceModule stubGameServiceModule = new FakeGameServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubGameServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            PersistedGameServiceInfos persistedGameServices = new PersistedGameServiceInfos(TEST_DIR, stubModulesPersistence);

            GameServiceInfo stubGameServiceInfo = new GameServiceInfo(stubModulesPersistence[ASSEMBLY_NAME][MODULE_NAME].InstantiateGameService(persistedGameServices.GetPathForKey(SERVICE_NAME), true), MODULE_NAME, ASSEMBLY_NAME);
            persistedGameServices[SERVICE_NAME] = stubGameServiceInfo;
            //When
            GameServiceInfo loadedService = persistedGameServices[SERVICE_NAME];
            //Then
            Assert.True(loadedService.ModuleName.Equals(MODULE_NAME));
            Assert.True(loadedService.GetAssemblyName().Equals(ASSEMBLY_NAME));

            Directory.Delete(TEST_DIR, true);
        }
    }
}