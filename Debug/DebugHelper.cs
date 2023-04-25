using System.Diagnostics;

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
            var stackTrace = new StackTrace();
            switch (type) {
                case (1):
                    ColorfulWrite(new ColorfulString(ConsoleColor.Blue, ConsoleColor.Black, "game [" + stackTrace.GetFrame(1).GetMethod().Name + "]"));
                    Console.Write(": " + text + "\n");
                    break;
                case (2):
                    ColorfulWrite(new ColorfulString(ConsoleColor.Yellow, ConsoleColor.Black, "warn [" + stackTrace.GetFrame(1).GetMethod().Name + "]"));
                    Console.Write(": " + text + "\n");
                    break;
                case (3):
                    ColorfulWrite(new ColorfulString(ConsoleColor.White, ConsoleColor.DarkRed, "bug [" + stackTrace.GetFrame(1).GetMethod().Name + "]"));
                    Console.Write(": " + text + "\n");
                    break;
                default:
                    ColorfulWrite(new ColorfulString(ConsoleColor.Green, ConsoleColor.Black, "server [" + stackTrace.GetFrame(1).GetMethod().Name + "]"));
                    Console.Write(": " + text + "\n");
                    break;
            }
        }
    }
}