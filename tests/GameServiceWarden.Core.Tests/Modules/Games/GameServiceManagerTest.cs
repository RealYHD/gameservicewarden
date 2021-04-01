using System;
using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.ModuleAPI;
using Xunit;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    public class GameServiceManagerTest
    {

        [Fact]
        public void CreateService_NewManager_NewServiceCreated()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Contains<string>(FAKE_SERVICE_NAME, serviceManager.GetServiceNames());
        }

        [Fact]
        public void CreateService_OneService_ServiceDeleted()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.DeleteService(FAKE_SERVICE_NAME);
            //Then
            Assert.DoesNotContain<string>(FAKE_SERVICE_NAME, serviceManager.GetServiceNames());
        }

        [Fact]
        public void GetServiceNames_MultipleServices_AllCorrectNames()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_PREFIX = "FakeService_";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            for (int i = 0; i < 100; i++)
            {
                serviceManager.CreateService(FAKE_SERVICE_PREFIX + i, ASSEMBLY_NAME, stubGameServiceModule.Name);
            }
            //Then
            for (int i = 0; i < 100; i++)
            {
                Assert.Contains<string>(FAKE_SERVICE_PREFIX + i, serviceManager.GetServiceNames());
            }
        }

        [Fact]
        public void GetServiceOptions_ThreeOptionService_CorrectOptions()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule(
                new FakeGameConfigurable("A"),
                new FakeGameConfigurable("B"),
                new FakeGameConfigurable("C")
            );
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Contains<string>("A", serviceManager.GetServiceOptions(FAKE_SERVICE_NAME));
            Assert.Contains<string>("B", serviceManager.GetServiceOptions(FAKE_SERVICE_NAME));
            Assert.Contains<string>("C", serviceManager.GetServiceOptions(FAKE_SERVICE_NAME));
        }

        [Fact]
        public void SetandGetServiceOptionValue_OneOption_OptionChanged()
        {
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule(
                new FakeGameConfigurable("A")
            );
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.SetServiceOptionValue(FAKE_SERVICE_NAME, "A", "Test");
            //Then
            Assert.True("Test".Equals(serviceManager.GetServiceOptionValue(FAKE_SERVICE_NAME, "A")));
        }

        [Fact]
        public void GetServiceState_NotRunning_ReturnsNotRunningState()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Stopped, serviceManager.GetServiceState(FAKE_SERVICE_NAME));
        }

        [Fact]
        public void GetServiceState_Running_ReturnsNotRunningState()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Running, serviceManager.GetServiceState(FAKE_SERVICE_NAME));
        }

        [Fact]
        public void StartService_NotStarted_SuccessfulStart()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Running, serviceManager.GetServiceState(FAKE_SERVICE_NAME));
        }

        [Fact]
        public void StopService_Stopped_StateUpdated()
        {
            //Given        
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            serviceManager.StopService(FAKE_SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Stopped, serviceManager.GetServiceState(FAKE_SERVICE_NAME));
        }

        [Fact]
        public void ExecuteCommand_ServiceStarted_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            serviceManager.ExecuteCommand(FAKE_SERVICE_NAME, "Test");
            //Then
            Stream stream = serviceManager.GetServiceConsoleStream(FAKE_SERVICE_NAME);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                Assert.True("Test".Equals(reader.ReadLine()));
            }
        }

        [Fact]
        public void GetServiceConsoleStream_ServiceStopped_ExceptionThrown()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IGameServiceModule>>();
            FakePersistence<GameServiceInfo> stubPersistentServiceDictionary = new FakePersistence<GameServiceInfo>();
            GameServiceManager serviceManager = new GameServiceManager(stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IGameServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IGameServiceModule>();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            stubAssemblyModulesDictionary.Add(stubGameServiceModule.Name, stubGameServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Action action = delegate()
            {
                serviceManager.GetServiceConsoleStream(FAKE_SERVICE_NAME);
            };
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}