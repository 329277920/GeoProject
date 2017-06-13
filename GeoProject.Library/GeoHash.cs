using System;
using System.Text;

namespace GeoProject.Library
{
    /// <summary>
    /// 提供对GeoHash的编码与解码操作
    /// </summary>
    public sealed class GeoHash
    {       
        private static string Base32Charset;

        private static byte[] Bits;

        /// <summary>
        /// 初始化相关参数
        /// </summary>
        static GeoHash()
        {            
            Base32Charset = "0123456789bcdefghjkmnpqrstuvwxyz";
            Bits = new byte[] { 1, 2, 4, 8, 16 };
        }

        /// <summary>
        /// 将一个经纬度值编码成GeoHash值
        /// </summary>
        /// <param name="longitude">经度值</param>
        /// <param name="latitude">纬度值</param>
        /// <param name="codingLen">编码长度，可取值为1-11</param>
        /// <returns>返回GeoHash值</returns>
        public string Encode(double longitude, double latitude, int codingLen = 11)
        {
            byte tempBits = 0;
            StringBuilder tempHash = new StringBuilder(codingLen);
            var longRange = new double[] { -180, 0, 180 };
            var latRange = new double[] { -90, 0, 90 };
            bool isLat = false;

            // GeoHash以5个位为一组进行编码，下面的一个循环，将有5次拆分操作 
            while (codingLen-- > 0)
            {
                tempBits = 0;
                for (var i = 4; i >= 0; i--)
                {
                    var tempRange = isLat ? latRange : longRange;
                    var tempValue = isLat ? latitude : longitude;

                    // 位于第一个区间，保持默认值
                    if (tempValue >= tempRange[0] && tempValue <= tempRange[1])
                    {
                        tempRange[2] = tempRange[1];                        
                    }
                    // 位于第二个区间，设置位1，设置区间值
                    else
                    {
                        tempRange[0] = tempRange[1];                       
                        tempBits |= Bits[i];
                    }
                    // 设置区间值
                    tempRange[1] = tempRange[0] + (Math.Abs(Math.Abs(tempRange[2]) - Math.Abs(tempRange[0]))) / 2;

                    isLat = !isLat;
                }
                tempHash.Append(Base32Charset[tempBits]);
            }            
           
            return tempHash.ToString();
        }
    }
}
