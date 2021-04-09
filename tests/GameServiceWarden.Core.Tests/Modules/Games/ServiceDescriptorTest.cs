using System.Collections.Generic;
using System.IO;
using GameServiceWarden.Core.Games;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.API.Module;
using Xunit;
using Xunit.Abstractions;

namespace GameServiceWarden.Core.Tests.Modules.Games
{
    // Testing convention from: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
    // fakes are generic test objects, 
    // mocks are the objects being asserted upon, 
    // stubs are objects used as part of the test.
    [CollectionDefinition("Service", DisableParallelization = true)]
    public class ServiceDescriptorTest
    {
        private readonly ITestOutputHelper output;
        private readonly XUnitLogger logger;

        public ServiceDescriptorTest(ITestOutputHelper output)
        {
            this.output = output;
            logger = new XUnitLogger(output);
            Logger.AddLogListener(logger);
        }

        //MethodTested_ScenarioTested_ExpectedBehavior
        [Fact]
        public void Start_FromStopped_StateIsRunning()
        {
            //Arrange, Act, Assert
            const string SERVICE_NAME = "Start_FromStopped_StateIsRunning";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            serviceInfo.Start();
            Assert.Equal(true, serviceInfo.GetServiceState());
            serviceInfo.Stop();
        }

        [Fact]
        public void Stop_FromStart_Stopped()
        {
            const string SERVICE_NAME = "Stop_FromStart_Stopped";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            serviceInfo.Start();
            serviceInfo.Stop();
            Assert.Equal(false, serviceInfo.GetServiceState());
        }

        [Fact]
        public void GetConfigurableOptions_ServiceStopped_ReturnsConfigurables()
        {
            //Given
            const string SERVICE_NAME = "GetConfigurableOptions_ServiceStopped_ReturnsConfigurables";
            FakeService stubService = new FakeService();
            FakeServiceConfigurable stubConfigurable = new FakeServiceConfigurable("Option");
            HashSet<IServiceConfigurable> configurables = new HashSet<IServiceConfigurable>();
            configurables.Add(stubConfigurable);
            stubService.Configurables = configurables;
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //Then
            Assert.Contains<string>(stubConfigurable.OptionName, serviceInfo.GetConfigurableOptions());
        }

        [Fact]
        public void SetAndGetConfigurationValue_ServiceStopped_AppropriateValueReturned()
        {
            //Given
            const string SERVICE_NAME = "SetAndGetConfigurationValue_ServiceStopped_AppropriateValueReturned";
            FakeService stubService = new FakeService();
            FakeServiceConfigurable stubConfigurable = new FakeServiceConfigurable("Option");
            HashSet<IServiceConfigurable> configurables = new HashSet<IServiceConfigurable>();
            configurables.Add(stubConfigurable);
            stubService.Configurables = configurables;
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.SetConfigurableValue(stubConfigurable.OptionName, "success");
            //Then
            Assert.True("success".Equals(serviceInfo.GetConfigurableValue(stubConfigurable.OptionName)));
        }

        [Fact]
        public void GetServiceState_ServiceNotStarted_ReturnsStoppedState()
        {
            //Given
            const string SERVICE_NAME = "GetServiceState_ServiceNotStarted_ReturnsStoppedState";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //Then
            Assert.False(serviceInfo.GetServiceState());
        }

        [Fact]
        public void GetServiceState_ServiceStarted_ReturnsRunningState()
        {
            //Given
            const string SERVICE_NAME = "GetServiceState_ServiceStarted_ReturnsRunningState";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.Start();
            //Then
            Assert.True(serviceInfo.GetServiceState());
            serviceInfo.Stop();
        }

        [Fact]
        public void GetModuleName_ServiceNotStarted_ReturnsSetName()
        {
            //Given
            const string SERVICE_NAME = "GetModuleName_ServiceNotStarted_ReturnsSetName";
            const string MODULE_NAME = "FakeModule";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, MODULE_NAME, "FakeAssembly");
            //Then
            Assert.Equal(MODULE_NAME, serviceInfo.GetModuleName());
        }

        [Fact]
        public void GetAssemblyName_ServiceNotStarted_ReturnsSetAssemblyName()
        {
            //Given
            const string ASSEMBLY_NAME = "FakeAssembly";
            const string SERVICE_NAME = "GetAssemblyName_ServiceNotStarted_ReturnsSetAssemblyName";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", ASSEMBLY_NAME);
            //Then
            Assert.Equal(ASSEMBLY_NAME, serviceInfo.GetAssemblyName());
        }

        [Fact]
        public void GetServiceName_ServiceNotStartedSingleThread_ServiceNameReturned()
        {
            //Given
            const string SERVICE_NAME = "GetServiceName_ServiceNotStartedSingleThread_ServiceNameReturned";
            IService stubService = new FakeService();
            //When
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssemblyName");
            //Then
            Assert.True(SERVICE_NAME.Equals(serviceInfo.ServiceName));
        }

        [Fact]
        public void ServiceLogPipeName_ServiceNotStarted_PipeNameReturned()
        {
            //Given
            const string SERVICE_NAME = "ServiceLogPipeName_ServiceNotStarted_PipeNameReturned";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //Then
            Assert.NotNull(serviceInfo.ServiceLogPipeName);
        }

        [Fact]
        public void ServiceLogPipeName_ServiceStarted_StreamReturned()
        {
            //Given
            const string SERVICE_NAME = "ServiceLogPipeName_ServiceStarted_StreamReturned";
            IService stubService = new FakeService();
            ServiceDescriptor serviceInfo = new ServiceDescriptor(stubService, SERVICE_NAME, "FakeModule", "FakeAssembly");
            //When
            serviceInfo.Start();
            //Then
            Assert.NotNull(serviceInfo.ServiceLogPipeName);
            serviceInfo.Stop();
        }
    }
}