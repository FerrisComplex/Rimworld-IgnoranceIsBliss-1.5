using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DFerrisIgnorance;

public static class SettingsUtils
{
    public static bool AnyPressed(this Widgets.DraggableResult result)
    {
        return result == Widgets.DraggableResult.Pressed || result == Widgets.DraggableResult.DraggedThenPressed;
    }

    public static void CheckboxEnhanced(this Listing_Standard listing, string name, string explanation, ref bool value, List<string> incompatibles = null, string tooltip = null)
    {
        float curHeight = listing.CurHeight;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        listing.CheckboxLabeled(name, ref value, null, 0f, 1f);
        Text.Font = GameFont.Tiny;
        listing.ColumnWidth -= 34f;
        GUI.color = Color.gray;
        listing.Label(explanation, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        Rect rect = listing.GetRect(0f, 1f);
        rect.height = listing.CurHeight - curHeight;
        rect.y -= rect.height;
        if (Mouse.IsOver(rect))
        {
            string text = "";
            if (!tooltip.NullOrEmpty())
            {
                text = tooltip;
            }

            if (!incompatibles.NullOrEmpty<string>())
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n\n";
                }

                text += "TweaksGalore.TooltipIncompatibleMods".Translate();
                for (int i = 0; i < incompatibles.Count; i++)
                {
                    if (i > 0)
                    {
                        text += "\n";
                    }

                    ModMetaData activeModWithIdentifier = ModLister.GetActiveModWithIdentifier(incompatibles[i], false);
                    if (activeModWithIdentifier != null)
                    {
                        text += "TweaksGalore.IncompatibleMod".Translate(activeModWithIdentifier.Name);
                    }
                }
            }

            Widgets.DrawHighlight(rect);
            if (!text.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, text);
            }
        }

        GUI.color = Color.white;
        listing.Gap(6f);
    }

    public static void Note(this Listing_Standard listing, string name, GameFont font = GameFont.Small, Color? color = null)
    {
        Text.Font = font;
        listing.ColumnWidth -= 34f;
        GUI.color = (color ?? Color.white);
        listing.Label(name, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
    }

    public static void ValueLabeled<T>(this Listing_Standard listing, string name, string explanation, ref T value, string tooltip = null)
    {
        float curHeight = listing.CurHeight;
        Rect rect = listing.GetRect(Text.LineHeight + listing.verticalSpacing, 1f);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect, name);
        Text.Anchor = TextAnchor.MiddleRight;
        if (typeof(T).IsEnum)
        {
            Widgets.Label(rect, value.ToString().Replace("_", " "));
        }
        else
        {
            Widgets.Label(rect, value.ToString());
        }

        Text.Anchor = anchor;
        Text.Font = GameFont.Tiny;
        listing.ColumnWidth -= 34f;
        GUI.color = Color.gray;
        listing.Label(explanation, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        rect = listing.GetRect(0f, 1f);
        rect.height = listing.CurHeight - curHeight;
        rect.y -= rect.height;
        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                T[] array = Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>();
                for (int i = 0; i < array.Length; i++)
                {
                    T t = array[(i + 1) % array.Length];
                    if (array[i].ToString() == value.ToString())
                    {
                        value = t;
                        break;
                    }
                }

                Event.current.Use();
            }
        }

        GUI.color = Color.white;
        listing.Gap(6f);
    }


    public static bool ButtonWithIconAndText(Rect rect, string text, Texture2D texture, Color color, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, HorizontalJustification iconJustification = HorizontalJustification.Left, TextAnchor overrideAnchor = TextAnchor.MiddleCenter)
    {
        if (texture == null)
            return Widgets.ButtonText(rect, text, true, true, color, true, overrideAnchor);
        
        // Save our original values
        TextAnchor anchorOriginal = Text.Anchor;
        Color colorOriginal = GUI.color;

        // Im lazy, margin is not a configurable thing
        const float HorizontalMargin = 3f;

        // Calculate our text height, this will be used for the icon size so we do this first!
        var textSize = Text.CalcSize(text);
        
        // Start our logic for icon box size
        Rect iconBox = new Rect(0, 0, textSize.y*0.85f, textSize.y*0.85f);

        // Calculate text width
        float textWidth = Mathf.Min(textSize.x, rect.width - (iconBox.width + HorizontalMargin));
        

        // Start our logic for text box size
        Rect textBox = new Rect(0, 0, textWidth, textSize.y);

        if (overrideAnchor == TextAnchor.LowerCenter || overrideAnchor == TextAnchor.UpperCenter) overrideAnchor = TextAnchor.MiddleCenter;
        if (overrideAnchor == TextAnchor.LowerLeft || overrideAnchor == TextAnchor.UpperLeft) overrideAnchor = TextAnchor.MiddleLeft;
        if (overrideAnchor == TextAnchor.LowerRight || overrideAnchor == TextAnchor.UpperRight) overrideAnchor = TextAnchor.MiddleRight;



        if (overrideAnchor == TextAnchor.MiddleCenter)
        {
            if (iconJustification == HorizontalJustification.Left)
            {
                var totalWidth = iconBox.width + HorizontalMargin + textBox.width;
                var halfWidth = totalWidth / 2;
                var centerPosition = rect.x + (rect.width / 2);

                var iconPosition = centerPosition - halfWidth;
                iconBox.x = iconPosition;
                textBox.x = iconPosition + iconBox.width + HorizontalMargin;
            }
            else
            {
                var totalWidth = iconBox.width + HorizontalMargin + textBox.width;
                var halfWidth = totalWidth / 2;
                var centerPosition = rect.x + (rect.width / 2);

                var iconPosition = (centerPosition + halfWidth) - iconBox.width;
                iconBox.x = iconPosition;
                textBox.x = iconPosition - (HorizontalMargin - textBox.width);
            }
        } else if (overrideAnchor == TextAnchor.MiddleRight)
        {
            if (iconJustification == HorizontalJustification.Left)
            {
                var textPosition = (rect.x + rect.width) - (HorizontalMargin + textBox.width);
                textBox.x = textPosition;
                iconBox.x = textPosition - (HorizontalMargin + iconBox.width);
            }
            else
            {
                var iconPosition = (rect.x + rect.width) - (HorizontalMargin + iconBox.width);
                iconBox.x = iconPosition;
                textBox.x = iconPosition - (HorizontalMargin + textBox.width);
            }
        } else if (overrideAnchor == TextAnchor.MiddleLeft)
        {
            if (iconJustification == HorizontalJustification.Left)
            {
                var iconPosition = rect.x + HorizontalMargin;
                iconBox.x = iconPosition;
                textBox.x = iconPosition + HorizontalMargin;
            }
            else
            {
                var textPosition = rect.x + HorizontalMargin;
                textBox.x = textPosition;
                iconBox.x = textPosition + HorizontalMargin;
            }
        }

        // Set our Y Values
        iconBox.y = rect.y + ((rect.height - iconBox.height)/2);
        textBox.y = rect.y + ((rect.height - textSize.y)/2);

        

        if (drawBackground) Widgets.DrawButtonGraphic(rect);
        if (doMouseoverSound) MouseoverSounds.DoRegion(rect);
        if (!drawBackground)
        {
            GUI.color = color;
            if (Mouse.IsOver(rect))
                GUI.color = Widgets.MouseoverOptionColor;
        }

        if (active)
            MouseoverSounds.DoRegion(rect);

        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.Label(textBox, text);
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.DrawTextureFitted(iconBox, texture, 1f, new Vector2(1f, 1f), new Rect(0, 0, 1, 1), 0f, null);

        Text.Anchor = anchorOriginal;
        GUI.color = colorOriginal;

        if (!active) return false;
        if (!Widgets.ButtonInvisible(rect, false)) return false;
        return true;
    }
    
    
     public static bool ButtonWithIconAndText_Backup_Working(Rect rect, string text, Texture2D texture, Color color, float iconOffset = 0, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, HorizontalJustification iconJustification = HorizontalJustification.Left, TextAnchor overrideAnchor = TextAnchor.MiddleCenter)
    {
        if (texture == null)
            return Widgets.ButtonText(rect, text, true, true, color, true, overrideAnchor);
        
        // Save our original values
        TextAnchor anchorOriginal = Text.Anchor;
        Color colorOriginal = GUI.color;

        const float HorizontalMargin = 6f;

        // Start our logic for icon box location
        Rect iconBox = rect;
        if (iconJustification == HorizontalJustification.Left)
        {
            iconBox.xMin += 4f;
            iconBox.xMax = rect.x + 27f;
            iconBox.yMin += 4f;
            iconBox.yMax = rect.y + 27f;
        }

        // Start our logic for text box location
        Rect textBox = rect;
        textBox.xMin += HorizontalMargin;
        textBox.xMax -= HorizontalMargin;
        textBox.xMax -= 4f;
        textBox.xMax -= iconOffset;
        if (iconJustification == HorizontalJustification.Left)
            textBox.x += iconOffset;


        float textSize = Mathf.Min(Text.CalcSize(text).x, textBox.width - 4f);
        float num2 = textBox.xMin + textSize;
        if (iconJustification == HorizontalJustification.Right)
        {
            iconBox.x = num2 + 4f;
            iconBox.width = 27f;
            iconBox.yMin += 4f;
            iconBox.yMax = rect.y + 27f;
            num2 += 27f;
        }

        if (drawBackground) Widgets.DrawButtonGraphic(rect);
        if (doMouseoverSound) MouseoverSounds.DoRegion(rect);
        if (!drawBackground)
        {
            GUI.color = color;
            if (Mouse.IsOver(rect))
                GUI.color = Widgets.MouseoverOptionColor;
        }

        if (active)
            MouseoverSounds.DoRegion(rect);

        Text.Anchor = overrideAnchor;
        Widgets.Label(textBox, text);
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.DrawTextureFitted(iconBox, texture, 1f, new Vector2(1f, 1f), new Rect(0, 0, 1, 1), 0f, null);

        Text.Anchor = anchorOriginal;
        GUI.color = colorOriginal;

        if (!active) return false;
        if (!Widgets.ButtonInvisible(rect, false)) return false;
        return true;
    }

    
    public static void SettingsCategoryDropdown(this Listing_Standard listing, string name, string explanation, ref CategoryDef def, float width)
    {
        float curHeight = listing.CurHeight;

        Rect rect = listing.GetRect(Text.LineHeight + listing.verticalSpacing, 1f);

        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;

        Widgets.Label(rect, name);
        Text.Anchor = TextAnchor.MiddleRight;


        var buttonSize = 15 + (def.IconTexture != null && def.IconTexture.width > 0 ? 32 : 0) + Text.CalcSize(def.Name).x + 15f; // GetLargestCategoryDefWidth()
        if (ButtonWithIconAndText(new Rect(width - buttonSize, 0f, buttonSize, 29f), def.Name, def.IconTexture, Widgets.NormalOptionColor, true, true, true, HorizontalJustification.Left, TextAnchor.MiddleCenter))
            //if (ButtonTextIcon(new Rect(width - buttonSize, 0f, buttonSize, 29f), def.Name, Widgets.NormalOptionColor, 29*0.75f, def.IconTexture, true, true, false, true, null).AnyPressed())
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.AddRange(SettingsHelper.AllCategories.Where(x => x != null && x.IsUsable() && !x.HeldSections.NullOrEmpty()).OrderBy(x => x.OrderID).Select(x => x.GetMenuWithIcon(() => SettingsHelper.CurrentCategory = x)));
            if (!list.NullOrEmpty())
                Find.WindowStack.Add(new FloatMenu(list));
        }

        Text.Anchor = anchor;
        Text.Font = GameFont.Tiny;
        listing.ColumnWidth -= 34f;
        GUI.color = Color.gray;
        if (!explanation.NullOrEmpty())
            listing.Label(explanation, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        rect = listing.GetRect(0f, 1f);
        rect.height = listing.CurHeight - curHeight;
        rect.y -= rect.height;
        GUI.color = Color.white;
        listing.Gap(6f);
    }


    public static void StackableThingDropdown(this Listing_Standard listing, string name, string explanation, out ThingDef def, float width)
    {
        def = null;
        float curHeight = listing.CurHeight;
        Rect rect = listing.GetRect(Text.LineHeight + listing.verticalSpacing, 1f);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect, name);
        Text.Anchor = TextAnchor.MiddleRight;
        Rect rect2 = new Rect(width - 150f, 0f, 150f, 29f);
        ThingDef custom = null;
        if (Widgets.ButtonText(rect2, "Add Rule...", true, true, true, null))
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<ThingDef> list2 = (from t in DefDatabase<ThingDef>.AllDefs
                where t.stackLimit > 1
                select t).ToList<ThingDef>();
            list2.SortBy((ThingDef c) => c.label);
            using (List<ThingDef>.Enumerator enumerator = list2.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ThingDef thing = enumerator.Current;
                    if (thing.stackLimit > 1)
                    {
                        list.Add(new FloatMenuOption(thing.LabelCap, delegate() { custom = thing; }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                    }
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        def = custom;
        Text.Anchor = anchor;
        Text.Font = GameFont.Tiny;
        listing.ColumnWidth -= 34f;
        GUI.color = Color.gray;
        listing.Label(explanation, -1f, null);
        listing.ColumnWidth += 34f;
        Text.Font = GameFont.Small;
        rect = listing.GetRect(0f, 1f);
        rect.height = listing.CurHeight - curHeight;
        rect.y -= rect.height;
        GUI.color = Color.white;
        listing.Gap(6f);
    }


    public static void LabelBacked(this Listing_Standard list, string inputText, Color color, bool translate = false)
    {
        string text = translate ? inputText.Translate() : inputText;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        float height = Text.CalcHeight(text, list.ColumnWidth - 3f - 6f) + 6f;
        Rect rect = list.GetRect(height, 1f).Rounded();
        Color color2 = color;
        color2.r *= 0.25f;
        color2.g *= 0.25f;
        color2.b *= 0.25f;
        color2.a *= 0.2f;
        GUI.color = color2;
        Rect position = rect.ContractedBy(1f);
        position.yMax -= 2f;
        GUI.DrawTexture(position, BaseContent.WhiteTex);
        GUI.color = color;
        rect.xMin += 6f;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
        Text.Anchor = anchor;
    }


    public static void LabelBackedHeader(this Listing_Standard list, string inputText, Color color, ref bool collapsed, GameFont font = GameFont.Medium, bool translate = false, string tooltip = null)
    {
        Text.Font = font;
        string text = translate ? inputText.Translate() : inputText;
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        float height = Text.CalcHeight(text, list.ColumnWidth - 3f - 6f) + 6f;
        Rect rect = list.GetRect(height, 1f).Rounded();
        Color color2 = color;
        color2.r *= 0.25f;
        color2.g *= 0.25f;
        color2.b *= 0.25f;
        color2.a *= 0.2f;
        GUI.color = color2;
        Rect position = rect.ContractedBy(1f);
        position.yMax -= 2f;
        GUI.DrawTexture(position, BaseContent.WhiteTex);
        GUI.color = color;
        rect.xMin += 6f;
        GUI.DrawTexture(new Rect(rect.x, rect.y + (rect.height - 18f) / 2f, 18f, 18f), collapsed ? TexButton.Reveal : TexButton.Collapse);
        if (Widgets.ButtonInvisible(rect, true))
        {
            collapsed = !collapsed;
            if (collapsed)
            {
                SoundDefOf.TabClose.PlayOneShotOnCamera(null);
            }
            else
            {
                SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
            }
        }

        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, new TipSignal(tooltip));
        }

        Widgets.Label(new Rect(rect.x + 18f, rect.y, rect.width - 18f, rect.height), text);
        GUI.color = Color.white;
        Text.Anchor = anchor;
        Text.Font = GameFont.Small;
    }
}
