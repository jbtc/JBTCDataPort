namespace JBTCDataPort
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.JBTCDataPortserviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.JBTCDataPortInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // JBTCDataPortserviceProcessInstaller
            // 
            this.JBTCDataPortserviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.JBTCDataPortserviceProcessInstaller.Password = null;
            this.JBTCDataPortserviceProcessInstaller.Username = null;
            // 
            // JBTCDataPortInstaller
            // 
            this.JBTCDataPortInstaller.DelayedAutoStart = true;
            this.JBTCDataPortInstaller.Description = "JBTCDataPort provides connectivity to Various Field Devices such as GPUs";
            this.JBTCDataPortInstaller.DisplayName = "JBTCDataPort";
            this.JBTCDataPortInstaller.ServiceName = "JBTCDataPortService";
            this.JBTCDataPortInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.JBTCDataPortInstaller,
            this.JBTCDataPortserviceProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller JBTCDataPortserviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller JBTCDataPortInstaller;
    }
}