using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴-- 外圆形铣削
    /// </summary>
    public class Axis3_ExternalCircularMilling:Axis3Milling
    {
        /// <summary>
        /// 外直径
        /// </summary>
        private double Dmax { get; set; }
        /// <summary>
        /// 内直径
        /// </summary>
        private double Dmin { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        private double Height { get; set; }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        private double CuttingLength { get; set; }

        public Axis3_ExternalCircularMilling(double Dia, double Dmax, double Dmin, double Height, int NoOfPlaces, Materials _Materials)
        {
            this.Dia = Dia;
            this.Dmax = Dmax;
            this.Dmin = Dmin;
            this.Height = Height;
            this.NoOfPlaces = NoOfPlaces;
            this.No = ChangeNo();
            this.FeedPer = 0.06;
            this.ReserveLength = 2;
            this._Materials = _Materials;
            this.CuttingSpeed = GetCuttingSpeed();
            Calculate_SpindleSpeed();
            Calculate_FeedRate();
            Calculate_CuttingLength();
            Calculate_CuttingTime();
            Calculate_TotalTime();
        }
        /// <summary>
        /// 根据刀具直径给定齿数
        /// </summary>
        /// <returns></returns>
        public int ChangeNo()
        {
            if (this.Dia >= 10)
                return 4;
            else
                return 2;
        }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        protected void Calculate_CuttingLength()
        {
            this.CuttingLength = 3.14 * ((this.Dmax - this.Dmin) / 2 + this.Dmin) * Math.Ceiling((this.Dmax - this.Dmin) / 2 / this.Dia) * Math.Ceiling(this.Height / 3) + this.Height;
        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = (this.CuttingLength + this.ReserveLength) * this.NoOfPlaces * 60 / this.FeedRate;
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            this.FeedRate = this.No * this.FeedPer * this.SpindleSpeed;
        }
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected override void Calculate_SpindleSpeed()
        {
            if ((this.CuttingSpeed * 1000 / (this.Dia * 3.14) - 6500) >= 0)
            {
                this.SpindleSpeed = 6500;
            }
            else
            {
                this.SpindleSpeed = this.CuttingSpeed * 1000 / (this.Dia * 3.14);
            }
        }

        protected override void Calculate_TotalTime()
        {
            this.TotalTime = this.AtcTime + this.OtherTime + this.CuttingTime;
        }
        /// <summary>
        /// 获取材料切割速度
        /// </summary>
        /// <returns></returns>
        protected override double GetCuttingSpeed()
        {
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 80;
                case Materials.Alloy:
                    return 80;
                case Materials.Stainless:
                    return 40;
                case Materials.Aluminum:
                    return 160;
                default:
                    return 160;
            }
        }
    }
}
