using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public readonly struct Fraction : IComparable<Fraction>, IEquatable<Fraction>, ISignedNumber<Fraction>
    {
        readonly BigInteger Top;
        readonly BigInteger Bottom;

        public static Fraction NegativeOne => new Fraction(-1, 1);

        public static Fraction One => new Fraction(1, 1);

        public static int Radix => 2;

        public static Fraction Zero => new Fraction(0, 1);

        public static Fraction AdditiveIdentity => Zero;

        public static Fraction MultiplicativeIdentity => One;

        public Fraction(BigInteger _top, BigInteger _bottom)
        {
            if (_top == BigInteger.Zero)
            {
                Top = BigInteger.Zero;
                Bottom = BigInteger.One;
                return;
            }

            var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(_top), BigInteger.Abs(_bottom));

            if (_bottom.Sign == -1)
            {
                Top = -_top / gcd;
                Bottom = -_bottom / gcd;
            }
            else
            {
                Top = _top / gcd;
                Bottom = _bottom / gcd;
            }
        }

        public override string ToString() => ((double)Top / (double)Bottom).ToString();

        public override bool Equals(object? obj)
        {
            return obj is Fraction fraction &&
                   Top == fraction.Top && Bottom == fraction.Bottom;
        }

        public override int GetHashCode() => HashCode.Combine(Top, Bottom);

        public static implicit operator Fraction(BigInteger _a) => new Fraction(_a, 1);
        public static implicit operator Fraction(long _a) => new Fraction(_a, 1);
        public static implicit operator Fraction(Int128 _a) => new Fraction(_a, 1);
        public static implicit operator Fraction(int _a) => new Fraction(_a, 1);

        public static explicit operator double(Fraction _a) => ((double)_a.Top / (double)_a.Bottom);


        public static implicit operator Fraction(double _a)
        {
            (var mantissa, var exponent) = ExtractMantissaExponent(_a);
            if (exponent >= 0)
                return new Fraction(new BigInteger(mantissa) << exponent, 1);
            else
                return new Fraction(mantissa, new BigInteger(1) << (-exponent));
        }


        //code by Jon Skeet, https://stackoverflow.com/a/390072/742404
        private static (long Mantissa, int Exponent) ExtractMantissaExponent(double d)
        {
            long bits = BitConverter.DoubleToInt64Bits(d);

            var exponent = (int)((bits >> 52) & 0x7ffL);
            var mantissa = bits & 0xfffffffffffffL;

            if (exponent == 0)
                // Subnormal numbers; exponent is effectively one higher,
                // but there's no extra normalisation bit in the mantissa
                exponent++;
            else
                // Normal numbers; leave exponent as it is but add extra
                // bit to the front of the mantissa
                mantissa = mantissa | (1L << 52);

            // Bias the exponent. It's actually biased by 1023, but we're
            // treating the mantissa as m.0 rather than 0.m, so we need
            // to subtract another 52 from it.
            exponent -= 1075;

            if (mantissa == 0)
                return (0, 0);

            //Normalize
            while ((mantissa & 1) == 0)
            {
                /*  i.e., Mantissa is even */
                mantissa >>= 1;
                exponent++;
            }
            return (mantissa, exponent);
        }

        public int CompareTo(Fraction other)
        {
            var diff = (this - other).Top;
            if (diff > 0)
                return 1;
            if (diff < 0)
                return -1;
            return 0;
        }

        public bool Equals(Fraction other) => Top == other.Top && Bottom == other.Bottom;

        public static Fraction Abs(Fraction value) => new Fraction(BigInteger.Abs(value.Top), value.Bottom);

        public static bool IsCanonical(Fraction value) => true;

        public static bool IsComplexNumber(Fraction value) => false;

        public static bool IsEvenInteger(Fraction value) => value.Top.IsEven && value.Bottom == 1;

        public static bool IsFinite(Fraction value) => true;

        public static bool IsImaginaryNumber(Fraction value) => false;

        public static bool IsInfinity(Fraction value) => false;

        public static bool IsInteger(Fraction value) => value.Bottom == 1;

        public static bool IsNaN(Fraction value) => false;

        public static bool IsNegative(Fraction value) => value.Top < 0;

        public static bool IsNegativeInfinity(Fraction value) => false;

        public static bool IsNormal(Fraction value) => true;

        public static bool IsOddInteger(Fraction value) => !value.Top.IsEven && value.Bottom == 1;

        public static bool IsPositive(Fraction value) => value.Top >= 0;

        public static bool IsPositiveInfinity(Fraction value) => false;

        public static bool IsRealNumber(Fraction value) => true;

        public static bool IsSubnormal(Fraction value) => false;

        public static bool IsZero(Fraction value) => value.Top == 0;

        public static Fraction MaxMagnitude(Fraction x, Fraction y) => Abs(x) > Abs(y) ? x : y;

        public static Fraction MaxMagnitudeNumber(Fraction x, Fraction y) => MaxMagnitude(x, y);

        public static Fraction MinMagnitude(Fraction x, Fraction y) => Abs(x) < Abs(y) ? x : y;

        public static Fraction MinMagnitudeNumber(Fraction x, Fraction y) => MinMagnitude(x, y);

        public static Fraction Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static Fraction Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Fraction result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Fraction result)
        {
            throw new NotImplementedException();
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }

        public static Fraction Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Fraction result)
        {
            throw new NotImplementedException();
        }

        public static Fraction Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Fraction result)
        {
            throw new NotImplementedException();
        }

        static bool TryConvertFromChecked<TOther>(TOther value, out Fraction result)
        {
            if (typeof(TOther) == typeof(double))
            {
                result = (Fraction)((double)((Half)(object)value!));
                return true;
            }
            else if (typeof(TOther) == typeof(Half))
            {
                result = (Fraction)((double)((Half)(object)value!));
                return true;
            }
            else if (typeof(TOther) == typeof(short))
            {
                result = (short)(object)value!;
                return true;
            }
            else if (typeof(TOther) == typeof(long))
            {
                result = (long)(object)value!;
                return true;
            }
            else if (typeof(TOther) == typeof(Int128))
            {
                result = (Int128)(object)value!;
                return true;
            }
            else if (typeof(TOther) == typeof(nint))
            {
                result = (long)((nint)(object)value!);
                return true;
            }
            else if (typeof(TOther) == typeof(sbyte))
            {
                result = (sbyte)(object)value!;
                return true;
            }
            else if (typeof(TOther) == typeof(float))
            {
                result = (float)(object)value!;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        static bool INumberBase<Fraction>.TryConvertFromChecked<TOther>(TOther value, out Fraction result) => TryConvertFromChecked(value, out result);

        static bool INumberBase<Fraction>.TryConvertFromSaturating<TOther>(TOther value, out Fraction result) => TryConvertFromChecked(value, out result);

        static bool INumberBase<Fraction>.TryConvertFromTruncating<TOther>(TOther value, out Fraction result) => TryConvertFromChecked(value, out result);

        static bool INumberBase<Fraction>.TryConvertToChecked<TOther>(Fraction value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Fraction>.TryConvertToSaturating<TOther>(Fraction value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<Fraction>.TryConvertToTruncating<TOther>(Fraction value, out TOther result)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(Fraction _a, Fraction _b) => (_a.Top == _b.Top) && (_a.Bottom == _b.Bottom);
        public static bool operator !=(Fraction _a, Fraction _b) => (_a.Top != _b.Top) || (_a.Bottom != _b.Bottom);

        public static bool operator >=(Fraction _a, Fraction _b) => (_a - _b).Top >= 0;
        public static bool operator <=(Fraction _a, Fraction _b) => (_a - _b).Top <= 0;
        public static bool operator >(Fraction _a, Fraction _b) => (_a - _b).Top > 0;
        public static bool operator <(Fraction _a, Fraction _b) => (_a - _b).Top < 0;

        public static Fraction operator -(Fraction _a) => new Fraction(-_a.Top, _a.Bottom);
        public static Fraction operator +(Fraction _a, Fraction _b) => new Fraction(_a.Top * _b.Bottom + _b.Top * _a.Bottom, _a.Bottom * _b.Bottom);
        public static Fraction operator -(Fraction _a, Fraction _b) => new Fraction(_a.Top * _b.Bottom - _b.Top * _a.Bottom, _a.Bottom * _b.Bottom);
        public static Fraction operator *(Fraction _a, Fraction _b) => new Fraction(_a.Top * _b.Top, _a.Bottom * _b.Bottom);
        public static Fraction operator /(Fraction _a, Fraction _b) => new Fraction(_a.Top * _b.Bottom, _a.Bottom * _b.Top);

        public static Fraction operator --(Fraction _a) => new Fraction(_a.Top - _a.Bottom, _a.Bottom);

        public static Fraction operator ++(Fraction _a) => new Fraction(_a.Top + _a.Bottom, _a.Bottom);

        public static Fraction operator +(Fraction _a) => _a;
    }
}
