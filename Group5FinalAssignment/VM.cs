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
                    Depend(cmd.Target, cmd.DepElement);
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
            List<string> inputLines = new List<string>();
            LineInput = new BindingList<Input>();
            for (var i = 0; i < inputfile.Length; i++)
            {
                inputLines.Add(inputfile[i]);///Add to inputLines to split up string.
                string[] elements = inputLines[i].Split(' ');
                LineInput.Add(new Input
                {
                    Command = elements[0],
                    DepElement = new List<string>()
                });
                int c = elements.Count();
                //InputDisplay = InputDisplay + LineInput[i].Command + " " + LineInput[i].Target + " ";
                if (elements.Count() > 1)
                {
                    LineInput[i].Target = elements[1];
                }
                if (c > 2)
                {
                    for (var j = 2; j < c; j++)
                    {
                        LineInput[i].DepElement.Add(elements[j]);
                        LineInput[i].DisplayDepElement = LineInput[i].DisplayDepElement + " " + LineInput[i].DepElement[j - 2];
                    }
                }
            }

        }
        #endregion

        public void Depend(string inputName, List<string>inputDepends)
        {
            //check if component installed under this name
            if (Components[inputName].isInstalled) 
            {
                Console.WriteLine(inputName + " is already installed, cannot change dependencies");
            }
            //check if component known under this name
            else if (Components.ContainsKey(inputName))
            {
                NewDependency(inputName, inputDepends);
            }
            //If no component under this name is installed or known
            else
            {
                Components.Add(inputName, new Component(inputName));          //add new component under this name to dictionary
                NewDependency(inputName, inputDepends);
            }
        }

        private void NewDependency(string inputName, List<string> inputDepends)
        {
            foreach (string depend in inputDepends)             //add inputDepends to copy of component
                //No duplicate or self-referential dependencies
                if ((depend != inputName) && (!Components[inputName].Dependencies.Contains(depend)))
                {
                    Components[inputName].Dependencies.Add(depend);           //add dependency to component
                    if (!Components.ContainsKey(depend))   //if dependency is unknown then add to known component list
                        Components.Add(depend, new Component(depend));
                    Components[depend].Dependents.Add(inputName);  //add new component to dependent list of its dependency
                }
                else
                    continue;
        }
        
        #region Install
        public void Install(string inputName, bool isExplicit)
        {
            //check: is it installed? 
            if (Components[inputName].isInstalled)
                Console.WriteLine(inputName + " is already installed.");
            //check: is this component known?
            else if (Components.ContainsKey(inputName))
            {
                //implicitly install dependencies.             
                if (Components[inputName].Dependencies.Count > 0)
                    foreach (string dependency in Components[inputName].Dependencies)
                        Install(dependency, false);
                
                //Install this new component
                Components[inputName].Setup(isExplicit);
                Console.WriteLine("Installing " + inputName);
            }
            //if component is not known or installed then add to system and install
            else
            {
                Components.Add(inputName, new Component(inputName));       //add new component under this name to dictionary
                Components[inputName].Setup(isExplicit);
                Console.WriteLine("Installing " + inputName);
            }
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
