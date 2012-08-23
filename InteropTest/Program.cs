using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InteropTest
{
    [StructLayout(LayoutKind.Sequential)]
    struct BBox
    {
        public float x1, y1, x2, y2;  //Corner points of the rectangle

        //Pure c#
        //does the rectangle lie between (0,0) and (1,1)
        public int isValid()
        {
            bool tr = (x1 < 1) && (x2 < 1) && (y1 < 1) && (y2 < 1) && (x1 > 0) && (x2 > 0) && (y1 > 0) && (y2 > 0);
            return tr ? 1 : 0;
        }
        public static float getPercentBB(BBox[] boxes, int size)
        {
            int sum = 0;
            for (int i = 0; i < size; i++)
            {
                 sum+= boxes[i].isValid();
            }
            return (float)sum/size * 100;
        }

        //Marshaling
        [DllImport("DllFuncs.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint= "nativef")]
        public static extern float getPercentBBMarshal(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] BBox[] boxes,
            int size);

        //Direct pointer access
        [DllImport("DllFuncs.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe float nativef(IntPtr p, int size);
        public static unsafe float getPercentBBInterop(BBox[] boxes, int size)
        {
            float result;
            fixed (BBox* p = boxes)
            {
                result = nativef((IntPtr)p, size);
            }
            return result;
        }

        //Initialize array
        public static BBox[] getBBArray(int n)
        {
            BBox[] arr = new BBox[n];
            Random r = new Random();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].x1 = (float)r.NextDouble() * 2;
                arr[i].y1 = (float)r.NextDouble() * 2;
                arr[i].x2 = (float)r.NextDouble() * 2;
                arr[i].y2 = (float)r.NextDouble() * 2;
            }
            return arr;
        }
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Initializing 10^7 boxes randomly lying between (0,0) and (2,2).\n");
            BBox[] arr = BBox.getBBArray(10000000);


            for (int j = 3; j < 8; j++)
            {

                int n = (int) Math.Pow(10, j);
                System.Console.WriteLine("For 10^{0} elements: \n", j);
                //Calculate using native c#
                System.Console.WriteLine("Calculating with C# [10 iterations]:");
                float perc = 0; double time = 0;
                for (int i = 0; i < 10; i++)
                {
                    var s1 = Stopwatch.StartNew();
                    perc = BBox.getPercentBB(arr, n);
                    s1.Stop();
                    time += s1.Elapsed.TotalMilliseconds;
                }
                System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
                System.Console.WriteLine("The Calculation took: {0} milliseconds. \n", time / 10);

                //Interop with Marshaling
                System.Console.WriteLine("Calculating with Marshaling Interop [10 iterations]:");
                time = 0;
                for (int i = 0; i < 10; i++)
                {
                    var s3 = Stopwatch.StartNew();
                    perc = BBox.getPercentBBMarshal(arr, n);
                    s3.Stop();
                    time += s3.Elapsed.TotalMilliseconds;

                }
                System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
                System.Console.WriteLine("The Calculation took: {0} milliseconds. \n", time / 10);

                //Interop with raw pointers
                System.Console.WriteLine("Calculating with Pointer Interop [10 iterations]:");
                time = 0;
                for (int i = 0; i < 10; i++)
                {
                    var s2 = Stopwatch.StartNew();
                    perc = BBox.getPercentBBInterop(arr, n);
                    s2.Stop();
                    time += s2.Elapsed.TotalMilliseconds;
                }
                System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
                System.Console.WriteLine("The Calculation took: {0} milliseconds. \n", time / 10);

            }

        }
    }
}
