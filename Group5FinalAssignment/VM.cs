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
    
        private Dictionary<string, Component> installedComponents = new Dictionary<string, Component>();
        public Dictionary<string, Component> InstalledComponents
        {
            get { return InstalledComponents; }
            set { installedComponents = value; NotifyChanged(); }
        }

        //trying to use dictionary instead of List. 
        private Dictionary<string, Component> knownComponents = new Dictionary<string, Component>();
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
            if (installedComponents.ContainsKey(inputName)) 
            {
                Console.WriteLine(inputName + " is already installed, cannot change dependencies");
            }
            //check if component known under this name
            else if (knownComponents.ContainsKey(inputName))
            {
                NewDependency(inputName, inputDepends);
            }
            //If no component under this name is installed or known
            else
            {
                knownComponents.Add(inputName, new Component(inputName));          //add new component under this name to dictionary
                NewDependency(inputName, inputDepends);
            }
        }

        private void NewDependency(string inputName, List<string> inputDepends)
        {
            foreach (string depend in inputDepends)             //add inputDepends to copy of component
                                                                //No duplicate or self-referential dependencies
                if ((depend != inputName) && (!knownComponents[inputName].Dependencies.Contains(depend)))
                {
                    knownComponents[inputName].Dependencies.Add(depend);           //add dependency to new component
                    if (!knownComponents.ContainsKey(depend))   //if dependency is unknown then add to known component list
                        knownComponents.Add(depend, new Component(depend));
                    knownComponents[depend].Dependents.Add(inputName);  //add new component to dependent list of its dependency
                }
                else
                    continue;
        }
        
        #region Install
        public void Install(string inputName, bool isExplicit)
        {
            //check: is it installed? 
            if (installedComponents.ContainsKey(inputName))
                Console.WriteLine(inputName + " is already installed.");
            //check: is this component known?
            else if (knownComponents.ContainsKey(inputName))
            {
                //implicitly install dependencies.             
                if (knownComponents[inputName].Dependencies.Count > 0)
                    foreach (string dependency in knownComponents[inputName].Dependencies)
                        Install(dependency, false);
                
                //Install this new component
                knownComponents[inputName].Install(isExplicit);
                Console.WriteLine("Installing " + inputName);
            }
            //if component is not known or installed then add to system and install
            else
            {
                knownComponents.Add(inputName, new Component(inputName));       //add new component under this name to dictionary
                knownComponents[inputName].Install(isExplicit);
                Console.WriteLine("Installing " + inputName);
            }
        }
        
        #endregion

        public void Remove(string inputName)
        {
            /* New idea for how to do this:
             * check: does component being removed have dependents other than the one already being uninstalled? If YES, no remove. If NO, proceed.
             * check: Is this component explicitly installed? If YES, must be removed manually. If NO, can be automatically removed. 
             */

            if (installedComponents.TryGetValue(inputName, out Component comp))
            {
                //PROBLEM: ALL DEPENDENCIES WILL FAIL THIS TEST AND BE UNABLE TO BE UNINSTALLED. There must be a more specific test than 
                //"is it a dependency for any other component" Make it check for dependents OTHER than the component we are trying to remove. 
                if (isDependency(inputName) == true)
                {
                    Console.WriteLine(inputName + " is still needed.");
                }
                else
                {
                    //before removing, try to remove its dependencies as well
                    if (comp.Dependencies.Count > 0)
                    {
                        foreach (string dep in comp.Dependencies)
                        {
                            //get name of each dependency and use as argument in Remove()
                            Remove(dep);
                        }
                    }

                    installedComponents.Remove(inputName);
                }
            }
            else
                Console.WriteLine("not installed.");                            //placeholder text. 
        }

        public List<string> GetDependencies(string inputName)
        {
            List<string> deps = new List<string>();

            //Check: is it already on the known component list? 
            if (knownComponents.TryGetValue(inputName, out Component comp))
                deps = comp.Dependencies;

            return deps;
        }

        public bool isDependency(string inputName)
        {
            bool isDependency = false;

            foreach (Component inst in installedComponents)
            {
                if (inst.Dependencies.Count > 0)
                {
                    foreach (string dep in inst.Dependencies)
                    {
                        if (inputName == dep)
                        {
                            isDependency = true;
                            return isDependency;
                        }
                        else
                            continue;
                    }
                }
                else
                    continue;
            }

            return isDependency;
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
