namespace NonVisuals
{
    using System;
    using System.Diagnostics;
    using System.Media;
    using System.Threading;

    using ClassLibraryCommon;

    using NonVisuals.Saitek;

    [Serializable]
    public class OSCommand
    {
        private string _file;
        private string _arguments;
        private string _name;
        private volatile bool _isRunning;


        public int GetHash()
        {
            unchecked
            {
                var result = string.IsNullOrWhiteSpace(_file) ? 0 : _file.GetHashCode();
                result = (result * 397) ^ (string.IsNullOrWhiteSpace(_arguments) ? 0 : _arguments.GetHashCode());
                result = (result * 397) ^ (string.IsNullOrWhiteSpace(_name) ? 0 : _name.GetHashCode());
                return result;
            }
        }

        public OSCommand()
        { }

        public OSCommand(string file, string arguments, string name)
        {
            _file = file;
            _arguments = arguments;
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
            _file = array[0];
            if (array.Length > 1)
            {
                _arguments = array[1];
            }

            if (array.Length > 2)
            {
                _name = array[2];
            }
        }

        public string ExportString()
        {
            if (string.IsNullOrEmpty(_file))
            {
                return null;
            }

            return "OSCommand{" + _file + SaitekConstants.SEPARATOR_SYMBOL + _arguments + SaitekConstants.SEPARATOR_SYMBOL + _name + "}";
        }

        public bool IsRunning()
        {
            return _isRunning;
        }

        public string Execute(CancellationToken cancellationToken)
        {
            var result = string.Empty;
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _file,
                        Arguments = _arguments,
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
                result = "Failed to start " + _file + " with arguments " + _arguments + "." + ex.Message + "\n" + ex.StackTrace;
                Common.LogError(result);
            }

            return result;
        }

        public string Command
        {
            get => _file;
            set => _file = value;
        }

        public string Arguments
        {
            get => _arguments;
            set => _arguments = value;
        }

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
            get => string.IsNullOrEmpty(_file);
        }
    }
}
