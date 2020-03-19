﻿using MvvmGo.Commands;
using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Publisher.Engines.Commands;
using SignalGo.Publisher.Engines.Interfaces;
using SignalGo.Shared.Log;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Models
{
    public class ProjectInfo : BaseViewModel
    {
        public ProjectInfo()
        {
            RunCommmands = new Command(async () =>
            {
                try
                {
                    await RunCommands();
                    
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ProjectInfo Constructor, Commands Initialize");
                }
            });
        }

        /// <summary>
        /// Run Command Prop
        /// </summary>
        public Command<ICommand> RunCommmand { get; set; }
        public Command RunCommmands { get; set; }

        [JsonIgnore]
        public ObservableCollection<TextLogInfo> Logs { get; set; } = new ObservableCollection<TextLogInfo>();

        //[JsonIgnore]
        //public ProjectProcessInfoBase CurrentServerBase { get; set; }

        //[JsonIgnore]
        //public Action ProcessStarted { get; set; }
        /// <summary>
        /// List of Commands
        /// </summary>
        public ObservableCollection<ICommand> Commands { get; set; } = new ObservableCollection<ICommand>();


        public ObservableCollection<SignalGo.ServerManager.ViewModels.ServerInfoViewModel> ServersInfo { get; set; } = new ObservableCollection<ServerManager.ViewModels.ServerInfoViewModel>();
        
        private ObservableCollection<string> _servers;
        public ObservableCollection<string> Servers
        {
            get { return _servers; ; }
            set
            {
                _servers = value;
                OnPropertyChanged(nameof(Servers));
            }
        }


        private string _Name;
        private Guid _ProjectKey;
        private string _AssemblyPath;
        private ServerInfoStatus _status = ServerInfoStatus.Stable;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// unique key of project
        /// </summary>
        public Guid ProjectKey
        {
            get
            {
                if (_ProjectKey != Guid.Empty)
                {
                    return _ProjectKey;
                }
                else
                {
                    _ProjectKey = Guid.NewGuid();
                    return _ProjectKey;
                }
            }
            set
            {
                _ProjectKey = value;
                OnPropertyChanged(nameof(ProjectKey));
            }
        }

        /// <summary>
        /// project files path
        /// </summary>
        public string AssemblyPath
        {
            get
            {
                return _AssemblyPath;
            }
            set
            {
                _AssemblyPath = value;
                OnPropertyChanged(nameof(AssemblyPath));
            }
        }

        /// <summary>
        /// status of server
        /// </summary>
        [JsonIgnore]
        public ServerInfoStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// dotnet build
        /// </summary>
        public async void Build()
        {
            try
            {
                var cmd = new BuildCommandInfo()
                {
                    Path = AssemblyPath
                };
                await cmd.Run();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Build Command");
            }

        }

        /// <summary>
        /// run each commands in queue async
        /// </summary>
        /// <returns></returns>
        public async Task RunCommands()
        {
            try
            {
                QueueCommandInfo queueCommandInfo = new QueueCommandInfo(Commands.ToList());
                await queueCommandInfo.Run();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Run Command Task");
            }
        }

        /// <summary>
        /// add a command to commands list
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(ICommand command)
        {
            command.Path = AssemblyPath;
            Commands.Add(command);
            // extra command item for test ui
            Commands.Add(command);
        }

        //public void UpdateDatabase()
        //{

        //}

        //public void ApplyMigrations()
        //{

        //}

        //public void Publish()
        //{
        //    List<ICommand> multipleCommands = new List<ICommand>();
        //    // call compiler
        //    multipleCommands.Add(new BuildCommandInfo
        //    {
        //        Path = AssemblyPath
        //    });
        //    //buildCommands.Add(new TestsCommand());
        //    // call publish tool
        //    multipleCommands.Add(new PublishCommandInfo
        //    {
        //        Path = AssemblyPath
        //    });

        //    foreach (var item in multipleCommands)
        //    {
        //        item.Run();
        //    }

        //}

        //public async Task RunTestsAsync()
        //{
        //    var cmd = new TestsCommandInfo()
        //    {
        //        Path = AssemblyPath
        //    };
        //    await cmd.Run();
        //}

        /// <summary>
        /// dotnet restore
        /// </summary>
        //public async Task RestorePackagesAsync()
        //{
        //    var cmd = new RestoreCommandInfo()
        //    {
        //        Path = AssemblyPath
        //    };
        //    await cmd.Run();
        //}

        ///Console Writer
        public class ConsoleWriter : TextWriter
        {
            public string ProjectName { get; set; }
            public Action<string, string> TextAddedAction { get; set; }

            public ConsoleWriter()
            {
            }

            public override void Write(char value)
            {
                try
                {
                    TextAddedAction?.Invoke(ProjectName, value.ToString());
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "Write char");
                }
            }

            /// <summary>
            /// write action
            /// </summary>
            /// <param name="value"></param>
            public override void Write(string value)
            {
                try
                {
                    TextAddedAction?.Invoke(ProjectName, value);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "Write string");
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
        /// <summary>
        /// status if server
        /// </summary>
        public enum ServerInfoStatus : byte
        {
            Stable = 1,
            NotStable = 2,
            Updating = 3,
            Restarting = 4,
            Disabled = 5
        }
        /// <summary>
        /// TextLog
        /// </summary>
        public class TextLogInfo : BaseViewModel
        {
            private string _Text;
            public string Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    _Text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
            public bool IsDone { get; set; }
        }

    }
}
