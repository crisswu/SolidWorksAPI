using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴 -- 腰形槽
    /// </summary>
    public class Axis3_OpenSlotMilling:Axis3Milling
    {
        /// <summary>
        /// 长度
        /// </summary>
        private double Length { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        private double Width { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        private double Depth { get; set; }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        private double CuttingLength { get; set; }

        public Axis3_OpenSlotMilling(double Dia, double Length, double Width, double Depth, int NoOfPlaces, Materials _Materials)
        {
            this.Dia = Dia;
            this.Depth = Depth;
            this.Length = Length;
            this.Width = Width;
            this.NoOfPlaces = NoOfPlaces;
            this.No = 4; 
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
        /// 裁剪长度
        /// </summary>
        protected void Calculate_CuttingLength()
        { 
            this.CuttingLength = (3.14*(this.Width - this.Dia)/2+2*(this.Length - this.Width/2)+ this.Dia) * Math.Round(this.Depth / 2, 0) + this.Depth + this.ReserveLength;
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
        protected override int GetCuttingSpeed()
        {
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 120;
                case Materials.Alloy:
                    return 100;
                case Materials.Stainless:
                    return 80;
                case Materials.Aluminum:
                    return 200;
                default:
                    return 200;
            }
        }
    }
}
