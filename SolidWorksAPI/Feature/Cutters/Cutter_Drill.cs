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
        public static double GetPoked(double length,double width)
        {
            if (length >= 100 && width > 20)
                return 20;
            else if (length >= 100 && width < 20)
            {
                if (width > 16)
                    return 16;
                else if (width > 10)
                    return 10;
                else
                    return 6;
            }
            else
            {
                if (length > 50 && width > 16)
                    return 16;
                else if (length > 50 && width < 16)
                {
                    if (width > 10)
                        return 10;
                    else
                        return 6;
                }
                else
                {
                    if (length > 30 && width > 10)
                        return 10;
                    else
                    {
                        return 6;
                    }
                }
            }
        }
        /// <summary>
        /// 精铣刀具（根据上把粗铣到 选择精铣刀具）
        /// </summary>
        /// <param name="aftherSize">上一把刀的直径</param>
        /// <returns></returns>
        public static double GetFinish(double aftherSize)
        {
            if (aftherSize >= 20)
                return 10;
            else
                return 6;
        }

        /// <summary>
        /// 获取槽铣刀
        /// </summary>
        /// <param name="diameter"></param>
        /// <returns></returns>
        public static double GetCirclePock(double diameter)
        {
            if (diameter > 20)
                return 20;
            else if (diameter <= 20 && diameter > 16)
                return 16;
            else if (diameter <= 16 && diameter > 10)
                return 10;
            else
                return 6;
        }
    }
}
