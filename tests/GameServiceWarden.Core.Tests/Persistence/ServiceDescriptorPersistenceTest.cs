using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.Core.Persistence;
using GameServiceWarden.Core.Tests.Modules;
using GameServiceWarden.Core.Tests.Modules.Games;
using GameServiceWarden.API.Module;
using Xunit;

namespace GameServiceWarden.Core.Tests.Persistence
{
    public class ServiceDescriptorPersistenceTest
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
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            Dictionary<string, IServiceModule> stubAssemblyDict = new Dictionary<string, IServiceModule>();
            FakeServiceModule stubServiceModule = new FakeServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            ServiceDescriptorPersistence persistedServices = new ServiceDescriptorPersistence(TEST_DIR, stubModulesPersistence);

            //Then
            Assert.True(persistedServices.GetPathForKey(SERVICE_NAME).Equals(Path.Combine(TEST_DIR, SERVICE_NAME)));
        }

        [Fact]
        public void Save_SavingService_FileCreated()
        {
            //Given
            const string TEST_DIR = "services";
            const string MODULE_NAME = "fake_module";
            const string ASSEMBLY_NAME = "fake_assembly";
            const string SERVICE_NAME = "fake_service";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            Dictionary<string, IServiceModule> stubAssemblyDict = new Dictionary<string, IServiceModule>();
            FakeServiceModule stubServiceModule = new FakeServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            ServiceDescriptorPersistence persistedServiceInfos = new ServiceDescriptorPersistence(TEST_DIR, stubModulesPersistence);

            ServiceDescriptor stubServiceInfo = new ServiceDescriptor(stubModulesPersistence[ASSEMBLY_NAME][MODULE_NAME].InstantiateService(persistedServiceInfos.GetPathForKey(SERVICE_NAME), true), SERVICE_NAME, MODULE_NAME, ASSEMBLY_NAME);
            //When
            persistedServiceInfos[SERVICE_NAME] = stubServiceInfo;
            //Then
            Assert.True(Directory.Exists(TEST_DIR));
            Assert.True(Directory.Exists(persistedServiceInfos.GetPathForKey(SERVICE_NAME)));
            string[] files = Directory.GetFiles(persistedServiceInfos.GetPathForKey(SERVICE_NAME));
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
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubModulesPersistence = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            Dictionary<string, IServiceModule> stubAssemblyDict = new Dictionary<string, IServiceModule>();
            FakeServiceModule stubServiceModule = new FakeServiceModule();

            stubAssemblyDict[MODULE_NAME] = stubServiceModule;
            stubModulesPersistence[ASSEMBLY_NAME] = stubAssemblyDict;

            ServiceDescriptorPersistence persistedServices = new ServiceDescriptorPersistence(TEST_DIR, stubModulesPersistence);

            ServiceDescriptor stubServiceInfo = new ServiceDescriptor(stubModulesPersistence[ASSEMBLY_NAME][MODULE_NAME].InstantiateService(persistedServices.GetPathForKey(SERVICE_NAME), true), SERVICE_NAME, MODULE_NAME, ASSEMBLY_NAME);
            persistedServices[SERVICE_NAME] = stubServiceInfo;
            //When
            ServiceDescriptor loadedService = persistedServices[SERVICE_NAME];
            //Then
            Assert.True(loadedService.GetModuleName().Equals(MODULE_NAME));
            Assert.True(loadedService.GetAssemblyName().Equals(ASSEMBLY_NAME));

            Directory.Delete(TEST_DIR, true);
        }
    }
}