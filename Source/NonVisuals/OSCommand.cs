namespace NonVisuals
{
    using System;
    using System.Diagnostics;
    using System.Media;
    using System.Threading;
    using NLog;
    using Panels.Saitek;




    /// <summary>
    /// Class for handling, executing, importing, exporting Windows commands.
    /// Can be used for starting up certain applications used while playing.
    /// </summary>
    [Serializable]
    public class OSCommand
    {
        internal static Logger Logger = LogManager.GetCurrentClassLogger();
        
        private string _name;
        private volatile bool _isRunning;
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return "Windows Command";
                }

                return _name;
            }

            set => _name = value;
        }
        public bool IsEmpty
        {
            get => string.IsNullOrEmpty(Command);
        }

        public int GetHash()
        {
            unchecked
            {
                var result = string.IsNullOrWhiteSpace(Command) ? 0 : Command.GetHashCode();
                result = (result * 397) ^ (string.IsNullOrWhiteSpace(Arguments) ? 0 : Arguments.GetHashCode());
                result = (result * 397) ^ (string.IsNullOrWhiteSpace(_name) ? 0 : _name.GetHashCode());
                return result;
            }
        }

        public OSCommand()
        { }

        public OSCommand(string command, string arguments, string name)
        {
            Command = command;
            Arguments = arguments;
            _name = name;
            if (string.IsNullOrEmpty(_name))
            {
                _name = "OS Command";
            }
        }

        public void ImportString(string value)
        {
            // OSCommand{FILE\o/ARGUMENTS\o/NAME}
            var tmp = value;
            tmp = tmp.Replace("OSCommand{", string.Empty).Replace("}", string.Empty);

            // FILE\o/ARGUMENTS\o/NAME]
            var array = tmp.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.None);
            Command = array[0];
            if (array.Length > 1)
            {
                Arguments = array[1];
            }

            if (array.Length > 2)
            {
                _name = array[2];
            }
        }

        public string ExportString()
        {
            if (string.IsNullOrEmpty(Command))
            {
                return null;
            }

            return "OSCommand{" + Command + SaitekConstants.SEPARATOR_SYMBOL + Arguments + SaitekConstants.SEPARATOR_SYMBOL + _name + "}";
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public string ExecuteCommand(CancellationToken cancellationToken)
        {
            var result = string.Empty;
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Command,
                        Arguments = Arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    _isRunning = true;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    result = result + " " + process.StandardOutput.ReadLine();
                }
                _isRunning = false;
            }
            catch (Exception ex)
            {
                SystemSounds.Beep.Play();
                result = $"Failed to start {Command} with arguments {Arguments}.{ex.Message}\n{ex.StackTrace}";
                Logger.Error(ex, result);
            }
            return result;
        }
    }
}
