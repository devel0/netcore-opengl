namespace test;

public class ColorTest
{
    [Fact]
    public void Test1()
    {

        // default  [6]                     [0]       [1]         [2]       [3]         [4]        [5]
        //       <------------------------------------------------------------------------------------------- HUE
        // color    Fuchsia     Violet      Blue      Cyan        Green     Yellow      Orange      Red
        // rgb      255.0.255   128.0.255   0.0.255   0.255.255   0.255.0   255.255.0   255.128.0   255.0.0
        // hue      300         270         240       180         120       60          30          0     

        var fuchsia = Color.FromArgb(255, 0, 255);
        var violet = Color.FromArgb(128, 0, 255);
        var blue = Color.FromArgb(0, 0, 255);
        var cyan = Color.FromArgb(0, 255, 255);
        var green = Color.FromArgb(0, 255, 0);
        var yellow = Color.FromArgb(255, 255, 0);
        var orange = Color.FromArgb(255, 128, 0);
        var red = Color.FromArgb(255, 0, 0);

        float delta;

        {
            delta = 300;

            Assert.Equal(blue, GetContourColor(0 / delta));
            Assert.Equal(cyan, GetContourColor(60 / delta));
            Assert.Equal(green, GetContourColor(120 / delta));
            Assert.Equal(yellow, GetContourColor(180 / delta));
            Assert.Equal(orange, GetContourColor(210 / delta));
            Assert.Equal(red, GetContourColor(240 / delta));
            Assert.Equal(fuchsia, GetContourColor(300 / delta));
        }

        {

            Assert.Equal(blue, GetContourColor(0, decreaseHue: false));
            Assert.Equal(violet, GetContourColor(.5f, decreaseHue: false));
            Assert.Equal(fuchsia, GetContourColor(1f, decreaseHue: false));
        }

        {
            delta = 150;

            var hueFrom = 240; // blue
                               // violet, fuchsia, red, orange
            var hueTo = 30; // orange            

            Assert.Equal(blue, GetContourColor(0 / delta, hueFrom, hueTo, decreaseHue: false));
            Assert.Equal(violet, GetContourColor(30f / delta, hueFrom, hueTo, decreaseHue: false));
            Assert.Equal(fuchsia, GetContourColor(60f / delta, hueFrom, hueTo, decreaseHue: false));
            Assert.Equal(red, GetContourColor(120f / delta, hueFrom, hueTo, decreaseHue: false));
            Assert.Equal(orange, GetContourColor(150f / delta, hueFrom, hueTo, decreaseHue: false));
        }

        {
            delta = 240 - 30;

            var hueFrom = 240; // blue
                               // cyan, green, yellow
            var hueTo = 30; // orange            

            Assert.Equal(blue, GetContourColor(0 / delta, hueFrom, hueTo, decreaseHue: true));
            Assert.Equal(cyan, GetContourColor(60f / delta, hueFrom, hueTo, decreaseHue: true));
            Assert.Equal(green, GetContourColor(120f / delta, hueFrom, hueTo, decreaseHue: true));
            Assert.Equal(yellow, GetContourColor(180f / delta, hueFrom, hueTo, decreaseHue: true));
            Assert.Equal(orange, GetContourColor(210f / delta, hueFrom, hueTo, decreaseHue: true));
        }

    }

}