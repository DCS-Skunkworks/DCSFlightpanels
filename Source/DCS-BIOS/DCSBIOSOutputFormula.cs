using Newtonsoft.Json;

namespace DCS_BIOS
{
    using System;
    using System.Collections.Generic;
    using NLog;

    [Serializable]
    public class DCSBIOSOutputFormula
    {
        [NonSerialized]
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<DCSBIOSOutput> _dcsbiosOutputs = new List<DCSBIOSOutput>();
        private readonly Dictionary<string, double> _variables = new Dictionary<string, double>();

        [NonSerialized]
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private string _formula;
        
        [NonSerialized]
        private readonly object _jaceLockObject = new object();

        [NonSerialized]
        private int _staticUpdateInterval = 0;
        

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
                        var dcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControl.Identifier);
                        _dcsbiosOutputs.Add(dcsbiosOutput);
                        DCSBIOSProtocolParser.RegisterAddressToBroadCast(dcsbiosOutput.Address);
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ExtractDCSBIOSOutputsInFormula() function");
                throw;
            }
        }


        public bool CheckForMatchAndNewValue(uint address, uint data, int staticUpdateInterval)
        {
            try
            {
                _staticUpdateInterval++;
                var result = false;
                lock (_jaceLockObject)
                {
                    foreach (var dcsbiosOutput in _dcsbiosOutputs)
                    {
                        if (dcsbiosOutput.Address == address)
                        {
                            if (dcsbiosOutput.LastIntValue != data)
                            {
                                dcsbiosOutput.CheckForValueMatchAndChange(data);
                                result = true;
                            }

                            result = result || _staticUpdateInterval > staticUpdateInterval;

                            if (result)
                            {
                                _staticUpdateInterval = 0;
                            }
                            // Console.WriteLine("Variable " + dcsbiosOutput.ControlId + " set to " + dcsbiosOutput.LastIntValue);
                            _variables[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;
                            
                            //_expression.Parameters[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CheckForMatch() function");
                throw;
            }
        }


        // Returns true if address was found in formula
        // If true do a subsequent call to Evaluate() to get new value
        public bool CheckForMatch(uint address, uint data)
        {
            try
            {
                var result = false;
                lock (_jaceLockObject)
                {
                    foreach (var dcsbiosOutput in _dcsbiosOutputs)
                    {
                        if (dcsbiosOutput.Address == address)
                        {
                            dcsbiosOutput.CheckForValueMatchAndChange(data);
                            result = true;

                            // Console.WriteLine("Variable " + dcsbiosOutput.ControlId + " set to " + dcsbiosOutput.LastIntValue);
                            _variables[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;

                            //_expression.Parameters[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "CheckForMatch() function");
                throw;
            }
        }

        public double Evaluate(bool throwException)
        {
            try
            {
                /*Console.WriteLine("_formula : " + _formula);
                foreach (var variable in _variables)
                {
                    Console.WriteLine("variable : " + variable.Key + " = " + variable.Value);
                }*/

                // Debug.WriteLine(_jaceExtended.CalculationEngine.Calculate(_formula, _variables));

                lock (_jaceLockObject)
                {
                    //TestCalculation();
                    
                    return _jaceExtended.CalculationEngine.Calculate(_formula, _variables);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Evaluate() function");
                if (throwException)
                {
                    throw;
                }
            }

            return -99;
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
