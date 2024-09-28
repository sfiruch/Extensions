// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

/// <summary>
/// Provides an implementation of the xoshiro256** algorithm. This implementation is used
/// on 64-bit when no seed is specified and an instance of the base Random class is constructed.
/// As such, we are free to implement however we see fit, without back compat concerns around
/// the sequence of numbers generated or what methods call what other methods.
/// </summary>
public class Random
{
    private static readonly ThreadLocal<Random> ThreadLocalStorage = new ThreadLocal<Random>(() => new Random());

    public static Random ThreadLocal => ThreadLocalStorage.Value!;

    // NextUInt64 is based on the algorithm from http://prng.di.unimi.it/xoshiro256starstar.c:
    //
    //     Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)
    //
    //     To the extent possible under law, the author has dedicated all copyright
    //     and related and neighboring rights to this software to the public domain
    //     worldwide. This software is distributed without any warranty.
    //
    //     See <http://creativecommons.org/publicdomain/zero/1.0/>.

    private ulong _s0, _s1, _s2, _s3;

    public Random()
    {
        do
        {
            var buf = RandomNumberGenerator.GetBytes(8 * 4);

            _s0 = BitConverter.ToUInt64(buf[0..8]);
            _s1 = BitConverter.ToUInt64(buf[8..16]);
            _s2 = BitConverter.ToUInt64(buf[16..24]);
            _s3 = BitConverter.ToUInt64(buf[24..32]);
        }
        while ((_s0 | _s1 | _s2 | _s3) == 0); // at least one value must be non-zero
    }

    public Random(ulong _seed)
    {
        using (var hash = SHA256.Create())
            do
            {
                var buf = hash.ComputeHash(BitConverter.GetBytes(_seed));
                _seed++;

                _s0 = BitConverter.ToUInt64(buf, 0);
                _s1 = BitConverter.ToUInt64(buf, 8);
                _s2 = BitConverter.ToUInt64(buf, 16);
                _s3 = BitConverter.ToUInt64(buf, 24);
            }
            while ((_s0 | _s1 | _s2 | _s3) == 0); // at least one value must be non-zero
    }

    /// <summary>Produces a value in the range [0, ulong.MaxValue].</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // small-ish hot path used by a handful of "next" methods
    internal ulong NextUInt64()
    {
        ulong result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return result;
    }

    public int Next()
    {
        while (true)
        {
            // Get top 31 bits to get a value in the range [0, int.MaxValue], but try again
            // if the value is actually int.MaxValue, as the method is defined to return a value
            // in the range [0, int.MaxValue).
            ulong result = NextUInt64() >> 33;
            if (result != int.MaxValue)
            {
                return (int)result;
            }
        }
    }

    public int Next(int maxValue)
    {
        if (maxValue > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling((uint)maxValue);
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < (uint)maxValue)
                {
                    return (int)result;
                }
            }
        }

        Debug.Assert(maxValue == 0 || maxValue == 1);
        return 0;
    }

    public int Next(int minValue, int maxValue)
    {
        ulong exclusiveRange = (ulong)(maxValue - minValue);

        if (exclusiveRange > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling(exclusiveRange);
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < exclusiveRange)
                {
                    return (int)result + minValue;
                }
            }
        }

        Debug.Assert(minValue == maxValue || minValue + 1 == maxValue);
        return minValue;
    }

    public long NextInt64()
    {
        while (true)
        {
            // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
            // if the value is actually long.MaxValue, as the method is defined to return a value
            // in the range [0, long.MaxValue).
            ulong result = NextUInt64() >> 1;
            if (result != long.MaxValue)
            {
                return (long)result;
            }
        }
    }

    /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Log2Ceiling(uint value)
    {
        int result = BitOperations.Log2(value);
        if (BitOperations.PopCount(value) != 1)
        {
            result++;
        }
        return result;
    }

    /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Log2Ceiling(ulong value)
    {
        int result = BitOperations.Log2(value);
        if (BitOperations.PopCount(value) != 1)
        {
            result++;
        }
        return result;
    }

    public long NextInt64(long maxValue)
    {
        if (maxValue > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling((ulong)maxValue);
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < (ulong)maxValue)
                {
                    return (long)result;
                }
            }
        }

        Debug.Assert(maxValue == 0 || maxValue == 1);
        return 0;
    }

    public long NextInt64(long minValue, long maxValue)
    {
        ulong exclusiveRange = (ulong)(maxValue - minValue);

        if (exclusiveRange > 1)
        {
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = Log2Ceiling(exclusiveRange);
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < exclusiveRange)
                {
                    return (long)result + minValue;
                }
            }
        }

        Debug.Assert(minValue == maxValue || minValue + 1 == maxValue);
        return minValue;
    }

    public void NextBytes(byte[] buffer) => NextBytes((Span<byte>)buffer);

    public unsafe void NextBytes(Span<byte> buffer)
    {
        while (buffer.Length >= sizeof(ulong))
        {
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), NextUInt64());
            buffer = buffer.Slice(sizeof(ulong));
        }

        if (!buffer.IsEmpty)
        {
            ulong next = NextUInt64();
            byte* remainingBytes = (byte*)&next;
            Debug.Assert(buffer.Length < sizeof(ulong));
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = remainingBytes[i];
            }
        }
    }

    public double NextDouble() =>
        // As described in http://prng.di.unimi.it/:
        // "A standard double (64-bit) floating-point number in IEEE floating point format has 52 bits of significand,
        //  plus an implicit bit at the left of the significand. Thus, the representation can actually store numbers with
        //  53 significant binary digits. Because of this fact, in C99 a 64-bit unsigned integer x should be converted to
        //  a 64-bit double using the expression
        //  (x >> 11) * 0x1.0p-53"
        (NextUInt64() >> 11) * (1.0 / (1ul << 53));

    public float NextSingle() =>
        // Same as above, but with 24 bits instead of 53.
        (NextUInt64() >> 40) * (1.0f / (1u << 24));


    public float NextGaussianF()
    {
        //see https://marc-b-reynolds.github.io/distribution/2021/03/18/CheapGaussianApprox.html
        var u0 = NextUInt64();
        long bd = BitOperations.PopCount(u0) - 32;
        var u1 = NextUInt64();
        return ((bd << 32) + unchecked((uint)(u1 >> 32)) - unchecked((uint)u1)) * 5.76916501E-11f;
    }

    private double NextGaussianLatched = double.NaN;
    public double NextGaussian()
    {
        if (!double.IsNaN(NextGaussianLatched))
        {
            var tmp = NextGaussianLatched;
            NextGaussianLatched = double.NaN;
            return tmp;
        }

        var u1 = NextDouble();
        var u2 = NextDouble();
        var temp1 = Math.Sqrt(-2 * Math.Log(u1));
        var temp2 = 2 * Math.PI * u2;

        NextGaussianLatched = temp1 * Math.Cos(temp2);
        return temp1 * Math.Sin(temp2);
    }
}
