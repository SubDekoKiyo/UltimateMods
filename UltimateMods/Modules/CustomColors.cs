using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateMods.Modules
{
    public class CustomColors
    {
        public static Dictionary<int, string> ColorStrings = new();
        public static uint pickableColors = (uint)Palette.ColorNames.Length;
        public static readonly List<int> ORDER = new List<int>() {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 
            12, 13, 14, 15, 16, 17, 18
        };

        protected internal struct CustomColor
        {
            public string longname;
            public Color32 color;
            public Color32 shadow;
        }

        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);
            List<CustomColor> colors = new();

            /* Custom Colors */
            /*colors.Add(new CustomColor
            {
                longname = "ColorTEST",
                color = new Color(0f, 0f, 0f, byte.MaxValue),
                shadow = new Color(0f, 0f, 0f, byte.MaxValue)
            });*/

            pickableColors += (uint)colors.Count;

            int id = 50000;
            foreach (CustomColor cc in colors)
            {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }
    }
}