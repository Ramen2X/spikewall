namespace spikewall.Debug
{
    /// <summary>
    /// Helper class for printing strings with color.
    /// Derived from https://stackoverflow.com/a/38517727
    /// </summary>
    public class ColorfulString
    {
        public ConsoleColor FGColor;
        public ConsoleColor BGColor;
        public string Text;

        public ColorfulString(ConsoleColor fgColor, ConsoleColor bgColor, string text)
        {
            FGColor = fgColor;
            BGColor = bgColor;
            Text = text;
        }
    }
}
