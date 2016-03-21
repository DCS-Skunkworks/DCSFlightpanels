using System;
using System.Collections.Generic;
using Jace;


namespace DCS_BIOS
{
    public class DCSBIOSOutputFormula
    {
        private string _formula;
        private List<DCSBIOSOutput> _dcsbiosOutputs = new List<DCSBIOSOutput>();
        Dictionary<string, double> _variables = new Dictionary<string, double>();
        CalculationEngine _engine = new CalculationEngine();

        public DCSBIOSOutputFormula()
        {
        }

        public DCSBIOSOutputFormula(string formula)
        {
            _formula = formula;
            ExtractDCSBIOSOutputsInFormula();
        }

        private void ExtractDCSBIOSOutputsInFormula()
        {
            try
            {
                var found = false;
                var controls = DCSBIOSControlLocator.GetControls();
                foreach (var dcsbiosControl in controls)
                {
                    if (_formula.Contains(dcsbiosControl.identifier))
                    {
                     //   Console.WriteLine("Variable " + dcsbiosControl.identifier + " set to 0");
                        _variables.Add(dcsbiosControl.identifier, 0);
                        var dcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControl.identifier);
                        _dcsbiosOutputs.Add(dcsbiosOutput);
                        DCSBIOSProtocolParser.RegisterAddressToBroadCast(dcsbiosOutput.Address);
                        found = true;
                    }
                }
                if (!found)
                {
                    throw new Exception("Could not find any DCS-BIOS Controls in formula expression.");
                }
                //_expression = new Expression(_formula);
            }
            catch (Exception ex)
            {
                DBCommon.LogError(1955594, ex, "ExtractDCSBIOSOutputsInFormula() function");
                throw;
            }
        }

        //Returns true if address was found in formula
        //If true do a subsequent call to Evaluate() to get new value
        public bool CheckForMatch(uint address, uint data)
        {
            try
            {
                var result = false;
                foreach (var dcsbiosOutput in _dcsbiosOutputs)
                {
                    if (dcsbiosOutput.Address == address)
                    {
                        dcsbiosOutput.CheckForValueMatchAndChange(data);
                        result = true;
                        //Console.WriteLine("Variable " + dcsbiosOutput.ControlId + " set to " + dcsbiosOutput.LastIntValue);
                        _variables[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;
                        //_expression.Parameters[dcsbiosOutput.ControlId] = dcsbiosOutput.LastIntValue;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                DBCommon.LogError(1933494, ex, "CheckForMatch() function");
                throw;
            }
        }

        public int Evaluate()
        {
            try
            {
                /*Console.WriteLine("_formula : " + _formula);
                foreach (var variable in _variables)
                {
                    Console.WriteLine("variable : " + variable.Key + " = " + variable.Value);
                }*/
                var a = _engine.Calculate(_formula, _variables);
                return Convert.ToInt32(a);
            }
            catch (Exception ex)
            {
                DBCommon.LogError(1033494, ex, "Evaluate() function");
                throw;
            }
        }

        /*
        //Returns true if address was found in formula
        public int Evaluate()
        {
            try
            {
                if (_expression.Parameters.Count != _dcsbiosOutputs.Count)
                {
                    //Return zero because if a formula contains lets say 7 DCSBIOSControls/outputs -> (ASAAA_D3_FLAG + ALT_MSL + RDR_CHAN + etc etc ) / 2
                    //then all these must be populated one time before the expression can be evaluated. So return 0 until all of them have been given a value
                    return 0;
                }
                DBCommon.DebugP(_expression.Evaluate().ToString());
                var a = double.Parse(_expression.Evaluate().ToString());
                return Convert.ToInt32(a);
            }
            catch (Exception ex)
            {
                DBCommon.LogError(1033494, ex, "Evaluate() function");
                throw;
            }
        }
        */
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
            value = value.Substring(value.IndexOf("{", StringComparison.InvariantCulture) + 1);
            //(AAP_EGIPWR+1)/2}
            value = value.Substring(0, value.Length - 1);
            //(AAP_EGIPWR+1)/2
            _formula = value;

            ExtractDCSBIOSOutputsInFormula();
        }

        public override string ToString()
        {
            return "DCSBiosOutputFormula{" + _formula + "}";
        }


        public string Formula
        {
            get { return _formula; }
        }
    }
}
