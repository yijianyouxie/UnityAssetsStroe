using UnityEngine;

namespace Games.Util
{
    public sealed class Helper
    {
        private static Vector3 tempVector3;
        public static Vector3 Normalize(Vector3 dir)
        {
            float sqr_len = dir.x * dir.x + dir.y * dir.y + dir.z * dir.z;
            //return dir * InvSqrt(sqr_len);
            float invSqrt = InvSqrt(sqr_len);
            //return new Vector3(dir.x * invSqrt, dir.y * invSqrt, dir.z * invSqrt);
            tempVector3.x = dir.x * invSqrt;
            tempVector3.y = dir.y * invSqrt;
            tempVector3.z = dir.z * invSqrt;
            return tempVector3;
        }
        unsafe public static float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = 0x5f3759df - (i >> 1);
            x = *(float*)&i;
            x = x * (1.5f - xhalf * x * x);
            return x;
        }
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float sqr_x = (a.x - b.x) * (a.x - b.x);
            float sqr_y = (a.y - b.y) * (a.x - b.x);
            float sqr_z = (a.z - b.z) * (a.z - b.z);
            return sqr_x + sqr_y + sqr_z;
        }
    }
    public static class Vector3Extension
    {
        private static Vector3 tempVector3;
        public static Vector3 FastNormalize(this Vector3 dir)
        {
            float sqr_len = dir.x * dir.x + dir.y * dir.y + dir.z * dir.z;
            //return dir * InvSqrt(sqr_len);
            float invSqrt = InvSqrt(sqr_len);
            //return new Vector3(dir.x * invSqrt, dir.y * invSqrt, dir.z * invSqrt);
            tempVector3.x = dir.x * invSqrt;
            tempVector3.y = dir.y * invSqrt;
            tempVector3.z = dir.z * invSqrt;
            return tempVector3;
        }
        unsafe public static float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = 0x5f3759df - (i >> 1);
            x = *(float*)&i;
            x = x * (1.5f - xhalf * x * x);
            return x;
        }

        public static float SqrDistance(this Vector3 a, Vector3 b)
        {
            float sqr_x = (a.x - b.x) * (a.x - b.x);
            float sqr_y = (a.y - b.y) * (a.x - b.x);
            float sqr_z = (a.z - b.z) * (a.z - b.z);
            return sqr_x + sqr_y + sqr_z;
        }

        public static Vector3 Add(this Vector3 a, Vector3 b)
        {
            tempVector3.x = a.x + b.x;
            tempVector3.y = a.y + b.y;
            tempVector3.z = a.z + b.z;
            return tempVector3;
        }
        public static Vector3 Sub(this Vector3 a, Vector3 b)
        {
            tempVector3.x = a.x - b.x;
            tempVector3.y = a.y - b.y;
            tempVector3.z = a.z - b.z;
            return tempVector3;
        }

        public static Vector3 Mul(this Vector3 a, float b)
        {
            tempVector3.x = a.x * b;
            tempVector3.y = a.y * b;
            tempVector3.z = a.z * b;
            return tempVector3;
        }

        public static Vector3 AddMul(this Vector3 a, Vector3 b, float c)
        {
            tempVector3.x = a.x + b.x;
            tempVector3.y = a.y + b.y;
            tempVector3.z = a.z + b.z;

            tempVector3.x *= c;
            tempVector3.y *= c;
            tempVector3.z *= c;
            return tempVector3;
        }
    }

    public static class QuaternionExtension
    {
        private static Vector3 tempVector3;
        private static Quaternion temQuaternion;
        public static Vector3 Mul(this Quaternion rotation, Vector3 point)
        {
            float num = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            tempVector3.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            tempVector3.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            tempVector3.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
            return tempVector3;
        }
        public static Quaternion Mul(this Quaternion lhs, Quaternion rhs)
        {
            temQuaternion.x = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
            temQuaternion.y = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
            temQuaternion.z = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
            temQuaternion.w = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
            return temQuaternion;
        }
    }
}

