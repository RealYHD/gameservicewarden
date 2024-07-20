using System;
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
            const string NAME = "Open_Closed_Opened";
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
            const string NAME = "Open_AlreadyOpened_Exception";
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
            const string NAME = "Close_Opened_Closed";
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