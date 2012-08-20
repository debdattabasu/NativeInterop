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

        //does the rectangle lie between (0,0) and (1,1)
        public int isValid()
        {
            bool tr = (x1 < 1) && (x2 < 1) && (y1 < 1) && (y2 < 1) && (x1 > 0) && (x2 > 0) && (y1 > 0) && (y2 > 0);
            return tr ? 1 : 0;
        }
        public static float getPercentBB(BBox[] boxes)
        {
            int sum = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                 sum+= boxes[i].isValid();
            }
            return (float)sum/boxes.Length * 100;
        }

        [DllImport("DllFuncs.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint= "nativef")]
        public static extern float getPercentBBMarshall(
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] BBox[] boxes,
            int size);


        [DllImport("DllFuncs.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe float nativef(IntPtr p, int size);
        public static unsafe float getPercentBBInterop(BBox[] boxes)
        {
            float result;
            fixed (BBox* p = boxes)
            {
                result = nativef((IntPtr)p, boxes.Length);

            }
            return result;
        }

        public static BBox[] getBBArray()
        {
            BBox[] arr = new BBox[10000000];
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
            System.Console.WriteLine("Initializing 10,000,000 boxes randomly lying between (0,0) and (2,2).\n");
            BBox[] arr = BBox.getBBArray();


            //Calculate using native c#
            System.Console.WriteLine("Calculating with C#:");
            var s1 = Stopwatch.StartNew();
            float perc = BBox.getPercentBB(arr);
            s1.Stop();
            System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
            System.Console.WriteLine("The Calculation took: {0} seconds. \n", s1.Elapsed.TotalSeconds);

            //Interop with Marshalling
            System.Console.WriteLine("Calculating with Marshalling Interop:");
            var s3 = Stopwatch.StartNew();
            perc = BBox.getPercentBBMarshall(arr, arr.Length);
            s3.Stop();
            System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
            System.Console.WriteLine("The Calculation took: {0} seconds. \n", s3.Elapsed.TotalSeconds);


            //Interop with raw pointers
            System.Console.WriteLine("Calculating with Pointer Interop:");
            var s2 = Stopwatch.StartNew();
            perc = BBox.getPercentBBInterop(arr);
            s2.Stop();
            System.Console.WriteLine("The Percentage of boxes that lie between (0,0) and (1,1) are : {0}%.", perc);
            System.Console.WriteLine("The Calculation took: {0} seconds. \n", s2.Elapsed.TotalSeconds);
        }
    }
}
