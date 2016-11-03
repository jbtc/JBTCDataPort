//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;


namespace jbt_opc
{
	internal static class ModuleNetworkNode
	{
		internal static string NetworkNode; // This is set by the user
		internal static string NetworkPath; // This is the network path to include for remoting
		internal static bool NetworkNodeChanged;
		internal static OPCSystems.OPCSystemsComponent OPCSystemsComponent1 = new OPCSystems.OPCSystemsComponent();

		internal static void SetNetworkNode(string NodeName)
		{
			NetworkNode = NodeName;
			if (string.Compare(NetworkNode, "localhost", true) == 0 || string.IsNullOrEmpty(NetworkNode))
			{
				NetworkPath = "";
			}
			else
			{
				NetworkPath = "\\\\" + NetworkNode + "\\";
				NetworkNodeChanged = true;
			}
		}
	}

}