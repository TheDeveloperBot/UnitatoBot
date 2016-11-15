﻿using System;
using BotCore.Bridge;
using BotCore.Bridge.Services;
using BotCore.Execution;
using BotCore.Util;
using Unitato.Execution;
using UnitatoBot.Configuration;
using UnitatoBot.Execution;

namespace UnitatoBot {

    public class Unitato {

        static void Main(string[] args) {
            Config.Load();

            Logger.Log("Initializing services");
            Logger.SectionStart();
            IService discordService = new DiscordService(Config.Settings.Token);
            Logger.SectionEnd();
            Logger.Log("Services initialized");

            ExecutionManager cmdManager = new ExecutionManager(discordService)
                .RegisterCommand("unitato", new InfoExecutor())
                .RegisterCommand("help", new HelpExecutor())
                .RegisterCommand("invite", new InviteExecutor())
                .RegisterCommand("uptime", new UptimeExecutor())
                .RegisterCommand("roll", new DiceExecutor())
                    .WithAlias("dice")
                .RegisterCommand("praise", new PraiseExecutor())
                    .WithAlias("dan")
                .RegisterCommand("faggot", new FaggotPointsExecutor())
                .RegisterCommand("lexicon", new LexiconExecutor())
                .RegisterCommand("emoticon", new EmoticonExecutor())
                    .WithAlias("e")
                .RegisterCommand("timer", new TimerExecutor())
                .RegisterCommand("checklist", new ChecklistExecutor())
                    .WithAlias("list")
                .RegisterCommand("sound", new SoundExecutor())
                    .WithAlias("s")
                    .WithAlias("play")
                .RegisterCommand("coin", new CoinFlipExecutor())
                    .WithAlias("flip")
                .RegisterCommand("roulette", new RussianRouletteExecutor())
                    .WithAlias("rr")
                .RegisterCommand("calc", new CalcExecutor())
                .RegisterCommand("reboot", new RebootExecutor())
                .RegisterCommand("permission", new PermissionExecutor())
                    .WithAlias("perm");

            cmdManager.Begin();
            Logger.Log("Ready to go.");

            Console.ReadKey();
        }

    }

}
