// Group 5, Final Assignment, PROG8010/F18.
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

        public bool IsInstalled { get; set; } 

        public bool ExplicitInstall { get; set; } 

        public Component(string name)
        {
            Name = name;
            Dependencies = new List<string>();
            Dependents = new List<string>();
            IsInstalled = false;
            ExplicitInstall = false;
        }

        public void Setup(bool isExplicit)
        {
            IsInstalled = true;
            ExplicitInstall = isExplicit;
        }
    }
}