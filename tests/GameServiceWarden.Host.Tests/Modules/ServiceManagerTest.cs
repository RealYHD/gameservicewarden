using System.IO;
using GameServiceWarden.Host.Modules;
using GameServiceWarden.ModuleAPI;
using Xunit;

namespace GameServiceWarden.Host.Tests.Modules
{
    public class ServiceManagerTest
    {
        [Fact]
        public void AddModule_NewManager_SuccessfulAddition()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            //Then
            Assert.Contains<string>(ASSEMBLY_NAME, serviceManager.GetAssemblyNames());
            Assert.Contains<string>(stubGameServiceModule.Name, serviceManager.GetModuleNames(ASSEMBLY_NAME));
        }

        [Fact]
        public void RemoveModule_NewManager_SuccessfulRemoval()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.RemoveModule(ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.DoesNotContain<string>(ASSEMBLY_NAME, serviceManager.GetAssemblyNames());
        }

        [Fact]
        public void CreateService_NewManager_NewServiceCreated()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            //When
            serviceManager.CreateService("FakeService", ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Contains<string>(FAKE_SERVICE_NAME, serviceManager.GetServiceNames());
        }

        [Fact]
        public void GetServiceNames_MultipleServices_AllCorrectNames()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_PREFIX = "FakeService_";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
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
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule(
                new FakeConfigurable("A"),
                new FakeConfigurable("B"),
                new FakeConfigurable("C")
            );
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Contains<string>("A", serviceManager.GetServiceOptions(SERVICE_NAME));
            Assert.Contains<string>("B", serviceManager.GetServiceOptions(SERVICE_NAME));
            Assert.Contains<string>("C", serviceManager.GetServiceOptions(SERVICE_NAME));
        }

        [Fact]
        public void SetandGetServiceOptionValue_OneOption_OptionChanged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule(
                new FakeConfigurable("A")
            );
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.SetServiceOptionValue(SERVICE_NAME, "A", "Test");
            //Then
            Assert.Equal<string>("Test", serviceManager.GetServiceOptionValue(SERVICE_NAME, "A"));
        }

        [Fact]
        public void GetServiceState_NotRunning_ReturnsNotRunningState()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Stopped, serviceManager.GetServiceState(SERVICE_NAME));
        }

        [Fact]
        public void GetServiceState_Running_ReturnsNotRunningState()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Running, serviceManager.GetServiceState(SERVICE_NAME));
        }

        [Fact]
        public void StartService_NotStarted_SuccessfulStart()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Running, serviceManager.GetServiceState(SERVICE_NAME));
        }

        [Fact]
        public void StopService_Stopped_StateUpdated()
        {
            //Given        
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(SERVICE_NAME);
            serviceManager.StopService(SERVICE_NAME);
            //Then
            Assert.Equal<ServiceState>(ServiceState.Stopped, serviceManager.GetServiceState(SERVICE_NAME));
        }

        [Fact]
        public void ExecuteCommand_ServiceStarted_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            serviceManager.StartService(SERVICE_NAME);
            serviceManager.ExecuteCommand(SERVICE_NAME, "Test");
            //Then
            Stream stream = serviceManager.GetServiceConsoleStream(SERVICE_NAME);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                Assert.Equal<string>("Test", reader.ReadLine());
            }
        }

        [Fact]
        public void GetServiceConsoleStream_ServiceStopped_ExceptionThrown()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "FakeService";
            ServiceManager serviceManager = new ServiceManager();
            IGameServiceModule stubGameServiceModule = new FakeGameServiceModule();
            //When
            serviceManager.AddModule(ASSEMBLY_NAME, stubGameServiceModule);
            serviceManager.CreateService(SERVICE_NAME, ASSEMBLY_NAME, stubGameServiceModule.Name);
            //Then
            Assert.Null(serviceManager.GetServiceConsoleStream(SERVICE_NAME));
        }
    }
}