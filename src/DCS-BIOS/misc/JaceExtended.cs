using System;
using Jace;
using NLog;

namespace DCS_BIOS.misc
{
    public class JaceExtended
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _lockObject = new();
        private readonly CalculationEngine _calculationEngine = new();

        public JaceExtended()
        {
            AddFunctions();
        }

        public double Evaluate(string expression)
        {
            try
            {
                lock (_lockObject)
                {
                    return _calculationEngine.Calculate(expression);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "JaceExtended.Evaluate() function");
                throw;
            }
        }

        private static void AddFunctions()
        {
            //_calculationEngine.AddFunction("floor", Math.Floor);
            //_calculationEngine.AddFunction("ceiling", Math.Ceiling);
            //_calculationEngine.AddFunction("truncate", Math.Truncate);
        }

        public CalculationEngine CalculationEngine => _calculationEngine;
    }
}
