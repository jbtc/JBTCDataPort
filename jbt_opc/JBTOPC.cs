using OPCSystemsDataConnector;
using OPCControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jbt_opc
{
    internal class ClassTagValues
    {
        internal string[] TagNames;
        internal object[] Values;
        internal bool[] Qualities;
        internal DateTime[] TimeStamps;

        public ClassTagValues(string[] NewTagNames, object[] NewValues, bool[] NewQualities, DateTime[] NewTimeStamps)
        {
            TagNames = NewTagNames;
            Values = NewValues;
            Qualities = NewQualities;
            TimeStamps = NewTimeStamps;
        }
    }

    public class JBTOPC
    {
        //internal OPCSystemsDataConnector.OPCSystemsData osd ;
        public volatile OPCControlsData OPCDataComponent1;
        private volatile string[] tags;
        public volatile string OASServer = @"\\10.204.152.11\";

        /// <summary>
        /// prepare all tags and add to subscription
        /// </summary>
        public JBTOPC( List<string> tagNames)
        {
            tags = new string[tagNames.Count];
            int tindex = 0; 
            foreach (string set in tagNames)
            {
                tags[tindex] = set;
                tindex++;
            }

            OPCDataComponent1 = new OPCControlsData();

            OPCDataComponent1.AddTags(tags);
            



            
        }

        /// <summary>
        /// remova all tags from subscription
        /// </summary>
        ~JBTOPC()
        {
            OPCDataComponent1.RemoveTags(tags);
            tags = null;
        }



        /// <summary>
        /// write tag with result - float
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        /// <returns></returns>
        public string WriteTag(string tagname, double tagvalue)
        {
            return WriteTags(tagname,tagvalue);
        }

        /// <summary>
        /// write tag with result - int
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        /// <returns></returns>
        public string WriteTag(string tagname, int tagvalue)
        {
            return WriteTags(tagname, tagvalue);
        }

        /// <summary>
        /// write tag with result - string
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        /// <returns></returns>
        public string WriteTag(string tagname, string tagvalue)
        {
            return WriteTags(tagname, tagvalue);
        }

        /// <summary>
        /// universal write tag
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="tagvalue"></param>
        /// <returns></returns>
        public string WriteTags(string tagname, object tagvalue)
        {
            string writeResult = "NONE";
            /*
             * excerpt from opc help .. 
             * The Errors array will be sized to the same length as your Tags array.
             * The following values will be returned to you in the Int32 array you pass.
             * 0 = Good Quality
             * 1 = Service was not reachable for writing
             * 2 = Timeout occurred on value, the value returned was never equal to the desired value or within the deadband for Double and Single values within the timeout period 
             * 3 = Tag array did not match the size of the Values array
             * The Timeout value is specified in milliseconds
             * The FloatDeadband is for comparing only Double and Single values.
             */

            string[] tags = new string[1];
            tags[0] = OASServer + tagname + ".Value";
            object[] values = new object[1];
            values[0] = tagvalue;

            // int array 
            var Errors = OPCDataComponent1.SyncWriteTagsWithConfirmation(tags, values, 10000, 0.0001);
            writeResult = "";
            foreach (int res in Errors)
            {
                
                switch (res)
                {
                    case 0:
                        writeResult += "Good Quality";
                        break;
                    case 1:
                        writeResult += "Service was not reachable for writing";
                        break;
                    case 2:
                        writeResult += "Timeout occurred on value, the value returned was never equal to the desired value or within the deadband for Double and Single values within the timeout period ";
                        break;
                    case 3:
                        writeResult += "Tag array did not match the size of the Values array";
                        break;

                    default:
                        writeResult += "not registered status: "+res.ToString();
                        break;
                }
                writeResult += "|";
            }
            return writeResult;
        }


        /// <summary>
        /// This event fires when the values of the Tags change.
        /// </summary>
        /// <param name="Tags"></param>
        /// <param name="Values"></param>
        /// <param name="Qualities"></param>
        /// <param name="TimeStamps"></param>
        private void OpcSystemsData_ValuesChangedAll(string[] Tags, object[] Values, bool[] Qualities, DateTime[] TimeStamps)
        {
            // High speed version with a lot of data values changing
            //lock (m_DataValuesQueue.SyncRoot)
            // {
            //     m_DataValuesQueue.Enqueue(new ClassTagValues(Tags, Values, Qualities, TimeStamps));
            // }

            // You can use the values directly here within the data event, but the example shown above with a Queue will work best with thousands of tags updating evey second.
            //' Simple version of just obtaining the tag value you are interested in.
            //Dim TagIndex As Int32
            //Dim NumberOfTagValues As Int32 = Tags.GetLength(0)
            //For TagIndex = 0 To NumberOfTagValues - 1
            //    Select Case Tags(TagIndex)
            //        Case "Ramp.Value"
            //            If Qualities(TagIndex) Then
            //                ' The value of Ramp.Value is contained in Values(TagIndex)
            //            Else
            //                ' The value of Ramp.Value is bad
            //            End If
            //    End Select
            //Next
        }


    }
}
