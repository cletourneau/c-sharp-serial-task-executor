using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Threading
{
    [TestFixture]
    class SerialTaskExecutorTest
    {
        private SerialTaskExecutor executor;

        [SetUp]
        public void CreateExecutor()
        {
            executor = new SerialTaskExecutor();
        }

        [Test]
        public void ExecutionIsAsync()
        {
            var value = 1;
            var m = new object();
            var setValueTo2 = new Action(() =>
                {
                    Thread.Sleep(100);
                    value = 2;
                    lock (m)
                    {
                        Monitor.Pulse(m);
                    }
                });

            executor.QueueTask(setValueTo2);

            Assert.That(value, Is.EqualTo(1));
            lock (m)
            {
                Monitor.Wait(m);
            }
            Assert.That(value, Is.EqualTo(2));
        }

        [Test]
        public void CanRunMultipleTasks()
        {
            var countdownEvent = new CountdownEvent(3);

            var signalCountdownEventOnce = new Action(() =>
                {
                    Thread.Sleep(50);
                    countdownEvent.Signal();
                });
            executor.QueueTask(signalCountdownEventOnce);
            executor.QueueTask(signalCountdownEventOnce);
            executor.QueueTask(signalCountdownEventOnce);

            Assert.That(countdownEvent.Wait(200), Is.True);
        }

        [Test]
        public void TasksAreRunSequentially()
        {
            var list = new List<int>();
            var countdownEvent = new CountdownEvent(3);

            var appendOne = new Action(() =>
                {
                    Thread.Sleep(200);
                    list.Add(1);
                    countdownEvent.Signal();
                });
            var appendTwo = new Action(() =>
                {
                    Thread.Sleep(100);
                    list.Add(2);
                    countdownEvent.Signal();
                });
            var appendThree = new Action(() =>
                {
                    list.Add(3);
                    countdownEvent.Signal();
                });

            executor.QueueTask(appendOne);
            executor.QueueTask(appendTwo);
            executor.QueueTask(appendThree);

            countdownEvent.Wait();

            Assert.That(list, Is.EqualTo(new List<int> {1, 2, 3}));
        }

        [Test]
        public void CanClearPendingTasks()
        {
            var value = 1;

            var setValueTo2 = new Action(() =>
                {
                    Thread.Sleep(200);
                    value = 2;
                });
            var setValueTo3 = new Action(() =>
                {
                    value = 3;
                });
            var setValueTo4 = new Action(() =>
                {
                    value = 4;
                });

            executor.QueueTask(setValueTo2);
            executor.QueueTask(setValueTo3);
            executor.QueueTask(setValueTo4);

            Thread.Sleep(50);

            executor.ClearPendingTasks();

            Thread.Sleep(200);

            Assert.That(value, Is.EqualTo(2));
        }
    }
}


