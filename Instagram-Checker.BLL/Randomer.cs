using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Checker.BLL
{
    public static class Randomer
    {
        private static Random _rand;

        static Randomer()
        {
            _rand = new Random();
        }

        public static int Next(int min, int max)
        {
            return _rand.Next(min, max);
        }
    }
}
