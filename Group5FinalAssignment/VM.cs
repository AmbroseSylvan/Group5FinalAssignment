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
            string[] inputfile = File.ReadAllLines("../../List.txt");
            InputLines = new BindingList<Input>();
            string[] cmdElements;

            for (int i = 0; i < inputfile.Length; i++)
            {
                cmdElements = inputfile[i].Split(' ');                     //Split each command line
                int c = cmdElements.Count();

                InputLines.Add(new Input
                {
                    Command = cmdElements[0]                               //Element at index 0 is command type
                });

                if (c > 1)                                                 //element at index 1 is target of command
                    InputLines[i].Target = cmdElements[1];
                if (c > 2)                                                 //elements at index 2 and higher are dependencies
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
                            BuildOutput(cmd.Command, cmd.Target, "");
                            Install(cmd.Target, true);
                            break;
                        case "DEPEND":
                            BuildOutput(cmd.Command, cmd.Target, cmd.DisplayDepElement);
                            Depend(cmd.Target, cmd.Dependencies);
                            break;
                        case "REMOVE":
                            BuildOutput(cmd.Command, cmd.Target, "");
                            Remove(cmd.Target, true);
                            break;
                        case "LIST":
                            BuildOutput(cmd.Command, "", "");
                            List();
                            break;
                        case "END":
                            BuildOutput(cmd.Command, cmd.Target, "");
                            return;
                    }
                }
            else
                BuildOutput("", "Invalid syntax.  No END statement found.","");
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

        public void Depend(string inputName, List<string> inputDepends)
        {
            if (!Components.ContainsKey(inputName))                         //If dependent component not known, add to dictionary
                Components.Add(inputName, new Component(inputName));
            if (Components[inputName].isInstalled)                          //If dependent component is installed, return void
            {
                string lineWrite = " is already installed, cannot change dependencies";
                BuildOutput(inputName, lineWrite,"");
                return;
            }
            foreach (string depend in inputDepends)                         
                //No duplicate or self-referential dependencies
                if ((depend != inputName) && (!Components[inputName].Dependencies.Contains(depend)))
                {
                    Components[inputName].Dependencies.Add(depend);         //add dependency to component
                    if (!Components.ContainsKey(depend))                    //if dependency is unknown then add to known component list
                        Components.Add(depend, new Component(depend));
                    Components[depend].Dependents.Add(inputName);           //add dependent component to dependency
                }
                else
                    continue;
        }

        public void Install(string inputName, bool isExplicit)
        {
            if (!Components.ContainsKey(inputName))                         //If component is not known, add to dictionary
                Components.Add(inputName, new Component(inputName));
            if (Components[inputName].isInstalled)                          //If component is installed, return void
            {
                string lineWrite = "    is already installed.";
                BuildOutput(inputName, lineWrite,"");
                return;
            }
            if (Components[inputName].Dependencies.Count == 0  && Components[inputName].isInstalled==false)
            {
                Components[inputName].Setup(isExplicit);
                BuildOutput("    Installing ", inputName,"");
            }


            if (Components[inputName].Dependencies.Count > 0)               //implicitly install dependencies. 
            { 
                foreach (string dependency in Components[inputName].Dependencies)
                {                                                             //added brackets here around foreach
                    if(Components[dependency].isInstalled==false)              //added statement to check if dependency installed before installing
                    { 
                    Install(dependency, false);
                    BuildOutput("    Installing ", dependency,"");
                    }
                }
                if (Components[inputName].isInstalled == false)
                { 
                Components[inputName].Setup(isExplicit);                        //Install component
                BuildOutput("    Installing ", inputName,"");
                }
            }
        }

        void Remove(string name, bool ExplicitlyRemove)
        {
            //Exit conditions
            if (Components[name].ExplicitInstall == true && ExplicitlyRemove == false)
                return;
            if (Components[name].isInstalled == false)
            {
                Console.WriteLine("   " + name + " is not installed.");
                return;
            }
            if (Components[name].Dependents.Count > 0)
                foreach (string d in Components[name].Dependents)
                    if (Components[d].isInstalled == true  && ExplicitlyRemove==true)
                    {
                        BuildOutput("   " + name, " is still needed.","");
                        return;
                    }
                    else
                        continue;
            
            //Uninstall components
            BuildOutput("    Removing ", name, "");
            Components[name].isInstalled = false;
            Components[name].ExplicitInstall = new bool();
            if (Components[name].Dependencies.Count > 0)
                foreach (string d in Components[name].Dependencies)
                    Remove(d, false);
        }
        
        public void List()
        {
            foreach (string c in Components.Keys)
                if (Components[c].isInstalled == true)
                    BuildOutput("    ", c, "");
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}