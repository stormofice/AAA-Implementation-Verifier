using System;

namespace AAARunCheck.Config
{
    public class ValueWrapper
    {
        private decimal _decimal;
        private string _string;

        public ValueWrapper(string s)
        {
            if (decimal.TryParse(s, out _decimal))
            {
            }
            else
            {
                _string = s;
            }
        }


        public bool VEquals(object? obj, decimal delta)
        {
            if (obj == this)
                return true;
            if (obj is not ValueWrapper vw)
                return false;

            if (_string == null && vw._string != null || _string != null && vw._string == null)
                return false;

            if (_string == null)
                return IsApprox(_decimal, vw._decimal, delta);

            return _string.Equals(vw._string);
        }


        // Checks if two decimal numbers are approximately the same
        private bool IsApprox(decimal expected, decimal actual, decimal delta)
        {
            // Logger.LogDebug("Comparing: Is {0} ~= {1} with delta {2}", actual, expected, delta);
            return Math.Abs(expected - actual) <= delta;
        }

        public override string ToString()
        {
            if (_string != null)
                return _string;
            return "" + _decimal;
        }
    }
}