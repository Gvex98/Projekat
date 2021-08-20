using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Model
{
    public class Node
    {
        private int x;
        private int y;
        private Node parent;

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public Node Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }
        public Node(int a, int b, Node par)
        {
            X = a;
            Y = b;
            Parent = par;
        }
        public Node()
        {

        }
    }
}
