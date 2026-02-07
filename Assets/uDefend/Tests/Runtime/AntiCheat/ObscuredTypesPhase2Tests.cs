using System;
using NUnit.Framework;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat
{
    public class ObscuredTypesPhase2Tests
    {
        // ===== ObscuredLong =====

        [Test]
        public void ObscuredLong_RoundTrip()
        {
            ObscuredLong obscured = 123456789012345L;
            long result = obscured;
            Assert.AreEqual(123456789012345L, result);
        }

        [Test]
        public void ObscuredLong_Arithmetic()
        {
            ObscuredLong a = 100L;
            ObscuredLong b = 30L;
            Assert.AreEqual(130L, (long)(a + b));
            Assert.AreEqual(70L, (long)(a - b));
            Assert.AreEqual(3000L, (long)(a * b));
        }

        [Test]
        public void ObscuredLong_NegativeValue()
        {
            ObscuredLong obscured = -999999999999L;
            Assert.AreEqual(-999999999999L, (long)obscured);
        }

        // ===== ObscuredDouble =====

        [Test]
        public void ObscuredDouble_RoundTrip()
        {
            ObscuredDouble obscured = 3.141592653589793d;
            double result = obscured;
            Assert.AreEqual(3.141592653589793d, result, 0.000000000001d);
        }

        [Test]
        public void ObscuredDouble_Arithmetic()
        {
            ObscuredDouble a = 10.5d;
            ObscuredDouble b = 3.2d;
            Assert.AreEqual(10.5d + 3.2d, (double)(a + b), 0.0001d);
            Assert.AreEqual(10.5d * 3.2d, (double)(a * b), 0.001d);
        }

        // ===== ObscuredByte =====

        [Test]
        public void ObscuredByte_RoundTrip()
        {
            ObscuredByte obscured = (byte)200;
            byte result = obscured;
            Assert.AreEqual((byte)200, result);
        }

        [Test]
        public void ObscuredByte_Arithmetic()
        {
            ObscuredByte a = (byte)10;
            ObscuredByte b = (byte)3;
            Assert.AreEqual((byte)13, (byte)(a + b));
        }

        // ===== ObscuredSByte =====

        [Test]
        public void ObscuredSbyte_RoundTrip()
        {
            ObscuredSbyte obscured = (sbyte)-50;
            sbyte result = obscured;
            Assert.AreEqual((sbyte)-50, result);
        }

        // ===== ObscuredShort =====

        [Test]
        public void ObscuredShort_RoundTrip()
        {
            ObscuredShort obscured = (short)-12345;
            short result = obscured;
            Assert.AreEqual((short)-12345, result);
        }

        [Test]
        public void ObscuredShort_Arithmetic()
        {
            ObscuredShort a = (short)100;
            ObscuredShort b = (short)50;
            Assert.AreEqual((short)150, (short)(a + b));
        }

        // ===== ObscuredUShort =====

        [Test]
        public void ObscuredUshort_RoundTrip()
        {
            ObscuredUshort obscured = (ushort)60000;
            ushort result = obscured;
            Assert.AreEqual((ushort)60000, result);
        }

        // ===== ObscuredUInt =====

        [Test]
        public void ObscuredUint_RoundTrip()
        {
            ObscuredUint obscured = 3000000000U;
            uint result = obscured;
            Assert.AreEqual(3000000000U, result);
        }

        [Test]
        public void ObscuredUint_Arithmetic()
        {
            ObscuredUint a = 100U;
            ObscuredUint b = 30U;
            Assert.AreEqual(130U, (uint)(a + b));
        }

        // ===== ObscuredULong =====

        [Test]
        public void ObscuredUlong_RoundTrip()
        {
            ObscuredUlong obscured = 18000000000000000000UL;
            ulong result = obscured;
            Assert.AreEqual(18000000000000000000UL, result);
        }

        // ===== ObscuredDecimal =====

        [Test]
        public void ObscuredDecimal_RoundTrip()
        {
            ObscuredDecimal obscured = 12345.6789m;
            decimal result = obscured;
            Assert.AreEqual(12345.6789m, result);
        }

        [Test]
        public void ObscuredDecimal_Arithmetic()
        {
            ObscuredDecimal a = 100.5m;
            ObscuredDecimal b = 0.5m;
            Assert.AreEqual(101m, (decimal)(a + b));
        }

        // ===== ObscuredChar =====

        [Test]
        public void ObscuredChar_RoundTrip()
        {
            ObscuredChar obscured = 'Z';
            char result = obscured;
            Assert.AreEqual('Z', result);
        }

        [Test]
        public void ObscuredChar_UnicodeRoundTrip()
        {
            ObscuredChar obscured = '\u3042'; // hiragana 'a'
            char result = obscured;
            Assert.AreEqual('\u3042', result);
        }

        // ===== ObscuredVector2 =====

        [Test]
        public void ObscuredVector2_RoundTrip()
        {
            ObscuredVector2 obscured = new Vector2(1.5f, 2.5f);
            Vector2 result = obscured;
            Assert.AreEqual(1.5f, result.x, 0.0001f);
            Assert.AreEqual(2.5f, result.y, 0.0001f);
        }

        [Test]
        public void ObscuredVector2_Arithmetic()
        {
            ObscuredVector2 a = new Vector2(1f, 2f);
            ObscuredVector2 b = new Vector2(3f, 4f);
            Vector2 sum = a + b;
            Assert.AreEqual(4f, sum.x, 0.0001f);
            Assert.AreEqual(6f, sum.y, 0.0001f);
        }

        // ===== ObscuredVector2Int =====

        [Test]
        public void ObscuredVector2Int_RoundTrip()
        {
            ObscuredVector2Int obscured = new Vector2Int(10, -20);
            Vector2Int result = obscured;
            Assert.AreEqual(10, result.x);
            Assert.AreEqual(-20, result.y);
        }

        [Test]
        public void ObscuredVector2Int_Arithmetic()
        {
            ObscuredVector2Int a = new Vector2Int(1, 2);
            ObscuredVector2Int b = new Vector2Int(3, 4);
            Vector2Int sum = a + b;
            Assert.AreEqual(4, sum.x);
            Assert.AreEqual(6, sum.y);
        }

        // ===== ObscuredVector3 =====

        [Test]
        public void ObscuredVector3_RoundTrip()
        {
            ObscuredVector3 obscured = new Vector3(1.5f, 2.5f, 3.5f);
            Vector3 result = obscured;
            Assert.AreEqual(1.5f, result.x, 0.0001f);
            Assert.AreEqual(2.5f, result.y, 0.0001f);
            Assert.AreEqual(3.5f, result.z, 0.0001f);
        }

        [Test]
        public void ObscuredVector3_Arithmetic()
        {
            ObscuredVector3 a = new Vector3(1f, 2f, 3f);
            ObscuredVector3 b = new Vector3(4f, 5f, 6f);
            Vector3 sum = a + b;
            Assert.AreEqual(5f, sum.x, 0.0001f);
            Assert.AreEqual(7f, sum.y, 0.0001f);
            Assert.AreEqual(9f, sum.z, 0.0001f);
        }

        [Test]
        public void ObscuredVector3_ScalarMultiply()
        {
            ObscuredVector3 v = new Vector3(1f, 2f, 3f);
            Vector3 result = v * 2f;
            Assert.AreEqual(2f, result.x, 0.0001f);
            Assert.AreEqual(4f, result.y, 0.0001f);
            Assert.AreEqual(6f, result.z, 0.0001f);
        }

        // ===== ObscuredVector3Int =====

        [Test]
        public void ObscuredVector3Int_RoundTrip()
        {
            ObscuredVector3Int obscured = new Vector3Int(10, -20, 30);
            Vector3Int result = obscured;
            Assert.AreEqual(10, result.x);
            Assert.AreEqual(-20, result.y);
            Assert.AreEqual(30, result.z);
        }

        [Test]
        public void ObscuredVector3Int_Arithmetic()
        {
            ObscuredVector3Int a = new Vector3Int(1, 2, 3);
            ObscuredVector3Int b = new Vector3Int(4, 5, 6);
            Vector3Int sum = a + b;
            Assert.AreEqual(5, sum.x);
            Assert.AreEqual(7, sum.y);
            Assert.AreEqual(9, sum.z);
        }

        // ===== ObscuredQuaternion =====

        [Test]
        public void ObscuredQuaternion_RoundTrip()
        {
            Quaternion original = Quaternion.Euler(30f, 60f, 90f);
            ObscuredQuaternion obscured = original;
            Quaternion result = obscured;
            Assert.AreEqual(original.x, result.x, 0.0001f);
            Assert.AreEqual(original.y, result.y, 0.0001f);
            Assert.AreEqual(original.z, result.z, 0.0001f);
            Assert.AreEqual(original.w, result.w, 0.0001f);
        }

        [Test]
        public void ObscuredQuaternion_Identity()
        {
            ObscuredQuaternion obscured = Quaternion.identity;
            Quaternion result = obscured;
            Assert.AreEqual(Quaternion.identity.x, result.x, 0.0001f);
            Assert.AreEqual(Quaternion.identity.y, result.y, 0.0001f);
            Assert.AreEqual(Quaternion.identity.z, result.z, 0.0001f);
            Assert.AreEqual(Quaternion.identity.w, result.w, 0.0001f);
        }

        // ===== Default struct behavior =====

        [Test]
        public void DefaultStruct_ReturnsZero()
        {
            ObscuredInt defaultInt = default;
            ObscuredFloat defaultFloat = default;
            ObscuredLong defaultLong = default;
            ObscuredDouble defaultDouble = default;
            ObscuredBool defaultBool = default;

            Assert.AreEqual(0, (int)defaultInt);
            Assert.AreEqual(0f, (float)defaultFloat, 0.0001f);
            Assert.AreEqual(0L, (long)defaultLong);
            Assert.AreEqual(0d, (double)defaultDouble, 0.0001d);
            Assert.IsFalse(defaultBool);
        }

        [Test]
        public void DefaultVectorStruct_ReturnsZero()
        {
            ObscuredVector2 defaultV2 = default;
            ObscuredVector3 defaultV3 = default;
            ObscuredVector2Int defaultV2I = default;
            ObscuredVector3Int defaultV3I = default;

            Assert.AreEqual(Vector2.zero, (Vector2)defaultV2);
            Assert.AreEqual(Vector3.zero, (Vector3)defaultV3);
            Assert.AreEqual(Vector2Int.zero, (Vector2Int)defaultV2I);
            Assert.AreEqual(Vector3Int.zero, (Vector3Int)defaultV3I);
        }
    }
}
