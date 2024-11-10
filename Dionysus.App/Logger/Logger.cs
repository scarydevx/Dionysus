namespace Dionysus.App.Logger;

public class Logger
{
    public enum LogType {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        FORCE
    }
    
    public string logFilePath { get; set; } = string.Empty;
        public string logTimeFormat { get; set; } = string.Empty;
        public string logTextFormat { get; set; } = string.Empty;
        public Dictionary<LogType, ConsoleColor> colors= new Dictionary<LogType, ConsoleColor>() {
            {LogType.DEBUG, ConsoleColor.White},
            {LogType.INFO, ConsoleColor.Blue},
            {LogType.WARNING, ConsoleColor.DarkYellow},
            {LogType.ERROR, ConsoleColor.Red},
            {LogType.FORCE, ConsoleColor.DarkGray}
        };
        private static string currentTime = string.Empty;
        
        public void Log(LogType logType, string logText) {
            if (logTimeFormat == string.Empty) {
                logTimeFormat = "yyyy-MM-dd HH:mm:ss";
            }
            if(logTextFormat == string.Empty){
                logTextFormat = "{0} - {1} - {2}";
            }
            currentTime = DateTime.Now.ToString(logTimeFormat);
            
            ConsoleColor color = colors[logType];
            Console.ForegroundColor = color;
            Console.WriteLine(logTextFormat, currentTime, logType.ToString(), logText);
            Console.ResetColor();
        }
}