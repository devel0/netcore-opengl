using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Window = Avalonia.Controls.Window;

namespace SearchAThing.SciExamples
{
    public class MainWindow : Window
    {

        #region Model
        private SampleGlModel _Model = null;

        public static readonly DirectProperty<MainWindow, SampleGlModel> ModelProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, SampleGlModel>("Model", o => o.Model, (o, v) => o.Model = v);

        public SampleGlModel Model
        {
            get => _Model;
            set => SetAndRaise(ModelProperty, ref _Model, value);
        }
        #endregion

        SampleGlControl glCtl;

        public MainWindow()
        {
            InitializeComponent();

            glCtl = this.FindControl<SampleGlControl>("glCtl");

            Model = new SampleGlModel(new OpenGlModelOptions
            {
                Debug = true
            });
            Model.AffectsRenderPropChanged += () =>
            {
                glCtl.InvalidateVisual();
            };
        }

        private void click_random(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            Model.ObjColorRed = (float)rnd.NextDouble();
            Model.ObjColorGreen = (float)rnd.NextDouble();
            Model.ObjColorBlue = (float)rnd.NextDouble();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}