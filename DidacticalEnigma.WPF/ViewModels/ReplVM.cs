using DidacticalEnigma.Models;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class ReplVM : INotifyPropertyChanged
    {
        private string consoleInput = string.Empty;

        public string ConsoleInput
        {
            get => consoleInput;
            set
            {
                if (consoleInput != value)
                {
                    consoleInput = value;
                    OnPropertyChanged();
                }
            }
        }

        public void RunCommand()
        {
            var str = ConsoleInput;
            Write(">>> ");
            Write(str);
            Write("\n");
            try
            {
                dynamic result = engine.Execute(str, mainScope);
                if (result != null)
                {
                    var formattedResult = engine.Operations.Format(result);
                    Write(formattedResult);
                    Write("\n");
                }
            }
            catch (Exception ex)
            {
                Write(ex.Message);
                Write("\n");
            }
            ConsoleInput = "";
        }

        public ObservableBatchCollection<string> ConsoleBuffer { get; } = new ObservableBatchCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        private string currentOutputLine = "";

        private ReplRootModule module;

        private ScriptEngine engine;

        private ScriptScope mainScope;

        public ReplVM(Dispatcher dispatcher, ReplRootModule rootModule)
        {
            engine = global::IronPython.Hosting.Python.CreateEngine();
            this.module = rootModule;
            var builtinScope = engine.GetBuiltinModule();
            builtinScope.SetVariable("de", rootModule);
            mainScope = engine.CreateScope();

            engine.Execute(
                @"
import clr
import itertools
clr.AddReference('System.Core')
import System
clr.ImportExtensions(System.Linq)
                ", mainScope);
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Write(string str)
        {
            if (str == null)
            {
                return;
            }

            Debug.WriteLine($"WRITE: {str}");
            var lines = str.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return;
            }
            if (lines.Length >= 1)
            {
                currentOutputLine += lines[0];
            }
            if (lines.Length > 1)
            {
                var lineSlice = lines.Take(1..^1);
                Action writeAction = () =>
                {
                    ConsoleBuffer.Add(currentOutputLine);
                    currentOutputLine = "";
                    ConsoleBuffer.AddRange(lineSlice);
                };
                writeAction();
                currentOutputLine += lines[^1];
            }
        }
    }
}
