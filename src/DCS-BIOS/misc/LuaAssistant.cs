using ClassLibraryCommon;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace DCS_BIOS.misc
{
    /// <summary>
    /// Class for reading lua code from dcs-bios for a certain control. Can also read the signature for the function from Module.lua.
    /// </summary>
    internal class LuaAssistant
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<string> LuaModuleSignatures = new();
        private static readonly List<KeyValuePair<string, string>> LuaControls = new();
        private static DCSAircraft _dcsAircraft;
        private static string _jsonDirectory;
        private static string _dcsbiosAircraftLuaLocation;
        private static string _dcsbiosModuleLuaFilePath;
        private static readonly object LockObject = new();
        private const string DCSBIOS_LUA_NOT_FOUND_ERROR_MESSAGE = "Error loading DCS-BIOS lua.";


        internal static DCSAircraft DCSAircraft
        {
            get => _dcsAircraft;
            set
            {
                if (_dcsAircraft == value)
                {
                    return;
                }

                _dcsAircraft = value;
                Reset();
            }
        }

        internal static string JSONDirectory
        {
            get => _jsonDirectory;
            set
            {
                _jsonDirectory = value;
                _dcsbiosAircraftLuaLocation = $@"{value}\..\..\lib\modules\aircraft_modules\";
                _dcsbiosModuleLuaFilePath = $@"{value}\..\..\lib\modules\Module.lua";
            }
        }

        internal static void Reset()
        {
            LuaModuleSignatures.Clear();
            LuaControls.Clear();
        }

        internal static string GetLuaCommand(string controlId, bool includeSignature)
        {
            if (_dcsAircraft == null || _dcsAircraft.IsMetaModule || string.IsNullOrEmpty(controlId)) return "";

            if (LuaControls.Count == 0)
            {
                ReadLuaCommandsFromLua();
                if (LuaControls.Count == 0) return "";
            }

            var result = LuaControls.Find(o => o.Key == controlId);

            if (result.Key != controlId) return "";

            var stringResult = result.Value;
            if (!includeSignature) return stringResult;

            var signature = GetLuaCommandSignature(result.Value);
            return string.IsNullOrEmpty(signature) ? result.Value : signature + "\n" + result.Value;
        }

        private static string GetLuaCommandSignature(string luaCommand)
        {
            try
            {
                //A_10C:definePotentiometer("HARS_LATITUDE", 44, 3005, 271, { 0, 1 }, "HARS", "HARS Latitude Dial")
                var startIndex = luaCommand.IndexOf(":", StringComparison.Ordinal);
                var endIndex = luaCommand.IndexOf("(", StringComparison.Ordinal) - startIndex;
                var functionName = "function Module" + luaCommand.Substring(startIndex, endIndex);

                var luaSignature = LuaModuleSignatures.Find(o => o.StartsWith(functionName + "("));
                return string.IsNullOrEmpty(luaSignature) ? "" : $"{luaSignature.Replace("function ", "")}";
            }
            catch (Exception exception)
            {
                Common.ShowErrorMessageBox(exception);
            }

            return "";
        }

        private static void ReadControlsFromLua(DCSAircraft dcsAircraft, string fileFullPath)
        {
            // input is a map from category string to a map from key string to control definition
            // we read it all then flatten the grand children (the control definitions)
            var lineArray = File.ReadAllLines(fileFullPath);
            try
            {
                var luaBuffer = "";

                foreach (var s in lineArray)
                {
                    //s.StartsWith("--") 
                    if (string.IsNullOrEmpty(s)) continue;

                    if (s.StartsWith(dcsAircraft.ModuleLuaName + ":define"))
                    {
                        luaBuffer = s;

                        if (CountParenthesis(true, luaBuffer) == CountParenthesis(false, luaBuffer))
                        {
                            LuaControls.Add(CopyControlFromLuaBuffer(luaBuffer));
                            luaBuffer = "";
                        }
                    }
                    else if (!string.IsNullOrEmpty(luaBuffer))
                    {
                        //We have incomplete data from previously
                        luaBuffer = luaBuffer + "\n" + s;
                        if (CountParenthesis(true, luaBuffer) == CountParenthesis(false, luaBuffer))
                        {
                            LuaControls.Add(CopyControlFromLuaBuffer(luaBuffer));
                            luaBuffer = "";
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "ReadControlsFromLua : Failed to read DCS-BIOS lua.");
            }
        }

        private static List<string> ReadModuleFunctionSignatures(string moduleFile)
        {
            if (LuaModuleSignatures.Count > 0) return LuaModuleSignatures;

            LuaModuleSignatures.Clear();


            var lineArray = File.ReadAllLines(moduleFile);
            try
            {
                var luaBuffer = "";

                foreach (var s in lineArray)
                {
                    //s.StartsWith("--") 
                    if (string.IsNullOrEmpty(s)) continue;

                    if (s.StartsWith("function Module:define"))
                    {
                        luaBuffer = s;

                        if (CountParenthesis(true, luaBuffer) == CountParenthesis(false, luaBuffer))
                        {
                            LuaModuleSignatures.Add(luaBuffer);
                            luaBuffer = "";
                        }
                    }
                    else if (!string.IsNullOrEmpty(luaBuffer))
                    {
                        //We have incomplete data from previously
                        luaBuffer = luaBuffer + "\n" + s;
                        if (CountParenthesis(true, luaBuffer) == CountParenthesis(false, luaBuffer))
                        {
                            LuaModuleSignatures.Add(luaBuffer);
                            luaBuffer = "";
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "GetModuleFunctionSignatures : Failed to read Module.lua.");
            }

            return LuaModuleSignatures;
        }

        private static KeyValuePair<string, string> CopyControlFromLuaBuffer(string luaBuffer)
        {
            // We have the whole control
            // F_16C_50:define3PosTumb("MAIN_PWR_SW", 3, 3001, 510, "Electric System", "MAIN PWR Switch, MAIN PWR/BATT/OFF")
            /*
             A_10C:defineString("ARC210_COMSEC_SUBMODE", function()
                return Functions.coerce_nil_to_string(arc_210_data["comsec_submode"])
             end, 5, "ARC-210 Display", "COMSEC submode (PT/CT/CT-TD)")
            */
            var startIndex = luaBuffer.IndexOf("\"", StringComparison.Ordinal);
            var endIndex = luaBuffer.IndexOf("\"", luaBuffer.IndexOf("\"") + 1);
            var controlId = luaBuffer.Substring(startIndex + 1, endIndex - startIndex - 1);

            return new KeyValuePair<string, string>(controlId, luaBuffer);
        }

        private static int CountParenthesis(bool firstParenthesis, string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var parenthesis = firstParenthesis ? '(' : ')';
            var result = 0;
            var insideQuote = false;

            foreach (var c in s)
            {
                if (c == '"') insideQuote = !insideQuote;

                if (c == parenthesis && !insideQuote) result++;
            }

            return result;
        }

        /// <summary>
        /// Load all lua controls
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void ReadLuaCommandsFromLua()
        {
            LuaControls.Clear();
            LuaModuleSignatures.Clear();

            try
            {
                lock (LockObject)
                {
                    ReadControlsFromLua(_dcsAircraft, $"{_dcsbiosAircraftLuaLocation}{_dcsAircraft.LuaFilename}");
                    ReadModuleFunctionSignatures(_dcsbiosModuleLuaFilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{DCSBIOS_LUA_NOT_FOUND_ERROR_MESSAGE} ==>[{_jsonDirectory}]<=={Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
