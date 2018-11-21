using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Group5FinalAssignment
{
    class VM
    {
        //For Tracking each item entered in window, with attributes for name, list of dependents, isInstalled and isDependent
        private BindingList<Item> items;
        public BindingList<Item> Items
        {
            get { return items; }
            set { items = value; NotifyChanged(); }
        }
        //Bound to the input textbox
        private string input;
        public string Input
        {
            get { return input; }
            set { input = value; NotifyChanged(); }
        }
        //Used to parse the input string into individual elements for further steps
        private BindingList<String> parsedInput;
        public BindingList<String> ParsedInput
        {
            get { return parsedInput; }
            set { parsedInput = value ;NotifyChanged(); }
        }
        //Used to display the output to the console.
        private string output;
        public string Output
        {
            get { return output; }
            set { output = value; NotifyChanged(); }
        }

        private List<Component> installedComponents;
        public List<Component> InstalledComponents
        {
            get { return InstalledComponents; }
            set { installedComponents = value; NotifyChanged(); }
        }

        private List<Component> knownComponents = new List<Component>();


        //Called by the button to execute the script
        public void ExecuteScript()
        {

        }

        public void Depend(string inputName, List<string>inputDepends)
        {
            Component comp = new Component(inputName);
            int index;

            if (FindInList(inputName, installedComponents) == true)
            {
                //already installed, cannot change dependencies
            }
            else if (FindInList(inputName, knownComponents) == true)
            {
                foreach (Component knowncomp in knownComponents)
                {
                    if (inputName == knowncomp.Name)
                    {
                        index = knownComponents.IndexOf(knowncomp);         //get index of known component
                        
                        foreach (string depend in inputDepends)             //add inputDepends to indexed component
                            knownComponents[index].Dependencies.Add(depend);

                        break;
                    }
                }
            }
            else
            {
                foreach (string depend in inputDepends)                    //add dependency to new component
                    comp.Dependencies.Add(depend);

                knownComponents.Add(comp);                                 //add new component to known component list
            }

            //Then check if it is a self referential dependency. ITERATE THRU LIST. VALIDATE INPUTS.
        }

        #region Install
        public void Install(string inputName)
        {
            List<string> deps;

            //First check: is it installed? 
            if (FindInList(inputName, installedComponents) == true)
                Console.WriteLine(inputName + " is already installed.");
            else
            {
                //second check: does it have dependencies?
                deps = GetDependencies(inputName);
                if (deps.Count > 0)                                        //has dependencies.             
                {
                    //Third check: install any missing dependencies
                    InstallMissing(deps);
                }
                else if (deps.Count <= 0)                                  //no dependencies
                {
                    //If all requirements are met, then install this new component
                    Component comp = new Component(inputName);
                    installedComponents.Add(comp);
                    comp.Install();
                    Console.WriteLine("Installing " + inputName);
                }
            }
        }
        
                                                                            //MAKE THIS APPLY TO KNOWN COMPONENTS LIST AS WELL
        public bool FindInList(string inputName, List<Component>list)
        {
            bool foundInList = false;

            //Check: is this component found on the Installed Components list?
            foreach (Component comp in list)
            {
                if (inputName == comp.Name)
                {
                    foundInList = true;
                    break;
                }
                else
                    continue;
            }

            return foundInList;
        }

        public List<string> GetDependencies(string inputName)
        {
            List<string> deps = new List<string>();

            //Check: is it already on the known component list? 
            //if (FindInList(inputName, knownComponents) == true)
                
            foreach (Component knowncomp in knownComponents)
            {
                if (inputName == knowncomp.Name)
                {
                    //get the dependency list of this component
                    deps = knowncomp.Dependencies;
                    break;
                }
                else
                    continue;
            }

            return deps;
        }

        public void InstallMissing(List<string> deps)
        {
            //check: if dependency is not installed, then install it.
            foreach (string dependency in deps)
            {
                if (FindInList(dependency, installedComponents) == true)
                    continue;
                else
                {
                    Install(dependency);
                    Console.WriteLine("Installing " + dependency);
                }
            }
        }
        #endregion

        public void Uninstall()
        {

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
