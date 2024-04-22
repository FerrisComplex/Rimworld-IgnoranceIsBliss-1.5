using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DFerrisIgnorance;

public static class EnhancedListingStandard
{
    public static float Gap { get; set; } = 12f;
    public static float LineGap { get; set; } = 3f;


    public class LabeledRadioValue<T>
    {
        public T Value { get; set; }
        public string Label { get; set; }

        public string Tooltip { get; set; }

        public Action<Listing_Standard> ExtraDrawActionWhenSelected { get; set; }
        public LabeledRadioValue(string label, T val, string tooltip = null, Action<Listing_Standard> extraDrawActionWhenSelected = null)
        {
            this.Label = label;
            this.Value = val;
            this.Tooltip = tooltip;
            this.ExtraDrawActionWhenSelected = extraDrawActionWhenSelected;
        }
    }


    public static Listing_Standard BeginListingStandard(this Rect rect, int columns = 1)
    {
        Listing_Standard listing_Standard = new Listing_Standard();
        listing_Standard.ColumnWidth = rect.width / (float)columns - (float)columns * 5f;
        listing_Standard.Begin(rect);
        return listing_Standard;
    }


    public static void AddHorizontalLine(this Listing_Standard listing_Standard, float? gap = null)
    {
        listing_Standard.Gap(gap ?? LineGap);
        listing_Standard.GapLine(gap ?? LineGap);
    }


    public static void AddLabelLine(this Listing_Standard listing_Standard, string label, float? height = null)
    {
        listing_Standard.Gap(Gap);
        Widgets.Label(listing_Standard.GetRect(height), label);
    }


    public static Rect GetRect(this Listing_Standard listing_Standard, float? height = null)
    {
        return listing_Standard.GetRect(height ?? Text.LineHeight, 1f);
    }


    public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, float leftPartPct = 0.5f, float? height = null)
    {
        Rect rect = listing_Standard.GetRect(height);
        leftHalf = rect.LeftPart(leftPartPct).Rounded();
        return rect;
    }


    public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, out Rect rightHalf, float leftPartPct = 0.5f, float? height = null)
    {
        Rect rect = listing_Standard.LineRectSpilter(out leftHalf, leftPartPct, height);
        rightHalf = rect.RightPart(1f - leftPartPct).Rounded();
        return rect;
    }


    public class RadioButtonLabel<T>
    {
        public readonly string Label;
        public readonly T Value;
        public readonly string Tooltip;
        public readonly Action<Listing_Standard> ExtraDrawActionWhenSelected;

        public RadioButtonLabel(string label, T value, string tooltip = null, Action<Listing_Standard> extraDrawActionWhenSelected = null)
        {
            Label = label;
            Value = value;
            Tooltip = tooltip;
            ExtraDrawActionWhenSelected = extraDrawActionWhenSelected;
        }
    }
    
    public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, List<RadioButtonLabel<int>> labels, ref int val, float? headerHeight = null)
    {
        listing_Standard.Gap(Gap);
        listing_Standard.AddLabelLine(header, headerHeight);
        listing_Standard.AddRadioList(GenerateLabeledRadioValues(labels), ref val, null);
    }

    public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, RadioButtonLabel<int>[] labels, ref int val, float? headerHeight = null)
    {
        listing_Standard.Gap(Gap);
        listing_Standard.AddLabelLine(header, headerHeight);
        listing_Standard.AddRadioList(GenerateLabeledRadioValues(labels), ref val, null);
    }

    public static void AddLabeledRadioList<T>(this Listing_Standard listing_Standard, string header, RadioButtonLabel<T>[] labels, ref T val, float? headerHeight = null)
    {
        listing_Standard.Gap(Gap);
        listing_Standard.AddLabelLine(header, headerHeight);
        listing_Standard.AddRadioList(GenerateLabeledRadioValues(labels), ref val, null);
    }


    public static bool RadioButtonLabeledTooltip(Rect rect, string labelText, bool chosen, bool disabled = false, string tooltip = null)
    {
        if(!tooltip.NullOrEmpty()) TooltipHandler.TipRegion(rect, tooltip);
        return Widgets.RadioButtonLabeled(rect, labelText, chosen, disabled);
    }


    public static void AddRadioList<T>(this Listing_Standard listing_Standard, List<LabeledRadioValue<T>> items, ref T val, float? height = null)
    {
        foreach (LabeledRadioValue<T> labeledRadioValue in items)
        {
            listing_Standard.Gap(Gap);
            if (RadioButtonLabeledTooltip(listing_Standard.GetRect(height), labeledRadioValue.Label, EqualityComparer<T>.Default.Equals(labeledRadioValue.Value, val), false, labeledRadioValue.Tooltip))
            {
                val = labeledRadioValue.Value;
            }
            if (EqualityComparer<T>.Default.Equals(labeledRadioValue.Value, val) && labeledRadioValue.ExtraDrawActionWhenSelected != null) labeledRadioValue.ExtraDrawActionWhenSelected.Invoke(listing_Standard);
        }
    }


    private static List<LabeledRadioValue<T>> GenerateLabeledRadioValues<T>(RadioButtonLabel<T>[] labels)
    {
        List<LabeledRadioValue<T>> list = new List<LabeledRadioValue<T>>();
        foreach (var label in labels)
            list.Add(new LabeledRadioValue<T>(label.Label, label.Value, label.Tooltip, label.ExtraDrawActionWhenSelected));

        return list;
    }
    
    private static List<LabeledRadioValue<T>> GenerateLabeledRadioValues<T>(List<RadioButtonLabel<T>> labels)
    {
        List<LabeledRadioValue<T>> list = new List<LabeledRadioValue<T>>();
        foreach (var label in labels)
            list.Add(new LabeledRadioValue<T>(label.Label, label.Value, label.Tooltip, label.ExtraDrawActionWhenSelected));

        return list;
    }
    

    public static void AddLabeledTextField(this Listing_Standard listing_Standard, string label, ref string settingsValue, float leftPartPct = 0.5f)
    {
        listing_Standard.Gap(Gap);
        Rect rect;
        Rect rect2;
        listing_Standard.LineRectSpilter(out rect, out rect2, leftPartPct, null);
        Widgets.Label(rect, label);
        string text = settingsValue.ToString();
        settingsValue = Widgets.TextField(rect2, text);
    }


    public static void AddLabeledNumericalTextField<T>(this Listing_Standard listing_Standard, string label, ref T settingsValue, float leftPartPct = 0.5f, float minValue = 1f, float maxValue = 100000f) where T : struct
    {
        listing_Standard.Gap(Gap);
        listing_Standard.LineRectSpilter(out var rect, out var rect2, leftPartPct, null);
        Widgets.Label(rect, label);
        string text = settingsValue.ToString();
        Widgets.TextFieldNumeric<T>(rect2, ref settingsValue, ref text, minValue, maxValue);
    }


    public static void AddLabeledCheckbox(this Listing_Standard listing_Standard, string label, ref bool settingsValue)
    {
        listing_Standard.Gap(Gap);
        listing_Standard.CheckboxLabeled(label, ref settingsValue, null, 0f, 1f);
    }


    public static void AddLabeledSlider(this Listing_Standard listing_Standard, string label, ref float value, float leftValue, float rightValue, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f, bool middleAlignment = false)
    {
        listing_Standard.Gap(Gap);
        listing_Standard.LineRectSpilter(out var rect, out var rect2, 0.5f, null);
        Widgets.Label(rect, label);
        value = HorizontalSlider(rect2.BottomPart(0.7f), value, leftValue, rightValue, middleAlignment, null, leftAlignedLabel, rightAlignedLabel, roundTo);
    }

    private static FieldInfo rangeControlTextColorField = AccessTools.Field(typeof(Widgets), "RangeControlTextColor");
    private static FieldInfo sliderRailAtlasField = AccessTools.Field(typeof(Widgets), "SliderRailAtlas");

    private static Color _RangeControlTextColor()
    {
        if (rangeControlTextColorField == null)
            return Color.white;
        return (Color)rangeControlTextColorField.GetValue(null);
    }

    private static void _RangeControlTextColor(Color color)
    {
        if (rangeControlTextColorField == null) return;
        rangeControlTextColorField.SetValue(null, color);
    }


    private static readonly Texture2D SliderRailAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/SliderRail", true);
    private static readonly Texture2D SliderHandle = ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle", true);
    private static readonly FieldInfo sliderDraggingIDField = AccessTools.Field(typeof(Widgets), "sliderDraggingID");
    private static readonly MethodInfo CheckPlayDragSliderSoundMethod = AccessTools.Method(typeof(Widgets), "CheckPlayDragSliderSound");

    private static int sliderDraggingID
    {
        get => (int)sliderDraggingIDField.GetValue(null);
        set => sliderDraggingIDField.SetValue(null, value);
    }


    private static void playSliderSound()
    {
        try
        {
            CheckPlayDragSliderSoundMethod.Invoke(null, null);
        }
        catch (Exception)
        {
        }
    }

    public static float HorizontalSlider(Rect rect, float value, float min, float max, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
    {
        float num = value;
        if (middleAlignment || !label.NullOrEmpty())
        {
            rect.y += Mathf.Round((rect.height - 10f) / 2f);
        }

        if (!label.NullOrEmpty())
        {
            rect.y += 5f;
        }

        int num2 = UI.GUIToScreenPoint(new Vector2(rect.x, rect.y)).GetHashCode();
        num2 = Gen.HashCombine<float>(num2, rect.width);
        num2 = Gen.HashCombine<float>(num2, rect.height);
        num2 = Gen.HashCombine<float>(num2, min);
        num2 = Gen.HashCombine<float>(num2, max);
        Rect rect2 = rect;
        rect2.xMin += 6f;
        rect2.xMax -= 6f;
        GUI.color = _RangeControlTextColor();
        Rect rect3 = new Rect(rect2.x, rect2.y + 2f, rect2.width, 8f);
        Widgets.DrawAtlas(rect3, SliderRailAtlas);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(Mathf.Clamp(rect2.x - 6f + rect2.width * Mathf.InverseLerp(min, max, num), rect2.xMin - 6f, rect2.xMax - 6f), rect3.center.y - 6f, 12f, 12f), SliderHandle);
        if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect) && sliderDraggingID != num2)
        {
            sliderDraggingID = num2;
            SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            Event.current.Use();
        }

        if (sliderDraggingID == num2 && UnityGUIBugsFixer.MouseDrag(0))
        {
            num = Mathf.Clamp((Event.current.mousePosition.x - rect2.x) / rect2.width * (max - min) + min, min, max);
            if (Event.current.type == EventType.MouseDrag)
            {
                Event.current.Use();
            }
        }

        if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
        {
            TextAnchor anchor = Text.Anchor;
            GameFont font = Text.Font;
            Text.Font = GameFont.Tiny;
            float num3 = label.NullOrEmpty() ? 18f : Text.CalcSize(label).y;
            rect.y = rect.y - num3 + 3f;
            if (!leftAlignedLabel.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(rect, leftAlignedLabel);
            }

            if (!rightAlignedLabel.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(rect, rightAlignedLabel);
            }

            if (!label.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect, label);
            }

            Text.Anchor = anchor;
            Text.Font = font;
        }

        if (roundTo > 0f)
        {
            num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
        }

        if (value != num)
            playSliderSound();


        return num;
    }


    public static void AddColorPickerButton(this Listing_Standard listing_Standard, string label, Color color, Action<Color> callback, string buttonText = "Change")
    {
        listing_Standard.Gap(Gap);
        Rect rect = listing_Standard.GetRect(null);
        float num = Text.CalcSize(buttonText).x + 10f;
        float num2 = num + 5f + rect.height;
        Rect rect2 = rect.RightPartPixels(num + 5f + rect.height);
        if (Widgets.ButtonText(rect2.LeftPartPixels(num), buttonText, true, false, true, null))
        {
            Find.WindowStack.Add(new Dialog_ColourPicker(color, callback, null));
        }

        GUI.color = color;
        GUI.DrawTexture(rect2.RightPartPixels(rect2.height), BaseContent.WhiteTex);
        GUI.color = Color.white;
        Widgets.Label(rect.LeftPartPixels(rect.width - num2), label);
    }


    public static void AddColorPickerButton(this Listing_Standard listing_Standard, string label, Color color, string fieldName, object colorContainer, string buttonText = "Change")
    {
        listing_Standard.AddColorPickerButton(label, color, delegate(Color c) { colorContainer.GetType().GetField(fieldName).SetValue(colorContainer, color); }, buttonText);
    }


    public static float Slider(this Listing_Standard listing_Standard, float val, float min, float max, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f, bool middleAlignment = false)
    {
        float result = HorizontalSlider(listing_Standard.GetRect(22f, 1f), val, min, max, middleAlignment, label, leftAlignedLabel, rightAlignedLabel, roundTo);
        listing_Standard.Gap(listing_Standard.verticalSpacing);
        return result;
    }


    public static void AddLabeledSlider<T>(this Listing_Standard listing_Standard, string label, ref T value) where T : Enum
    {
        object value2 = value;
        listing_Standard.Gap(10f);
        Rect rect;
        Rect rect2;
        listing_Standard.LineRectSpilter(out rect, out rect2, 0.5f, null);
        Widgets.Label(rect, label);
        float value3 = (float)Convert.ToInt32(value2);
        float num = HorizontalSlider(rect2.BottomPart(0.7f), value3, 0f, (float)(Enum.GetValues(typeof(T)).Length - 1), true, Enum.GetName(typeof(T), value), null, null, 1f);
        value = (T)((object)Enum.ToObject(typeof(T), (int)num));
    }
}
