namespace spikewall.Debug
{
    public static class DebugHelper
    {
        public static void ColorfulWrite(params ColorfulString[] strings)
        {
            var originalFGColor = Console.ForegroundColor;
            var originalBGColor = Console.BackgroundColor;
            foreach (var str in strings) {
                Console.ForegroundColor = str.FGColor;
                Console.BackgroundColor = str.BGColor;
                Console.Write(str.Text);
            }
            Console.ForegroundColor = originalFGColor;
            Console.BackgroundColor = originalBGColor;
        }

        public static void Log(string text, int type = 0)
        {
            switch (type) {
                case (1):
                    ColorfulWrite(new ColorfulString(ConsoleColor.Blue, ConsoleColor.Black, "game"));
                    Console.Write(": " + text + "\n");
                    break;
                case (2):
                    ColorfulWrite(new ColorfulString(ConsoleColor.Yellow, ConsoleColor.Black, "warn"));
                    Console.Write(": " + text + "\n");
                    break;
                default:
                    ColorfulWrite(new ColorfulString(ConsoleColor.Green, ConsoleColor.Black, "server"));
                    Console.Write(": " + text + "\n");
                    break;
            }
        }
    }
}