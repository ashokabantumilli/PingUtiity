using PingAnalysis;
using System.Windows;

namespace TestPing
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Thread thread = new Thread(() =>
            {
                int count = 0;
                var application = new App();

                while (true)
                {
                    application.Dispatcher.InvokeAsync(() =>
                    {
                        var vm = new MainWindow();
                        vm.Show();
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    Thread.Sleep(1000);
                    if (count++ == 5)
                    {
                        break;
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}