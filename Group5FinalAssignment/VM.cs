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
        //Bound to the input textbox
        private string inputDisplay;
        public string InputDisplay
        {
            get { return inputDisplay; }
            set { inputDisplay = value; NotifyChanged(); }
        }
        //Used to parse the input string into individual elements for further steps
        private BindingList<Input> lineInput;
        public BindingList<Input> LineInput
        {
            get { return lineInput; }
            set { lineInput = value; NotifyChanged(); }
        }
        //Used to display the output to the console.
        private string outputDisplay;
        public string OutputDisplay
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
            ReadInputFile();
            ExecuteScript();
        }

        public void ExecuteScript()
        {
            foreach (Input cmd in LineInput)
            {
                if (cmd.Command == "INSTALL")
                    Install(cmd.Target, true);
                else if (cmd.Command == "DEPEND")
                    Depend(cmd.Target, cmd.Dependencies);
                else if (cmd.Command == "REMOVE")
                    Remove(cmd.Target, true);
                else if (cmd.Command == "LIST")
                    continue;
                else
                {
                    continue;
                }
            }
        }

        public void ReadInputFile()
        {
            string[] inputfile = File.ReadAllLines("List.txt");
            LineInput = new BindingList<Input>();
            string[] cmdElements;

            for (int i = 0; i < inputfile.Length; i++)
            {
                cmdElements = inputfile[i].Split(' ');                     //Split each command line
                int c = cmdElements.Count();

                LineInput.Add(new Input
                {
                    Command = cmdElements[0]                               //Element at index 0 is command type
                });

                if (c > 1)                                                 //element at index 1 is target of command
                    LineInput[i].Target = cmdElements[1];
                if (c > 2)                                                 //elements at index 2 and higher are dependencies
                    for (int j = 2; j < c; j++)                            
                    {
                        LineInput[i].Dependencies.Add(cmdElements[j]);
                        LineInput[i].DisplayDepElement = LineInput[i].DisplayDepElement + " " + LineInput[i].Dependencies[j - 2];
                    }
            }
        }
        #endregion

        public void Depend(string inputName, List<string>inputDepends)
        {
            if (!Components.ContainsKey(inputName))                         //If dependent component not known, add to dictionary
                Components.Add(inputName, new Component(inputName));
            if (Components[inputName].isInstalled)                          //If dependent component is installed, return void
            {
                Console.WriteLine(inputName + " is already installed, cannot change dependencies");
                return;
            }
            foreach (string depend in inputDepends)                         //Else
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
                Console.WriteLine(inputName + " is already installed.");
                return;
            }             
            if (Components[inputName].Dependencies.Count > 0)               //implicitly install dependencies. 
                foreach (string dependency in Components[inputName].Dependencies)
                        Install(dependency, false);
            Components[inputName].Setup(isExplicit);                        //Install component
            Console.WriteLine("Installing " + inputName);
        }

        void Remove(string name, bool ExplicitlyRemove)
        {
            //Exit conditions
            if (Components[name].ExplicitInstall == true && ExplicitlyRemove == false)
                return;
            if (Components[name].isInstalled == false)
            {
                Console.WriteLine(name + " is not installed.");
                return;
            }
            if (Components[name].Dependents.Count > 0)
                foreach (string d in Components[name].Dependents)
                    if (Components[d].isInstalled == true)
                    {
                        Console.WriteLine(name + " is still needed.");
                        return;
                    }
                    else
                        continue;

            //Uninstall components
            Console.WriteLine("Removing " + name);
            Components[name].isInstalled = false;
            if (Components[name].Dependencies.Count > 0)
                foreach (string d in Components[name].Dependencies)
                    Remove(d, false);
        }
        
        public void List()
        {

        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}