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
using System.Windows.Shapes;

namespace LicenseUserApp
{
    /// <summary>
    /// Interaction logic for AssignLicense.xaml
    /// </summary>
    public partial class AssignLicense : Window
    {
        public AssignLicense()
        {
            InitializeComponent();
        }

        public LicenseType Type { get; private set; } = LicenseType.Unset;

        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            if (rbGold.IsChecked == true)
            {
                Type = LicenseType.Gold;
            }

            if (rbSilver.IsChecked == true)
            {
                Type = LicenseType.Silver;
            }

        }
    }
}
