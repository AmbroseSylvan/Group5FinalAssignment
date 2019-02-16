// Group 5, Final Assignment, PROG8010/F18.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Group5FinalAssignment
{
    public class Output
    {
        public string Command { get; set; }

        public string Target { get; set; }

        public string DepOutput { get; set; }

        public override string ToString() => Command;

        /* public Output(string command,  string target)
         {
             Command = command;
             Target = target;
             DepOutput = new List<String>();
         }*/
    }
}