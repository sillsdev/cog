using System.Windows;

namespace SIL.Cog.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation
    {
        public LoadingAnimation()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LoadingProperty = DependencyProperty.Register(
            "Loading",                             // The name of the dependency property to register
            typeof(bool),                       // The type of the property
            typeof(LoadingAnimation),               // The owner type that is registering the dependency property.
            new FrameworkPropertyMetadata(      // Property metadata for the dependency property
                false
                )
            );

        public bool Loading
        {
            get { return (bool) GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }
    }
}
