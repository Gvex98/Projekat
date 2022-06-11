using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Model
{
    public class PowerEntity
    {
        private long id;
        private string name;
        private double x;
        private double y;
        private double boardx;
        private double boardz;
    
    

        public PowerEntity()
        {

        }
        public long Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
        public double X
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
        public double Y
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
        public double BoardX
        {
            get
            {
                return boardx;
            }

            set
            {
                boardx = value;
            }
        }
        public double BoardZ
        {
            get
            {
                return boardz;
            }

            set
            {
                boardz = value;
            }
        }
        
    }
}
