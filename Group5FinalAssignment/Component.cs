using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group5FinalAssignment
{
    class Component
    {
        public string Name { get; set; }

        public List<string> Dependencies { get; set; }

        public int InstallIndex { get; set; }

        public bool isInstalled { get; set; }

        //Mudt track EXPLICIT or IMPLICIT installation. 
        //Must track if it has dependencies (other things depend on it?)

        public Component(string name)
        {
            Name = name;
            Dependencies = new List<string>();
            isInstalled = false;
        }

        public void Depend(string dependsOn)
        {
            Dependencies.Add(dependsOn);
        }

        public void Install()
        {
            isInstalled = true;
        }
    }
}
