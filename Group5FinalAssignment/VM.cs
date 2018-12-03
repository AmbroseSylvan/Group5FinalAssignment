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
        #region Properties
        //Used to parse the input string into individual elements for further steps and to display to list box.
        private BindingList<Input> inputLines;
        public BindingList<Input> InputLines
        {
            get { return inputLines; }
            set { inputLines = value; NotifyChanged(); }
        }
        //Used to display the output to the console.
        private BindingList<Output> outputDisplay;
        public BindingList<Output> OutputDisplay
        {
            get { return outputDisplay; }
            set { outputDisplay = value; NotifyChanged(); }
        }
        private Dictionary<string, Component> Components = new Dictionary<string, Component>();
        #endregion

        #region Command Line Input
        //Called by the button to execute the script
        public void Run()
        {
            OutputDisplay = new BindingList<Output>();
            ReadInputFile();
            ExecuteScript();
        }

        public void ReadInputFile()
        {
            string[] inputfile = File.ReadAllLines("List.txt");
            InputLines = new BindingList<Input>();
            string[] cmdElements;

            for (int i = 0; i < inputfile.Length; i++)
            {
                //Split each command line
                cmdElements = inputfile[i].Split(' ');                     
                int c = cmdElements.Count();

                InputLines.Add(new Input
                {
                    //Element at index 0 is command type
                    Command = cmdElements[0]                               
                });

                if (c > 1)
                    //element at index 1 is target of command
                    InputLines[i].Target = cmdElements[1];
                //elements at index 2 and higher are dependencies
                if (c > 2)                                                 
                for (int j = 2; j < c; j++)
                    {
                        InputLines[i].Dependencies.Add(cmdElements[j]);
                        InputLines[i].DisplayDepElement = InputLines[i].DisplayDepElement + " " + InputLines[i].Dependencies[j - 2];
                    }
            }
        }

        public void ExecuteScript()
        {
            //Check if the input contains an END statement
            bool oktoExecute = false;
            foreach (Input line in InputLines)
                if (line.Command == "END")
                {
                    oktoExecute = true;
                    break;
                }

            if (oktoExecute == true)
                foreach (Input cmd in InputLines)
                {
                    switch (cmd.Command)
                    {
                        case "INSTALL":
                            BuildOutput(cmd.Command, cmd.Target);
                            Install(cmd.Target, true);
                            break;
                        case "DEPEND":
                            BuildOutput(cmd.Command, cmd.Target);
                            Depend(cmd.Target, cmd.Dependencies);
                            break;
                        case "REMOVE":
                            BuildOutput(cmd.Command, cmd.Target);
                            Remove(cmd.Target, true);
                            break;
                        case "LIST":
                            BuildOutput(cmd.Command, "");
                            List();
                            break;
                        case "END":
                            BuildOutput(cmd.Command, cmd.Target);
                            return;
                    }
                }
            else
                BuildOutput("", "Invalid syntax.  No END statement found.");
        }

        private void BuildOutput(string command, string target)
        {
            OutputDisplay.Add(new Output
            {
                Command = command,
                Target = target
            });
        }
        #endregion

        public void Depend(string inputName, List<string> inputDepends)
        {
            //If dependent component not known, add to dictionary
            if (!Components.ContainsKey(inputName))                         
                Components.Add(inputName, new Component(inputName));
            //If dependent component is installed, return void
            if (Components[inputName].isInstalled)                          
            {
                string lineWrite = " is already installed, cannot change dependencies";
                BuildOutput(inputName, lineWrite);
                return;
            }

            foreach (string depend in inputDepends)                         
                //No duplicate or self-referential dependencies
                if ((depend != inputName) && (!Components[inputName].Dependencies.Contains(depend)))
                {
                    Components[inputName].Dependencies.Add(depend);         
                    if (!Components.ContainsKey(depend))                    
                        Components.Add(depend, new Component(depend));
                    Components[depend].Dependents.Add(inputName);           
                }
                else
                    continue;
        }

        public void Install(string inputName, bool isExplicit)
        {
            //If component is not known, add to dictionary.
            if (!Components.ContainsKey(inputName))                         
                Components.Add(inputName, new Component(inputName));
            //If component is installed, return void.
            if (Components[inputName].isInstalled && isExplicit == true)                          
            {
                string lineWrite = " is already installed.";
                BuildOutput(inputName, lineWrite);
                return;
            }
            else if (Components[inputName].isInstalled &&isExplicit == false)
                return;

            //Implicitly install dependencies. 
            if (Components[inputName].Dependencies.Count > 0)               
                foreach (string dependency in Components[inputName].Dependencies)
                        Install(dependency, false);
            //Install component.
            Components[inputName].Setup(isExplicit);                        
            BuildOutput("    Installing ", inputName);
        }

        void Remove(string name, bool ExplicitlyRemove)
        {
            //Guard conditions
            if (Components[name].ExplicitInstall == true && ExplicitlyRemove == false)
                return;
            if (Components[name].isInstalled == false)
            {
                BuildOutput(name, " is not installed.");
                return;
            }
            if (Components[name].Dependents.Count > 0)
                foreach (string d in Components[name].Dependents)
                    if (Components[d].isInstalled == true && ExplicitlyRemove == true)
                    {
                        BuildOutput(name, " is still needed.");
                        return;
                    }
                    else if (Components[d].isInstalled == true && ExplicitlyRemove == false)
                        return;
                    else
                        continue;
            
            //Uninstall components.
            BuildOutput("    Removing ", name);
            Components[name].isInstalled = false;
            Components[name].ExplicitInstall = new bool();
            //Implicit uninstallation of dependencies.
            if (Components[name].Dependencies.Count > 0)
                foreach (string d in Components[name].Dependencies)
                    Remove(d, false);
        }
        
        public void List()
        {
            foreach (string c in Components.Keys)
                if (Components[c].isInstalled == true)
                    BuildOutput("    ", c);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}