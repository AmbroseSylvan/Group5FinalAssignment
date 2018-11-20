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

        public Component(string name)
        {
            Name = name;
            Dependencies = new List<string>();
            isInstalled = false;
        }

        public void Depend(string dependsOn)
        {
            Dependencies.Add(dependsOn);
            Console.WriteLine("dependency added");            //test for printing output
        }

        public void Install()
        {
            isInstalled = true;
        }
    }
}
