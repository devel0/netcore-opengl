using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Window = Avalonia.Controls.Window;
using static SearchAThing.OpenGlToolkit;

namespace SearchAThing.SciExamples
{
    public class MainWindow : Window
    {

        #region Model        
        private OpenGlModelBase _Model = null;

        public static readonly DirectProperty<MainWindow, OpenGlModelBase> ModelProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, OpenGlModelBase>("Model", o => o.Model, (o, v) => o.Model = v);

        public OpenGlModelBase Model
        {
            get => _Model;
            set => SetAndRaise(ModelProperty, ref _Model, value);
        }
        #endregion

        #region CreateControlSample
        private Func<Control> _CreateControlSample = null;

        public static readonly DirectProperty<MainWindow, Func<Control>> CreateControlSampleProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, Func<Control>>("CreateControlSample", o => o.CreateControlSample, (o, v) => o.CreateControlSample = v);

        public Func<Control> CreateControlSample
        {
            get => _CreateControlSample;
            set => SetAndRaise(CreateControlSampleProperty, ref _CreateControlSample, value);
        }
        #endregion                 

        public OpenGlControl NewGlControl() => new SampleGlControl();

        void GlPointerPressed(object sender, PointerPressedEventArgs e)
        {

        }

        void GlPointerMoved(object sender, PointerEventArgs e)
        {

        }

        GridSplitterManager gridSplitterManager;

        public MainWindow()
        {
            InitializeComponent();

            gridSplitterManager = this.FindControl<GridSplitterManager>("gridSplitterManager");

            Model = new SampleGlModel(new OpenGlModelOptions
            {
                Debug = true
            });

            var obs = Model.GetObservable(OpenGlModelBase.FocusedControlProperty);
            obs.Subscribe(value =>
            {
                if (value == null) return;
            });

            CreateControlSample = () =>
            {
                var fc = Model.FocusedControl as SampleGlControl;

                var ctl = new SampleGlControl((x) =>
                {
                    //setRndColor(x);
                    if (fc != null)
                    {
                        x.ShowOrbit = fc.ShowOrbit;
                        x.ShowModel = fc.ShowModel;
                        x.ShowModelBBox = fc.ShowModelBBox;
                        x.Perspective = fc.Perspective;
                    }
                });
                ctl.Model = Model;
                Model.FocusedControl = ctl;

                return ctl;
            };

        }

        private void click_random(object sender, RoutedEventArgs e)
        {
            var ctl = Model.FocusedControl as SampleGlControl;
            setRndColor(ctl);
        }

        void setRndColor(SampleGlControl ctl)
        {
            ctl.ObjColor = RndColor(0.5, 0.8);
        }

        private void click_reset(object sender, RoutedEventArgs e)
        {
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.Reset();
        }

        private void click_exportDxf(object sender, RoutedEventArgs e)
        {
            var ctl = Model.FocusedControl as SampleGlControl;
            var model = ctl.Model as SampleGlModel;
            model.exportDxfPending = "Model.dxf";
        }

        private void splitHorizontalClick(object sender, RoutedEventArgs e)
        {
            gridSplitterManager.Split(GridSplitDirection.Horizontally);
        }

        private void splitVerticalClick(object sender, RoutedEventArgs e)
        {
            gridSplitterManager.Split(GridSplitDirection.Vertically);
        }

        private void removeViewClick(object sender, RoutedEventArgs e)
        {
            gridSplitterManager.Remove();
        }

        private void viewLeftClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewLeft();
        }

        private void viewRightClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewRight();
        }

        private void viewTopClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewTop();
        }

        private void viewBottomClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewBottom();
        }

        private void zoomFitClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ZoomFit();
        }

        private void viewFrontClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewFront();
        }

        private void viewBackClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ViewBack();
        }

        private void viewPerspectiveClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.SetViewProjection(perspective: true);
        }

        private void viewOrthoClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.SetViewProjection(perspective: false);
        }

        private void viewWireframeClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.Wireframe = !ctl.Wireframe;
        }

        private void ortbitClick(object sender, RoutedEventArgs e)
        {
            var Model = this.Model as SampleGlModel;
            var ctl = Model.FocusedControl as SampleGlControl;
            ctl.ShowOrbit = !ctl.ShowOrbit;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}