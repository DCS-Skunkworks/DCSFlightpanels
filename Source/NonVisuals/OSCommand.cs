using System;
using System.Diagnostics;

namespace NonVisuals
{
    public class OSCommand
    {
        private string _file;
        private string _arguments;
        private string _name;
        private const string SEPARATOR_CHARS = "\\o/";

        public OSCommand()
        {}
        
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
            //OSCommand{FILE\o/ARGUMENTS\o/NAME}
            var tmp = value;
            tmp = tmp.Replace("OSCommand{", "").Replace("}", "");
            //FILE\o/ARGUMENTS\o/NAME]
            var array = tmp.Split(new[] { SEPARATOR_CHARS }, StringSplitOptions.None);
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
            return "OSCommand{" + _file + SEPARATOR_CHARS + _arguments + SEPARATOR_CHARS + _name + "}";
        }

        public string Execute()
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
            var result = "";
            while (!process.StandardOutput.EndOfStream)
            {
                result = result + " " + process.StandardOutput.ReadLine();
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
