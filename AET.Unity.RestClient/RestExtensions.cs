using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AET.Unity.SimplSharp;

namespace AET.Unity.RestClient {
  public static class RestExtensions {
    #region Helper Methods
    public static ushort ConvertTo16Bit(this int value, int scale) {
      return (ushort)(value * 65535 / scale);
    }

    public static ushort ConvertTo16Bit(this long? value, int scale) {
      return ConvertTo16Bit((int)(value ?? 0), scale);
    }

    public static ushort ConvertFrom16Bit(this ushort value, int scale) {
      return (ushort)((value * scale) / 65535);
    }

    public static bool FalseWithErrorMessage(string message) {
      ErrorMessage.Error(message);
      return false;
    }

    public static bool FalseWithErrorMessage(string message, params object[] args) {
      ErrorMessage.Error(message, args);
      return false;
    }

    public static string Clean(this string value) {
      if (value == null) return null;
      if (value == "") return null;
      return value.ToLower();
    }

    public static bool ValueIsValid<T>(this T value, string name, T[] allowedValues) {
      if (value == null) return true;
      if (allowedValues.Contains(value)) return true;
      return FalseWithErrorMessage("{0} must be {1}.", name, allowedValues.FormatAsList());
    }

    public static bool ValueIsValid(this ushort? value, string name, ushort minValue, ushort maxValue) {
      if (value == null) return true;
      if (value >= minValue && value <= maxValue) return true;
      return FalseWithErrorMessage("{0} must be {1} to {2}.", name, minValue, maxValue);
    }

    #endregion

  }
}
