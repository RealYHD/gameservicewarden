using System;
using GameServiceWarden.Core.Logging;
using GameServiceWarden.Core.UI;
using Xunit;
using Xunit.Abstractions;

namespace GameServiceWarden.Core.Tests.UI
{
    public class IPCMediatorTest
    {
        public IPCMediatorTest(ITestOutputHelper output)
        {
            Logger.AddLogListener(new XUnitLogger(output));
        }

        [Fact]
        public void Open_Closed_Opened()
        {
            //Given
            const string NAME = "MEDIATOR";
            IPCMediator mediator = new IPCMediator(NAME);
            //When
            mediator.Open();
            //Then
            Assert.True(mediator.IsRunning);
        }

        [Fact]
        public void Open_AlreadyOpened_Exception()
        {
            //Given
            const string NAME = "MEDIATOR";
            IPCMediator mediator = new IPCMediator(NAME);
            //When
            mediator.Open();
            //Then
            Assert.Throws<InvalidOperationException>(() => mediator.Open());
        }

        [Fact]
        public void Close_Opened_Closed()
        {
            //Given
            const string NAME = "MEDIATOR";
            IPCMediator mediator = new IPCMediator(NAME);
            //When
            mediator.Open();
            //Then
            Assert.True(mediator.IsRunning);
            mediator.Close();
            Assert.False(mediator.IsRunning);
        }
    }
}