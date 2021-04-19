using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using GameServiceWarden.Core.Module;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.API.Module;
using Xunit;
using Xunit.Abstractions;
using System.Text;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace GameServiceWarden.Core.Tests.Modules.Games
{
    [CollectionDefinition("Service")]
    public class ServiceManagerTest
    {
        public ServiceManagerTest(ITestOutputHelper output)
        {
            Logger.AddLogListener(new XUnitLogger(output));
        }

        [Fact]
        public void CreateService_NewManager_NewServiceCreated()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "CreateService_NewManager_NewServiceCreated";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            //Then
            Assert.Contains<string>(FAKE_SERVICE_NAME, stubMonitor.GetLastState().services);
        }

        [Fact]
        public void CreateService_OneService_ServiceDeleted()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "CreateService_OneService_ServiceDeleted";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.DeleteService(FAKE_SERVICE_NAME);
            //Then
            Assert.True(stubMonitor.GetLastState().delta);
            Assert.True(stubMonitor.GetLastState().subtract);
            Assert.Contains(FAKE_SERVICE_NAME, stubMonitor.GetLastState().services);
        }

        [Fact]
        public void GetServiceNames_MultipleServices_AllCorrectNames()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_PREFIX = "GetServiceNames_MultipleServices_AllCorrectNames_";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            for (int i = 0; i < 100; i++)
            {
                serviceManager.CreateService(FAKE_SERVICE_PREFIX + i, ASSEMBLY_NAME, stubServiceModule.Name);
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
            const string FAKE_SERVICE_NAME = "GetServiceOptions_ThreeOptionService_CorrectOptions";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule(
                new FakeServiceConfigurable("A"),
                new FakeServiceConfigurable("B"),
                new FakeServiceConfigurable("C")
            );
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            //Then
            Assert.Contains<string>("A", serviceManager.GetOptions()[FAKE_SERVICE_NAME].Keys);
            Assert.Contains<string>("B", serviceManager.GetOptions()[FAKE_SERVICE_NAME].Keys);
            Assert.Contains<string>("C", serviceManager.GetOptions()[FAKE_SERVICE_NAME].Keys);
        }

        [Fact]
        public void SetandGetServiceOptionValue_OneOption_OptionChanged()
        {
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "SetandGetServiceOptionValue_OneOption_OptionChanged";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule(
                new FakeServiceConfigurable("A")
            );
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.SetServiceOptionValue(FAKE_SERVICE_NAME, "A", "Test");
            //Then
            Assert.True("Test".Equals(serviceManager.GetOptions()[FAKE_SERVICE_NAME]["A"]));
        }

        [Fact]
        public void GetServiceState_NotRunning_ReturnsNotRunningState()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "GetServiceState_NotRunning_ReturnsNotRunningState";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            //Then
            Assert.DoesNotContain(FAKE_SERVICE_NAME, serviceManager.GetRunningServiceNames());
        }

        [Fact]
        public void StartService_NotStarted_SuccessfulStart()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "StartService_NotStarted_SuccessfulStart";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            //Then
            Assert.Contains(FAKE_SERVICE_NAME, serviceManager.GetRunningServiceNames());
            serviceManager.StopService(FAKE_SERVICE_NAME);
        }

        [Fact]
        public void StopService_ServiceStartedThenStopped_StateUpdated()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "StopService_ServiceStartedThenStopped_StateUpdated";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            serviceManager.StopService(FAKE_SERVICE_NAME);
            //Then
            Assert.DoesNotContain(FAKE_SERVICE_NAME, serviceManager.GetRunningServiceNames());
        }

        [Fact]
        public void ExecuteCommand_ServiceStarted_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "ExecuteCommand_ServiceStarted_CommandLogged";
            const string COMMAND = "TEST";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<IReadOnlyDictionary<string, string>> stubPersistentServiceDictionary = new FakePersistence<IReadOnlyDictionary<string, string>>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            serviceManager.ExecuteCommand(FAKE_SERVICE_NAME, COMMAND);
            //Then
            using (MemoryStream mem = new MemoryStream(serviceManager.GetLogBuffer()[FAKE_SERVICE_NAME]))
            {
                using (StreamReader reader = new StreamReader(mem))
                {
                    Assert.Equal(COMMAND, reader.ReadLine());
                }
            }
            serviceManager.StopService(FAKE_SERVICE_NAME);
        }
    }
}