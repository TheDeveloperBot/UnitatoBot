﻿using BotCore.Component;
using Newtonsoft.Json;

namespace Unitato.Component.Checklist {

    internal partial class Checklist : SavableMessageContainer {

        private class Entry {

            public string Text { private set; get; }

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public bool IsChecked { private set; get; } = false;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public string CheckedBy { private set; get; }

            public Entry(string text) {
                Text = text;
            }

            public void SetState(bool state, string owner) {
                IsChecked = state;
                CheckedBy = owner;
            }

        }

    }
}