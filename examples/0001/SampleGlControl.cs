namespace SearchAThing.SciExamples
{

    public class SampleGlControl : OpenGlControl
    {

        public SampleGlControl()
        {
        }

        protected override void OnModelAttached()
        {
            InvalidateVisual();
        }

    }


}