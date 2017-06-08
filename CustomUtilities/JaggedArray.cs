using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUtilities
{
    public static class JaggedArray
    {
        public static T[][] Initialize2D<T>(int x, int y)
        {
            T[][] output = new T[x][];
            for (int i = 0; i < x; i++)
            {
                output[i] = new T[y];
            }
            return output;
        }
        public static T[][][] Initialize3D<T>(int x, int y, int z)
        {
            T[][][] output = new T[x][][];
            for (int i = 0; i < x; i++)
            {
                output[i] = new T[y][];
                for (int j = 0; j < y; j++)
                {
                    output[i][j] = new T[z];
                }
            }
            return output;
        }
        public static T[][][][] Initialize4D<T>(int x, int y, int z, int a)
        {
            T[][][][] output = new T[x][][][];
            for (int i = 0; i < x; i++)
            {
                output[i] = new T[y][][];
                for (int j = 0; j < y; j++)
                {
                    output[i][j] = new T[z][];
                    for (int k = 0; k < z; k++)
                    {
                        output[i][j][k] = new T[a];
                    }
                }
            }
            return output;
        }
    }
}
