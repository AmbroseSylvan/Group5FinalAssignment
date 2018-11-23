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
    
        private List<Component> installedComponents;
        public List<Component> InstalledComponents
        {
            get { return InstalledComponents; }
            set { installedComponents = value; NotifyChanged(); }
        }

        private List<Component> knownComponents = new List<Component>();
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
                    Install(cmd.Target);
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
            Component comp = new Component(inputName);
            int index;

            if (FindListIndex(inputName, installedComponents) != -1)
            {
                //already installed, cannot change dependencies
            }
            else if (FindListIndex(inputName, knownComponents) != -1)
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
            if (FindListIndex(inputName, installedComponents) != -1)
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
        
        public void InstallMissing(List<string> deps)
        {
            //check: if dependency is not installed, then install it.
            foreach (string dependency in deps)
            {
                if (FindListIndex(dependency, installedComponents) != -1)
                    continue;
                else
                {
                    Install(dependency);
                    Console.WriteLine("Installing " + dependency);
                }
            }
        }
        #endregion

        public void Remove(string inputName)
        {
            int installIndex = FindListIndex(inputName, installedComponents);
            int depIndex;
            string depName;

            if (installIndex != -1)                //this means it is installed.
            {
                if (isDependency(inputName) == true)
                {
                    Console.WriteLine(inputName + " is still needed.");
                }
                else
                {
                    //before removing, try to remove its dependencies as well
                    foreach (string dep in installedComponents[installIndex].Dependencies)
                    {
                        //get name of each dependency and use as argument in Remove()
                        depIndex = FindListIndex(dep, installedComponents);
                        depName = installedComponents[depIndex].Name;
                        Remove(depName);
                    }

                    installedComponents.Remove(installedComponents[installIndex]);
                }
            }
            else
                Console.WriteLine("not installed.");                            //placeholder text. 
        }

        //MAKE THIS APPLY TO KNOWN COMPONENTS LIST AS WELL
        public int FindListIndex(string inputName, List<Component> list)
        {
            //bool foundInList = false;
            int index = -1;

            //Check: is this component found on the Installed Components list?
            foreach (Component comp in list)
            {
                if (inputName == comp.Name)
                {
                    //foundInList = true;
                    index = list.IndexOf(comp);
                    return index;
                }
                else
                    continue;
            }

            //return foundInList;
            return index;
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
