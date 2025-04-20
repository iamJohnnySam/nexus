using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexusWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double _collapsedWidth = 75;
        private readonly double _expandedWidth = 200;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Sidebar_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateSidebarWidth(_expandedWidth);
        }

        private void Sidebar_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateSidebarWidth(_collapsedWidth);
        }

        private void AnimateSidebarWidth(double toWidth)
        {
            var animation = new DoubleAnimation
            {
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(200),
                AccelerationRatio = 0.2,
                DecelerationRatio = 0.2
            };
            Sidebar.BeginAnimation(WidthProperty, animation);
        }
    }
}