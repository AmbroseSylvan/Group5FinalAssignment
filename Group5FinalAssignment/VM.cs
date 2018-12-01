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
        const string END = "END";
        const string INSTALL = "INSTALL";
        const string DEPEND = "DEPEND";
        const string REMOVE = "REMOVE";
        const string LIST = "LIST";
        const string INVENDSYNTAX = "Invalid syntax.  No END statement found.";
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
            string[] inputfile = File.ReadAllLines(FILEPATH);
            InputLines = new BindingList<Input>();
            string[] cmdElements;

            for (int i = 0; i < inputfile.Length; i++)
            {
                cmdElements = inputfile[i].Split(' ');                     //Split each command line
                int c = cmdElements.Count();

                InputLines.Add(new Input
                {
                    Command = cmdElements[0].Trim()                               //Element at index 0 is command type
                });

                if (c > 1)                                                 //element at index 1 is target of command
                    InputLines[i].Target = cmdElements[1].Trim();
                if (c > 2)                                                 //elements at index 2 and higher are dependencies
                    for (int j = 2; j < c; j++)
                    {
                        InputLines[i].Dependencies.Add(cmdElements[j].Trim());
                        InputLines[i].DisplayDepElement = InputLines[i].DisplayDepElement + " " + InputLines[i].Dependencies[j - 2];
                    }
            }
        }

        public void ExecuteScript()
        {
            //Check if the input contains an END statement
            bool oktoExecute = false;
            foreach (Input line in InputLines)
                if (line.Command == END)
                {
                    oktoExecute = true;
                    break;
                }

            if (oktoExecute == true)
                foreach (Input cmd in InputLines)
                {
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
                BuildOutput(EMPTY, INVENDSYNTAX, EMPTY);
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
                string lineWrite = ISINST1;
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
            if (Components[inputName].isInstalled==true)                          //If component is installed, return void
            {
                string lineWrite = ISINST2;
                BuildOutput(inputName, lineWrite, EMPTY);
                return;
            }
            else if (Components[inputName].Dependencies.Count == 0  && Components[inputName].isInstalled==false)
            {
                Components[inputName].Setup(isExplicit);
                BuildOutput(INSTALLING, inputName,EMPTY);
            }
            else if (Components[inputName].Dependencies.Count == 0 && Components[inputName].isInstalled == true)
            {
                BuildOutput(inputName, ISINST2, EMPTY);
            }

            if (Components[inputName].Dependencies.Count > 0)               //implicitly install dependencies. 
            {
                foreach (string dependency in Components[inputName].Dependencies)
                {                                                             //added brackets here around foreach
                    if (Components[dependency].isInstalled == false)              //added statement to check if dependency installed before installing
                    {
                        Install(dependency, false);
                    }
                }
                    if (Components[inputName].isInstalled == false)
                    {
                        Components[inputName].Setup(isExplicit);                      //Install component if it has not been installed and if it has been called to be installed
                        BuildOutput(INSTALLING, inputName, EMPTY);
                    } 
            }
        }

        void Remove(string name, bool ExplicitlyRemove)
        {
            //Exit conditions
            if (Components[name].ExplicitInstall == true && ExplicitlyRemove == false)
            {
                /*BuildOutput(TAB + name, ISNEEDED, EMPTY);*/
                return;
            }
            if (Components[name].isInstalled == false && ExplicitlyRemove == true)
            {
                BuildOutput(TAB + name, ISNOTINSTALLED, EMPTY);
                return;
            }
            if(Components[name].ExplicitInstall==false && Components[name].isInstalled==true && ExplicitlyRemove==true && Components[name].Dependents.Count==0)
            {
                Components[name].isInstalled = false;
                Components[name].ExplicitInstall = false;
                BuildOutput(REMOVING, name, EMPTY);
            }


            if (Components[name].Dependents.Count > 0)
                foreach (string d in Components[name].Dependents)
                    if (Components[d].isInstalled == true  && ExplicitlyRemove==true)
                    {
                        BuildOutput(TAB + name, ISNEEDED,EMPTY);
                        return;
                    }
                    else
                        continue;
            
            //Uninstall components
            BuildOutput(REMOVING, name, EMPTY);
            Components[name].isInstalled = false;
            Components[name].ExplicitInstall = false;
            if (Components[name].Dependencies.Count > 0)
                foreach (string d in Components[name].Dependencies)
                    Remove(d, false);
        }
        
        public void List()
        {
            foreach (string c in Components.Keys)
                if (Components[c].isInstalled == true)
                    BuildOutput(TAB, c, EMPTY);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}