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
        /// 获取槽铣刀
        /// </summary>
        /// <param name="diameter"></param>
        /// <returns></returns>
        public static double GetPoked(double length,double width)
        {
            if (width > 200)  
                return 20;
            else if (width >= 80) //200-80 = 16
                return 16;
            else if (width >= 40) //79-40 = 12
                return 12;
            else if (width >= 20) //39-20 =10
                return 10;
            else if (width >= 6)  //19-6 = 6
                return 6;
            else
                return 3;
        }
        /// <summary>
        /// 获取每把刀下刀的切削深度(mm)
        /// </summary>
        /// <returns></returns>
        public static double GetDepthOfCut(double Dia)
        {
            switch (Convert.ToInt32(Dia))
            {
                case 20:
                    return 5;
                case 16:
                    return 4;
                case 12:
                    return 2.5;
                case 10:
                    return 2;
                case 6:
                    return 1;
                case 3:
                    return 0.5;
                default:
                    return -1000;
            }
        }
        /// <summary>
        /// 获取槽铣刀的进给速率
        /// </summary>
        public static double Pocket_FeedRate(double Dia)
        {
            //进给率根据 刀具的直径 给出 进给率
            switch (Convert.ToInt32(Dia))
            {
                case 20:
                    return 135; 
                case 16:
                    return 120;
                case 12:
                    return 250;
                case 10:
                   return 115; 
                case 6:
                    return 100; 
                default:
                    return -1000;
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
