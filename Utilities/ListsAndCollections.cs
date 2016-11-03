using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class ListsAndCollections
    {

        /// <summary>
        /// get indexes of occurrences
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="searchval"></param>
        /// <returns></returns>
        public static List<int> GetSampleIndexes(List<byte> inputList, byte searchval)
        {
            List<int> result = Enumerable.Range(0, inputList.Count)
             .Where(i => inputList[i] == searchval)
             .ToList();
            return result;
        }

        /// <summary>
        /// count how often a value occurs in list
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="searchval"></param>
        /// <returns></returns>
        public static int CountSampleOccurrecnces(List<byte> sample, byte searchval)
        {
            return sample.Where(x => x.Equals(searchval)).Count();
        }

        /// <summary>
        /// check if list a contains a subset of list b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool ContainsAllItems(List<Guid> a, List<Guid> b)
        {
            return !b.Except(a).Any();
        }


        /// <summary>
        /// convert Byte array to string
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string ConvertByteArrayToString(List<byte> res)
        {
            string version = ConfigurationManager.AppSettings["byteStringConvertsionMode"]; 
            string returnresult = "";
            switch(version)
            {
                case "manual":
                    foreach(byte item in res)
                    {
                        returnresult += (char)item;
                    }
                    break;
                case "dotnet":
                default:
                    returnresult =  System.Text.Encoding.UTF8.GetString(res.ToArray());
                    break;                
            }
            return returnresult;
        }
    }
}
