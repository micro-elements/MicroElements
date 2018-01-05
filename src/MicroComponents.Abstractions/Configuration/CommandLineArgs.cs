namespace MicroComponents.Configuration
{
    /// <summary>
    /// Аргументы командной строки.
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// Null object.
        /// </summary>
        public static readonly CommandLineArgs Null = new CommandLineArgs(new string[0]);

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public CommandLineArgs(string[] args)
        {
            Args = args ?? new string[0];
        }

        /// <summary>
        /// Аргументы командной строки.
        /// </summary>
        public string[] Args { get; }
    }
}