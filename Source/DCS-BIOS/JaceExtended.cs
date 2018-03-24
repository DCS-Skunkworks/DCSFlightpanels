using System;
using ClassLibraryCommon;
using Jace;
using Jace.Execution;

namespace DCS_BIOS
{
    public class JaceExtended
    {
        private readonly CalculationEngine _calculationEngine = new CalculationEngine();

        public JaceExtended()
        {
            AddFunctions();
        }

        public double Evaluate(string expression)
        {
            try
            {
                return _calculationEngine.Calculate(expression);
            }
            catch (Exception ex)
            {
                Common.LogError(1933494, ex, "JaceExtended.Evaluate() function");
                throw;
            }
        }

        private void AddFunctions()
        {
            //_calculationEngine.AddFunction("floor", Math.Floor);
            //_calculationEngine.AddFunction("ceiling", Math.Ceiling);
            //_calculationEngine.AddFunction("truncate", Math.Truncate);
        }

        public CalculationEngine CalculationEngine => _calculationEngine;
    }
}
