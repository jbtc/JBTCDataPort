using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Tools
    {
        /// <summary>
        /// Run or open file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public static void RunOrOpenFile(string filename)
        {
            Process.Start(filename);
        }       


        /// <summary>
        /// are object equal (using reflection)
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool areObjectsEqual(object o1, object o2)
        {
            Type typeX = o1.GetType();
            Type typeY = o2.GetType();
            if (!typeX.Equals(typeY))
            {
                // incompatible types
                return false;
            }
            else
            {
                // we know that the types o1 and o2 are equal 
                // this leads to the fact that we can index over 
                // the property set of the second object
                int o2Indexer = 0;
                // now get the fields of o2 as array
                var propO2 = ((System.Reflection.TypeInfo)o2.GetType().UnderlyingSystemType).DeclaredFields.ToArray();

                // iterate over the fields of the object O1
                foreach (var propO1 in ((System.Reflection.TypeInfo)o1.GetType().UnderlyingSystemType).DeclaredFields)
                {
                    // get prop names and values
                    //props of O1
                    string propO1Name = propO1.Name;
                    var propO1Value = propO1.GetValue(o1);

                    // props of O2                    
                    string propO2Name = propO2[o2Indexer].Name;
                    var propO2Value = propO2[o2Indexer].GetValue(o2);

                    if (propO1Name == propO2Name)
                    {
                        if (propO1Value.Equals(propO2Value))
                        {
                            // go on 
                        }
                        else
                        {
                            // value unequal
                            return false;
                        }
                    }
                    else
                    {
                        // field unequal
                        return false;
                    }
                    // we did have a match .. so continue
                    o2Indexer++;  //o2Indexer = o2Indexer +1; //o2Indexer +=1;
                }
            }
            // since we did not break out before,
            // we have proven that both objects are equal to another
            return true;
        }
    }
}