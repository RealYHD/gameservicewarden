using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using GameServiceWarden.Core.Games;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.API.Module;
using Xunit;
using Xunit.Abstractions;

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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
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
        public void ExecuteCommand_CommandExecutedBeforeConnected_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "ExecuteCommand_CommandExecutedBeforeConnected_CommandLogged";
            const string COMMAND = "TEST";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            string pipeName = serviceManager.GetLogPipeNames()[FAKE_SERVICE_NAME];
            NamedPipeClientStream clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
            serviceManager.ExecuteCommand(FAKE_SERVICE_NAME, COMMAND);
            clientStream.Connect(1000);
            Thread.Sleep(1000);
            //Then
            byte[] buffer = new byte[1024 * 8];
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            ValueTask<int> task = clientStream.ReadAsync(buffer, cancelToken.Token);
            Assert.False(task.AsTask().Wait(1000));
            serviceManager.StopService(FAKE_SERVICE_NAME);
        }

        [Fact]
        public void ExecuteCommand_CommandExecutedAfterConnected_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "ExecuteCommand_CommandExecutedAfterConnected_CommandLogged";
            const string COMMAND = "TEST";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            string pipeName = serviceManager.GetLogPipeNames()[FAKE_SERVICE_NAME];
            NamedPipeClientStream clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
            clientStream.Connect(1000);
            Thread.Sleep(1000);
            serviceManager.ExecuteCommand(FAKE_SERVICE_NAME, COMMAND);
            //Then
            using (StreamReader reader = new StreamReader(clientStream))
            {
                CancellationTokenSource cancelToken = new CancellationTokenSource();
                string message = null;
                Task task = Task.Run(() => message = reader.ReadLine(), cancelToken.Token);
                Assert.True(task.Wait(1000));
                Assert.True(COMMAND.Equals(message), $"Received message \"{message}\" when expecting \"{COMMAND}\"");
            }
            serviceManager.StopService(FAKE_SERVICE_NAME);
        }

[Fact]
        public void ExecuteCommand_CommandExecutedAfterMultipleLogListenersConnected_CommandLogged()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "ExecuteCommand_CommandExecutedAfterMultipleLogListenersConnected_CommandLogged";
            const string COMMAND = "TEST";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            serviceManager.StartService(FAKE_SERVICE_NAME);
            string pipeName = serviceManager.GetLogPipeNames()[FAKE_SERVICE_NAME];
            NamedPipeClientStream[] clientStreams = new NamedPipeClientStream[5];
            for (int i = 0; i < clientStreams.Length; i++)
            {
                clientStreams[i] = new NamedPipeClientStream(".", pipeName, PipeDirection.In);
                clientStreams[i].Connect(1000);
            }
            Thread.Sleep(1000);
            serviceManager.ExecuteCommand(FAKE_SERVICE_NAME, COMMAND);
            //Then
            for (int i = 0; i < clientStreams.Length; i++)
            {
                using (StreamReader reader = new StreamReader(clientStreams[i]))
                {
                    CancellationTokenSource cancelToken = new CancellationTokenSource();
                    string message = null;
                    Task task = Task.Run(() => message = reader.ReadLine(), cancelToken.Token);
                    Assert.True(task.Wait(10000));
                    Assert.True(COMMAND.Equals(message), $"Received message \"{message}\" when expecting \"{COMMAND}\"");
                }
            }
            serviceManager.StopService(FAKE_SERVICE_NAME);
        }

        [Fact]
        public void GetServiceConsoleStream_ServiceStopped_ExceptionThrown()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string FAKE_SERVICE_NAME = "GetServiceConsoleStream_ServiceStopped_ExceptionThrown";
            FakePersistence<IReadOnlyDictionary<string, IServiceModule>> stubPersistentModuleDictionary = new FakePersistence<IReadOnlyDictionary<string, IServiceModule>>();
            FakePersistence<ServiceDescriptor> stubPersistentServiceDictionary = new FakePersistence<ServiceDescriptor>();
            FakeServiceManagerMonitor stubMonitor = new FakeServiceManagerMonitor();
            ServiceManager serviceManager = new ServiceManager(stubMonitor, stubPersistentServiceDictionary, stubPersistentModuleDictionary);
            Dictionary<string, IServiceModule> stubAssemblyModulesDictionary = new Dictionary<string, IServiceModule>();
            IServiceModule stubServiceModule = new FakeServiceModule();
            stubAssemblyModulesDictionary.Add(stubServiceModule.Name, stubServiceModule);
            stubPersistentModuleDictionary.AddToPersistence(ASSEMBLY_NAME, stubAssemblyModulesDictionary);
            //When
            serviceManager.CreateService(FAKE_SERVICE_NAME, ASSEMBLY_NAME, stubServiceModule.Name);
            //Then
            Assert.Throws<KeyNotFoundException>(() => serviceManager.GetLogPipeNames()[FAKE_SERVICE_NAME]);
        }
    }
}