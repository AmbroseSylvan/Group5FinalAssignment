using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Class to store the elements of each input line for further evaluation
namespace Group5FinalAssignment
{
    public class Input
    {
        public string Command { get; set; }

        public string Target { get; set; }

        public List<string> Dependencies { get; set; } = new List<string>();

        public string DisplayDepElement { get; set; }

        public override string ToString() => Command;
    }
}