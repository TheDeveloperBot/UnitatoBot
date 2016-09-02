﻿using System;

namespace UnitatoBot {

    public static class SymbolFactory {

        public enum Emoji {
            Checklist,
            BoxUnchecked,
            BoxChecked,
            Stopwatch,
            Radio,
            Note,
            Die,
            Gun,
            Bang
        }

        public enum Emoticon {
            Magic,
            Dance,
            Praise,
            Greet,
            Pleased,
            But,
            Yeeaahh,
            Creep,
            Cry,
            Hide,
            Food,
            Why,
            Sleep
        }

        public static string AsString(Emoji emoji) {
            switch(emoji) {
                case Emoji.Checklist:
                    return ":clipboard:";
                case Emoji.BoxUnchecked:
                    return ":white_medium_small_square:";
                case Emoji.BoxChecked:
                    return ":black_medium_small_square:";
                case Emoji.Stopwatch:
                    return ":stopwatch:";
                case Emoji.Radio:
                    return ":radio:";
                case Emoji.Note:
                    return ":musical_score:";
                case Emoji.Die:
                    return ":game_die:";
                case Emoji.Gun:
                    return ":gun:";
                case Emoji.Bang:
                    return ":boom:";
            }

            return string.Empty;
        }

        public static string AsString(Emoticon emoji) {
            switch(emoji) {
                case Emoticon.Magic:
                    return "(ﾉ◕ヮ◕)ﾉ*:・ﾟ✧";
                case Emoticon.Dance:
                    return "（〜^∇^)〜";
                case Emoticon.Praise:
                    return "ヽ(´▽｀)ノ";
                case Emoticon.Greet:
                    return "(•̀ᴗ•́)و";
                case Emoticon.Pleased:
                    return "(　＾∇＾)";
                case Emoticon.But:
                    return "(☉.☉)7";
                case Emoticon.Yeeaahh:
                    return "(⌐■_■)";
                case Emoticon.Creep:
                    return "ಠᴗಠ";
                case Emoticon.Cry:
                    return "(╥_╥)";
                case Emoticon.Hide:
                    return "|_・)";
                case Emoticon.Food:
                    return "( ^-^)_旦””";
                case Emoticon.Why:
                    return "ლ(ಠ_ಠლ)";
                case Emoticon.Sleep:
                    return "(ᴗ˳ᴗ)";
            }

            return string.Empty;
        }

        public static Emoticon? FromName(string name) {
            foreach(Emoticon emoticon in Enum.GetValues(typeof(Emoticon))) {
                if(name.ToLower().Equals(emoticon.ToString().ToLower()))
                    return emoticon;
            }

            return null;
        }

    }

}
