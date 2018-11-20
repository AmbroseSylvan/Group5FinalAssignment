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

        public void Depend(Component dependent, string dependsOn)
        {
            dependent.Dependencies.Add(dependsOn);
            Console.WriteLine("dependency added");            //test for printing output
        }

        public bool FindInstallation(string inputName)
        {
            bool isInstalled = false;

            //Check Installed Components list for a match to the user-supplied name.
            foreach (Component inst in InstalledComponents)
            {
                if (inputName == inst.Name)
                {
                    isInstalled = true;
                    break;
                }
                else
                    continue;
            }

            return isInstalled;
        }

        public List<string> FindDependencies(string inputName)
        {
            //First check: is it already on the known component list? 
            List<string> deps = new List<string>();

            //bool isKnown = false;

            foreach (Component comp in knownComponents)
            {
                if (inputName == comp.Name)
                {
                    //isKnown = true;
                    //get the dependency list of this comp
                    deps = comp.Dependencies;
                    break;
                }
                else
                    continue;
            }

            return deps;

            /*
            //If it is known then check dependency list. 
            //If it is not known then it has no dependencies by default. Create independent component by this name. 
            if (isKnown == true)
            {
                //check the dependency list of this comp
                foreach (string dependency in deps)
                {
                    //if dependency is not installed, then install it
                    if (FindInstallation(dependency) == false)
                        Install(dependency);                            //PATCH ME UP.
                }
            }
            else if (isKnown == false)
            {
                Install(inputName);
            }
            */
            
        }
        public void Install(string inputName)
        {
            //If not, then run the install command on the dependency before completing this installation. 
            //Install component. 

            List<string> deps;

            //First check: is it already installed? Remember, each unique named component can only be installed once. 
            if (FindInstallation(inputName) == false)
            {
                //Second check: does it have dependencies?
                deps = FindDependencies(inputName);                         //This and below commented line should be encapsulated together. 

                if (deps.Count > 0)         //has dependencies.             //
                {
                    //Third check: do those dependencies exist on the Installed Cmponent List? 
                    foreach (string dependency in deps)
                    {
                        if (FindInstallation(dependency) == true)
                            continue;
                        else
                        {
                            var depdep = FindDependencies(dependency);      //replace: with the encapsulated method including the two above commented lines. 
                        }
                    }
                }
                else if (deps.Count <= 0)       //no dependencies
                {

                }

                Component comp;

                comp = new Component(inputName);     //placeholder text
                comp.Install();
                installedComponents.Add(comp);
            }
            else
                Console.WriteLine("Already installed.");        //placeholder

        }

        public void CheckDependencies(Component comp)
        {
            //does this component require others to function?
            if (comp.Dependencies.Count > 0)
            {
                foreach (Component inst in InstalledComponents)
                {
                    //if new component's dependencies not matched to installed component list:
                    if (comp.Dependencies[0].IndexOf(inst.Name) == -1)
                    {

                    }
                }
            }
            else if (comp.Dependencies.Count <= 0)
            {

            }
        }

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
