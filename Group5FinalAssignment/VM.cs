// Group 5, Final Assignment, PROG8010/F18.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Group5FinalAssignment
{
    class VM : INotifyPropertyChanged
    {
        #region Constants
        const int TARGET_INDEX = 1;
        const int COMMAND_INDEX = 0;
        const int DEPENDENCIES_INDEX = 2;
        const string END = "END";
        const string INSTALL = "INSTALL";
        const string DEPEND = "DEPEND";
        const string REMOVE = "REMOVE";
        const string LIST = "LIST";
        const string INVALID_TARGET = "Cannot use command as component name.";
        const string INVALID_END = "Invalid syntax.  No END statement found.";
        const string EMPTY = "";
        const string FILEPATH = "../../List.txt";
        const string ISINST1 = " is already installed, cannot change dependencies";
        const string ISINST2 = "    is already installed.";
        const string INSTALLING = "    Installing ";
        const string ISNOTINSTALLED = " is not installed.";
        const string TAB = "    ";
        const string ISNEEDED = " is still needed.";
        const string REMOVING = "    Removing ";
        #endregion

        #region Properties
        // Used to parse the input string into individual elements for further steps and to display to list box.
        private BindingList<Input> inputLines;
        public BindingList<Input> InputLines
        {
            get { return inputLines; }
            set { inputLines = value; NotifyChanged(); }
        }
        // Used to display the output to the console.
        private BindingList<Output> outputDisplay;
        public BindingList<Output> OutputDisplay
        {
            get { return outputDisplay; }
            set { outputDisplay = value; NotifyChanged(); }
        }
        private Dictionary<string, Component> Components = new Dictionary<string, Component>();
        #endregion

        #region Command Line Input
        // Called by the button to execute the script.
        public void Run()
        {
            OutputDisplay = new BindingList<Output>();
            ReadInputFile();
            ExecuteScript();
        }

        public void ReadInputFile()
        {
            string[] inputfile = File.ReadAllLines(FILEPATH);
            InputLines = new BindingList<Input>();
            string[] cmdElements;

            for (var index = 0; index < inputfile.Length; index++)
            {
                cmdElements = inputfile[index].Split(' ');                        
                var count = cmdElements.Count();

                InputLines.Add(new Input
                {
                    // Element at this index is command type.
                    Command = cmdElements[COMMAND_INDEX].Trim()                    
                });
                if (count > TARGET_INDEX)
                {
                    // Element at this index is target of command.
                    InputLines[index].Target = cmdElements[TARGET_INDEX].Trim();
                }
                if (count > DEPENDENCIES_INDEX)
                    // Elements at this index and higher are dependencies.
                    for (var i = DEPENDENCIES_INDEX; i < count; i++)
                    {
                        InputLines[index].Dependencies.Add(cmdElements[i].Trim());
                        InputLines[index].DisplayDepElement = $"{InputLines[index].DisplayDepElement} {InputLines[index].Dependencies[i - DEPENDENCIES_INDEX]}             ";
                    }
            }
        }

        public void ExecuteScript()
        {
            // Input must contain an END command to be valid.
            var oktoExecute = false;
            foreach (var line in InputLines)
                if (line.Command == END)
                {
                    oktoExecute = true;
                    break;
                }

            if (oktoExecute == true)
                foreach (var cmd in InputLines)
                {
                    // Command names are not valid component names.
                    if ((cmd.Target == INSTALL) || (cmd.Target == DEPEND) || (cmd.Target == REMOVE ) || (cmd.Target == LIST ) || (cmd.Target == END))
                    {
                        BuildOutput(EMPTY, EMPTY, INVALID_TARGET);
                        continue;
                    }
                    if ((cmd.Dependencies.Contains(INSTALL)) || (cmd.Dependencies.Contains(DEPEND)) || (cmd.Dependencies.Contains(REMOVE)) || (cmd.Dependencies.Contains(LIST)) || (cmd.Dependencies.Contains(END)))
                    {
                        BuildOutput(EMPTY, EMPTY, INVALID_TARGET);
                        continue;
                    }

                    switch (cmd.Command)
                    {
                        case INSTALL:
                            BuildOutput(cmd.Command, cmd.Target, EMPTY);
                            Install(cmd.Target, true);
                            break;
                        case DEPEND:
                            BuildOutput(cmd.Command, cmd.Target, cmd.DisplayDepElement);
                            Depend(cmd.Target, cmd.Dependencies);
                            break;
                        case REMOVE:
                            BuildOutput(cmd.Command, cmd.Target, EMPTY);
                            Remove(cmd.Target, true);
                            break;
                        case LIST:
                            BuildOutput(cmd.Command, EMPTY, EMPTY);
                            List();
                            break;
                        case END:
                            BuildOutput(cmd.Command, cmd.Target, EMPTY);
                            return;
                    }
                }
            else
                BuildOutput(EMPTY, INVALID_END, EMPTY);
        }

        private void BuildOutput(string command, string target, string depOutput)
        {
            OutputDisplay.Add(new Output
            {
                Command = command,
                Target = target,
                DepOutput = depOutput
            });
        }
        #endregion

        #region User Commands
        public void Depend(string inputName, List<string> inputDepends)
        {
            // If dependent component not known, add to dictionary.
            if (!Components.ContainsKey(inputName))
            {
                Components.Add(inputName, new Component(inputName));
            }
            // If dependent component is installed, return void.
            if (Components[inputName].IsInstalled)                          
            {
                string lineWrite = ISINST1;
                BuildOutput(inputName, lineWrite,"");
                return;
            }
            // Add dependencies to component.
            foreach (string depend in inputDepends)
            {
                // No duplicate or self-referential dependencies
                if ((depend != inputName) && (!Components[inputName].Dependencies.Contains(depend)))
                {
                    Components[inputName].Dependencies.Add(depend);

                    if (!Components.ContainsKey(depend))
                    {
                        Components.Add(depend, new Component(depend));
                    }

                    Components[depend].Dependents.Add(inputName);
                }
                else
                    continue;
            }
        }
        
        public void Install(string inputName, bool isExplicit)
        {
            // If component is not known, add to dictionary.
            if (!Components.ContainsKey(inputName))
            {
                Components.Add(inputName, new Component(inputName));
            }
            // If component is installed, return void.
            if (Components[inputName].IsInstalled && isExplicit)      
            {
                string lineWrite = ISINST2;
                BuildOutput(inputName, lineWrite, EMPTY);
                return;
            }

            // Implicitly install dependencies.
            if (Components[inputName].Dependencies.Count > 0)                
            {
                foreach (var dependency in Components[inputName].Dependencies)
                {
                    Install(dependency, false);
                }
            }
            // Install component either explicitly or implicitly.
            if (!Components[inputName].IsInstalled)
            {
                Components[inputName].Setup(isExplicit);
                BuildOutput(INSTALLING, inputName,EMPTY);
            }
        }

        void Remove(string name, bool ExplicitlyRemove)
        {
            // Guard conditions.
            // Cannot automatically remove components that were installed explicitly.
            if (Components[name].ExplicitInstall && !ExplicitlyRemove)
            {
                return;
            }
            // Display message if component is not installed.
            if (!Components[name].IsInstalled && ExplicitlyRemove)
            {
                BuildOutput(TAB + name, ISNOTINSTALLED, EMPTY);
                return;
            }
            // Cannot remove component that is still needed.
            if (Components[name].Dependents.Count > 0 )
            {
                foreach (var d in Components[name].Dependents)
                {
                    if (Components[d].IsInstalled && ExplicitlyRemove)
                    {
                        BuildOutput(TAB + name, ISNEEDED, EMPTY);
                        return;
                    }
                }
            }

            // Remove component.
            Components[name].IsInstalled = false;
            Components[name].ExplicitInstall = new bool();
            BuildOutput(REMOVING, name, EMPTY);
            // Implicitly installed components can be removed automatically.
            if (Components[name].Dependencies.Count > 0)
            {
                foreach (var d in Components[name].Dependencies)
                {
                    Remove(d, false);
                }
            }
        }
        public void List()
        {
            foreach (var c in Components.Keys)
                if (Components[c].IsInstalled == true)
                {
                    BuildOutput(TAB, c, EMPTY);
                }
        }
        #endregion

        #region PC Event Handler
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}