// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace McMaster.DotNet.Serve;

internal static class DotNetUtilities
{
    public static RSA ToRSA(this RsaPrivateCrtKeyParameters privKey)
    {
        return RSA.Create(privKey.ToRSAParameters());
    }

    // partially copied from https://github.com/bcgit/bc-csharp/blob/7248688e6f513cbdde1ccc1d39904cb964b0c88a/crypto/src/security/DotNetUtilities.cs
    private static RSAParameters ToRSAParameters(this RsaPrivateCrtKeyParameters privKey)
    {
        var rp = new RSAParameters
        {
            Modulus = privKey.Modulus.ToByteArrayUnsigned(),
            Exponent = privKey.PublicExponent.ToByteArrayUnsigned(),
            P = privKey.P.ToByteArrayUnsigned(),
            Q = privKey.Q.ToByteArrayUnsigned(),
        };
        rp.D = ConvertRSAParametersField(privKey.Exponent, rp.Modulus.Length);
        rp.DP = ConvertRSAParametersField(privKey.DP, rp.P.Length);
        rp.DQ = ConvertRSAParametersField(privKey.DQ, rp.Q.Length);
        rp.InverseQ = ConvertRSAParametersField(privKey.QInv, rp.Q.Length);
        return rp;
    }

    private static byte[] ConvertRSAParametersField(this BigInteger n, int size)
    {
        var bs = n.ToByteArrayUnsigned();

        if (bs.Length == size)
        {
            return bs;
        }

        if (bs.Length > size)
        {
            throw new ArgumentException("Specified size too small", nameof(size));
        }

        var padded = new byte[size];
        Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
        return padded;
    }
}
