using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;


namespace SFTP_File_Upload
{
    class Program
    {
        const string host = "10.14.1.201"; //cp4
        const int port = 22;
        const string username = "TSIAdmin"; //must create this user and password or use a known good one
        const string password = "tsiGearP@ss1985";
        const string workingdirectory = "/USER/SSHTEST";
        

        static void Main(string[] args)
        {
            ConnectionInfo connInfo = new ConnectionInfo(host, port, username,new AuthenticationMethod[]{new PasswordAuthenticationMethod(username, password)});

            var sshclient = new SshClient(connInfo);

            try
            {
                sshclient.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error connecting ssh client: {e.Message}");
                return;
            }
            

            if (sshclient.IsConnected)
            {
                Console.WriteLine("Ssh Client connected");
                var cmd = sshclient.CreateCommand("ipconfig /all");
                var result = cmd.Execute();
                Console.WriteLine($"Ver Result: {result}");
            }

            sshclient.Disconnect();
            Console.WriteLine("Ssh client disconnected");


            //files dropped onto this exe are imported as args...args[0] in partcular
            if (!(args.Length == 0))
            {
                string uploadfile = Path.GetFullPath(args[0]);                      //get the full path of the file dropped
                Console.WriteLine("$ Dropped file path: {uploadfile}");

                try
                {
                    var client = new SftpClient(host, port, username, password);    //create an sftpclient

                    Console.WriteLine("Creating client and connecting");
                    client.Connect();                                               //connect client

                    if (client.IsConnected)
                    {
                        Console.WriteLine($"Connected to {host}");

                        try
                        {
                            client.ChangeDirectory(workingdirectory);
                            Console.WriteLine("Changed directory to {0}", workingdirectory);
                        }
                        catch (SftpPathNotFoundException)
                        {
                            client.CreateDirectory(workingdirectory);
                            Console.WriteLine("Directory '{0}' does not exist. Creating...", workingdirectory);
                            Console.WriteLine("Changed directory to {0}", workingdirectory);
                            client.ChangeDirectory(workingdirectory);
                        }

                        using (var fileStream = new FileStream(uploadfile, FileMode.Open))
                        {
                            Console.WriteLine("Uploading {0} ({1:N0} bytes)", uploadfile, fileStream.Length);
                            client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fileStream, Path.GetFileName(uploadfile));
                        }
                        client.Disconnect();
                        Console.WriteLine("Disconnecting client.");


                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating SftpClient: {0}", e.Message);
                }

                Console.Read();
            }
            
        }
    }
}
