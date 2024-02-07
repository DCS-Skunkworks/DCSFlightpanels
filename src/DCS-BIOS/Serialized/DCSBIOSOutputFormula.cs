using Newtonsoft.Json;

namespace DCS_BIOS.Serialized
{
    using System;
    using System.Collections.Generic;
    using ClassLibraryCommon;
    using ControlLocator;
    using misc;
    using NLog;

    /// <summary>
    /// Handles user specified formula and DCSBIOSOutput(s)
    /// to produce a result based on DCS-BIOS value(s).
    /// </summary>
    [Serializable]
    [SerializeCritical]
    public class DCSBIOSOutputFormula
    {
        [NonSerialized]
        internal static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<DCSBIOSOutput> _dcsbiosOutputs = new();
        private readonly Dictionary<string, double> _variables = new();

        [NonSerialized]
        private readonly JaceExtended _jaceExtended = new();
        private string _formula;

        [NonSerialized]
        private readonly object _jaceLockObject = new();

        public double FormulaResult { get; set; }

        public DCSBIOSOutputFormula()
        {
        }

        public DCSBIOSOutputFormula(string formula)
        {
            _formula = formula;
            ExtractDCSBIOSOutputsInFormula();
        }


        public List<DCSBIOSOutput> DCSBIOSOutputs()
        {
            return _dcsbiosOutputs;
        }

        private void ExtractDCSBIOSOutputsInFormula()
        {
            try
            {
                _dcsbiosOutputs.Clear();
                var controls = DCSBIOSControlLocator.GetControls();
                foreach (var dcsbiosControl in controls)
                {
                    if (_formula.Contains(dcsbiosControl.Identifier))
                    {
                        // Console.WriteLine("Variable " + dcsbiosControl.identifier + " set to 0");
                        _variables.Add(dcsbiosControl.Identifier, 0);
                        var dcsbiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput(dcsbiosControl.Identifier);
                        _dcsbiosOutputs.Add(dcsbiosOutput);
                        DCSBIOSProtocolParser.RegisterAddressToBroadCast(dcsbiosOutput.Address);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ExtractDCSBIOSOutputsInFormula() function");
                throw;
            }
        }


        /// <summary>
        /// <para>Loops all dcs-bios outputs and checks for address match and that dcs-bios value has changed
        /// and also that the comparison operator test is true against dcs-bios value.</para>
        /// <para>Evaluates formula if all matches OK.</para>
        /// <para>Get result via FormulaResult.</para>
        /// </summary>
        /// <returns></returns>
        public bool Evaluate(uint address, uint data)
        {
            try
            {
                var result = false;
                lock (_jaceLockObject)
                {
                    foreach (var dcsbiosOutput in _dcsbiosOutputs)
                    {
                        if (!dcsbiosOutput.UIntValueHasChanged(address, data))
                        {
                            continue;
                        }

                        _variables[dcsbiosOutput.ControlId] = dcsbiosOutput.LastUIntValue;
                        result = true;
                    }

                    if (result)
                    {
                        Evaluate(false);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CheckForMatch() function");
                throw;
            }
        }


        /// <summary>
        /// Returns true if address was found in formula
        /// and that the value has changed.
        /// If true do a subsequent call to Evaluate() to get new value
        /// </summary>
        /// <returns></returns>
        public bool CheckForMatch(uint address, uint data)
        {
            try
            {
                var result = false;
                lock (_jaceLockObject)
                {
                    foreach (var dcsbiosOutput in _dcsbiosOutputs)
                    {
                        if (dcsbiosOutput.UIntValueHasChanged(address, data))
                        {
                            result = true;
                            _variables[dcsbiosOutput.ControlId] = dcsbiosOutput.LastUIntValue;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CheckForMatch() function");
                throw;
            }
        }

        public double Evaluate(bool throwException)
        {
            try
            {
                lock (_jaceLockObject)
                {
                    FormulaResult = _jaceExtended.CalculationEngine.Calculate(_formula, _variables);
                    return FormulaResult;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Evaluate() function");
                if (throwException)
                {
                    throw;
                }
            }

            return double.MinValue;
        }

        public void ImportString(string str)
        {
            ////DCSBiosOutputFormula{(AAP_EGIPWR+1)/2}
            var value = str;
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("DCSBiosOutputFormula cannot import null string.");
            }

            if (!str.StartsWith("DCSBiosOutputFormula{") || !str.EndsWith("}"))
            {
                throw new Exception("DCSBiosOutputFormula cannot import string : " + str);
            }

            value = value.Replace("DCSBiosOutputFormula{", string.Empty).Replace("}", string.Empty);

            // (AAP_EGIPWR+1)/2
            _formula = value;

            ExtractDCSBIOSOutputsInFormula();
        }

        [JsonProperty("Formula", Required = Required.Default)]
        public string Formula
        {
            get => _formula;
            set
            {
                _formula = value;
                if (!string.IsNullOrEmpty(_formula))
                {
                    ExtractDCSBIOSOutputsInFormula();
                }
            }
        }

        public override string ToString()
        {
            return "DCSBiosOutputFormula{" + _formula + "}";
        }
    }
}
