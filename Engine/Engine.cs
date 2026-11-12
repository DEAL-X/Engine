using MiMFa.Core.Resources;
using MiMFa.Engine;
using MiMFa.Engine.Core;
using MiMFa.Engine.Put;
using MiMFa.Engine.Stage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiMFa.Engine
{
    public class Engine : IDisposable
    {
        public IList<StageBase> CompileStages { get; set; }
        public StageBase ExecuteStage { get; set; }

        public Core.Version Version { get; } = new Core.Version(1);

        public Options Options { get; }

        public List<string> Dependencies { get; protected set; } = new List<string>();

        public event LogEventHandler Logged = null;
        public event DependencyEventHandler UsedDependency = null;

        public ResourceProvider Resource { get; set; }
        public Input Input { get; set; }
        public Output Output { get; set; }

        public int LogsCapacity { get; set; } = 1000;
        public List<LogEventArgs> Logs { get; set; } = new List<LogEventArgs>();
        public IEnumerable<LogEventArgs> Subjects => Logs.Where(l => l.Status == LogStatus.Subject);
        public IEnumerable<LogEventArgs> Infos => Logs.Where(l => l.Status == LogStatus.Info);
        public IEnumerable<LogEventArgs> Messages => Logs.Where(l => l.Status == LogStatus.Message);
        public IEnumerable<LogEventArgs> Warnings => Logs.Where(l => l.Status == LogStatus.Warning);
        public IEnumerable<LogEventArgs> Errors => Logs.Where(l => l.Status == LogStatus.Error);


        public bool HasSource => !string.IsNullOrWhiteSpace(Input?.Source);


        public Engine(StageBase[] compileStages, StageBase executeStage, Options options = null, ResourceProvider resourceProvider = null)
        {
            this.CompileStages = new List<StageBase>(compileStages ?? Array.Empty<StageBase>());
            this.ExecuteStage = executeStage ?? new Stage.Stage((i, c) => i);
            this.Options = options ?? new Options();
            this.Resource = resourceProvider ?? new ResourceProvider();
        }
        public virtual void Dispose()
        {
        }

        public Input CreateInput(object value = null)
        {
            if (value == null) return new Input();
            if (value is Input)
                return (Input)value;
            else if (value is string)
                if (Resource.Exists((string)value))
                    return new Input(Resource.GetFileText((string)value), (string)value);
                else return new Input((string)value);
            return new Input((string)value);
        }

        public virtual Output Compile(object value = null) => Compile(CreateInput(value));
        public virtual Output Compile(Input input)
        {
            string root = Resource.CurrentDirectory;
            bool isfirst = Input == null;
            Input = input;
            object data = input.Content;
            Output = new Output(null, Input.Source);
            string sn = null;
            if (isfirst) OnLogged("Compiling is Started", LogStatus.Subject);
            else if(!string.IsNullOrWhiteSpace(Input.Source)) Resource.CurrentDirectory = Path.GetDirectoryName(Input.Source).TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
            try
            {
                if (!string.IsNullOrWhiteSpace(Input.Source)) OnLogged($"Compiling the {Input.Source.Replace(Resource.CurrentDirectory, "."+Path.DirectorySeparatorChar)}", LogStatus.Message);
                foreach (var stage in CompileStages)
                {
                    sn = stage.GetType().Name;
                    if (isfirst) OnLogged($"{sn} stage is checking...", LogStatus.Info);
                    data = stage.Transform(data, this);
                    if (isfirst) OnLogged($"{sn} stage tasks completed. ✔️ ", LogStatus.Info);
                    else OnLogged(" .", null);
                }
                Output.Content = (data ?? string.Empty).ToString();
                if (isfirst) OnLogged($"Script compiled successfully! ✔️ ", LogStatus.Success);
                else OnLogged(" ✔️ ", null);
                return Output;
            }
            catch (Exception e)
            {
                if (isfirst) OnLogged($"{sn} stage is not completed! ❌ ", LogStatus.Error);
                else OnLogged($" ❌ ", null);
                OnLogged(e.Message, LogStatus.Error);
                return Output.Error(e);
            }
            finally
            {
                if (isfirst) OnLogged("Compiling is Ended", LogStatus.Subject);
                else Resource.CurrentDirectory = root;
            }
        }

        public virtual object Execute(object value = null) => Execute(CreateInput(value));
        public virtual object Execute(Input input)
        {
            return Execute(Compile(input));
        }
        public virtual object Execute(Output output)
        {
            return Execute(output.Content, output.Source);
        }
        public virtual IEnumerable<object> Execute(params KeyValuePair<string, string>[] pathScripts)
        {
            foreach (var item in pathScripts)
                yield return Execute(item.Key, item.Value);
        }
        public virtual object Execute(string script, string path)
        {
            string root = Resource.CurrentDirectory;
            bool isfirst = string.IsNullOrWhiteSpace(path);
            if (isfirst) OnLogged("Executing is Started", LogStatus.Subject);
            else
            {
                Resource.CurrentDirectory = Path.GetDirectoryName(path).TrimEnd('/', '\\') + Path.DirectorySeparatorChar;
                OnLogged("Source: " + path, LogStatus.Info);
            }
            try
            {
                var res = ExecuteStage.Transform(script, this);
                if (isfirst) OnLogged($"Code executed successfully! ✔️ ", LogStatus.Success);
                else OnLogged($" ✔️ ", null);
                return res;
            }
            catch (Exception e)
            {
                OnLogged($"Could not execute the {(string.IsNullOrWhiteSpace(path) ?"scripts":"\""+ path + "\" file")}! ❌ ", LogStatus.Error);
                var p = e.GetType().GetProperty("ErrorDetails");
                if (p != null) OnLogged((string)p.GetValue(e), LogStatus.Error);
                else OnLogged(e.Message, LogStatus.Error);
                Output?.Error(e);
                return null;
            }
            finally
            {
                if (isfirst) OnLogged("Executing is Ended", LogStatus.Subject);
                else Resource.CurrentDirectory = root;
            }
        }

        public virtual bool UseDependency(Input input, bool force = false)
        {
            if (!force && HasDependency(input.Source)) return false;
            Input inp = Input;
            Output oup = Output;
            int ec = Output.Errors.Count;
            Dependencies.Add(input.Source?.ToLower());
            int ind = Dependencies.Count - 1;
            var o = Execute(input);
            Input = inp;
            Output = oup;
            if (o == null && Output.Errors.Count > ec)
            {
                Dependencies.RemoveAt(ind);
                return false;
            }
            OnUsedDependency(input.Source, o);
            return true;
        }
        public virtual bool HasDependency(string source)
        {
            return Dependencies.Contains(source?.ToLower());
        }

        public void OnUsedDependency(string source, object value = null)
        {
            if (UsedDependency != null) UsedDependency(this, new DependencyEventArgs(source, value));
        }
        public void OnLogged(string message = "", LogStatus? status = LogStatus.Info, DateTime? time = null)
        {
            var log = new LogEventArgs(message, status, time);
            if (log.Status.HasValue)
            {
                Logs.Add(log);
                if (Logs.Count > LogsCapacity) Logs.RemoveAt(0);
            }
            if (Logged != null) Logged(this, log);
        }

    }
}
