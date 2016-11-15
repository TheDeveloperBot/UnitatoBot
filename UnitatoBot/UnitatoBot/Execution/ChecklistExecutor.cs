﻿using BotCore.Bridge;
using BotCore.Command;
using BotCore.Execution;
using BotCore.Util;
using BotCore.Util.Symbol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unitato.Component.Checklist;

namespace Unitato.Execution {

    internal class ChecklistExecutor : IInitializable, IExecutionHandler {

        private List<Checklist> Checklists;

        // IInitializable

        public void Initialize(CommandManager manager) {
            if(!Directory.Exists("checklist"))
                Directory.CreateDirectory("checklist");

            Checklists = Load(manager);
        }

        // IExecutionHandler

        public string GetDescription() {
            return "Create checklist with items that you can check out. To create a checklist use with argument 'create' [checklist-id] [title]. To add item to checklist use with argument 'add' [checklist-id](optional) [text]. To check or uncheck the item use with argument 'check' or 'uncheck' [checklist-id](optional) [item-index](from 0), you can use multiple indexes (ex: 'check 1 2 3'). To destroy checklist use with argument 'destroy' [checklist-id]. To remove item from checklist use use with argument 'remove' [checklist-id](optional) [item-index](from 0). To add multiple items to checklist use with argument 'import' [checklist-id](optional) [separator](string that splits entries) [text] (example: '/checklist import - -item1 -item2 -item3'). When checklist is no longer needed to be editable use with argument 'finish [checklist-id](optional). In order to rerender chaklist as new message use argument 'rerender'.";
        }

        public bool CanExecute(CommandContext context) {
            return context.HasArguments;
        }

        public bool Execute(CommandContext context) {
            switch(context.Args[0]) {
                case "create":
                    if(context.Args.Length > 2 && !Checklists.Exists(c => c.Id.Equals(context.Args[1]))) {
                        Checklist checklist = new Checklist(context.Args[1], context.Message.Sender, context.RawArguments.Substring(context.RawArguments.IndexOf(context.Args[1]) + context.Args[1].Length + 1));

                        ServiceMessage msg = context.ResponseBuilder
                            .Text(Emoji.Checklist)
                            .Text("{0} (Checklist", checklist.Title)
                            .Block(checklist.Id)
                            .Text("by")
                            .Block(checklist.Owner)
                            .Text(")")
                            .NewLine()
                                .Space(8).Text("Use").Block("!checklist add [text]").Text("to add item to last checklist (example: '!checklist add Hello world')")
                            .NewLine()
                                .Space(8).Text("Use").Block("!checklist import [separator] [text]").Text("to add multiple items to last checklist (example: '!checklist import - item1-item2-item3')")
                            .Send();

                        checklist.Message = msg;
                        Checklists.Add(checklist);

                        checklist.Save();

                        return true;
                    }
                    break;

                case "destroy":
                    if(Checklists.Count > 0 && context.Args.Length == 2) {
                        Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));

                        if(checklist != null) {
                            context.Message.Delete();
                            Checklists.Remove(checklist);
                            checklist.Message.Delete();
                            checklist.Delete();

                            Logger.Info("Checklist {0} was deleted", checklist.Id);
                            return true;
                        }
                    }
                    break;

                case "finish":
                    if(Checklists.Count > 0) {

                        Checklist checklist = null;
                        if(context.Args.Length == 1) {
                            checklist = Checklists.Last();
                        } else if(context.Args.Length > 1) {
                            checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));   
                        }

                        if(checklist != null) {
                            context.Message.Delete();
                            checklist.Status = "Checklist was marked as finished, no further edits can be made.";
                            Checklists.Remove(checklist);
                            checklist.Delete();

                            Logger.Info("Checklist {0} was marked as finished and deleted", checklist.Id);
                            return true;
                        }

                    }
                    break;

                case "add":
                    if(Checklists.Count > 0 && context.Args.Length > 1) {
                        Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));

                        if(checklist == null) {
                            Checklists.Last().Add(context.RawArguments.Substring(context.RawArguments.IndexOf(context.Args[0]) + context.Args[0].Length + 1));
                            context.Message.Delete();
                            return true;

                        } else if(context.Args.Length > 2) {
                            checklist.Add(context.RawArguments.Substring(context.RawArguments.IndexOf(context.Args[1]) + context.Args[1].Length + 1));
                            context.Message.Delete();
                            return true;

                        }

                    }
                    break;

                case "remove":
                    if(Checklists.Count > 0 && context.Args.Length > 1) {
                        Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));

                        bool hasId = true;
                        if(checklist == null) {
                            checklist = Checklists.Last();
                            hasId = false;
                        }

                        if(!hasId || (hasId && context.Args.Length > 2)) {

                            byte index;
                            if(byte.TryParse(context.Args[hasId ? 2 : 1], out index)) {
                                context.Message.Delete();
                                checklist.Remove(index);
                                return true;
                            }

                        }
                    }
                    break;

                case "import":
                    if(Checklists.Count > 0 && context.Args.Length > 2) {
                        Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));

                        bool hasId = true;
                        if(checklist == null) {
                            checklist = Checklists.Last();
                            hasId = false;
                        }

                        if(!hasId || (hasId && context.Args.Length > 3)) {

                            string[] separator = new string[] { context.Args[hasId ? 2 : 1] };
                            string import = context.RawArguments.Substring(context.RawArguments.IndexOf(separator[0]) + separator[0].Length + 1).Trim();

                            foreach(string entry in import.Split(separator, StringSplitOptions.RemoveEmptyEntries)) {
                                checklist.Add(entry, false);
                            }

                            context.Message.Delete();
                            checklist.UpdateMessage();
                            return true;

                        }
                    }
                    break;

                case "check":
                case "uncheck":
                    if(Checklists.Count > 0 && context.Args.Length > 1) {
                        Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));
                        bool checkState = context.Args[0] == "check";

                        if(checklist == null)
                            return SetEntryState(context.Message, Checklists.Last(), checkState, context.Args.Skip(1).ToArray());

                        return context.Args.Length > 2 && SetEntryState(context.Message, checklist, checkState, context.Args.Skip(2).ToArray());
                    }
                    break;

                case "rerender":
                    if(Checklists.Count > 0) {

                        if(context.Args.Length > 1) {
                            Checklist checklist = Checklists.Find(c => c.Id.Equals(context.Args[1]));

                            if(checklist != null)
                                checklist.RerenderMessage();
                            else
                                return false;

                        } else
                            Checklists.Last().RerenderMessage();

                        context.Message.Delete();
                        return true;
                    }
                    break;
            }

            return false;;
        }

        // Utilities

        private bool SetEntryState(ServiceMessage commandMsg, Checklist checklist, bool state, params string[] indexes) {
            bool anySucess = false; byte i;

            foreach(string index in indexes) {
                if(byte.TryParse(index, out i)) {
                    if(checklist.SetEntryState(i, state, commandMsg.Sender) && anySucess == false) {
                        anySucess = true;
                    }
                }
            }

            if(checklist.IsCompleted) {
                Checklists.Remove(checklist);
                checklist.Delete();
                checklist.Status = "All entries on checklist was checked, no further edits can be made.";
                Logger.Info("Checklist {0} was deleted", checklist.Id);
            }

            if(anySucess) {
                checklist.UpdateMessage();
                commandMsg.Delete();
            }

            return anySucess;
        }

        private List<Checklist> Load(CommandManager manager) {
            Logger.SectionStart();

            List<Checklist> list = new List<Checklist>();
            foreach(FileInfo file in new DirectoryInfo("checklist").GetFiles("*.json", SearchOption.TopDirectoryOnly)) {
                Logger.Info("Loading file {0} ...", file);

                Checklist checklist = Checklist.Load(file, manager);

                if(checklist != null)
                    list.Add(checklist);
            }

            Logger.SectionEnd();
            Logger.Info("Loaded {0} entr{1}", list.Count, list.Count == 1 ? "y" : "ies");

            return list;
        }
 
    }
}
