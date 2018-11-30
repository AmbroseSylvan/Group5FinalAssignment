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

        public List<string> Dependents { get; set; }

        public bool isInstalled { get; set; }

        public bool ExplicitInstall { get; set; }

        public Component(string name)
        {
            Name = name;
            Dependencies = new List<string>();
            Dependents = new List<string>();
            isInstalled = false;
        }

        public void Setup (bool isExplicit)
        {
            isInstalled = true;
            ExplicitInstall = isExplicit;
        }
    }
}