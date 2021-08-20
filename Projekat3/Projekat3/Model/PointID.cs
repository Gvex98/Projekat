using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Projekat3.Model
{
    public class PointID
    {
        public Point p;
        public long id;

        public PointID()
        {

        }

        public PointID(Point po, long idd)
        {
            p = po;
            id = idd;
        }
    }
}
