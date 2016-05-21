﻿using Discord;
using Ini.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitatoBot.Connector.Connectors;
using UnitatoBot.Configuration;
using UnitatoBot.Command;
using UnitatoBot.Command.Execution.Executors;

namespace UnitatoBot {

    public class Unitato {

        static void Main(string[] args) {

            DiscordConnector discordConnection = new DiscordConnector(
                Config.GetEntry("Email"),
                Config.GetEntry("Password"),
                ulong.Parse(Config.GetEntry("ChannelUUID"))
            );

            discordConnection.CommandManager
                .RegisterCommand("unitato", new InfoExecutor())
                .RegisterCommand("help", new HelpExecutor())
                .RegisterCommand("uptime", new UptimeExecutor())
                .RegisterCommand("roll", new DiceExecutor())
                    .WithAlias("dice")
                .RegisterCommand("praise", new PraiseExecutor())
                    .WithAlias("dan")
                .RegisterCommand("faggot", new FaggotStatsExecutor())
                .RegisterCommand("lexicon", new LexiconExecutor());
            discordConnection.Begin();

            Console.ReadKey();
        }

    }

}
