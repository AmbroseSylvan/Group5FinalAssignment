﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Group5FinalAssignment
{
    class VM
    {
        #region Properties
        //For Tracking each item entered in window, with attributes for name, list of dependents, isInstalled and isDependent
        private BindingList<Item> items;
        public BindingList<Item> Items
        {
            get { return items; }
            set { items = value; NotifyChanged(); }
        }
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
    
        //trying to use dictionary instead of List. 
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
                {
                    Install(cmd.Target, true);
                }
                else if (cmd.Command == "DEPENDS")
                {
                    Depend(cmd.Target, cmd.Dependencies);
                }
                else if (cmd.Command == "UNINSTALL")
                {
                    Remove(cmd.Target);
                }
                else if (cmd.Command == "LIST")
                    ;
                else
                {

                }
            }
        }

        public void ReadInputFile()
        {
            string[] inputfile = File.ReadAllLines("List.txt");
            LineInput = new BindingList<Input>();
            string[] cmdElements;

            for (var i = 0; i < inputfile.Length; i++)
            {
                cmdElements = inputfile[i].Split(' ');
                LineInput.Add(new Input
                {
                    Command = cmdElements[0],
                    Dependencies = new List<string>()
                });

                int c = cmdElements.Count();                                //element at index 1 is target of command
                if (c > 1)                                                  
                    LineInput[i].Target = cmdElements[1];
                if (c > 2)
                {
                    for (int j = 2; j < c; j++)                             //Start adding dependencies from element at index 2
                    {
                        LineInput[i].Dependencies.Add(cmdElements[j]);
                        LineInput[i].DisplayDepElement = LineInput[i].DisplayDepElement + " " + LineInput[i].Dependencies[j - 2];
                    }
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
                    if (!Components.ContainsKey(depend))   //if dependency is unknown then add to known component list
                        Components.Add(depend, new Component(depend));
                    Components[depend].Dependents.Add(inputName);           //add dependent component to dependency
                }
                else
                    continue;
        }

        #region Install
        public void Install(string inputName, bool isExplicit)
        {
            if (!Components.ContainsKey(inputName))                         //If component is not known, add to dictionary
                Components.Add(inputName, new Component(inputName));
            else if (Components[inputName].isInstalled)                     //If component is installed, return void
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
        
        #endregion

        public void Remove(string inputName, string ExplicitRemoval)
        {
            // INCOMPLETE. How Do I exclude the component that is being explicitly removed if it is a dependent of the component being implicitly removed?

            //ExplicitRemoval is used to pass the name of the component user is explicitly removing down thru each recursion
            //ExplicitRemoval is null ("") in first loop of this method

            //check: is it installed? 
            if (Components[inputName].isInstalled)
            {
                //check: does component being removed have dependents?
                if (Components[inputName].Dependents.Count > 0)
                    //Cancel removal of component if any dependents are installed 
                    foreach (string dependent in Components[inputName].Dependents)
                        //exclude the component being removed from this check
                        if ((Components[dependent].isInstalled) && (dependent != ExplicitRemoval))
                        {
                            Console.WriteLine(inputName + " is still needed.");
                            break;
                        }
                        else
                            Components[inputName].isInstalled = false;
                else
                    Components[inputName].isInstalled = false;

                foreach (string dependency in Components[inputName].Dependencies)
                {
                    if (Components[dependency].ExplicitInstall == false)
                        Remove(dependency, inputName);
                }
            }
            else
                Console.WriteLine(inputName + " is not installed.");
            
        }

        public void List()
        {

        }    
        public void isNew(string newItem)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}