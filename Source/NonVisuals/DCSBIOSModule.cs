using System;
using System.Collections.Generic;
using System.IO;

namespace NonVisuals
{
    public class DCSBIOSModule
    {
        private string _luaFilename;
        private string _description;

        public DCSBIOSModule(string luaFilename, string description)
        {
            _luaFilename = luaFilename;
            _description = description;
        }

        
        public static List<DCSBIOSModule> ParseSettings(string filename)
        {
            var result = new List<DCSBIOSModule>();

            if (!File.Exists(filename))
            {
                return result;
            }

            var stringArray = File.ReadAllLines(filename);

            foreach (var s in stringArray)
            {
                if (string.IsNullOrEmpty(s) || s.Trim().StartsWith("#"))
                {
                    continue;
                }

                var settings = s.Trim().Split(new char[]{ ':' }, StringSplitOptions.RemoveEmptyEntries);
                //A-4E Skyhawk : A-4E-C.lua
                var dcsBiosModule = new DCSBIOSModule(settings[1], settings[0]);
                result.Add(dcsBiosModule);
            }

            return result;
        }


        public string LuaFilename
        {
            get => _luaFilename;
            set => _luaFilename = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }
    }
}
