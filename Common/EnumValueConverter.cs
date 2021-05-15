﻿using Avalonia.Data.Converters;
using System;

namespace BugFablesEntityEditor
{
  public class EnumValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
    {
      int intValue = 0;
      if (parameter is Type)
      {
        intValue = (int)Enum.Parse((Type)parameter, value.ToString());
      }
      return intValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
    {
      Enum enumValue = default(Enum);
      if (parameter is Type)
      {
        enumValue = (Enum)Enum.Parse((Type)parameter, value.ToString());
      }
      return enumValue;
    }
  }
}
