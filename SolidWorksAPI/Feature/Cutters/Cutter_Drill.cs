using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 临时刀具类
    /// </summary>
    public class Cutter_Drill
    {
        public static double drill_1 = 2.5;
        public static double pocked = 4;
        public static double openSlot = 26;
        public static double closeSlot = 26;
        public static double chamfer = 6;

        /// <summary>
        /// 槽铣刀 规格
        /// </summary>
        public static double[] pockeds = { 6,10,16,20 };

        /// <summary>
        /// 获取槽铣刀
        /// </summary>
        /// <param name="diameter"></param>
        /// <returns></returns>
        public static double GetPoked(double diameter)
        {
            return 0;
        }
    }
}
