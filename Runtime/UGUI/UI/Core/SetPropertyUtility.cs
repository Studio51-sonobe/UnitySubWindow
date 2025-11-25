
using UnityEngine;
using System.Collections.Generic;

namespace MultiWindow.UI
{
    internal static class SetPropertyUtility
    {
        public static bool SetColor( ref Color currentValue, Color newValue)
        {
            if( currentValue.r == newValue.r
            &&  currentValue.g == newValue.g
            &&  currentValue.b == newValue.b
            &&  currentValue.a == newValue.a)
            {
                return false;
            }
            currentValue = newValue;
            return true;
        }
        public static bool SetStruct<T>( ref T currentValue, T newValue) where T : struct
        {
            if( EqualityComparer<T>.Default.Equals( currentValue, newValue) != false)
            {
                return false;
            }
            currentValue = newValue;
            return true;
        }
        public static bool SetClass<T>( ref T currentValue, T newValue) where T : class
        {
            if( (currentValue == null && newValue == null)
            ||  (currentValue != null && currentValue.Equals( newValue) != false))
            {
                return false;
            }
            currentValue = newValue;
            return true;
        }
    }
}
