using System;
using System.Collections.Generic;
using System.Linq;
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

namespace IntegerKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CommittedKeys> Keys { get; set; } = new List<CommittedKeys>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void tbVariable_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void FetchData(object sender, RoutedEventArgs e)
        {
            DataContext = null;

            client = new SawtoothClient(tbUrl.Text);

            Blocks.Clear();

            await LoadBlocksAsync(null); //load first page

            DataContext = this;

        }
    }
}
