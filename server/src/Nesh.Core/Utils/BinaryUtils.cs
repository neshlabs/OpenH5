using System;
using System.Linq;

namespace Nesh.Core.Utils
{
    public static class BinaryUtils
    {
        public const long POW_2_62 = 4611686018427387904;

        public static bool IsBinaryPower(long n)
        {
            return (n & (n - 1)) == 0;
        }

        public static long GetBinaryPower(int x)
        {
            return (long)Math.Pow(2, x);
        }

        public static string GetBinary(long n)
        {
            return Convert.ToString(n, 2);
        }

        public static long GenAuthCode(params long[] arr)
        {
            if (arr == null)
                throw new Exception("arr cant be null");
            long code = 0;
            arr.ToList().ForEach(x =>
            {
                if (!IsBinaryPower(x))
                    throw new Exception(string.Format($"AuthCode {x} unable 2 power", x));
                if (x < 0 || x > 4611686018427387904)
                    throw new Exception(string.Format($"AuthCode {x} must > 0 and < {POW_2_62}"));
                code = code | x;
            });
            return code;
        }

        public static long AddAuth(long auth_code, long auth)
        {
            if (!IsBinaryPower(auth))
                throw new Exception(string.Format($"AuthCode {auth} unable 2 power", auth));

            if (auth < 0 || auth > 4611686018427387904)
                throw new Exception(string.Format($"AuthCode {auth} must > 0 and < {POW_2_62}"));

            long code = auth_code | auth;
            return code;
        }

        public static long RemoveAuth(long auth_code, long auth)
        {
            if (!IsBinaryPower(auth))
                throw new Exception(string.Format($"AuthCode {auth} unable 2 power", auth));

            if (auth < 0 || auth > 4611686018427387904)
                throw new Exception(string.Format($"AuthCode {auth} must > 0 and < {POW_2_62}"));

            long code = auth_code & (~auth);
            return code;
        }

        public static bool IsHasAuth(long auth_code, long auth)
        {
            if (!IsBinaryPower(auth))
                throw new Exception(string.Format($"AuthCode {auth} unable 2 power", auth));

            if (auth_code <= 0 || auth <= 0)
                return false;

            return auth == (auth_code & auth);
        }
    }
}
