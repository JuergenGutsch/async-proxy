namespace ATG.AsyncProxy.Service
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
            this.ConteogsServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ContegosServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ConteogsServiceProcessInstaller
            // 
            this.ConteogsServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ConteogsServiceProcessInstaller.Password = null;
            this.ConteogsServiceProcessInstaller.Username = null;
            // 
            // ContegosServiceInstaller
            // 
            this.ContegosServiceInstaller.ServiceName = "ATG Contegos Proxy";
            this.ContegosServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ConteogsServiceProcessInstaller,
            this.ContegosServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller ContegosServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller ConteogsServiceProcessInstaller;
    }
}