using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MathExtensions
{
    /// <summary>
    /// Converts NaN's and Infinity's into -1
    /// </summary>
    /// <param name="_v"></param>
    /// <returns></returns>
    public static double Sanitize(double _v) => double.IsInfinity(_v) || double.IsNaN(_v) ? -1 : _v;

    /// <summary>
    /// Computes the Sigmoid function
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double Sigmoid(double value) => 1.0 / (1.0 + Math.Exp(-value));
}
