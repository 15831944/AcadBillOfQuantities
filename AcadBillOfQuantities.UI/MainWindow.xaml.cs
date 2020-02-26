using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AcadBillOfQuantities.UI.ViewModel;

namespace AcadBillOfQuantities.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Handle closing event to hide window instead of closing it

            Closing += delegate (object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                (DispatcherOperationCallback)(arg =>
                {
                    Hide();
                    return null;
                }), null);
            };

            // if opened from Acad dont assign Datacontext from here
            if (Assembly.GetEntryAssembly() != null)
                DataContext = ViewModelLocator.Instance.Main;
        }
    }
}
