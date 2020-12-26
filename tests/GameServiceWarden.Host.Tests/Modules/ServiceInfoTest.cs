using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Host.Modules;
using GameServiceWarden.ModuleAPI;
using Xunit;

namespace GameServiceWarden.Host.Tests.Modules
{
    // Testing convention from: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
    // Fakes are generic test objects, 
    // mocks are the objects being asserted upon, 
    // stubs are objects used as part of the test.
    public class ServiceInfoTest
    {
        //MethodTested_ScenarioTested_ExpectedBehavior
        [Fact]
        public void Start_FromStopped_StateIsRunning()
        {
            //Arrange, Act, Assert
            IGameService stubGameService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubGameService, "FakeModule", "FakeAssembly");
            serviceInfo.Start();
            Assert.Equal(ServiceState.Running, serviceInfo.GetServiceState());
            serviceInfo.Dispose();
        }

        [Fact]
        public void Stop_FromStart_Stopped()
        {
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            serviceInfo.Start();
            serviceInfo.Stop();
            Assert.Equal(ServiceState.Stopped, serviceInfo.GetServiceState());
            serviceInfo.Dispose();
        }

        [Fact]
        public void GetConfigurableOptions_ServiceStopped_ReturnsConfigurables()
        {
            //Given
            FakeService stubService = new FakeService();
            FakeConfigurable stubConfigurable = new FakeConfigurable("Option");
            HashSet<IConfigurable> configurables = new HashSet<IConfigurable>();
            configurables.Add(stubConfigurable);
            stubService.Configurables = configurables;
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //Then
            Assert.Contains<string>(stubConfigurable.OptionName, serviceInfo.GetConfigurableOptions());
            serviceInfo.Dispose();
        }

        [Fact]
        public void SetAndGetConfigurationValue_ServiceStopped_AppropriateValueReturned()
        {
            //Given
            FakeService stubService = new FakeService();
            FakeConfigurable stubConfigurable = new FakeConfigurable("Option");
            HashSet<IConfigurable> configurables = new HashSet<IConfigurable>();
            configurables.Add(stubConfigurable);
            stubService.Configurables = configurables;
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.SetConfigurableValue(stubConfigurable.OptionName, "success");
            //Then
            Assert.Equal<string>("success", serviceInfo.GetConfigurableValue(stubConfigurable.OptionName));
        }

        [Fact]
        public void GetServiceState_ServiceNotStarted_ReturnsStoppedState()
        {
            //Given
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //Then
            Assert.Equal(ServiceState.Stopped, serviceInfo.GetServiceState());
            serviceInfo.Dispose();
        }

        [Fact]
        public void GetServiceState_ServiceStarted_ReturnsRunningState()
        {
            //Given
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.Start();
            //Then
            Assert.Equal(ServiceState.Running, serviceInfo.GetServiceState());
            serviceInfo.Dispose();
        }

        [Fact]
        public void GetModuleName_ServiceNotStarted_ReturnsSetName()
        {
            //Given
            const string MODULE_NAME = "FakeModule";
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, MODULE_NAME, "FakeAssembly");
            //Then
            Assert.Equal(MODULE_NAME, serviceInfo.GetModuleName());
            serviceInfo.Dispose();
        }

        [Fact]
        public void GetAssemblyName_ServiceNotStarted_ReturnsSetAssemblyName()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", ASSEMBLY_NAME);
            //Then
            Assert.Equal(ASSEMBLY_NAME, serviceInfo.GetAssemblyName());
            serviceInfo.Dispose();
        }

        [Fact]
        public void SetAndGetServiceName_ServiceNotStartedSingleThread_ServiceNameUpdated()
        {
            //Given
            const string SERVICE_NAME = "Service";
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssemblyName");
            //When
            serviceInfo.ServiceName = SERVICE_NAME;
            //Then
            Assert.Equal(SERVICE_NAME, serviceInfo.ServiceName);
            serviceInfo.Dispose();
        }

        [Fact]
        public void ServiceConsoleStream_ServiceNotStarted_NullReturned()
        {
            //Given
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //Then
            Assert.Null(serviceInfo.ServiceConsoleStream);
            serviceInfo.Dispose();
        }

        [Fact]
        public void ServiceConsoleStream_ServiceStarted_StreamReturned()
        {
            //Given
            IGameService stubService = new FakeService();
            ServiceInfo serviceInfo = new ServiceInfo(stubService, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.Start();
            //Then
            Assert.IsAssignableFrom<Stream>(serviceInfo.ServiceConsoleStream);
            serviceInfo.Dispose();
        }
    }
}