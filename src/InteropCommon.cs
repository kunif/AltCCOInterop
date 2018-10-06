
namespace POS.AltCCOInterop
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.PointOfService;
    using POS.Devices;

    internal static class InteropCommon
    {
        #region Device Common Utility function

        internal static Version ToVersion(int oposVersion)
        {
            int major = oposVersion / 1000000;
            int minor = (oposVersion % 1000000) / 1000;
            int build = oposVersion % 1000;
            return new Version(major, minor, build);
        }

        internal static string ToStatisticsString(Statistic[] statisticsArray)
        {
            string Result = "";

            if ((statisticsArray != null) && (statisticsArray.Length > 0))
            {
                foreach (Statistic stc in statisticsArray)
                {
                    Result += stc.Name + "=" + stc.Value + ",";
                }
                Result = Result.TrimEnd(',');
            }
            return Convert.ToString(Result);
        }

        internal static int[] ToIntegerArray(string integerListString, Char separator)
        {
            List<int> Result = new List<int>();

            if (!string.IsNullOrWhiteSpace(integerListString))
            {
                foreach (string s in integerListString.Split(separator))
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }

                    int value = 0;

                    if (Int32.TryParse(s.Trim(), (System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowLeadingSign), System.Globalization.CultureInfo.InvariantCulture, out value))
                    {
                        Result.Add(value);
                    }
                }
            }

            return Result.ToArray();
        }

        #endregion

        #region BinaryConversion related function

        internal static string ToStringFromByteArray(byte[] value, int binaryConversion)
        {
            if (value == null) return null;
            if (value.Length <= 0) return String.Empty;

            StringBuilder Result = new StringBuilder(value.Length);

            switch (binaryConversion)
            {
                case 0: // None
                    foreach (byte b in value)
                    {
                        Result.Append((char)b);
                    }

                    break;

                case 1: // Nibble
                    Result.Capacity = value.Length * 2;

                    foreach (byte b in value)
                    {
                        Result.Append((char)(((b & 0xF0) >> 4) + '0'));
                        Result.Append((char)((b & 0x0F) + '0'));
                    }

                    break;

                case 2: // Decimal
                    Result.Capacity = value.Length * 3;

                    foreach (byte b in value)
                    {
                        Result.AppendFormat("D3", b);
                    }

                    break;
            }

            return Result.ToString();
        }

        internal static byte[] ToByteArrayFromString(string value, int binaryConversion)
        {
            if (value == null) return null;

            byte[] Result = null;
            int ResultLength = value.Length;
            int Index = 0;
            if (value.Length <= 0) return Result;

            switch (binaryConversion)
            {
                case 0: // None
                    Result = new byte[ResultLength];

                    foreach (char c in value)
                    {
                        Result[Index++] = (byte)c;
                    }

                    break;

                case 1: // Nibble
                    ResultLength /= 2;
                    Result = new byte[ResultLength];

                    for (int i = 0; i < ResultLength; i++)
                    {
                        Result[i] = (byte)(((value[Index++] & 0x0F) << 4) + (value[Index++] & 0x0F));
                    }

                    break;

                case 2: // Decimal
                    ResultLength /= 3;
                    Result = new byte[ResultLength];

                    for (int i = 0; i < ResultLength; i++)
                    {
                        Result[i] = (byte)(int.Parse(value.Substring((i * 3), 3)));
                    }

                    break;

                default:
                    Result = new byte[0];
                    break;
            }

            return Result;
        }

        #endregion

        #region Print Rotation related function

        internal static Rotation[] ToRotationArray(string rotationListString)
        {
            List<Rotation> Result = new List<Rotation>();

            if (!string.IsNullOrWhiteSpace(rotationListString))
            {
                foreach (string s in rotationListString.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }

                    switch (s.Trim().ToUpper())
                    {
                        case "0": Result.Add(Rotation.Normal); break;

                        case "R90": Result.Add(Rotation.Right90); break;

                        case "L90": Result.Add(Rotation.Left90); break;

                        case "180": Result.Add(Rotation.Rotate180); break;
                    }
                }
            }

            return Result.ToArray();
        }

        #endregion

        #region Cash related function, CashChanger, Coin/Bill Acceptor/Dispenser

        internal static CashUnits ToCashUnits(string cashUnitsListString)
        {
            int[] Coins = null;
            int[] Bills = null;

            if (!string.IsNullOrWhiteSpace(cashUnitsListString))
            {
                string[] sa = cashUnitsListString.Split(';');

                if (!string.IsNullOrWhiteSpace(sa[0]))
                {
                    Coins = ToIntegerArray(sa[0], ',');
                }

                if ((sa.Length >= 2) && (!string.IsNullOrWhiteSpace(sa[1])))
                {
                    Bills = ToIntegerArray(sa[1], ',');
                }
            }

            return new CashUnits(Coins, Bills);
        }

        internal static CashCount[] ToCashCountArray(string cashCountsListString)
        {
            List<CashCount> Result = new List<CashCount>();

            if (!string.IsNullOrWhiteSpace(cashCountsListString))
            {
                string[] sa = cashCountsListString.Split(';');

                if (!string.IsNullOrWhiteSpace(sa[0]))
                {
                    foreach (string s in sa[0].Split(','))
                    {
                        int[] cc = ToIntegerArray(s, ':');

                        if ((cc.Length == 1) && (cc[0] != 0))
                        {
                            Result.Add(new CashCount(CashCountType.Coin, cc[0], 0));
                        }

                        if ((cc.Length >= 2) && (cc[0] != 0))
                        {
                            Result.Add(new CashCount(CashCountType.Coin, cc[0], cc[1]));
                        }
                    }
                }

                if ((sa.Length >= 2) && (!string.IsNullOrWhiteSpace(sa[1])))
                {
                    foreach (string s in sa[1].Split(','))
                    {
                        int[] cc = ToIntegerArray(s, ':');

                        if ((cc.Length == 1) && (cc[0] != 0))
                        {
                            Result.Add(new CashCount(CashCountType.Bill, cc[0], 0));
                        }

                        if ((cc.Length >= 2) && (cc[0] != 0))
                        {
                            Result.Add(new CashCount(CashCountType.Bill, cc[0], cc[1]));
                        }
                    }
                }
            }

            return Result.ToArray();
        }

        internal static string ToCashCountsString(IEnumerable<CashCount> cashCountsArray)
        {
            if (cashCountsArray == null) return String.Empty;

            string Result = "";

            foreach (CashCount cc in cashCountsArray)
            {
                if (cc.Type == CashCountType.Coin)
                {
                    Result += Convert.ToString(cc.NominalValue) + ':' + Convert.ToString(cc.Count) + ',';
                }
            }

            Result.TrimEnd(',');
            Result += ';';

            foreach (CashCount cc in cashCountsArray)
            {
                if (cc.Type == CashCountType.Bill)
                {
                    Result += Convert.ToString(cc.NominalValue) + ':' + Convert.ToString(cc.Count) + ',';
                }
            }

            Result.TrimEnd(',');
            return Convert.ToString(Result);
        }

        #endregion
    }

    #region Enum convert function

    internal static class InteropEnum<TEnum>
    {
        internal static TEnum ToEnumFromInteger(int value)
        {
            try
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }
            catch
            {
                throw new Microsoft.PointOfService.PosControlException(typeof(TEnum).Name, ErrorCode.Illegal, value);
            }
        }
    }

    #endregion
}
