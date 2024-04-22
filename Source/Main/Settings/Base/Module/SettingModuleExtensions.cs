using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace DFerrisIgnorance;

public static class SettingModuleExtensions
{

    public static V FindOrDefault<T, V>(this Dictionary<T, V> dict, T toFind, V defaultValue)
    {
        return (dict.TryGetValue(toFind, out var result)) ? result : defaultValue;
    } 
    
    public static bool betterIntegerTextEntry(this Listing_Standard listingStandard, string text, ref string _bufferText, ref int value)
    {
        var textInput = new string(listingStandard.TextEntryLabeled(text, _bufferText).Where(char.IsDigit).ToArray());
        if (!textInput.NullOrEmpty() && int.TryParse(textInput, out var parsed))
        {
            value = parsed;
            _bufferText = value.ToString();
            return true;
        }
        _bufferText = textInput;
        return false;
    }

    public static bool betterLongTextEntry(this Listing_Standard listingStandard, string text, ref string _bufferText, ref long value)
    {
        var textInput = new string(listingStandard.TextEntryLabeled(text, _bufferText).Where(char.IsDigit).ToArray());
        if (!textInput.NullOrEmpty() && long.TryParse(textInput, out var parsed))
        {
            value = parsed;
            _bufferText = value.ToString();
            return true;
        }
        _bufferText = textInput;
        return false;
    }

    
    private static string convertToDecimalString(string input)
    {
        StringBuilder resultText = new StringBuilder();
        bool foundDecimal = false;
        foreach (var v in input.ToCharArray())
        {
            if (v == '.')
            {
                if(foundDecimal)
                    break;
                foundDecimal = true;
                resultText.Append(v);
                continue;
            }

            if (char.IsDigit(v)) resultText.Append(v);
        }

        var result = resultText.ToString();
        if (result.EndsWith("."))return result.Substring(0, result.Length - 1);
        return result;
    }
    
    public static bool betterFloatTextEntry(this Listing_Standard listingStandard, string text, ref string _bufferText, ref float value)
    {
        var textInput = convertToDecimalString(listingStandard.TextEntryLabeled(text, _bufferText));
        if (!textInput.NullOrEmpty() && float.TryParse(textInput, out var parsed))
        {
            value = parsed;
            _bufferText = value.ToString();
            return true;
        }
        _bufferText = textInput;
        return false;
    }
    
    public static bool betterDoubleTextEntry(this Listing_Standard listingStandard, string text, ref string _bufferText, ref double value)
    {
        var textInput = convertToDecimalString(listingStandard.TextEntryLabeled(text, _bufferText));
        if (!textInput.NullOrEmpty() && double.TryParse(textInput, out var parsed))
        {
            value = parsed;
            _bufferText = value.ToString();
            return true;
        }
        _bufferText = textInput;
        return false;
    }
    
    
    
}
